using System;
using System.Data;
using System.Text;
using System.Runtime.InteropServices;
using AST;
using tools;

public class Lox
{

  private static bool hadError = false;
  private static bool hadRuntimeError = false;
  private static Interpreter interpreter = new Interpreter();

  public static void Main(string[] args)
  {
    //GenerateAST.Generate("AST");
    //return;

    if (args.Length > 1)
    {
      Console.WriteLine("Usage: sLox [script]");
      System.Environment.Exit(64);
    }
    else if (args.Length == 1)
    {
      RunFile(args[0]);
    }
    else
    {
      RunPrompt();
    }
  }

  public static void RunFile(string path)
  {
    byte[] bytes = File.ReadAllBytes(Path.GetFullPath(path));
    Run(Encoding.UTF8.GetString(bytes));
    if (hadError) System.Environment.Exit(65);
    if (hadRuntimeError) System.Environment.Exit(70);
  }

  public static void RunPrompt()
  {
    TextReader input = Console.In;

    for (; ; )
    {
      Console.Write("> ");
      string? line = input.ReadLine();
      if (String.IsNullOrEmpty(line)) break;
      Run(line, true);
      if (hadError) hadError = false;
    }
  }

  private static void Run(string source, bool repl = false)
  {
    Scanner scanner = new Scanner(source);
    List<Token> tokens = scanner.ScanTokens();

    Parser parser = new Parser(tokens);
    List<Stmt> statements = parser.Parse();

    if (hadError) return;

    interpreter.Interpret(statements, repl);
    //Console.WriteLine(new AstPrinter().Print(statements[0]));
  }

  public static void Error(int line, string message)
  {
    Report(line, "", message);
  }

  private static void Report(int line, string where, string message)
  {
    Console.WriteLine($"Error at line {line} {where}: {message}");
    hadError = true;
  }

  public static void Error(Token token, string message)
  {
    if (token.type == TokenType.EOF) Report(token.line, " at end", message);
    else Report(token.line, $" at '{token.lexeme}'", message);
  }

  public static void RuntimeError(RuntimeError ex)
  {
    Console.Error.WriteLine($"{ex.Message}\n[line {ex.token.line}]");
    hadRuntimeError = true;
  }

}
