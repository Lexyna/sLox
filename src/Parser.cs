using System.Data;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.ConstrainedExecution;
using AST;

public class Parser
{

  private class ParseError : Exception
  {

  }

  List<Token> tokens;
  int current = 0;
  int loopDepth = 0;

  public Parser(List<Token> tokens)
  {
    this.tokens = tokens;
  }

  public List<Stmt> Parse()
  {
    List<Stmt> statments = new List<Stmt>();
    while (!IsAtEnd())
    {
      statments.Add(Declaration());
    }
    return statments;
  }

  private Stmt Declaration()
  {
    try
    {
      if (Match(TokenType.FUN)) return Function("function");
      if (Match(TokenType.VAR)) return VarDeclaration();

      return Statement();
    }
    catch (ParseError error)
    {
      Syncronize();
      return null;
    }
  }

  private Stmt Statement()
  {
    if (Match(TokenType.BREAK)) return BreakStatement();
    if (Match(TokenType.FOR)) return ForStatement();
    if (Match(TokenType.IF)) return IfStatement();
    if (Match(TokenType.PRINT)) return PrintStatement();
    if (Match(TokenType.RETURN)) return ReturnStatement();
    if (Match(TokenType.WHILE)) return WhileStatement();
    if (Match(TokenType.LEFT_BRACE)) return new Stmt.Block(Block());
    return ExpressionStatement();
  }

  private Stmt BreakStatement()
  {
    if (loopDepth == 0)
      Error(Previous(), "Must be inside a loop to use 'break'.");

    Consume(TokenType.SEMICOLON, "Expected ';' after 'break'.");
    return new Stmt.Break();
  }

  private Stmt ForStatement()
  {
    Consume(TokenType.LEFT_PAREM, "Expected '(' after 'for'.");

    Stmt initializer;
    if (Match(TokenType.SEMICOLON))
      initializer = null;
    else if (Match(TokenType.VAR))
      initializer = VarDeclaration();
    else
      initializer = ExpressionStatement();

    Expr condition = null;
    if (!Check(TokenType.SEMICOLON))
      condition = Expression();
    Consume(TokenType.SEMICOLON, "Expected ';' after loop condition.");

    Expr increment = null;
    if (!Check(TokenType.RIGHT_PAREN))
      increment = Expression();

    Consume(TokenType.RIGHT_PAREN, "Expected ')' after clause.");

    try
    {
      loopDepth++;
      Stmt body = Statement();

      if (increment != null)
      {
        body = new Stmt.Block(new List<Stmt>(){
        body,
        new Stmt.Expression(increment)
      });
      }

      if (condition == null) condition = new Expr.Literal(true);
      body = new Stmt.While(condition, body);

      if (initializer != null)
        body = new Stmt.Block(new List<Stmt>() { initializer, body });

      return body;
    }
    finally
    {
      loopDepth--;
    }
  }

  private Stmt IfStatement()
  {
    Consume(TokenType.LEFT_PAREM, "Expected '()' after 'if'.");
    Expr condition = Expression();
    Consume(TokenType.RIGHT_PAREN, "Expected '(' after if condition.");

    Stmt thenBranch = Statement();
    Stmt elseBranch = null;
    if (Match(TokenType.ELSE))
      elseBranch = Statement();

    return new Stmt.If(condition, thenBranch, elseBranch);
  }

  private Stmt PrintStatement()
  {
    Expr value = Expression();
    Consume(TokenType.SEMICOLON, "Expected ';' after value.");
    return new Stmt.Print(value);
  }

  private Stmt ReturnStatement()
  {
    Token keyword = Previous();
    Expr value = null;
    if (!Check(TokenType.SEMICOLON))
      value = Expression();

    Consume(TokenType.SEMICOLON, "Expected ';' after return value.");
    return new Stmt.Return(keyword, value);
  }

  private Stmt ExpressionStatement()
  {
    Expr expr = Expression();
    Consume(TokenType.SEMICOLON, "Expected ';' after expression.");
    return new Stmt.Expression(expr);
  }

  private Stmt.Function Function(string kind)
  {
    Token name = Consume(TokenType.IDENTIFIER, $"Expected {kind} name.");

    Consume(TokenType.LEFT_PAREM, $"Expected ')' after {kind} name.");
    List<Token> parameters = new List<Token>();
    if (!Check(TokenType.RIGHT_PAREN))
    {
      do
      {
        if (parameters.Count >= 255) Error(Peek(), "Can't have more than 255 parameters.");
        parameters.Add(Consume(TokenType.IDENTIFIER, "Expected parameter name."));
      } while (Match(TokenType.COMMA));
    }

    Consume(TokenType.RIGHT_PAREN, "Expected ')' after parameters.");

    Consume(TokenType.LEFT_BRACE, "Expected '{' before" + kind + " body.");
    List<Stmt> body = Block();
    return new Stmt.Function(name, parameters, body);
  }

  private List<Stmt> Block()
  {
    List<Stmt> statements = new List<Stmt>();

    while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
    {
      statements.Add(Declaration());
    }

    Consume(TokenType.RIGHT_BRACE, "Expected, '}' after block.");
    return statements;
  }

  private Expr Assignment()
  {
    Expr expr = Or();

    if (Match(TokenType.EQ))
    {
      Token equals = Previous();
      Expr value = Assignment();

      if (expr is Expr.Variable)
      {
        Token name = ((Expr.Variable)expr).name;
        return new Expr.Assign(name, value);
      }

      Error(equals, "Invalid assignment target.");
    }
    return expr;
  }

