namespace CraftInterpreter.Lox.Parser;

public interface ErrorHandler
{
	void Error(int line, string message);
	void Error(Token token, string message);
	void RuntimeError(RuntimeError error);
}