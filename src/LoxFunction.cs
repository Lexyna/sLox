
using AST;

public class LoxFunction : LoxCallable
{
  private readonly Stmt.Function declaration;

  public LoxFunction(Stmt.Function declaration)
  {
    this.declaration = declaration;
  }

  public int Arity()
  {
    return declaration.param.Count;
  }

  public object Call(Interpreter interpreter, List<object> arguments)
  {
    Environment environment = new Environment(interpreter.globals);
    for (int i = 0; i < declaration.param.Count; i++)
      environment.Define(declaration.param[i].lexeme, arguments[i]);

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
    return $"<fn {declaration.name.lexeme}>";
  }
}