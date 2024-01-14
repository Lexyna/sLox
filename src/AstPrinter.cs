using System.Text;
using AST;

public class AstPrinter : Expr.Visitor<String>
{

  public string Print(Expr expr)
  {
    return expr.Accept(this);
  }

  public string VisitBinaryExpr(Expr.Binary expr)
  {
    return Parenthesize(expr.op.lexeme, expr.left, expr.right);
  }

  public string VisitGroupingExpr(Expr.Grouping expr)
  {
    return Parenthesize("group", expr.expression);
  }

  public string VisitLiteralExpr(Expr.Literal expr)
  {
    if (expr.value == null) return "nil";
    return expr.value.ToString();
  }

  public string VisitUnaryExpr(Expr.Unary expr)
  {
    return Parenthesize(expr.op.lexeme, expr.right);
  }

  private string Parenthesize(string name, params Expr[] exprs)
  {
    var builder = new StringBuilder();

    builder.Append("(").Append(name);
    foreach (var expr in exprs)
    {
      builder.Append(" ");
      builder.Append(expr.Accept(this));
    }
    builder.Append(")");

    return builder.ToString();
  }

}