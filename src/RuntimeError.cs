using System.Data.Common;

public class RuntimeError : Exception
{
  public readonly Token token;

  public RuntimeError(Token token, String msg) : base(msg)
  {
    this.token = token;
  }


}