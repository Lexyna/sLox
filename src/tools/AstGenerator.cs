namespace tools
{

  public class GenerateAST
  {

    public static void Generate(string outputDir)
    {
      /*DefineAST(outputDir, "Expr", new List<string>(){
        "Binary : Expr left, Token op, Expr right",
        "Grouping : Expr expression",
        "Literal : Object value",
        "Unary : Token op, Expr right"
      });*/
      DefineAST(outputDir, "Stmt", new List<string>()
      {
        "Expression : Expr expression",
        "Print : Expr expression"
      });
    }

    private static void DefineAST(string outputDir, string baseName, List<string> types)
    {
      string path = $"{outputDir}/{baseName}.cs";
      if (!Directory.Exists(outputDir))
        Directory.CreateDirectory(outputDir);

      TextWriter writer = new StreamWriter(path);

      writer.WriteLine("using System;");
      writer.WriteLine();
      writer.WriteLine("namespace AST{");
      writer.WriteLine();
      writer.WriteLine($"public abstract class {baseName}{{");

      DefineVisitor(writer, baseName, types);

      //AST classes
      foreach (string type in types)
      {
        string classsName = type.Split(":")[0].Trim();
        string fields = type.Split(":")[1].Trim();
        DefineType(writer, baseName, classsName, fields);
      }

      writer.WriteLine();
      writer.WriteLine($"public abstract R Accept<R>(Visitor<R> visitor);");

      writer.WriteLine("}");
      writer.WriteLine("}");
      writer.Flush();
      writer.Close();
    }

    public static void DefineType(TextWriter writer, string baseName, string className, string fieldList)
    {
      writer.WriteLine($"public class {className}: {baseName}{{");

      //constructor
      writer.WriteLine($"public {className}( {fieldList} ){{");

      string[] fields = fieldList.Split(", ");
      foreach (string field in fields)
      {
        string name = field.Split(" ")[1];
        writer.WriteLine($"this.{name} = {name};");
      }

      writer.WriteLine("}");

      //Visitor
      writer.WriteLine();
      writer.WriteLine($"public override R Accept<R>(Visitor<R> visitor){{");
      writer.WriteLine($"return visitor.visit{className}{baseName}<R>(this);");
      writer.WriteLine("}");

      //fields
      writer.WriteLine();
      foreach (string field in fields)
      {
        writer.WriteLine($"public readonly {field};");
      }

      writer.WriteLine("}");
    }

    private static void DefineVisitor(TextWriter writer, string baseName, List<string> types)
    {
      writer.WriteLine($"public interface Visitor<R> {{");

      foreach (string type in types)
      {
        string typeName = type.Split(":")[0].Trim();
        writer.Write("R visit");
        writer.Write($"{typeName}");
        writer.Write($"{baseName}<R>");
        writer.Write($"({typeName}");
        writer.Write($" {baseName.ToLower()});");
        writer.WriteLine();
        //writer.WriteLine($"R visit<R>{typeName}{baseName}({typeName} {baseName.ToLower()})");
      }
      writer.WriteLine("}");
    }
  }
}