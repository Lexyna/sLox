using AST;
public class Interpreter : Expr.Visitor<Object>
{

  public void interpret(Expr expression)
  {
    try
    {
      Object value = Evaluate(expression);
      Console.WriteLine(Stringify(value));
    }
    catch (RuntimeError error)
    {
      Lox.RuntimeError(error);
    }
  }

  public Object VisitLiteralExpr(Expr.Literal expr)
  {
    return expr.value;
  }

  public Object VisitGroupingExpr(Expr.Grouping expr)
  {
    return Evaluate(expr.expression);
  }

  public Object VisitUnaryExpr(Expr.Unary expr)
  {
    Object right = Evaluate(expr.right);

    switch (expr.op.type)
    {
      case TokenType.BANG: return !IsTruthy(right);
      case TokenType.MINUS: CheckNumberOperand(expr.op, right); return -(double)right;
    }
    return null;
  }

  public Object VisitBinaryExpr(Expr.Binary expr)
  {
    Object left = Evaluate(expr.left);
    Object right = Evaluate(expr.right);

    switch (expr.op.type)
    {
      case TokenType.GREATER:
        CheckNumberOperands(expr.op, left, right);
        return (double)left > (double)right;
      case TokenType.GREATER_EQ:
        CheckNumberOperands(expr.op, left, right);
        return (double)left >= (double)right;
      case TokenType.LESS:
        CheckNumberOperands(expr.op, left, right);
        return (double)left < (double)right;
      case TokenType.LESS_EQ:
        CheckNumberOperands(expr.op, left, right);
        return (double)left <= (double)right;
      case TokenType.BANG_EQ:
        return !IsEqual(left, right);
      case TokenType.EQ_EQ:
        return IsEqual(left, right);
      case TokenType.MINUS:
        CheckNumberOperands(expr.op, left, right);
        return (double)left - (double)right;
      case TokenType.PLUS:
        {
          if (left.GetType() == typeof(Double) && right.GetType() == typeof(Double))
            return (double)left + (double)right;
          if (left.GetType() == typeof(String) && right.GetType() == typeof(String))
            return (String)left + (String)right;
          throw new RuntimeError(expr.op, "Opreands must both either be numbers or strings.");
        }
      case TokenType.SLASH:
        CheckNumberOperands(expr.op, left, right);
        return (double)left / (double)right;
      case TokenType.STAR:
        CheckNumberOperands(expr.op, left, right);
        return (double)left * (double)right;
    }
    return null;
  }

  private bool IsEqual(Object a, Object b)
  {
    if (a == null && b == null) return true;
    if (a == null) return false;

    return a.Equals(b);
  }

  private Object Evaluate(Expr expr)
  {
    return expr.Accept(this);
  }

  private bool IsTruthy(Object obj)
  {
    if (obj == null) return false;
    if (obj.GetType() == typeof(Boolean)) return (bool)obj;
    return true;
  }

  private void CheckNumberOperand(Token op, Object operand)
  {
    if (operand.GetType() == typeof(Double)) return;
    throw new RuntimeError(op, "Operant must be a number.");
  }

  private void CheckNumberOperands(Token op, Object left, Object right)
  {
    if (left.GetType() == typeof(Double) && right.GetType() == typeof(Double)) return;
    throw new RuntimeError(op, "Operants must be numbers.");
  }

  private string Stringify(Object obj)
  {
    if (obj == null) return "nil";
    if (obj.GetType() == typeof(Double))
    {
      string text = ((double)obj).ToString();
      if (text.EndsWith(".0")) text = text.Substring(0, text.Length - 2);
      return text;
    }
    return obj.ToString();
  }

}