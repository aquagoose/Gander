namespace Gander.Parser;

public class ClassInfo
{
    public string Name;
    public string Implements;

    public bool ValueType;
    public bool Instance;

    public int StartPoint;
    public int EndPoint;

    public FnInfo[] Functions;

    public ClassInfo()
    {
        Name = null;
        Implements = null;

        ValueType = false;
        Instance = false;

        StartPoint = -1;
        EndPoint = -1;
        Functions = null;
    }

    public override string ToString()
    {
        return "Class name: " + Name + ", Implements: " + Implements + ", ValueType? " +
               (ValueType ? "Yes" : "No") + ", Instance? " + (Instance ? "Yes" : "No") + ", Start point: " + StartPoint +
               ", End point: " + EndPoint;
    }

    public bool IsValid()
    {
        return !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Implements);
    }
}