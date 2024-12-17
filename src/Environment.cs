public class Environment
{

  private readonly Dictionary<String, Object> values = new Dictionary<String, Object>();

  public Object Get(Token name)
  {
    if (values.ContainsKey(name.lexeme))
      return values[name.lexeme];

    throw new RuntimeError(name, $"Undefined variable + '{name.lexeme}'.");
  }

  public void Assign(Token name, Object value)
  {
    if (values.ContainsKey(name.lexeme))
    {
      values[name.lexeme] = value;
      return;
    }
    throw new RuntimeError(name, $"undefined variable '{name.lexeme}'.");
  }

  public void Define(string name, Object value)

  {
    values[name] = value;
  }
}