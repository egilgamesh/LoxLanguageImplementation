namespace CraftInterpreter.Lox;

public class LoxExceptions : Exception
{
	private string message;

	protected LoxExceptions(string message) => this.message = message;
}