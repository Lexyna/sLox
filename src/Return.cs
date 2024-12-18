public class Return : Exception
{
  public readonly Object value;

  public Return(Object value)
  {
    this.value = value;
  }
}