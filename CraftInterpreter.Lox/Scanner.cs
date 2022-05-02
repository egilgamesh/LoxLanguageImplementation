namespace CraftInterpreter.Lox;

public class Scanner
{
	private static readonly Dictionary<string, TokenType> Keywords = new()
	{
		{ "and", TokenType.And },
		{ "class", TokenType.Class },
		{ "else", TokenType.Else },
		{ "false", TokenType.False },
		{ "for", TokenType.For },
		{ "fun", TokenType.Fun },
		{ "if", TokenType.If },
		{ "nil", TokenType.Nil },
		{ "or", TokenType.Or },
		{ "print", TokenType.Print },
		{ "return", TokenType.Return },
		{ "super", TokenType.SuperClass },
		{ "this", TokenType.This },
		{ "true", TokenType.True },
		{ "var", TokenType.Var },
		{ "while", TokenType.While }
	};
	private readonly string source;
	private readonly List<Token> tokens = new();
	private int current;
	private int line = 1;
	private int start;

	public Scanner(string source) => this.source = source;

	public List<Token> ScanTokens()
	{
		while (!IsAtEnd())
		{
			// We are at the beginning of the next lexeme.
			start = current;
			ScanToken();
		}
		tokens.Add(new Token(TokenType.Eof, "", null!, line));
		return tokens;
	}

	private bool IsAtEnd() => current >= source.Length;

	// ReSharper disable once MethodTooLong
	public void ScanToken()
	{
		var c = Advance();
		switch (c)
		{
		case '(':
			AddToken(TokenType.LeftParenthesis);
			break;
		case ')':
			AddToken(TokenType.RightParenthesis);
			break;
		case '{':
			AddToken(TokenType.LeftBrace);
			break;
		case '}':
			AddToken(TokenType.RightBrace);
			break;
		case ',':
			AddToken(TokenType.Comma);
			break;
		case '.':
			AddToken(TokenType.Dot);
			break;
		case '-':
			AddToken(TokenType.Minus);
			break;
		case '+':
			AddToken(TokenType.Plus);
			break;
		case ';':
			AddToken(TokenType.Semicolon);
			break;
		case '*':
			AddToken(TokenType.Star);
			break;
		case '!':
			AddToken(Match('=')
				? TokenType.Equal
				: TokenType.Bang);
			break;
		case '=':
			AddToken(Match('=')
				? TokenType.EqualEqual
				: TokenType.Equal);
			break;
		case '<':
			AddToken(Match('=')
				? TokenType.LessEqual
				: TokenType.Less);
			break;
		case '>':
			AddToken(Match('=')
				? TokenType.GreaterEqual
				: TokenType.Greater);
			break;
		case '/':
			if (Match('/'))
				// A comment goes until the end of the line.
				while (Peek() != '\n' && !IsAtEnd())
					Advance();
			else
				AddToken(TokenType.Slash);
			break;
		case ' ':
		case '\r':
		case '\t':
			// Ignore whitespace.
			break;
		case '\n':
			line++;
			break;
		case '"':
			StringToken();
			break;
		default:
			if (IsDigit(c))
				Number();
			else if (IsAlpha(c))
				Identifier();
			else
			{
				throw new UnexpectedCharacter();
				Program.Error(line, "Unexpected character.");
			}
			break;
		}
	}

	private bool IsDigit(char c) => c >= '0' && c <= '9';

	private bool IsAlpha(char c) => c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' || c == '_';

	private bool IsAlphaNumeric(char c) => IsAlpha(c) || IsDigit(c);

	private void Identifier()
	{
		while (IsAlphaNumeric(Peek())) Advance();

		// See if the identifier is a reserved word.
		var text = source.Substring(start, current - start);
		TokenType type;
		if (!Keywords.TryGetValue(text, out type))
			type = TokenType.Identifier;
		AddToken(type);
	}

	private void Number()
	{
		while (IsDigit(Peek())) Advance();

		// Look for a fractional part.
		if (Peek() == '.' && IsDigit(PeekNext()))
		{
			// Consume the "."
			Advance();
			while (IsDigit(Peek())) Advance();
		}
		AddToken(TokenType.Number, double.Parse(source.Substring(start, current - start)));
	}

	private char PeekNext()
	{
		if (current + 1 >= source.Length) return '\0';
		return source[current + 1];
	}

	private char Peek()
	{
		if (IsAtEnd()) return '\0';
		return source[current];
	}

	private void StringToken()
	{
		while (Peek() != '"' && !IsAtEnd())
		{
			if (Peek() == '\n') line++;
			Advance();
		}

		// Unterminated string.
		if (IsAtEnd())
		{
			Program.Error(line, "Unterminated string.");
			return;
		}

		// The closing ".
		Advance();

		// Trim the surrounding quotes.
		var value = source.Substring(start + 1, current - 1 - (start + 1));
		AddToken(TokenType.String, value);
	}

	private bool Match(char expected)
	{
		if (IsAtEnd()) return false;
		if (source[current] != expected) return false;
		current++;
		return true;
	}

	private char Advance()
	{
		current++;
		return source[current - 1];
	}

	private void AddToken(TokenType type) => AddToken(type, null!);

	private void AddToken(TokenType type, object literal)
	{
		var text = source.Substring(start, current - start);
		tokens.Add(new Token(type, text, literal, line));
	}
}

public class UnexpectedCharacter : Exception { }