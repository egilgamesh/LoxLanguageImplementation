namespace CraftInterpreter.Lox;

public class Program
{
	private static bool hadError;

	private static void Main(string[] args)
	{
		if (args.Length > 1)
			Console.WriteLine("Usage: Lox [script]");
		else if (args.Length == 1)
			RunFile(args[0]);
		else
			RunPrompt();
	}

	private static void RunPrompt()
	{
		for (;;)
		{
			Console.Write("> ");
			var line = Console.ReadLine();
			if (line == null)
				return;
			RunScanner(line);
			hadError = false;
		}
	}

	private static void RunFile(string path)
	{
		var text = File.ReadAllText(path);
		RunScanner(text);
		if (hadError)
			Environment.Exit(65);
	}

	private static void RunScanner(string source)
	{
		var scanner = new Scanner(source);
		var tokens = scanner.Tokens();
		foreach (var token in tokens)
			Console.WriteLine(token);
	}

	public static void Error(int line, string message) => Report(line, "", message);
	
	private static void Report(int line, string where, string message)
	{
		Console.Error.WriteLine("[line " + line + "] Error" + where + ": " + message);
		hadError = true;
	}
}