  private Expr Or()
  {
    Expr expr = And();

    while (Match(TokenType.OR))
    {
      Token op = Previous();
      Expr right = And();
      expr = new Expr.Logical(expr, op, right);
    }

    return expr;
  }

  private Expr And()
  {
    Expr expr = Equality();

    while (Match(TokenType.AND))
    {
      Token op = Previous();
      Expr right = Equality();
      expr = new Expr.Logical(expr, op, right);
    }

    return expr;
  }

  private Expr Expression()
  {
    return Assignment();
  }

  private Stmt VarDeclaration()
  {
    Token name = Consume(TokenType.IDENTIFIER, "Expected variable name");

    Expr initializer = null;
    if (Match(TokenType.EQ))
      initializer = Expression();

    Consume(TokenType.SEMICOLON, "Expected `;` after variable declaration");
    return new Stmt.Var(name, initializer);
  }

  private Stmt WhileStatement()
  {
    Consume(TokenType.LEFT_PAREM, "Expected '(' after 'while'.");
    Expr condition = Expression();
    Consume(TokenType.RIGHT_PAREN, "Expected ')' after condition.");

    try
    {
      loopDepth++;
      Stmt body = Statement();
      return new Stmt.While(condition, body);
    }
    finally
    {
      loopDepth--;
    }
  }

  private Expr Equality()
  {
    Expr expr = Comparison();

    while (Match(TokenType.BANG_EQ, TokenType.EQ_EQ))
    {
      Token op = Previous();
      Expr right = Comparison();
      expr = new Expr.Binary(expr, op, right);
    }

    return expr;
  }

  private Expr Comparison()
  {
    Expr expr = Term();

    while (Match(TokenType.GREATER, TokenType.GREATER_EQ, TokenType.LESS, TokenType.LESS_EQ))
    {
      Token op = Previous();
      Expr right = Term();
      expr = new Expr.Binary(expr, op, right);
    }

    return expr;
  }

  private Expr Term()
  {
    Expr expr = Factor();

    while (Match(TokenType.MINUS, TokenType.PLUS))
    {
      Token op = Previous();
      Expr right = Factor();
      expr = new Expr.Binary(expr, op, right);
    }

    return expr;
  }

  private Expr Factor()
  {
    Expr expr = Unary();

    while (Match(TokenType.SLASH, TokenType.STAR))
    {
      Token op = Previous();
      Expr right = Unary();
      expr = new Expr.Binary(expr, op, right);
    }

    return expr;
  }

  private Expr Unary()
  {
    if (Match(TokenType.BANG, TokenType.MINUS))
    {
      Token op = Previous();
      Expr right = Unary();
      return new Expr.Unary(op, right);
    }

    return Call();
  }

  private Expr FinishCall(Expr callee)
  {
    List<Expr> arguments = new List<Expr>();
    if (!Check(TokenType.RIGHT_PAREN))
    {
      do
      {
        if (arguments.Count >= 255)
          Error(Peek(), "Can't have more than 255 arguments.");
        arguments.Add(Expression());
      } while (Match(TokenType.COMMA));
    }

    Token paren = Consume(TokenType.RIGHT_PAREN, "Expected ')' after arguments.");
    return new Expr.Call(callee, paren, arguments);
  }

  private Expr Call()
  {
    Expr expr = Primary();

    while (true)
    {
      if (Match(TokenType.LEFT_PAREM))
        expr = FinishCall(expr);
      else
        break;
    }
    return expr;
  }

  private Expr Primary()
  {
    if (Match(TokenType.FALSE)) return new Expr.Literal(false);
    if (Match(TokenType.TRUE)) return new Expr.Literal(true);
    if (Match(TokenType.NIL)) return new Expr.Literal(null);

    if (Match(TokenType.NUMBER, TokenType.STRING))
    {
      return new Expr.Literal(Previous().literal);
    }

    if (Match(TokenType.IDENTIFIER))
    {
      return new Expr.Variable(Previous());
    }

    if (Match(TokenType.LEFT_PAREM))
    {
      Expr expr = Expression();
      Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
      return new Expr.Grouping(expr);
    }

    throw Error(Peek(), "Expect expression.");
  }

  private void Syncronize()
  {
    Advance();

    while (!IsAtEnd())
    {
      if (Previous().type == TokenType.SEMICOLON) return;

      switch (Peek().type)
      {
        case TokenType.CLASS:
        case TokenType.FUN:
        case TokenType.VAR:
        case TokenType.FOR:
        case TokenType.IF:
        case TokenType.WHILE:
        case TokenType.PRINT:
        case TokenType.RETURN:
          return;
      }

      Advance();
    }
  }

  private Token Consume(TokenType type, string message)
  {
    if (Check(type)) return Advance();

    throw Error(Peek(), message);
  }

  private bool Match(params TokenType[] types)
  {
    foreach (TokenType type in types)
    {
      if (Check(type))
      {
        Advance();
        return true;
      }
    }

    return false;
  }

  private bool Check(TokenType type)
  {
    if (IsAtEnd()) return false;
    return Peek().type == type;
  }

  private Token Advance()
  {
    if (!IsAtEnd()) current++;
    return Previous();
  }

  private bool IsAtEnd()
  {
    return Peek().type == TokenType.EOF;
  }

  private Token Peek()
  {
    return tokens[current];
  }

  private Token Previous()
  {
    return tokens[current - 1];
  }

  private ParseError Error(Token token, string message)
  {
    Lox.Error(token, message);
    return new ParseError();
  }

}