namespace CraftInterpreter.Lox.Parser;

public class ParserErrorHandler : ErrorHandler
{
	public void Error(int line, string message) { }
	public void Error(Token token, string message) { }
	public void RuntimeError(RuntimeError error) { }
}