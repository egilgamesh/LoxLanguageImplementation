namespace CraftInterpreter.Lox;

public abstract class Statement
{
	public abstract T Accept<T>(Visitor<T> visitor);

	public interface Visitor<T>
	{
		T Visit(Block blockStatement);
		T Visit(Class classStatement);
		T Visit(ExpressionStatement expressionStatement);
		T Visit(Function functionStatement);
		T Visit(IfStatement ifStatementStatement);
		T Visit(Print printStatement);
		T Visit(Return returnStatement);
		T Visit(Var varStatement);
		T Visit(While whileStatement);
		T Visit(Break breakStatement);
	}

	public class Block : Statement
	{
		public List<Statement> statements;

		public Block(List<Statement> statements) => this.statements = statements;

		public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
	}

	public class Class : Statement
	{
		public List<Function> methods;
		public Token name;
		public Expression superClass;

		public Class(Token name, Expression superClass, List<Function> methods)
		{
			this.name = name;
			this.superClass = superClass;
			this.methods = methods;
		}

		public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
	}

	public class ExpressionStatement : Statement
	{
		public Expression expression;

		public ExpressionStatement(Expression expression) => this.expression = expression;

		public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
	}

	public class Function : Statement
	{
		public List<Statement> body;
		public Token name;
		public List<Token> parameters;

		public Function(Token name, List<Token> parameters, List<Statement> body)
		{
			this.name = name;
			this.parameters = parameters;
			this.body = body;
		}

		public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
	}

	public class IfStatement : Statement
	{
		public readonly Expression condition;
		public readonly Statement elseBranch;
		public readonly Statement thenBranch;

		public IfStatement(Expression condition, Statement thenBranch, Statement elseBranch)
		{
			this.condition = condition;
			this.thenBranch = thenBranch;
			this.elseBranch = elseBranch;
		}

		public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
	}

	public class Print : Statement
	{
		public readonly Expression expression;

		public Print(Expression expression) => this.expression = expression;

		public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);

		public override string ToString() => "print "+expression;
	}

	public class Return : Statement
	{
		public Token keyword;
		public Expression value;

		public Return(Token keyword, Expression value)
		{
			this.keyword = keyword;
			this.value = value;
		}

		public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
	}

	public class Var : Statement
	{
		public Expression initializer;
		public Token name;

		public Var(Token name, Expression initializer)
		{
			this.name = name;
			this.initializer = initializer;
		}

		public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
	}

	public class While : Statement
	{
		public Statement body;
		public Expression condition;

		public While(Expression condition, Statement body)
		{
			this.condition = condition;
			this.body = body;
		}

		public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
	}

	public class Break : Statement
	{
		public override T Accept<T>(Visitor<T> visitor) => visitor.Visit(this);
	}
}