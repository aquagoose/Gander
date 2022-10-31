namespace Gander.Parser;

public class VarInfo
{
    public string Name;
    public Types Type;

    public bool Private;
    public bool Instance;

    public VarInfo()
    {
        Name = null;
        Type = Types.Void;

        Private = false;
        Instance = false;
    }

    public override string ToString()
    {
        return
            $"Name: {Name}, Type: {Type}, Private? {(Private ? "Yes" : "No")}, Instance? {(Instance ? "Yes" : "No")}";
    }
}