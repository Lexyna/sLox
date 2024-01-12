using System.ComponentModel;

public class Scanner
{

  private static readonly Dictionary<String, TokenType> keywords = new Dictionary<string, TokenType>(){
    {"and", TokenType.AND },
    {"or", TokenType.OR},
    {"if", TokenType.IF},
    {"else", TokenType.ELSE},
    {"class", TokenType.CLASS},
    {"for", TokenType.FOR},
    {"while", TokenType.WHILE},
    {"nil", TokenType.NIL},
    {"fun", TokenType.FUN},
    {"true", TokenType.TRUE},
    {"false", TokenType.FALSE},
    {"print", TokenType.PRINT},
    {"super", TokenType.SUPER},
    {"return", TokenType.RETURN},
    {"this", TokenType.THIS},
    {"var", TokenType.VAR}
  };

  private string source;
  private List<Token> tokens = new List<Token>();

  private int start = 0;
  private int current = 0;
  private int line = 1;

  public Scanner(string source)
  {
    this.source = source;
  }

  public List<Token> ScanTokens()
  {
    while (!IsAtEnd())
    {
      start = current;
      ScanToken();
    }

    tokens.Add(new Token(TokenType.EOF, "", null, line));
    return tokens;
  }

  private void ScanToken()
  {
    char c = Advance();

    switch (c)
    {
      case ')': AddToken(TokenType.RIGHT_PAREN); break;
      case '(': AddToken(TokenType.LEFT_PAREM); break;
      case '}': AddToken(TokenType.RIGHT_BRACE); break;
      case '{': AddToken(TokenType.LEFT_BRACE); break;
      case ',': AddToken(TokenType.COMMA); break;
      case '.': AddToken(TokenType.DOT); break;
      case '+': AddToken(TokenType.PLUS); break;
      case '-': AddToken(TokenType.MINUS); break;
      case '*': AddToken(TokenType.STAR); break;
      case ';': AddToken(TokenType.SEMICOLON); break;
      case '!': AddToken(Match('=') ? TokenType.BANG_EQ : TokenType.BANG); break;
      case '=': AddToken(Match('=') ? TokenType.EQ_EQ : TokenType.EQ); break;
      case '<': AddToken(Match('=') ? TokenType.LESS_EQ : TokenType.LESS); break;
      case '>': AddToken(Match('=') ? TokenType.GREATER_EQ : TokenType.GREATER); break;
      case '/':
        {
          if (Match('/'))
          {
            while (Peek() != '\n' && !IsAtEnd()) Advance();
          }
          else
          {
            AddToken(TokenType.SLASH);
          }
          break;
        }
      case ' ':
      case '\r':
      case '\t': break;
      case '\n': line++; break;
      case '"': ParseString(); break;
      default:
        {
          if (IsDigit(c))
          {
            ParseNumber();
          }
          else if (IsAlpha(c))
          {
            ParseIdentifier();
          }
          else
          {
            Lox.Error(line, "Unexpected character."); break;
          }
          break;
        }
    }
  }

  private void ParseIdentifier()
  {
    while (IsAlphaNumeric(Peek())) Advance();

    string identifier = source.Substring(start, current - start);
    if (keywords.ContainsKey(identifier))
    {
      TokenType type = keywords[identifier];
      AddToken(type);
      return;
    }

    AddToken(TokenType.IDENTIFIER);
  }

  private void ParseNumber()
  {
    while (IsDigit(Peek())) Advance();

    if (Peek() == '.' && IsDigit(PeekNext()))
    {
      Advance();
      while (IsDigit(Peek())) Advance();
    }

    AddToken(TokenType.NUMBER,
    Double.Parse(source.Substring(start, current - start)));
  }

  private void ParseString()
  {
    while (Peek() != '"' && !IsAtEnd())
    {
      if (Peek() == '\n') line++;
      Advance();
    }

    if (IsAtEnd())
    {
      Lox.Error(line, "Unterminated string.");
      return;
    }

    Advance();
    String value = source.Substring(start + 1, current - start - 2);
    AddToken(TokenType.STRING, value);
  }

  private void AddToken(TokenType type)
  {
    AddToken(type, null);
  }

  private void AddToken(TokenType type, Object literal)
  {
    string text = source.Substring(start, current - start);
    tokens.Add(new Token(type, text, literal, line));
  }

  private char Advance()
  {
    return source[current++];
  }

  private char Peek()
  {
    if (IsAtEnd()) return '\0';
    return source[current];
  }

  private char PeekNext()
  {
    if (current + 1 >= source.Length) return '\0';
    return source[current + 1];
  }

  private bool IsAlphaNumeric(char c)
  {
    return IsAlpha(c) || IsDigit(c);
  }

  private bool IsAlpha(char c)
  {
    return (c >= 'a' && c <= 'z') ||
            (c >= 'A' && c <= 'Z') ||
            (c == '_');
  }

  private bool IsDigit(char c)
  {
    return c >= '0' && c <= '9';
  }

  private bool Match(char expected)
  {
    if (IsAtEnd()) return false;
    if (source[current] != expected) return false;
    current++;
    return true;
  }

  private bool IsAtEnd()
  {
    return current >= source.Length;
  }

}