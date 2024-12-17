public class Environment
{
  private Environment enclosing;

  private readonly Dictionary<String, Object> values = new Dictionary<String, Object>();

  public Environment()
  {
    enclosing = null;
  }

  public Environment(Environment enclosing)
  {
    this.enclosing = enclosing;
  }

  public Object Get(Token name)
  {
    if (values.ContainsKey(name.lexeme))
      return values[name.lexeme];

    if (enclosing != null)
      return enclosing.Get(name);

    throw new RuntimeError(name, $"Undefined variable + '{name.lexeme}'.");
  }

  public void Assign(Token name, Object value)
  {
    if (values.ContainsKey(name.lexeme))
    {
      values[name.lexeme] = value;
      return;
    }

    if (enclosing != null)
    {
      enclosing.Assign(name, value);
      return;
    }

    throw new RuntimeError(name, $"undefined variable '{name.lexeme}'.");
  }

  public void Define(string name, Object value)

  {
    values[name] = value;
  }
}