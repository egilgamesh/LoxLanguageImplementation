namespace CraftInterpreter.Lox;

public class RuntimeError : LoxExceptions
{
	public Token Token { get; protected set; }

	public RuntimeError(Token token, string message) : base(message) => Token = token;
}