using System.IO;
using CraftInterpreter.Lox.Parser;
using NUnit.Framework;

namespace CraftInterpreter.Lox.Tests;

public class LoxTests
{
	[Test]
	public void TestDoxtRunPrompt()
	{
		const string DoxtSyntaxSource = "print \"Hello, world!\";";
		Assert.That(new Scanner(DoxtSyntaxSource).Tokens().Count, Is.EqualTo(4));
		Assert.That(new Scanner(DoxtSyntaxSource).Tokens().ToArray()[^1].ToString().Trim(),
			Is.EqualTo(TokenType.Eof.ToString()));
	}

	[Test]
	public void TestDoxtRunFile()
	{
		var path = "HelloWorld.doxtream";
		var loxSource = File.ReadAllText(path);
		Assert.That(new Scanner(loxSource).Tokens().Count, Is.EqualTo(4));
	}

	[Test]
	public void GenerateValidTokens()
	{
		var tokens = new Scanner("var LoxVersion = 0.1;").Tokens();
		Assert.That(() => tokens.Count, Is.EqualTo(6));
		Assert.That(() => tokens[0].lexeme, Is.EqualTo("var"));
		Assert.That(() => tokens[1].lexeme, Is.EqualTo("LoxVersion"));
		Assert.That(() => tokens[2].lexeme, Is.EqualTo("="));
		Assert.That(() => tokens[3].lexeme, Is.EqualTo("0.1"));
		Assert.That(() => tokens[4].lexeme, Is.EqualTo(";"));
	}

	[Test]
	public void InvalidCharacterShouldThrowException() =>
		Assert.Throws<UnexpectedCharacter>(() => new Scanner("var #@x=2;").Tokens());

	[Test]
	public void ValidateTokenTypes()
	{
		var tokens = new Scanner("var LoxVersion = 0.1;").Tokens();
		Assert.That(() => tokens[0].type, Is.EqualTo(TokenType.Var));
		Assert.That(() => tokens[1].type, Is.EqualTo(TokenType.Identifier));
		Assert.That(() => tokens[2].type, Is.EqualTo(TokenType.Equal));
		Assert.That(() => tokens[3].type, Is.EqualTo(TokenType.Number));
		Assert.That(() => tokens[4].type, Is.EqualTo(TokenType.Semicolon));
	}

	[Test]
	public void CheckVarDeclarationIsValid()
	{
		ErrorHandler errorHandler = new ParserErrorHandler();
		var statementList = new parser(new Scanner("var x=10;").Tokens(), errorHandler).Parse();
		Assert.That(statementList.Count, Is.EqualTo(1));
		var statement = statementList[0];
		Assert.That(statement.GetType(), Is.EqualTo(typeof(Statement.Var)));
	}

	[Test]
	public void TestGenerateAst()
	{
		var path = Directory.GetCurrentDirectory();
			//"C:\\Users\\LENOVO\\source\\repos\\CraftInterpreter.Lox\\CraftInterpreter.Lox\\LoxGenerate";
		var className = "ExpressionGenerated";
		var generateAbstractSyntaxTree = new GenerateAbstractSyntaxTree(path, className);
		//Assert.That(typeof(Expression), Is.EqualTo(typeof(ExpressionGenerated)));
	}

	//[Test]
	//public void TestPrintAst()
	//{
	//	var unary = new Expression.Unary(new Token(TokenType.Minus, "-", null, 1), new Expression.Literal(123));
	//	Expression exp = new Expression.Binary(unary, new Token( TokenType.Star,"*", null!,1), new Expression.Grouping( new Expression.Literal(45.67)));
	//	Assert.That(new AstPrinter().Print(exp), Is.EqualTo("(* (- 123) (group 45.67))"));
	//}
}