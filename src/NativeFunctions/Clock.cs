
public class Clock : LoxCallable
{
  public int Arity()
  {
    return 0;
  }

  public object Call(Interpreter interpreter, List<object> arguments)
  {
    return (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) / 1000.0;
  }

  public override string ToString()
  {
    return "<native fn>";
  }
}