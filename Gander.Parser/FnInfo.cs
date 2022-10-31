namespace Gander.Parser;

public class FnInfo
{
    public string Name;
    public Types ReturnType;

    public bool Instance;
    public bool EntryPoint;
    public bool Constructor;
    public bool Private;

    public Types[] Arguments;
    public Instruction[] Instructions;

    public int StartPoint;
    public int EndPoint;

    public FnInfo()
    {
        Name = null;
        ReturnType = Types.Void;

        Instance = false;
        EntryPoint = false;
        Constructor = false;
        Private = false;

        Arguments = null;
        Instructions = null;

        StartPoint = -1;
        EndPoint = -1;
    }

    public override string ToString()
    {
        return
            $"Name: {Name}, Return type: {ReturnType}, Instance? {(Instance ? "Yes" : "No")}, Entry point? {(EntryPoint ? "Yes" : "No")}, Constructor? {(Constructor ? "Yes" : "No")}, Private? {(Private ? "Yes" : "No")}, Arguments: {(Arguments == null ? "None" : string.Join(", ", Arguments))}, Start point: {StartPoint}, End point: {EndPoint}";
    }
}