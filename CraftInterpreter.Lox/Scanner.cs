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
		var character = Advance();
		switch (character)
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
			if (IsDigit(character))
				Number();
			else if (IsAlpha(character))
				Identifier();
			else
			{
				Program.Error(line, "Unexpected character.");
				throw new UnexpectedCharacter();
			}
			break;
		}
	}

	private static bool IsDigit(char character) => character is >= '0' and <= '9';

	private static bool IsAlpha(char character) => character is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or '_';

	private static bool IsAlphaNumeric(char character) => IsAlpha(character) || IsDigit(character);

	private void Identifier()
	{
		while (IsAlphaNumeric(Peek())) Advance();

		// See if the identifier is a reserved word.
		var text = source.Substring(start, current - start);
		if (!Keywords.TryGetValue(text, out var type))
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

	private char PeekNext() =>
		current + 1 >= source.Length
			? '\0'
			: source[current + 1];

	private char Peek() =>
		IsAtEnd()
			? '\0'
			: source[current];

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