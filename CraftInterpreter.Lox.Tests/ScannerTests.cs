using System.Linq;
using NUnit.Framework;

namespace CraftInterpreter.Lox.Tests;

public class ScannerTests
{
	[Test]
	public void IfStatementCanBeScanned()
	{
		var tokens = new Scanner(ExampleIfStatement).ScanTokens();
		Assert.That(tokens.Select(t => t.type).ToArray(), Is.EqualTo(new[]
		{
			TokenType.If,
			TokenType.LeftParenthesis,
			TokenType.Number,
			TokenType.EqualEqual,
			TokenType.Number,
			TokenType.RightParenthesis,
			TokenType.Print,
			TokenType.String,
			TokenType.Semicolon,
			TokenType.Eof
		}));
	}

	public const string ExampleIfStatement = "if(5 == 5)\n\tprint \"he\";";
}