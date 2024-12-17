using System;

namespace AST{

public abstract class Stmt{
public interface Visitor<R> {
R visitExpressionStmt(Expression stmt);
R visitPrintStmt(Print stmt);
R visitVarStmt(Var stmt);
}
public class Expression: Stmt{
public Expression( Expr expression ){
this.expression = expression;
}

public override R Accept<R>(Visitor<R> visitor){
return visitor.visitExpressionStmt(this);
}

public readonly Expr expression;
}
public class Print: Stmt{
public Print( Expr expression ){
this.expression = expression;
}

public override R Accept<R>(Visitor<R> visitor){
return visitor.visitPrintStmt(this);
}

public readonly Expr expression;
}
public class Var: Stmt{
public Var( Token name ){
this.name = name;
}

public override R Accept<R>(Visitor<R> visitor){
return visitor.visitVarStmt(this);
}

public readonly Token name;
}

public abstract R Accept<R>(Visitor<R> visitor);
}
}
