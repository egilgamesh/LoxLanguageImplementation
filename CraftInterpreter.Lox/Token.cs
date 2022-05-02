using System.Diagnostics;

namespace CraftInterpreter.Lox;

[DebuggerDisplay("{type} Token: {line} {lexeme}")]
public struct Token
{
	public TokenType type;
	public string lexeme;
	public readonly object literal;
	public int line;

	public Token(TokenType type, string lexeme, object literal, int line)
	{
		this.type = type;
		this.lexeme = lexeme;
		this.literal = literal;
		this.line = line;
	}

	public override string ToString() =>
		literal != null
			? $"{type} {lexeme} {literal}"
			: $"{type} {lexeme}";
}