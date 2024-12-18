
using AST;

public class LoxFunction : LoxCallable
{
  private readonly string name;
  private readonly Expr.Function declaration;
  private readonly Environment closure;

  public LoxFunction(string name, Expr.Function declaration, Environment closure)
  {
    this.name = name;
    this.declaration = declaration;
    this.closure = closure;
  }

  public int Arity()
  {
    return declaration.parameters.Count;
  }

  public object Call(Interpreter interpreter, List<object> arguments)
  {
    Environment environment = new Environment(closure);
    for (int i = 0; i < declaration.parameters.Count; i++)
      environment.Define(declaration.parameters[i].lexeme, arguments[i]);

    try
    {
      interpreter.ExecuteBlock(declaration.body, environment);
    }
    catch (Return returnValue)
    {
      return returnValue.value;
    }

    return null;
  }

  public override string ToString()
  {
    if (String.IsNullOrEmpty(name)) return "<fn>";
    return $"<fn {name}>";
  }
}