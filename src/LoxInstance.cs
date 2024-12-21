public class LoxInstance
{

  private LoxClass klass;
  private readonly Dictionary<string, Object> fields = new Dictionary<string, object>();

  public LoxInstance(LoxClass klass)
  {
    this.klass = klass;
  }

  public Object Get(Token name)
  {
    if (fields.ContainsKey(name.lexeme))
      return fields[name.lexeme];

    LoxFunction method = klass.FindMethod(name.lexeme);
    if (method != null) return method.Bind(this);

    throw new RuntimeError(name, $"Undefined property '{name.lexeme}'.");
  }

  public void Set(Token name, Object value)
  {
    fields[name.lexeme] = value;
  }

  public override string ToString()
  {
    return $"{klass.name} instance";
  }

}