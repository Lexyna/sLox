using System.Security.Cryptography.X509Certificates;
using AST;

public class Resolver : Expr.Visitor<Object>, Stmt.Visitor<Object>
{
  internal class Variable
  {
    public readonly Token name;
    internal VariableState state;

    public Variable(Token name, VariableState state)
    {
      this.name = name;
      this.state = state;
    }

    public enum VariableState
    {
      DECLARED,
      DEFINED,
      READ
    }
  }

  private readonly Interpreter interpreter;
  private readonly Stack<Dictionary<string, Variable>> scopes = new Stack<Dictionary<string, Variable>>();
  private FunctionType currentFunction = FunctionType.NONE;

  enum FunctionType
  {
    NONE,
    FUNCTION,
    LAMBDA_FUNCTION
  }

  public Resolver(Interpreter interpreter)
  {
    this.interpreter = interpreter;
  }

  public void Resolve(List<Stmt> stmts)
  {
    foreach (Stmt stmt in stmts)
      Resolve(stmt);
  }

  public Object VisitBlockStmt(Stmt.Block stmt)
  {
    BeginScope();
    Resolve(stmt.statements);
    EndScope();
    return null;
  }

  public Object VisitExpressionStmt(Stmt.Expression stmt)
  {
    Resolve(stmt.expression);
    return null;
  }

  public Object VisitFunctionStmt(Stmt.Function stmt)
  {
    Declare(stmt.name);
    Define(stmt.name);

    ResolveFunction(stmt, FunctionType.FUNCTION);
    return null;
  }

  public Object VisitFunctionExpr(Expr.Function expr)
  {
    Token lambda = new Token(TokenType.IDENTIFIER, Guid.NewGuid().ToString(), null, 0);
    Declare(lambda);
    Define(lambda);

    Stmt.Function fun = new Stmt.Function(lambda, expr);
    ResolveFunction(fun, FunctionType.LAMBDA_FUNCTION);

    return null;
  }

  public Object VisitIfStmt(Stmt.If stmt)
  {
    Resolve(stmt.condition);
    Resolve(stmt.thenBranch);
    if (stmt.elseBranch != null) Resolve(stmt.elseBranch);
    return null;
  }

  public Object VisitPrintStmt(Stmt.Print stmt)
  {
    Resolve(stmt.expression);
    return null;
  }

  public Object VisitReturnStmt(Stmt.Return stmt)
  {
    if (currentFunction == FunctionType.NONE)
      Lox.Error(stmt.keyword, "Can't return from top-level code.");
    if (stmt.value != null) Resolve(stmt.value);
    return null;
  }

  public Object VisitBreakStmt(Stmt.Break stmt)
  {
    return null;
  }

  public Object VisitVarStmt(Stmt.Var stmt)
  {
    Declare(stmt.name);
    if (stmt.initializer != null)
      Resolve(stmt.initializer);
    Define(stmt.name);
    return null;
  }

  public Object VisitWhileStmt(Stmt.While stmt)
  {
    Resolve(stmt.condition);
    Resolve(stmt.body);
    return null;
  }

  public Object VisitAssignExpr(Expr.Assign expr)
  {
    Resolve(expr.value);
    ResolveLocal(expr, expr.name, false);
    return null;
  }

  public Object VisitBinaryExpr(Expr.Binary expr)
  {
    Resolve(expr.left);
    Resolve(expr.right);
    return null;
  }

  public Object VisitCallExpr(Expr.Call expr)
  {
    Resolve(expr.calle);
    foreach (Expr argument in expr.arguments)
      Resolve(argument);
    return null;
  }

  public Object VisitGroupingExpr(Expr.Grouping expr)
  {
    Resolve(expr.expression);
    return null;
  }

  public Object VisitLiteralExpr(Expr.Literal expr)
  {
    return null;
  }

  public Object VisitLogicalExpr(Expr.Logical expr)
  {
    Resolve(expr.left);
    Resolve(expr.right);
    return null;
  }

  public Object VisitUnaryExpr(Expr.Unary expr)
  {
    Resolve(expr.right);
    return null;
  }

  public Object VisitVariableExpr(Expr.Variable expr)
  {
    if (scopes.Count > 0 &&
    scopes.Peek()[expr.name.lexeme].state == Variable.VariableState.DECLARED)
      Lox.Error(expr.name, "Can't read local variable in its own initializer.");

    ResolveLocal(expr, expr.name, true);
    return null;
  }

  public void Resolve(Stmt stmt)
  {
    stmt.Accept(this);
  }

  public void Resolve(Expr expr)
  {
    expr.Accept(this);
  }

  private void ResolveFunction(Stmt.Function stmt, FunctionType type)
  {
    FunctionType enclosingFunction = currentFunction;
    currentFunction = type;

    BeginScope();
    foreach (Token param in stmt.function.parameters)
    {
      Declare(param);
      Define(param);
    }
    Resolve(stmt.function.body);
    EndScope();
    currentFunction = enclosingFunction;
  }

  public void BeginScope()
  {
    scopes.Push(new Dictionary<string, Variable>());
  }

  private void EndScope()
  {
    Dictionary<string, Variable> scope = scopes.Pop();
    foreach (KeyValuePair<string, Variable> pair in scope)
    {
      if (pair.Value.state == Variable.VariableState.DEFINED)
        Lox.Error(pair.Value.name, "Local Variable is never used.");
    }
  }

  private void Declare(Token name)
  {
    if (scopes.Count == 0) return;
    Dictionary<string, Variable> scope = scopes.Peek();
    if (scope.ContainsKey(name.lexeme))
      Lox.Error(name, "Variable with same name already in scope.");
    scope[name.lexeme] = new Variable(name, Variable.VariableState.DECLARED);
  }

  private void Define(Token name)
  {
    if (scopes.Count == 0) return;
    scopes.Peek()[name.lexeme] = new Variable(name, Variable.VariableState.DEFINED);
  }

  public void ResolveLocal(Expr expr, Token name, bool isRead)
  {
    for (int i = scopes.Count - 1; i >= 0; i--)
    {
      if (scopes.ElementAt(i).ContainsKey(name.lexeme))
      {
        interpreter.Resolve(expr, scopes.Count - 1 - i);

        if (isRead)
          scopes.ElementAt(i)[name.lexeme].state = Variable.VariableState.READ;

        return;
      }
    }
  }
}