using System.Runtime.CompilerServices;
using AST;
public class Interpreter : Expr.Visitor<Object>, Stmt.Visitor<Object>
{
  private class BreakException : Exception { }

  public readonly Environment globals = new Environment();
  private readonly Dictionary<Expr, int> locals = new Dictionary<Expr, int>();
  private Environment environment;

  public Interpreter()
  {
    globals.Define("clock", new Clock());

    environment = globals;
  }

  private Object uninitialized = new Object();

  public void Interpret(List<Stmt> statments, bool repl = false)
  {
    try
    {
      if (repl && statments.Count == 1 && statments[0] is Stmt.Expression)
      {
        Object value = Evaluate(((Stmt.Expression)statments[0]).expression);
        Console.WriteLine(value);
        return;
      }

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

  public Object VisitLogicalExpr(Expr.Logical expr)
  {
    Object left = Evaluate(expr.left);

    if (expr.op.type == TokenType.OR)
    {
      if (IsTruthy(left)) return left;
    }
    else
    {
      if (!IsTruthy(left)) return left;
    }

    return Evaluate(expr.right);
  }

  public Object VisitSetExpr(Expr.Set expr)
  {
    Object obj = Evaluate(expr.obj);

    if (!(obj is LoxInstance))
      throw new RuntimeError(expr.name, "Only instances can have fields.");

    Object value = Evaluate(expr.value);
    ((LoxInstance)obj).Set(expr.name, value);
    return value;
  }

  public Object VisitThisExpr(Expr.This expr)
  {
    return LookupVariable(expr.keyword, expr);
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
    Object value = LookupVariable(expr.name, expr);
    if (value == uninitialized)
      throw new RuntimeError(expr.name, $"Variable must first be initialized");
    return value;
  }

  private Object LookupVariable(Token name, Expr expr)
  {
    if (locals.ContainsKey(expr))
    {
      int distance = locals[expr];
      return environment.GetAt(distance, name.lexeme);
    }
    return globals.Get(name);
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

  public Object VisitCallExpr(Expr.Call expr)
  {
    Object callee = Evaluate(expr.calle);

    List<Object> arguments = new List<Object>();
    foreach (Expr argument in expr.arguments)
      arguments.Add(Evaluate(argument));

    if (!(callee is LoxCallable))
      throw new RuntimeError(expr.paren, "Can only call functions and classes.");

    LoxCallable function = callee as LoxCallable;
    if (arguments.Count != function.Arity())
      throw new RuntimeError(expr.paren, $"Expected {function.Arity()} arguments but got {arguments.Count}.");


    return function.Call(this, arguments);
  }

  public Object VisitGetExpr(Expr.Get expr)
  {
    Object obj = Evaluate(expr.obj);
    if (obj is LoxInstance)
      return ((LoxInstance)obj).Get(expr.name);
    throw new RuntimeError(expr.name, "Only instances have properties.");
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

  public void Resolve(Expr expr, int depth)
  {
    locals[expr] = depth;
  }

  public void ExecuteBlock(List<Stmt> statements, Environment environment)
  {
    Environment previous = this.environment;
    try
    {
      this.environment = environment;

      foreach (Stmt statement in statements)
        Execute(statement);
    }
    finally
    {
      this.environment = previous;
    }
  }

  public Object VisitBlockStmt(Stmt.Block stmt)
  {
    ExecuteBlock(stmt.statements, new Environment(environment));
    return null;
  }

  public Object VisitClassStmt(Stmt.Class stmt)
  {
    environment.Define(stmt.name.lexeme, null);

    Dictionary<string, LoxFunction> methods = new Dictionary<string, LoxFunction>();
    foreach (Stmt.Function method in stmt.methods)
    {
      LoxFunction function = new LoxFunction(method.function, environment);
      methods.Add(method.name.lexeme, function);
    }

    LoxClass klass = new LoxClass(stmt.name.lexeme, methods);
    environment.Assign(stmt.name, klass);
    return null;
  }

  public Object VisitBreakStmt(Stmt.Break stmt)
  {
    throw new BreakException();
  }

  public Object VisitExpressionStmt(Stmt.Expression stmt)
  {
    Evaluate(stmt.expression);
    return typeof(void);
  }

  public Object VisitFunctionStmt(Stmt.Function stmt)
  {
    string fnName = stmt.name.lexeme;
    environment.Define(stmt.name.lexeme, new LoxFunction(fnName, stmt.function, environment));
    return null;
  }

  public Object VisitFunctionExpr(Expr.Function expr)
  {
    return new LoxFunction(null, expr, environment);
  }

  public Object VisitIfStmt(Stmt.If stmt)
  {
    if (IsTruthy(Evaluate(stmt.condition)))
      Execute(stmt.thenBranch);
    else if (stmt.elseBranch != null)
      Execute(stmt.elseBranch);
    return null;
  }

  public Object VisitPrintStmt(Stmt.Print stmt)
  {
    Object value = Evaluate(stmt.expression);
    Console.WriteLine(Stringify(value));
    return typeof(void);
  }

  public Object VisitReturnStmt(Stmt.Return stmt)
  {
    Object value = null;
    if (stmt.value != null) value = Evaluate(stmt.value);
    throw new Return(value);
  }

  public Object VisitVarStmt(Stmt.Var stmt)
  {
    Object value = uninitialized;
    if (stmt.initializer != null)
      value = Evaluate(stmt.initializer);
    environment.Define(stmt.name.lexeme, value);
    return null;
  }

  public Object VisitWhileStmt(Stmt.While stmt)
  {
    try
    {
      while (IsTruthy(Evaluate(stmt.condition)))
        Execute(stmt.body);
    }
    catch (BreakException ex) { }

    return null;
  }

  public Object VisitAssignExpr(Expr.Assign expr)
  {
    Object value = Evaluate(expr.value);

    if (locals.ContainsKey(expr))
      environment.AssignAt(locals[expr], expr.name, value);
    else
      globals.Assign(expr.name, value);
    return value;
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