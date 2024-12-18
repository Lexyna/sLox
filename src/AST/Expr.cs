using System;

namespace AST
{

  public abstract class Expr
  {
    public interface Visitor<R>
    {
      R VisitFunctionExpr(Function expr);
      R VisitAssignExpr(Assign expr);
      R VisitBinaryExpr(Binary expr);
      R VisitCallExpr(Call expr);
      R VisitGroupingExpr(Grouping expr);
      R VisitLiteralExpr(Literal expr);
      R VisitLogicalExpr(Logical expr);
      R VisitUnaryExpr(Unary expr);
      R VisitVariableExpr(Variable expr);
    }
    public class Function : Expr
    {
      public Function(List<Token> parameters, List<Stmt> body)
      {
        this.parameters = parameters;
        this.body = body;
      }

      public override R Accept<R>(Visitor<R> visitor)
      {
        return visitor.VisitFunctionExpr(this);
      }

      public readonly List<Token> parameters;
      public readonly List<Stmt> body;
    }
    public class Assign : Expr
    {
      public Assign(Token name, Expr value)
      {
        this.name = name;
        this.value = value;
      }

      public override R Accept<R>(Visitor<R> visitor)
      {
        return visitor.VisitAssignExpr(this);
      }

      public readonly Token name;
      public readonly Expr value;
    }
    public class Binary : Expr
    {
      public Binary(Expr left, Token op, Expr right)
      {
        this.left = left;
        this.op = op;
        this.right = right;
      }

      public override R Accept<R>(Visitor<R> visitor)
      {
        return visitor.VisitBinaryExpr(this);
      }

      public readonly Expr left;
      public readonly Token op;
      public readonly Expr right;
    }
    public class Call : Expr
    {
      public Call(Expr calle, Token paren, List<Expr> arguments)
      {
        this.calle = calle;
        this.paren = paren;
        this.arguments = arguments;
      }

      public override R Accept<R>(Visitor<R> visitor)
      {
        return visitor.VisitCallExpr(this);
      }

      public readonly Expr calle;
      public readonly Token paren;
      public readonly List<Expr> arguments;
    }
    public class Grouping : Expr
    {
      public Grouping(Expr expression)
      {
        this.expression = expression;
      }

      public override R Accept<R>(Visitor<R> visitor)
      {
        return visitor.VisitGroupingExpr(this);
      }

      public readonly Expr expression;
    }
    public class Literal : Expr
    {
      public Literal(Object value)
      {
        this.value = value;
      }

      public override R Accept<R>(Visitor<R> visitor)
      {
        return visitor.VisitLiteralExpr(this);
      }

      public readonly Object value;
    }
    public class Logical : Expr
    {
      public Logical(Expr left, Token op, Expr right)
      {
        this.left = left;
        this.op = op;
        this.right = right;
      }

      public override R Accept<R>(Visitor<R> visitor)
      {
        return visitor.VisitLogicalExpr(this);
      }

      public readonly Expr left;
      public readonly Token op;
      public readonly Expr right;
    }
    public class Unary : Expr
    {
      public Unary(Token op, Expr right)
      {
        this.op = op;
        this.right = right;
      }

      public override R Accept<R>(Visitor<R> visitor)
      {
        return visitor.VisitUnaryExpr(this);
      }

      public readonly Token op;
      public readonly Expr right;
    }
    public class Variable : Expr
    {
      public Variable(Token name)
      {
        this.name = name;
      }

      public override R Accept<R>(Visitor<R> visitor)
      {
        return visitor.VisitVariableExpr(this);
      }

      public readonly Token name;
    }

    public abstract R Accept<R>(Visitor<R> visitor);
  }
}
