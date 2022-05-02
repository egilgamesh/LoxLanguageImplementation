namespace CraftInterpreter.Lox.Parser;

// ReSharper disable MethodTooLong
// ReSharper disable once ClassTooBig
public class Parser
{
	private const int MaxFunctionArgumentsCount = 32;
	private readonly ErrorHandler errorHandler;
	private readonly IList<Token> tokens;
	private int current;
	private int loopDepth;

	/// <summary>
	///   Creates a new instance of our parser. The
	///   <see>
	///     <cref>IErrorHandler</cref>
	///   </see>
	///   was
	///   added by me to handle errors.
	/// </summary>
	/// <param name="tokens"></param>
	/// <param name="errorHandler"></param>
	public Parser(IList<Token> tokens, ErrorHandler errorHandler)
	{
		this.errorHandler = errorHandler;
		this.tokens = tokens;
		current = 0;
	}

	/// <summary>
	///   Starts the parsing processes for our list of tokens.
	/// </summary>
	/// <returns></returns>
	public List<Statement> Parse()
	{
		var statements = new List<Statement>();
		while (!IsAtEnd())
			statements.Add(Declaration());
		return statements;
	}

	private Statement Declaration()
	{
		try
		{
			if (MatchAny(TokenType.Var)) return VarDeclaration();
			return MatchAny(TokenType.Fun)
				? Function("function")
				: Statement();
		}
		catch (LoxExceptions)
		{
			Synchronize();
			return null!;
		}
	}

	private Statement Function(string kind)
	{
		var name = Consume(TokenType.Identifier, "Expect " + kind + " name.");
		Consume(TokenType.LeftParenthesis, "Expect '(' after " + kind + " name.");
		var parameters = new List<Token>();
		if (!Check(TokenType.RightParenthesis))
			do
			{
				if (parameters.Count >= MaxFunctionArgumentsCount)
					Error(Peek(),
						"Cannot have more then " + MaxFunctionArgumentsCount + " parameters.");
				parameters.Add(Consume(TokenType.Identifier, "Expect parameter name."));
			} while (MatchAny(TokenType.Comma));
		Consume(TokenType.RightParenthesis, "Expect ')' after parameters.");
		Consume(TokenType.LeftBrace, "Expect '{' before " + kind + " body.");
		var body = Block();
		return new Statement.Function(name, parameters, body);
	}

	private Statement VarDeclaration()
	{
		var name = Consume(TokenType.Identifier, "Expect variable name.");
		Expression initializer = null!;
		if (MatchAny(TokenType.Equal))
			initializer = Expression();
		Consume(TokenType.Semicolon, "Expect ';' after variable declaration.");
		return new Statement.Var(name, initializer);
	}

	private Statement Statement()
	{
		if (MatchAny(TokenType.For)) return ForStatement();
		if (MatchAny(TokenType.Break)) return BreakStatement();
		if (MatchAny(TokenType.If)) return IfStatement();
		if (MatchAny(TokenType.Print)) return PrintStatement();
		if (MatchAny(TokenType.Return)) return ReturnStatement();
		if (MatchAny(TokenType.While)) return WhileStatement();
		if (MatchAny(TokenType.LeftBrace)) return new Statement.Block(Block());
		return ExpressionStatement();
	}

	private Expression Expression() => Assignment();

	private Expression Assignment()
	{
		var expr = OrOperator();
		if (!MatchAny(TokenType.Equal))
			return expr;
		var equal = Previous();
		var value = Assignment();
		if (expr is Expression.Variable variable)
		{
			var name = variable.name;
			return new Expression.Assign(name, value);
		}
		Error(equal, "Invalid assignment target.");
		return expr;
	}

	private Expression OrOperator()
	{
		var expr = AndOperator();
		while (MatchAny(TokenType.Or))
		{
			var @operator = Previous();
			var right = AndOperator();
			expr = new Expression.Logical(expr, @operator, right);
		}
		return expr;
	}

	private Expression AndOperator()
	{
		var expr = Equality();
		while (MatchAny(TokenType.And))
		{
			var @operator = Previous();
			var right = Equality();
			expr = new Expression.Logical(expr, @operator, right);
		}
		return expr;
	}

	private Statement PrintStatement()
	{
		var value = Expression();
		Consume(TokenType.Semicolon, "Expect ';' after value.");
		return new Statement.Print(value);
	}

	private Statement ReturnStatement()
	{
		var keyword = Previous();
		Expression value = null!;
		if (!Check(TokenType.Semicolon))
			value = Expression();
		Consume(TokenType.Semicolon, "Expect ';' after return value.");
		return new Statement.Return(keyword, value);
	}

