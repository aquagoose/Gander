namespace Gander.Parser;

public class Instruction
{
    public OpCode OpCode;
    public Types Type;
    public object Object;

    public Instruction(OpCode opCode, Types type = Types.Void, object o = null)
    {
        OpCode = opCode;
        Type = type;
        Object = o;
    }

    public override string ToString()
    {
        return $"Opcode: {OpCode}, Type: {Type}, Object: {Object}";
    }
}