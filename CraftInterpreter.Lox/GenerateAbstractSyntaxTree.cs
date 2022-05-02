namespace CraftInterpreter.Lox;

public class GenerateAbstractSyntaxTree
{
	public GenerateAbstractSyntaxTree(string outputDirectory, string className) =>
		DefineAbstractSyntaxTree(outputDirectory, className,
			new List<string>
			{
				"Binary   : Expression left, Token @operatorSign, Expression right",
				"Grouping : Expression expression",
				"Literal  : objectExpression objectExpression",
				"Unary    : Token @operatorSign, Expression right"
			});

	// ReSharper disable once MethodTooLong
	private static void DefineAbstractSyntaxTree(string outputDirectory, string baseName, List<string> types)
	{
		var path = outputDirectory + "/" + baseName + ".cs";
		using var writer = new StreamWriter(path);
		writer.WriteLine("using System.Collections.Generic;");
		writer.WriteLine("");
		writer.WriteLine("namespace CraftInterpreter.Lox.LoxGenerate");
		writer.WriteLine("{");
		writer.WriteLine("\tpublic abstract class " + baseName + " {");
		DefineVisitor(writer, baseName, types);

		// The AST classes.
		foreach (string type in types)
		{
			var className = type.Split(':')[0].Trim();
			var fields = type.Split(':')[1].Trim();
			DefineClassType(writer, baseName, className, fields);
		}

		// The base accept() method.
		writer.WriteLine("");
		writer.WriteLine("\t\tabstract public T Accept<T>(Visitor<T> visitor);");
		writer.WriteLine("\t}");
		writer.WriteLine("}");
	}

	private static void DefineVisitor(StreamWriter writer, string baseName, List<string> types)
	{
		writer.WriteLine("\t\tpublic interface Visitor<T> {");
		foreach (var type in types)
		{
			var typeName = type.Split(':')[0].Trim();
			writer.WriteLine("\t\t\tT Visit" + typeName + baseName + "(" + typeName + " " +
				baseName.ToLower() + ");");
		}
		writer.WriteLine("\t\t}");
	}

	// ReSharper disable once MethodTooLong
	private static void DefineClassType(StreamWriter writer, string baseName, string className,
		string fieldList)
	{
		writer.WriteLine("");
		writer.WriteLine("\t\tpublic class " + className + ": " + baseName + " {");
		writer.WriteLine("\t\t\tpublic " + className + "(" + fieldList + ") {");
		var fields = fieldList.Split(new[] { ", " }, StringSplitOptions.None);
		foreach (var field in fields)
		{
			var name = field.Split(' ')[1];
			writer.WriteLine("\t\t\t\tthis." + name + " = " + name + ";");
		}
		writer.WriteLine("\t\t\t}");

		// Visitor pattern.
		writer.WriteLine();
		writer.WriteLine("\t\t\tpublic override T Accept<T>(Visitor<T> visitor) {");
		writer.WriteLine("\t\t\t\treturn visitor.Visit" + className + baseName + "(this);");
		writer.WriteLine("\t\t\t}");

		// Fields.
		writer.WriteLine();
		foreach (var field in fields)
			writer.WriteLine("\t\t\tpublic " + field + ";");
		writer.WriteLine("\t\t}");
	}
}