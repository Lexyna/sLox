
using AST;

public class LoxFunction : LoxCallable
{
  private readonly string name;
  private readonly Expr.Function declaration;
  private readonly Environment closure;
  private readonly bool isInitializer;

  public LoxFunction(Expr.Function declaration, Environment environment, bool isInitializer = false)
  {
    this.declaration = declaration;
    this.closure = environment;
    this.isInitializer = isInitializer;
  }

  public LoxFunction(string name, Expr.Function declaration, Environment closure, bool isInitializer = false)
  {
    this.name = name;
    this.declaration = declaration;
    this.closure = closure;
    this.isInitializer = isInitializer;
  }

  public LoxFunction Bind(LoxInstance instance)
  {
    Environment environment = new Environment(closure);
    environment.Define("this", instance);
    return new LoxFunction(declaration, environment, isInitializer);
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
      if (isInitializer) return closure.GetAt(0, "this");
      return returnValue.value;
    }

    if (isInitializer) return closure.GetAt(0, "this");

    return null;
  }

  public override string ToString()
  {
    if (String.IsNullOrEmpty(name)) return "<fn>";
    return $"<fn {name}>";
  }
}