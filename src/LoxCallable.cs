interface LoxCallable
{
  int Arity();
  Object Call(Interpreter interpreter, List<Object> arguments);
}