using System.Runtime.CompilerServices;
using AST;
public class Interpreter : Expr.Visitor<Object>, Stmt.Visitor<Object>
{

  private Environment environment = new Environment();

  public void Interpret(List<Stmt> statments)
  {
    try
    {
      foreach (var statment in statments)
        Execute(statment);
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

  public Object VisitVariableExpr(Expr.Variable expr)
  {
    return environment.Get(expr.name);
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
          if (left.GetType() == typeof(String) && right.GetType() == typeof(Double))
            return (string)left + (double)right;
          if (left.GetType() == typeof(Double) && right.GetType() == typeof(String))
            return (double)left + (string)right;
          throw new RuntimeError(expr.op, "Opreands must both either be numbers or strings.");
        }
      case TokenType.SLASH:
        CheckNumberOperands(expr.op, left, right);
        if ((double)right == 0) throw new RuntimeError(expr.op, "Illegal division by 0 detected.");
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

  private void Execute(Stmt stmt)
  {
    stmt.Accept(this);
  }

  public Object VisitExpressionStmt(Stmt.Expression stmt)
  {
    Evaluate(stmt.expression);
    return typeof(void);
  }

  public Object VisitPrintStmt(Stmt.Print stmt)
  {
    Object value = Evaluate(stmt.expression);
    Console.WriteLine(Stringify(value));
    return typeof(void);
  }

  public Object VisitVarStmt(Stmt.Var stmt)
  {
    Object value = null;
    if (stmt.initializer != null)
      value = Evaluate(stmt.initializer);
    environment.Define(stmt.name.lexeme, value);
    return null;
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