	private Statement ForStatement()
	{
		Consume(TokenType.LeftParenthesis, "Expect '(' after 'for'.");
		Statement initializer;
		if (MatchAny(TokenType.Semicolon))
			// None is defined
			initializer = null!;
		else if (MatchAny(TokenType.Var))
			initializer = VarDeclaration();
		else
			initializer = ExpressionStatement();
		Expression condition = null!;
		if (!Check(TokenType.Semicolon))
			condition = Expression();
		Consume(TokenType.Semicolon, "Expect ';' after loop condition");
		Expression increment = null!;
		if (!Check(TokenType.RightParenthesis))
			increment = Expression();
		Consume(TokenType.RightParenthesis, "Expect ')' after for clauses.");
		Exception exception;
		try
		{
			loopDepth++;
			var body = Statement();
			// ReSharper disable once ConditionIsAlwaysTrueOrFalse
			if (increment != null)
			{
				var expression = new Statement.ExpressionStatement(increment);
				var content = new List<Statement> { body, expression };
				body = new Statement.Block(content);
			}
			// ReSharper disable once ConditionIsAlwaysTrueOrFalse
			// ReSharper disable once ConstantNullCoalescingCondition
			condition ??= new Expression.Literal(true);
			body = new Statement.While(condition, body);
			// ReSharper disable once ConditionIsAlwaysTrueOrFalse
			if (initializer == null)
				return body;
			{
				var content = new List<Statement> { initializer, body };
				body = new Statement.Block(content);
			}
			return body;
		}
		catch (Exception e)
		{
			exception = e;
		}
		finally
		{
			loopDepth--;
		}
		throw exception;
	}

	private Statement BreakStatement()
	{
		if (loopDepth == 0)
			Error(Previous(), "Must be inside a loop to use 'break'");
		Consume(TokenType.Semicolon, "Expect ';' after 'break'.");
		return new Statement.Break();
	}

	private Statement IfStatement()
	{
		Consume(TokenType.LeftParenthesis, "Expect '(' after 'if'.");
		var condition = Expression();
		Consume(TokenType.RightParenthesis, "Expect ')' after if condition.");
		var thenBranch = Statement();
		Statement elseBranch = null!;
		if (MatchAny(TokenType.Else))
			elseBranch = Statement();
		return new Statement.IfStatement(condition, thenBranch, elseBranch);
	}

	private Statement WhileStatement()
	{
		Consume(TokenType.LeftParenthesis, "Expect '(' after 'while'.");
		var condition = Expression();
		Consume(TokenType.RightParenthesis, "Expect ')' after condition.");
		Exception exception;
		try
		{
			loopDepth++;
			var body = Statement();
			return new Statement.While(condition, body);
		}
		catch (Exception e)
		{
			exception = e;
		}
		finally
		{
			loopDepth--;
		}
		throw exception;
	}

	private List<Statement> Block()
	{
		var statements = new List<Statement>();
		while (!Check(TokenType.RightBrace) && !IsAtEnd())
			statements.Add(Declaration());
		Consume(TokenType.RightBrace, "Expect '}' after block.");
		return statements;
	}

	public Statement ExpressionStatement()
	{
		var expr = Expression();
		Consume(TokenType.Semicolon, "Expect ':' after expression.");
		return new Statement.ExpressionStatement(expr);
	}

	private Expression Conditional()
	{
		var expression = Equality();
		if (MatchAny(TokenType.Question))
		{
			var thenBranch = Expression();
			Consume(TokenType.Colon, "Expect ':' after then branch of conditional expression.");
			var elseBranch = Conditional();
			expression = new Expression.Conditional(expression, thenBranch, elseBranch);
		}
		return expression;
	}

	public Expression Equality()
	{
		var expression = Comparison();
		while (MatchAny(TokenType.BangEqual, TokenType.EqualEqual))
		{
			var @operator = Previous();
			var right = Comparison();
			expression = new Expression.Binary(expression, @operator, right);
		}
		return expression;
	}

	/// <summary>
	///   It checks to see if the current token is any of the given types. If so, it
	///   consumes it and returns true. Otherwise, it returns false and leaves the token where it is.
	/// </summary>
	private bool MatchAny(params TokenType[] types)
	{
		for (var i = 0; i < types.Length; i++)
			if (Check(types[i]))
			{
				Advance();
				return true;
			}
		return false;
	}

	/// <summary>
	///   This returns true if the current token is of the given type. Unlike match(), it doesn’t consume the
	///   token, it only looks at it.
	/// </summary>
	private bool Check(TokenType tokenType)
	{
		if (IsAtEnd())
			return false;
		return Peek().type == tokenType;
	}

	/// <summary>
	///   This consumes the current token and returns it, similar to
	///   how our scanner’s advance() method did with characters.
	/// </summary>
	private Token Advance()
	{
		if (!IsAtEnd())
			current++;
		return Previous();
	}

	/// <summary>
	///   Returns true if we are on the last token or end of file and
	///   false if we are not.
	/// </summary>
	private bool IsAtEnd() => tokens[current].type == TokenType.Eof;

