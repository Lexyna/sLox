using System;

namespace AST
{

  public abstract class Stmt
  {
    public interface Visitor<R>
    {
      R visitExpressionStmt<R>(Expression stmt);
      R visitPrintStmt<R>(Print stmt);
    }
    public class Expression : Stmt
    {
      public Expression(Expr expression)
      {
        this.expression = expression;
      }

      public override R Accept<R>(Visitor<R> visitor)
      {
        return visitor.visitExpressionStmt<R>(this);
      }

      public readonly Expr expression;
    }
    public class Print : Stmt
    {
      public Print(Expr expression)
      {
        this.expression = expression;
      }

      public override R Accept<R>(Visitor<R> visitor)
      {
        return visitor.visitPrintStmt<R>(this);
      }

      public readonly Expr expression;
    }

    public abstract R Accept<R>(Visitor<R> visitor);
  }
}
