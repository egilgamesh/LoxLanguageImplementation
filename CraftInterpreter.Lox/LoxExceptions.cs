namespace CraftInterpreter.Lox;

public class LoxExceptions : Exception
{
	private string message;

	public LoxExceptions(string message) => this.message = message;
}