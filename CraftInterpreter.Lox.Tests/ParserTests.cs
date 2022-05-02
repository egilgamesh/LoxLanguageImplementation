using CraftInterpreter.Lox.Parser;
using NUnit.Framework;

namespace CraftInterpreter.Lox.Tests;

public class ParserTests
{

	[Test]
	public void CheckVarDeclarationIsValid()
	{
		ErrorHandler errorHandler = new ParserErrorHandler();
		var statementList = new parser(new Scanner("var x=10;").ScanTokens(), errorHandler).Parse();
		Assert.That(statementList.Count, Is.EqualTo(1));
		var statement = statementList[0];
		Assert.That(statement.GetType(), Is.EqualTo(typeof(Statement.Var)));
	}

	[Test]
	public void PrintStatementCanBeScanned()
	{
	string examplePrintStatement = "print \"hello World\";";
	var tokens = new Scanner(examplePrintStatement).ScanTokens();
		var errors = new ParserErrorHandler();
		var statements = new Parser.parser(tokens, errors).Parse();
		Assert.That(statements[0].GetType(), Is.EqualTo(typeof(Statement.Print)));
		var printStatement = (Statement.Print)statements[0];
		Assert.That(printStatement.expression.GetType(), Is.EqualTo(typeof(Expression.Literal)));
		Assert.That(printStatement.expression.ToString(), Is.EqualTo("hello World"));
	}

	[Test]
	public void IfStatementCanBeScanned()
	{
		var tokens = new Scanner(ScannerTests.ExampleIfStatement).ScanTokens();
		var errors = new ParserErrorHandler();
		var statements = new parser(tokens, errors).Parse();
		Assert.That(statements[0].GetType(), Is.EqualTo(typeof(Statement.IfStatement)));
		var ifStatement = (Statement.IfStatement)statements[0];
		Assert.That(ifStatement.condition.GetType(), Is.EqualTo(typeof(Expression.Binary)));
		Assert.That(ifStatement.condition.ToString(), Is.EqualTo("5 == 5"));
		Assert.That(ifStatement.thenBranch.ToString(), Is.EqualTo("print \"5 is actually 5\";"));
		Assert.That(ifStatement.elseBranch, Is.Null);
	}
	[Test]
	public void forStatementCanBeScanned()
	{
		const string forExample = "for (var i = 1; i < 5; i = i + 1) { \n  print i * i; \n }";
		var tokens = new Scanner(forExample).ScanTokens();
		var errors = new ParserErrorHandler();
		var statements = new parser(tokens, errors).Parse();
		Assert.That(statements[0].GetType(), Is.EqualTo(typeof(Statement.Print)));
		var printStatement = (Statement.Print)statements[0];
		Assert.That(printStatement.expression.GetType(), Is.EqualTo(typeof(Expression.Literal)));
	}
}