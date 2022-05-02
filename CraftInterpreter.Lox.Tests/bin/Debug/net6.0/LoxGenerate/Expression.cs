namespace CraftInterpreter.Lox;

public abstract class Expression
{
	public abstract T Accept<T>(Visitor<T> visitor);

	public interface Visitor<T>
	{
		T VisitBinaryExpr(Binary expression);
		T VisitGroupingExpr(Grouping expression);
		T VisitLiteralExpr(Literal expression);
		T VisitUnaryExpr(Unary expression);
	}

	public class Binary : Expression
	{
		public readonly Expression left;
		public Token @operator;
		public readonly Expression right;

		public Binary(Expression left, Token @operator, Expression right)
		{
			this.left = left;
			this.@operator = @operator;
			this.right = right;
		}

		public override T Accept<T>(Visitor<T> visitor) => visitor.VisitBinaryExpr(this);
	}

	public class Grouping : Expression
	{
		public readonly Expression expression;

		public Grouping(Expression expression) => this.expression = expression;

		public override T Accept<T>(Visitor<T> visitor) => visitor.VisitGroupingExpr(this);
	}

	public class Literal : Expression
	{
		public readonly object value;

		public Literal(object value) => this.value = value;

		public override T Accept<T>(Visitor<T> visitor) => visitor.VisitLiteralExpr(this);
	}

	public class Unary : Expression
	{
		public Token @operator;
		public readonly Expression right;

		public Unary(Token @operator, Expression right)
		{
			this.@operator = @operator;
			this.right = right;
		}

		public override T Accept<T>(Visitor<T> visitor) => visitor.VisitUnaryExpr(this);
	}
}