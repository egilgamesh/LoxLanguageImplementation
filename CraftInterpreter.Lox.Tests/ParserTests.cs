using CraftInterpreter.Lox.Parser;
using NUnit.Framework;

namespace CraftInterpreter.Lox.Tests;

public class ParserTests
{
	[Test]
	public void CheckVarDeclarationIsValid()
	{
		var errorHandler = new ParserErrorHandler();
		var statementList =
			new Parser.Parser(new Scanner("var x=10;").Tokens(), errorHandler).Parse();
		Assert.That(statementList.Count, Is.EqualTo(1));
		var statement = statementList[0];
		Assert.That(statement.GetType(), Is.EqualTo(typeof(Statement.Var)));
	}

	[Test]
	public void PrintStatementCanBeScanned()
	{
		const string ExamplePrintStatement = "print \"hello World\";";
		var tokens = new Scanner(ExamplePrintStatement).Tokens();
		var errors = new ParserErrorHandler();
		var statements = new Parser.Parser(tokens, errors).Parse();
		Assert.That(statements[0].GetType(), Is.EqualTo(typeof(Statement.Print)));
		var printStatement = (Statement.Print)statements[0];
		Assert.That(printStatement.expression.GetType(), Is.EqualTo(typeof(Expression.Literal)));
		Assert.That(printStatement.expression.ToString(), Is.EqualTo("hello World"));
	}

	[Test]
	public void IfStatementCanBeScanned()
	{
		var tokens = new Scanner(ScannerTests.ExampleIfStatement).Tokens();
		var errors = new ParserErrorHandler();
		var statements = new Parser.Parser(tokens, errors).Parse();
		Assert.That(statements[0].GetType(), Is.EqualTo(typeof(Statement.IfStatement)));
		var ifStatement = (Statement.IfStatement)statements[0];
		Assert.That(ifStatement.condition.GetType(), Is.EqualTo(typeof(Expression.Binary)));
		Assert.That(ifStatement.condition.ToString(), Is.EqualTo("5 == 5"));
		Assert.That(ifStatement.thenBranch.ToString(), Is.EqualTo("print hello Doxtream"));
		Assert.That(ifStatement.elseBranch, Is.Null);
	}

	[Test]
	public void ForStatementCanBeScanned()
	{
		const string ForExample = "for (var i = 1; i < 5; i = i + 1) { \n  print i * i; \n }";
		var tokens = new Scanner(ForExample).Tokens();
		var errors = new ParserErrorHandler();
		var statements = new Parser.Parser(tokens, errors).Parse();
		Assert.That(statements[0].GetType(), Is.EqualTo(typeof(Statement.Block)));
		var printStatement = (Statement.Block)statements[0];
		Assert.That(printStatement.statements[0].GetType(), Is.EqualTo(typeof(Statement.Var)));
	}
}