namespace CraftInterpreter.Lox;

public class RuntimeError : LoxExceptions
{
	public Token token { get; protected set; }

	public RuntimeError(Token token, string message) : base(message) => this.token = token;
}