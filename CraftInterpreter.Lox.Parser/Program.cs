using System.Diagnostics;

namespace CraftInterpreter.Lox.Parser;

public class Program
{
	private static void Main(string[] args)
	{
		var unary = new Expression.Unary(new Token(TokenType.Minus, "-", null, 1), new Expression.Literal(123));
		Expression exp = new Expression.Binary(unary, new Token( TokenType.Star,"*", null!,1), new Expression.Grouping( new Expression.Literal(45.67)));
		//AstPrinter ast = new AstPrinter();
		//Console.WriteLine(ast.Print(exp));
		Console.ReadLine();
	}
}