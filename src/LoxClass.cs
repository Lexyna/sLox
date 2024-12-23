
public class LoxClass : LoxCallable
{
  public readonly string name;
  private readonly Dictionary<string, LoxFunction> methods;

  public LoxClass(string name, Dictionary<string, LoxFunction> methods)
  {
    this.name = name;
    this.methods = methods;
  }

  public LoxFunction FindMethod(string name)
  {
    if (methods.ContainsKey(name))
      return methods[name];
    return null;
  }

  public int Arity()
  {
    LoxFunction initializer = FindMethod("init");
    if (initializer == null) return 0;
    return initializer.Arity();
  }

  public object Call(Interpreter interpreter, List<object> arguments)
  {
    LoxInstance instance = new LoxInstance(this);
    LoxFunction initializer = FindMethod("init");
    if (initializer != null)
      initializer.Bind(instance).Call(interpreter, arguments);
    return instance;
  }

  public override string ToString()
  {
    return name;
  }
}