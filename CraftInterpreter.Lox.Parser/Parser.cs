namespace CraftInterpreter.Lox.Parser;

// ReSharper disable once ClassTooBig
public class parser
{
	private readonly ErrorHandler errorHandler;
	private readonly List<Token> tokens;
	private int current;

	public parser(List<Token> tokens, ErrorHandler errorHandler)
	{
		this.tokens = tokens;
		this.errorHandler = errorHandler;
	}

	public List<Statement> Parse()
	{
		var statements = new List<Statement>();
		while (!IsAtEnd())
			statements.Add(Declaration());
		return statements;
	}

	// ReSharper disable once MethodTooLong
	private Statement Statement()
	{
		if (MatchAny(TokenType.For))
			return ForStatement();
		if (MatchAny(TokenType.If))
			return IfStatement();
		if (MatchAny(TokenType.Print))
			return PrintStatement();
		if (MatchAny(TokenType.While))
			return WhileStatement();
		if (MatchAny(TokenType.LeftBrace))
			return new Statement.Block(Block());
		return ExpressionStatement();
	}

	// ReSharper disable once MethodTooLong
	private Statement ForStatement()
	{
		Consume(TokenType.LeftParenthesis, "Expect '(' after 'for'.");
		Statement initializer;
		if (MatchAny(TokenType.Semicolon))
			initializer = null!;
		else if (MatchAny(TokenType.Var))
			initializer = VarDeclaration();
		else
			initializer = ExpressionStatement();
		Expression condition = null!;
		if (!Check(TokenType.Semicolon))
			condition = Expression();
		Consume(TokenType.Semicolon, "Expect ';' after loop condition.");
		Expression increment = null!;
		if (!Check(TokenType.RightParenthesis))
			increment = Expression();
		Consume(TokenType.RightParenthesis, "Expect ')' after for clauses.");
		var body = Statement();
		body = new Statement.Block(new List<Statement>
		{
			body, new Statement.ExpressionStatement(increment)
		});
		condition ??= new Expression.Literal(true);
		body = new Statement.While(condition, body);
		// ReSharper disable once ConditionIsAlwaysTrueOrFalse
		if (initializer != null)
			body = new Statement.Block(new List<Statement> { initializer, body });
		return body;
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

	private Statement PrintStatement()
	{
		var value = Expression();
		Consume(TokenType.Semicolon, "Expect ';' after value.");
		return new Statement.Print(value);
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

	private Statement WhileStatement()
	{
		Consume(TokenType.LeftParenthesis, "Expect '(' after 'while'.");
		var condition = Expression();
		Consume(TokenType.RightParenthesis, "Expect ')' after 'while' condition.");
		var body = Statement();
		return new Statement.While(condition, body);
	}

	private Statement ExpressionStatement()
	{
		var expr = Expression();
		Consume(TokenType.Semicolon, "Expect ';' after expression.");
		return new Statement.ExpressionStatement(expr);
	}

	private List<Statement> Block()
	{
		var statements = new List<Statement>();
		while (!Check(TokenType.RightBrace) && !IsAtEnd())
			statements.Add(Declaration());
		Consume(TokenType.RightBrace, "Expect '}' after block.");
		return statements;
	}

	private Expression Assignment()
	{
		var expr = OrOperator();
		if (!MatchAny(TokenType.Equal))
			return expr;
		var equals = Previous();
		var value = Assignment();
		if (expr is Expression.Variable variable)
		{
			var name = variable.name;
			return new Expression.Assign(name, value);
		}
		Error(equals, "Invalid assignment target.");
		return expr;
	}

	private Expression OrOperator()
	{
		var expr = AndOperator();
		while (MatchAny(TokenType.Or))
		{
			var opr = Previous();
			var right = AndOperator();
			expr = new Expression.Logical(expr, opr, right);
		}
		return expr;
	}

	private Expression AndOperator()
	{
		var expr = Equality();
		while (MatchAny(TokenType.And))
		{
			var opr = Previous();
			var right = Equality();
			expr = new Expression.Logical(expr, opr, right);
		}
		return expr;
	}

	private Expression Expression() => Assignment();

	private Statement Declaration()
	{
		try
		{
			return MatchAny(TokenType.Var)
				? VarDeclaration()
				: Statement();
		}
		catch (ParseError)
		{
			Synchronize();
			return null!;
		}
	}

	private Expression Equality()
	{
		var expression = Comparison();
		while (MatchAny(TokenType.BangEqual, TokenType.EqualEqual))
		{
			var operatorSign = Previous();
			var right = Comparison();
			expression = new Expression.Binary(expression, operatorSign, right);
		}
		return expression;
	}

	private Expression Comparison()
	{
		var expression = Term();
		while (MatchAny(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less,
						TokenType.LessEqual))
		{
			var operatorSign = Previous();
			var right = Term();
			expression = new Expression.Binary(expression, operatorSign, right);
		}
		return expression;
	}

	private Expression Term()
	{
		var expression = Factor();
		while (MatchAny(TokenType.Minus, TokenType.Plus))
		{
			var operatorSign = Previous();
			var right = Factor();
			expression = new Expression.Binary(expression, operatorSign, right);
		}
		return expression;
	}

	private Expression Factor()
	{
		var expression = Unary();
		while (MatchAny(TokenType.Slash, TokenType.Star))
		{
			var operatorSign = Previous();
			var right = Unary();
			expression = new Expression.Binary(expression, operatorSign, right);
		}
		return expression;
	}

	private Expression Unary()
	{
		if (!MatchAny(TokenType.Bang, TokenType.Minus))
			return Primary();
		var operatorSign = Previous();
		var right = Unary();
		return new Expression.Unary(operatorSign, right);
	}

	// ReSharper disable once MethodTooLong
	private Expression Primary()
	{
		if (MatchAny(TokenType.False)) return new Expression.Literal(false);
		if (MatchAny(TokenType.True)) return new Expression.Literal(true);
		if (MatchAny(TokenType.Nil)) return new Expression.Literal(null!);
		if (MatchAny(TokenType.Number, TokenType.String))
			return new Expression.Literal(Previous().literal);
		if (MatchAny(TokenType.Identifier)) return new Expression.Variable(Previous());
		if (!MatchAny(TokenType.LeftParenthesis)) throw Error(Peek(), "Expect expression.");
		var expr = Expression();
		Consume(TokenType.RightParenthesis, "Expect ')' after expression.");
		return new Expression.Grouping(expr);
	}

	private bool MatchAny(params TokenType[] types)
	{
		if (!types.Any(Check))
			return false;
		Advance();
		return true;
	}

	// ReSharper disable once FlagArgument
	private Token Consume(TokenType type, string message)
	{
		if (Check(type)) return Advance();
		throw Error(Peek(), message);
	}

	private ParseError Error(Token token, string message)
	{
		errorHandler.Error(token, message);
		return new ParseError();
	}

	private void Synchronize()
	{
		Advance();
		while (!IsAtEnd())
		{
			if (Previous().type == TokenType.Semicolon) return;
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

	private bool Check(TokenType tokenType)
	{
		if (IsAtEnd()) return false;
		return Peek().type == tokenType;
	}

	private Token Advance()
	{
		if (!IsAtEnd()) current++;
		return Previous();
	}

	private bool IsAtEnd() => Peek().type == TokenType.Eof;

	private Token Peek() => tokens[current];

	private Token Previous() => tokens[current - 1];

	private class ParseError : Exception { }
}