	/// <summary>
	///   returns the current token we have yet to consume.
	/// </summary>
	private Token Peek() => tokens[current];

	/// <summary>
	///   returns the most recently consumed token
	/// </summary>
	private Token Previous() => tokens[current - 1];

	/// <summary>
	///   The grammar rule is virtually identical and so is the code.
	///   The only differences are the token types for the operators we match, and the
	///   method we call for the operands, now term(). The remaining two binary
	///   operator rules follow the same pattern:
	/// </summary>
	/// <returns></returns>
	private Expression Comparison()
	{
		var expression = Term();
		var right = Term();
		while (MatchAny(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less,
						TokenType.LessEqual))
		{
			var @operator = Previous();
			expression = new Expression.Binary(expression, @operator, right);
		}
		return expression;
	}

	private Expression Term()
	{
		var expression = Factor();
		while (MatchAny(TokenType.Minus, TokenType.Plus))
		{
			var @operator = Previous();
			var right = Factor();
			expression = new Expression.Binary(expression, @operator, right);
		}
		return expression;
	}

	private Expression Factor()
	{
		var expression = Unary();
		var right = Unary();
		while (MatchAny(TokenType.Slash, TokenType.Star, TokenType.Modulus))
		{
			var @operator = Previous();
			expression = new Expression.Binary(expression, @operator, right);
		}
		return expression;
	}

	private Expression Unary()
	{
		if (!MatchAny(TokenType.Bang, TokenType.Minus))
			return Primary();
		Previous();
		var right = Unary();
		return Primary();
	}

	//private Expression Call()
	//{
	//	var expr = Postfix();
	//	while (true)
	//		if (MatchAny(TokenType.LeftParenthesis))
	//			expr = FinishCall(expr);
	//		else
	//			break;
	//	return expr;
	//}

	//private Expression Postfix()
	//{
	//	var expression = Primary();
	//	while (MatchAny(TokenType.MinusMinus, TokenType.PlusPlus))
	//		expression = new Expression.Postfix(Previous(), expression);
	//	return expression;
	//}

	private Expression Primary()
	{
		if (MatchAny(TokenType.False)) return new Expression.Literal(false);
		if (MatchAny(TokenType.True)) return new Expression.Literal(true);
		if (MatchAny(TokenType.Nil)) return new Expression.Literal(null!);
		if (MatchAny(TokenType.Number, TokenType.String))
			return new Expression.Literal(Previous().literal);
		if (MatchAny(TokenType.Identifier))
			return new Expression.Variable(Previous());
		if (MatchAny(TokenType.LeftParenthesis))
		{
			var expression = Expression();
			Consume(TokenType.RightParenthesis, "Expect ')' after expression.");
			return new Expression.Grouping(expression);
		}

		// Error productions
		if (MatchAny(TokenType.BangEqual, TokenType.EqualEqual))
		{
			Error(Previous(), "Missing left-hand operand.");
			Equality();
			return null!;
		}
		if (MatchAny(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual))
		{
			Error(Previous(), "Missing left-hand operand.");
			Comparison();
			return null!;
		}
		if (MatchAny(TokenType.Plus))
		{
			Error(Previous(), "Missing left-hand operand.");
			Term();
			return null!;
		}
		if (MatchAny(TokenType.Slash, TokenType.Star, TokenType.Modulus))
		{
			Error(Previous(), "Missing left-hand operand.");
			Factor();
			return null!;
		}
		throw new Lox.LoxExceptions("Expected expression.");
	}

	private Expression FinishCall(Expression callee)
	{
		var arguments = new List<Expression>();
		if (!Check(TokenType.RightParenthesis))
			do
			{
				arguments.Add(Expression());
				if (arguments.Count > MaxFunctionArgumentsCount)
					Error(Peek(),
						"Cannon have more then " + MaxFunctionArgumentsCount +
						" arguments for a function");
			} while (MatchAny(TokenType.Comma));
		var paren = Consume(TokenType.RightParenthesis, "Expect ')' after arguments.");
		return new Expression.CallExpression(callee, paren, arguments);
	}

	// ReSharper disable once FlagArgument
	private Token Consume(TokenType type, string errorMessageIfNotFound)
	{
		if (Check(type))
			return Advance();
		throw new Lox.LoxExceptions(errorMessageIfNotFound);
	}

	private void Error(Token token, string message) => errorHandler.Error(token, message);

	private void Synchronize()
	{
		Advance();
		while (!IsAtEnd())
		{
			if (Previous().type == TokenType.Semicolon)
				return;
			switch (Peek().type)
			{
			case TokenType.Class:
			case TokenType.Fun:
			case TokenType.Var:
			case TokenType.For:
			case TokenType.If:
			case TokenType.While:
			case TokenType.Print:
			case TokenType.Return:
				return;
			}
			Advance();
		}
	}
}