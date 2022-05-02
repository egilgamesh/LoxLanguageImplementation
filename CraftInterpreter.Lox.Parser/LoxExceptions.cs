namespace CraftInterpreter.Lox.Parser;

public abstract class LoxExceptions : Exception
{
	protected LoxExceptions() : base()
	{

	}

	public void Error(Token token, string message) { }
}