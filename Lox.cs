using System;
using System.Runtime.InteropServices;
using System.Text;

public class Lox
{

  private static bool hadError = false;

  public static void Main(string[] args)
  {
    if (args.Length > 1)
    {
      Console.WriteLine("Usage: sLox [script]");
      Environment.Exit(64);
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
    if (hadError) Environment.Exit(65);
  }

  public static void RunPrompt()
  {
    TextReader input = Console.In;

    for (; ; )
    {
      Console.Write("> ");
      string? line = input.ReadLine();
      if (String.IsNullOrEmpty(line)) break;
      Run(line);
      if (hadError) Environment.Exit(65);
    }
  }

  private static void Run(string source)
  {
    Scanner scanner = new Scanner(source);
    List<Token> tokens = scanner.ScanTokens();

    foreach (Token token in tokens)
    {
      Console.WriteLine(token);
    }
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

}