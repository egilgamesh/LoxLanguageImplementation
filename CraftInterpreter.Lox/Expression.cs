namespace CraftInterpreter.Lox;

public abstract class Expression
{
	public abstract T Accept<T>(Visitor<T> visitor);

	public interface Visitor<T>
	{
		T Visit(Binary expression);
		T Visit(Grouping expression);
		T Visit(Literal expression);
		T Visit(Unary expression);
		T Visit(Variable expression);
		T Visit(CallExpression expression);
		T Visit(Assign expression);
		T Visit(Logical expression);
		T Visit(ThisExpression expression);
		T Visit(Conditional expression);
	}

	public class Binary : Expression
	{
		public readonly Expression left;
		public readonly Expression right;
		public Token @operator;

		public Binary(Expression left, Token @operator, Expression right)
		{
			this.left = left;
			this.@operator = @operator;
			this.right = right;
		}

		public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);

		public override string ToString() => left + " " + @operator.lexeme + " " + right;
	}

	public class Grouping : Expression
	{
		public readonly Expression expression;

		public Grouping(Expression expression) => this.expression = expression;

		public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
	}

	public class Literal : Expression
	{
		public readonly object value;

		public Literal(object value) => this.value = value;

		public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);

		public override string ToString() =>
			value is Token tokenValue
				? tokenValue.lexeme
				: value.ToString()!;
	}

	public class Unary : Expression
	{
		public readonly Expression right;
		public Token @operator;

		public Unary(Token @operator, Expression right)
		{
			this.@operator = @operator;
			this.right = right;
		}

		public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
	}

	public class Variable : Expression
	{
		public Token name;

		public Variable(Token name) => this.name = name;

		public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
	}

	public class CallExpression : Expression
	{
		public List<Expression> arguments;
		public Expression callExpression;
		public Token paren;

		public CallExpression(Expression callExpression, Token paren, List<Expression> arguments)
		{
			this.callExpression = callExpression;
			this.paren = paren;
			this.arguments = arguments;
		}

		public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
	}

	public class Assign : Expression
	{
		public Token name;
		public Expression value;

		public Assign(Token name, Expression value)
		{
			this.name = name;
			this.value = value;
		}

		public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
	}

	public class Logical : Expression
	{
		public Expression left;
		public Token operatorSign;
		public Expression right;

		public Logical(Expression left, Token operatorSign, Expression right)
		{
			this.left = left;
			this.operatorSign = operatorSign;
			this.right = right;
		}

		public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
	}

	public class ThisExpression : Expression
	{
		public Token Keyword;

		public ThisExpression(Token keyword) => Keyword = keyword;

		public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
	}
	public class Conditional : Expression
	{
		public Expression expression;
		public Expression thenBranch;
		public Expression elseBranch;
             
		public Conditional(Expression expression, Expression thenBranch, Expression elseBranch)
		{
			this.expression = expression;
			this.thenBranch = thenBranch;
			this.elseBranch = elseBranch;
		}
             
		public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
	}
}