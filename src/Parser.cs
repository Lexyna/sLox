using AST;

public class Parser
{

  private class ParseError : Exception
  {

  }

  List<Token> tokens;
  int current = 0;

  public Parser(List<Token> tokens)
  {
    this.tokens = tokens;
  }

  public Expr Parse()
  {
    try
    {
      return Expression();
    }
    catch (ParseError error)
    {
      return null;
    }
  }

  private Expr Expression()
  {
    return Equality();
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

    return Primary();
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