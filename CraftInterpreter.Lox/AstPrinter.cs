//using System.Text;

//namespace CraftInterpreter.Lox;

//public class AstPrinter : Expression.Visitor<string>
//{
//	private Expression.Visitor<string> visitorImplementation;

//	public string VisitBinaryExpr(Expression.Binary expression) =>
//		Parenthesize(expression.@operator.lexeme, expression.left, expression.right);

//	public string? Visit(Expression.Binary expression) => null;

//	public string Visit(Expression.Grouping expression) =>
//		Parenthesize("group", expression.expression);

//	public string Visit(Expression.Literal expression) =>
//		(expression.value == null
//			? "nil"
//			: expression.value.ToString())!;

//	public string Visit(Expression.Unary expression) =>
//		Parenthesize(expression.@operator.lexeme, expression.right);

//	public string Print(Expression expression) => expression.Accept(this);

//	private string Parenthesize(string name, params Expression[] expressionsParameters)
//	{
//		var builder = new StringBuilder();
//		builder.Append("(").Append(name);
//		foreach (var expr in expressionsParameters)
//		{
//			builder.Append(" ");
//			builder.Append(expr.Accept(this));
//		}
//		builder.Append(")");
//		return builder.ToString();
//	}
	
//	public string Visit(Expression.Variable expression) => throw new NotImplementedException();

//	public string Visit(Expression.CallExpression expression) => throw new NotImplementedException();

//	public string Visit(Expression.Assign expression) => throw new NotImplementedException();

//	public string Visit(Expression.Logical expression) => throw new NotImplementedException();
//	public string Visit(Expression.ThisExpression expression) => null;
//}