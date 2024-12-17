using System;

namespace AST{

public abstract class Expr{
public interface Visitor<R> {
R visitBinaryExpr(Binary expr);
R visitGroupingExpr(Grouping expr);
R visitLiteralExpr(Literal expr);
R visitUnaryExpr(Unary expr);
R visitVariableExpr(Variable expr);
}
public class Binary: Expr{
public Binary( Expr left, Token op, Expr right ){
this.left = left;
this.op = op;
this.right = right;
}

public override R Accept<R>(Visitor<R> visitor){
return visitor.visitBinaryExpr(this);
}

public readonly Expr left;
public readonly Token op;
public readonly Expr right;
}
public class Grouping: Expr{
public Grouping( Expr expression ){
this.expression = expression;
}

public override R Accept<R>(Visitor<R> visitor){
return visitor.visitGroupingExpr(this);
}

public readonly Expr expression;
}
public class Literal: Expr{
public Literal( Object value ){
this.value = value;
}

public override R Accept<R>(Visitor<R> visitor){
return visitor.visitLiteralExpr(this);
}

public readonly Object value;
}
public class Unary: Expr{
public Unary( Token op, Expr right ){
this.op = op;
this.right = right;
}

public override R Accept<R>(Visitor<R> visitor){
return visitor.visitUnaryExpr(this);
}

public readonly Token op;
public readonly Expr right;
}
public class Variable: Expr{
public Variable( Token name ){
this.name = name;
}

public override R Accept<R>(Visitor<R> visitor){
return visitor.visitVariableExpr(this);
}

public readonly Token name;
}

public abstract R Accept<R>(Visitor<R> visitor);
}
}
