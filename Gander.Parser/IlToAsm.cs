using System;
using System.Collections.Generic;
using System.IO;

namespace Gander.Parser;

public class IlToAsm
{
    public IlToAsm()
    {
        
    }

    public byte[] Process(string code)
    {
        List<ClassInfo> classes = new List<ClassInfo>();
        List<FnInfo> functions = new List<FnInfo>();
        List<VarInfo> variables = new List<VarInfo>();

        ClassInfo currentClassInfo = new ClassInfo(); 
        bool inClass = false;
     
        FnInfo currentFnInfo = new FnInfo();
        bool inFn = false;

        VarInfo currentVarInfo = new VarInfo();
        
        string[] splitCode = code.Split('\n');
        
        // Preprocess class names and functions.
        Console.Write("Processing class & function structures... ");
        for (int i = 0; i < splitCode.Length; i++)
        {
            string line = splitCode[i].Trim();
            
            if (!line.StartsWith(':'))
                continue;

            string[] splitLine = line[1..].Split(' ');
            switch (splitLine[0])
            {
                case "class":
                    if (inClass)
                        throw Error("Nested classes are not supported.", i);

                    inClass = true;
                    currentClassInfo = new ClassInfo();
                    currentClassInfo.StartPoint = i;

                    for (int p = 1; p < splitLine.Length; p++)
                    {
                        switch (splitLine[p])
                        {
                            case "vtype":
                                currentClassInfo.ValueType = true;
                                break;
                            case "instance":
                                currentClassInfo.Instance = true;
                                break;
                            default:
                                if (splitLine[p].StartsWith("implements"))
                                {
                                    currentClassInfo.Implements =
                                        splitLine[p][("implements(".Length) .. (splitLine[p].Length - 1)];
                                }
                                else if (p >= splitLine.Length - 1)
                                    currentClassInfo.Name = splitLine[p];
                                else
                                    throw Error("Unrecognized attribute \"" + splitLine[p] + "\".", i);

                                break;
                        }
                    }
                    
                    if (!currentClassInfo.IsValid())
                        throw Error("Invalid class signature.", i);

                    break;

                case "clend":
                    currentClassInfo.EndPoint = i;
                    inClass = false;
                    currentClassInfo.Functions = functions.ToArray();
                    functions.Clear();
                    currentClassInfo.Variables = variables.ToArray();
                    variables.Clear();
                    classes.Add(currentClassInfo);
                    break;
                
                case "fn":
                    if (inFn)
                        throw Error("Cannot nest functions.", i);

                    inFn = true;
                    currentFnInfo = new FnInfo();
                    currentFnInfo.StartPoint = i;

                    for (int p = 1; p < splitLine.Length; p++)
                    {
                        switch (splitLine[p])
                        {
                            case "entrypoint":
                                currentFnInfo.EntryPoint = true;
                                break;
                            case "constructor":
                                currentFnInfo.Constructor = true;
                                break;
                            case "private":
                                currentFnInfo.Private = true;
                                break;
                            case "instance":
                                currentFnInfo.Instance = true;
                                break;
                            
                            case "void" when p == splitLine.Length - 2:
                                currentFnInfo.ReturnType = Types.Void;
                                break;
                            case "i8" when p == splitLine.Length - 2:
                                currentFnInfo.ReturnType = Types.I8;
                                break;
                            case "i16" when p == splitLine.Length - 2:
                                currentFnInfo.ReturnType = Types.I16;
                                break;
                            case "i32" when p == splitLine.Length - 2:
                                currentFnInfo.ReturnType = Types.I32;
                                break;
                            case "i64" when p == splitLine.Length - 2:
                                currentFnInfo.ReturnType = Types.I64;
                                break;
                            case "u8" when p == splitLine.Length - 2:
                                currentFnInfo.ReturnType = Types.U8;
                                break;
                            case "u16" when p == splitLine.Length - 2:
                                currentFnInfo.ReturnType = Types.U16;
                                break;
                            case "u32" when p == splitLine.Length - 2:
                                currentFnInfo.ReturnType = Types.U32;
                                break;
                            case "u64" when p == splitLine.Length - 2:
                                currentFnInfo.ReturnType = Types.U64;
                                break;
                            case "f32" when p == splitLine.Length - 2:
                                currentFnInfo.ReturnType = Types.F32;
                                break;
                            case "f64" when p == splitLine.Length - 2:
                                currentFnInfo.ReturnType = Types.F64;
                                break;
                            case "str" when p == splitLine.Length - 2:
                                currentFnInfo.ReturnType = Types.String;
                                break;
                            
                            default:
                                if (p == splitLine.Length - 1)
                                {
                                    int funcNamePos;
                                    
                                    if ((funcNamePos = splitLine[p].IndexOf('(')) == -1 || !splitLine[p].EndsWith(')'))
                                        throw Error("Function name must contain valid braces.", i);

                                    currentFnInfo.Name = splitLine[p][..funcNamePos];

                                    if (funcNamePos + 1 == splitLine[p].Length - 1)
                                        break;
                                    
                                    string[] arguments = splitLine[p][(funcNamePos + 1)..(splitLine[p].Length - 1)].Split(',');
                                    {
                                        currentFnInfo.Arguments = new Types[arguments.Length];
                                        for (int a = 0; a < arguments.Length; a++)
                                        {
                                            currentFnInfo.Arguments[a] = arguments[a] switch
                                            {
                                                "i8" => Types.I8,
                                                "i16" => Types.I16,
                                                "i32" => Types.I32,
                                                "i64" => Types.I64,
                                                "u8" => Types.U8,
                                                "u16" => Types.U16,
                                                "u32" => Types.U32,
                                                "u64" => Types.U64,
                                                "f32" => Types.F32,
                                                "f64" => Types.F64,
                                                "str" => Types.String,
                                                _ => throw Error("Unsupported argument type \"" + arguments[a] + "\".", i)
                                            };
                                        }
                                    }
                                }
                                else
                                    throw Error("Unrecognized attribute \"" + splitLine[p] + "\"", i);

                                break;
                        }
                    }
                    
                    break;
                case "fnend":
                    currentFnInfo.EndPoint = i;
                    inFn = false;
                    functions.Add(currentFnInfo);
                    break;
                
                case "var":
                    if (!inClass)
                        throw Error("\"var\" is only valid in a class.", i);

                    currentVarInfo = new VarInfo();

                    for (int p = 1; p < splitLine.Length; p++)
                    {
                        switch (splitLine[p])
                        {
                            case "private":
                                currentVarInfo.Private = true;
                                break;
                            case "instance":
                                currentVarInfo.Instance = true;
                                break;
                            
                            case "void" when p == splitLine.Length - 2:
                                currentVarInfo.Type = Types.Void;
                                break;
                            case "i8" when p == splitLine.Length - 2:
                                currentVarInfo.Type = Types.I8;
                                break;
                            case "i16" when p == splitLine.Length - 2:
                                currentVarInfo.Type = Types.I16;
                                break;
                            case "i32" when p == splitLine.Length - 2:
                                currentVarInfo.Type = Types.I32;
                                break;
                            case "i64" when p == splitLine.Length - 2:
                                currentVarInfo.Type = Types.I64;
                                break;
                            case "u8" when p == splitLine.Length - 2:
                                currentVarInfo.Type = Types.U8;
                                break;
                            case "u16" when p == splitLine.Length - 2:
                                currentVarInfo.Type = Types.U16;
                                break;
                            case "u32" when p == splitLine.Length - 2:
                                currentVarInfo.Type = Types.U32;
                                break;
                            case "u64" when p == splitLine.Length - 2:
                                currentVarInfo.Type = Types.U64;
                                break;
                            case "f32" when p == splitLine.Length - 2:
                                currentVarInfo.Type = Types.F32;
                                break;
                            case "f64" when p == splitLine.Length - 2:
                                currentVarInfo.Type = Types.F64;
                                break;
                            case "str" when p == splitLine.Length - 2:
                                currentVarInfo.Type = Types.String;
                                break;
                            
                            default:
                                if (p == splitLine.Length - 1)
                                    currentVarInfo.Name = splitLine[p];
                                else
                                    throw Error("Unrecognized attribute \"" + splitLine[p] + "\".", i);
                                break;
                        }
                    }
                    
                    variables.Add(currentVarInfo);
                    break;
                
                case "lbl":
                    break;
                
                default:
                    throw Error("Unrecognized command \"" + splitLine[0] + "\".", i);
            }
        }
        
        Console.WriteLine("Done!");
        
        Console.WriteLine("Parsing instructions... ");

        List<Instruction> instructions = new List<Instruction>();

        foreach (ClassInfo cInfo in classes)
        {
            for (int f = 0; f < cInfo.Functions.Length; f++)
            {
                ref FnInfo info = ref cInfo.Functions[f];
                
                Console.Write("Parsing " + cInfo.Name + "::" + info.Name + "(" + (info.Arguments == null ? "" : string.Join(',', info.Arguments)) + ")... ");

                for (int i = info.StartPoint; i < info.EndPoint; i++)
                {
                    string line = splitCode[i].Trim();
                    if (line == "" || line.StartsWith(":"))
                        continue;

                    // Strip excess spaces and preprocess strings a bit.
                    string compressedLine = "";
                    int numSpace = 0;
                    bool inString = false;
                    for (int s = 0; s < line.Length; s++)
                    {
                        switch (line[s])
                        {
                            case ' ':
                                if (inString)
                                {
                                    compressedLine += ' ';
                                    break;
                                }

                                numSpace++;
                                if (numSpace <= 1)
                                    compressedLine += ' ';
                                break;
                            
                            case '"':
                                inString = !inString;
                                compressedLine += '"';
                                break;
                            default:
                                numSpace = 0;
                                compressedLine += line[s];
                                break;
                        }
                    }

                    string[] splitLine = compressedLine.Split(' ');
                    string[] splitDot = splitLine[0].Split('.');
                    switch (splitDot[0])
                    {
                        case "nop":
                            instructions.Add(new Instruction(OpCode.Nop));
                            break;
                        
                        case "ld":
                            switch (splitDot[1])
                            {
                                case "i8":
                                    instructions.Add(new Instruction(OpCode.Ld_I8, Types.I8, sbyte.Parse(splitLine[1])));
                                    break;
                                case "i16":
                                    instructions.Add(new Instruction(OpCode.Ld_I16, Types.I16, short.Parse(splitLine[1])));
                                    break;
                                case "i32":
                                    instructions.Add(new Instruction(OpCode.Ld_I32, Types.I32, int.Parse(splitLine[1])));
                                    break;
                                case "i64":
                                    instructions.Add(new Instruction(OpCode.Ld_I64, Types.I64, long.Parse(splitLine[1])));
                                    break;
                                case "u8":
                                    instructions.Add(new Instruction(OpCode.Ld_U8, Types.U8, byte.Parse(splitLine[1])));
                                    break;
                                case "u16":
                                    instructions.Add(new Instruction(OpCode.Ld_U16, Types.U16, ushort.Parse(splitLine[1])));
                                    break;
                                case "u32":
                                    instructions.Add(new Instruction(OpCode.Ld_U32, Types.U32, uint.Parse(splitLine[1])));
                                    break;
                                case "u64":
                                    instructions.Add(new Instruction(OpCode.Ld_U64, Types.U64, ulong.Parse(splitLine[1])));
                                    break;
                                case "f32":
                                    instructions.Add(new Instruction(OpCode.Ld_F32, Types.F32, float.Parse(splitLine[1])));
                                    break;
                                case "f64":
                                    instructions.Add(new Instruction(OpCode.Ld_F64, Types.F64, double.Parse(splitLine[1])));
                                    break;
                                case "str":
                                    instructions.Add(new Instruction(OpCode.LD_Str, Types.String, string.Join(' ', splitLine[1..]).Trim('"')));
                                    break;
                            }

                            break;

                        case "conv":
                            switch (splitDot[1])
                            {
                                case "i8":
                                    instructions.Add(new Instruction(OpCode.Conv_I8));
                                    break;
                                case "i16":
                                    instructions.Add(new Instruction(OpCode.Conv_I16));
                                    break;
                                case "i32":
                                    instructions.Add(new Instruction(OpCode.Conv_I32));
                                    break;
                                case "i64":
                                    instructions.Add(new Instruction(OpCode.Conv_I64));
                                    break;
                                case "u8":
                                    instructions.Add(new Instruction(OpCode.Conv_U8));
                                    break;
                                case "u16":
                                    instructions.Add(new Instruction(OpCode.Conv_U16));
                                    break;
                                case "u32":
                                    instructions.Add(new Instruction(OpCode.Conv_U32));
                                    break;
                                case "u64":
                                    instructions.Add(new Instruction(OpCode.Conv_U64));
                                    break;
                                case "f32":
                                    instructions.Add(new Instruction(OpCode.Conv_F32));
                                    break;
                                case "f64":
                                    instructions.Add(new Instruction(OpCode.Conv_F64));
                                    break;
                                case "str":
                                    instructions.Add(new Instruction(OpCode.Conv_Str));
                                    break;
                            }

                            break;
                        
                        case "add":
                            instructions.Add(new Instruction(OpCode.Add));
                            break;
                        case "sub":
                            instructions.Add(new Instruction(OpCode.Sub));
                            break;
                        case "mul":
                            instructions.Add(new Instruction(OpCode.Mul));
                            break;
                        case "div":
                            instructions.Add(new Instruction(OpCode.Div));
                            break;
                        
                        case "sttmp":
                            instructions.Add(new Instruction((OpCode) ((int) OpCode.Sttmp_0 + int.Parse(splitDot[1]))));
                            break;
                        case "ldtmp":
                            instructions.Add(new Instruction((OpCode) ((int) OpCode.Ldtmp_0 + int.Parse(splitDot[1]))));
                            break;
                        case "ldarg":
                            instructions.Add(new Instruction((OpCode) ((int) OpCode.Ldarg_0 + int.Parse(splitDot[1]))));
                            break;
                        
                        case "call":
                            OpCode callCode;

                            if (splitDot.Length == 1)
                                callCode = OpCode.Call;
                            else
                            {
                                callCode = splitDot[1] switch
                                {
                                    "i" => OpCode.Call_I,
                                    _ => throw Error("Unrecognized call opcode type \"" + splitDot[1] + "\".", i)
                                };
                            }
                            
                            instructions.Add(new Instruction(callCode, o: splitLine[1]));
                            break;
                        case "callstd":
                            instructions.Add(new Instruction(OpCode.CallStd, o: splitLine[1]));
                            break;
                        
                        case "create":
                            OpCode createCode;
                            
                            if (splitDot.Length == 1)
                                createCode = OpCode.Create;
                            else
                            {
                                createCode = splitDot[1] switch
                                {
                                    "c" => OpCode.Create_C,
                                    _ => throw Error("Unrecognized create opcode type \"" + splitDot[1] + "\".", i)
                                };
                            }
                            
                            instructions.Add(new Instruction(createCode, o: splitLine[1]));
                            break;
                        
                        case "stvar":
                            OpCode stCode;
                            
                            if (splitDot.Length == 1)
                               stCode = OpCode.Stvar;
                            else
                            {
                                stCode = splitDot[1] switch
                                {
                                    "i" => OpCode.Stvar_I,
                                    _ => throw Error("Unrecognized stvar opcode type \"" + splitDot[1] + "\".", i)
                                };
                            }
                            
                            instructions.Add(new Instruction(stCode, o: splitLine[1]));
                            break;
                        
                        case "br":
                            instructions.Add(new Instruction(OpCode.Br, o: splitLine[1]));
                            break;

                        default:
                            throw Error("Unrecognized opcode \"" + splitLine[0] + "\".", i);
                    }
                }

                info.Instructions = instructions.ToArray();
                
                instructions.Clear();
                
                Console.WriteLine("Done!");
            }
        }
        
        Console.WriteLine("Instructions parsed with 0 errors.");
        
        // Skip the compiler checks for now - it will just assume everything is perfect :)
        /*Console.WriteLine("Performing checks...");
        bool hasEntryPoint = false;
        foreach ((_, ClassInfo ci) in classes)
        {
            
        }*/
        
        // Likewise, don't perform optimization right now.
        // Console.WriteLine("Beginning optimizations...");
        
        Console.Write("Creating metadata... ");

        List<string> strings = new List<string>();

        foreach (ClassInfo ci in classes)
        {
            for (int f = 0; f < ci.Functions.Length; f++)
            {
                ref FnInfo info = ref ci.Functions[f];

                for (int i = 0; i < info.Instructions.Length; i++)
                {
                    ref Instruction inst = ref info.Instructions[i];

                    switch (inst.OpCode)
                    {
                        case OpCode.LD_Str:
                            strings.Add((string) inst.Object);
                            inst.Object = strings.Count - 1;
                            break;
                        case OpCode.Call:
                        case OpCode.Call_I:
                            string[] splitCall = ((string) inst.Object).Split("::");
                            for (int c = 0; c < classes.Count; c++)
                            {
                                if (classes[c].Name != splitCall[0])
                                    continue;

                                string funcName = splitCall[1][..(splitCall[1].IndexOf('('))];
                                for (int fn = 0; fn < classes[c].Functions.Length; fn++)
                                {
                                    if (classes[c].Functions[fn].Name != funcName)
                                        continue;

                                    inst.Object = (c << 16) | fn;
                                    // Hacky but it works
                                    inst.Type = Types.I32;
                                }
                            }
                            break;
                    }
                }
            }
        }
        
        Console.WriteLine("Done!");
        
        Console.Write("Creating the binary... ");

        using MemoryStream ms = new MemoryStream();
        using BinaryWriter writer = new BinaryWriter(ms);
        writer.Write("GNDB".ToCharArray());
        // Version 0
        writer.Write(0);
        writer.Write("strs".ToCharArray());
        for (int i = 0; i < strings.Count; i++)
            writer.Write(strings[i]);

        foreach (ClassInfo ci in classes)
        {
            writer.Write("clss".ToCharArray());
            writer.Write(ci.Name);
            byte classFlags = 0;
            classFlags |= (byte) (ci.Implements != "none" ? 1 : 0);
            classFlags |= (byte) ((ci.Instance ? 1 : 0) << 1);
            classFlags |= (byte) ((ci.ValueType ? 1 : 0) << 2);
            
            writer.Write(classFlags);
            if ((classFlags & 1) == 1)
                writer.Write(ci.Implements);
            
            writer.Write((byte) ci.Functions.Length);

            for (int f = 0; f < ci.Functions.Length; f++)
            {
                writer.Write("fn".ToCharArray());
                ref FnInfo info = ref ci.Functions[f];
                writer.Write(info.Name);
                byte fnFlags = 0;
                fnFlags |= (byte) (info.EntryPoint ? 1 : 0);
                fnFlags |= (byte) ((info.Constructor ? 1 : 0) << 1);
                fnFlags |= (byte) ((info.Instance ? 1 : 0) << 2);
                fnFlags |= (byte) ((info.Private ? 1 : 0) << 3);
                fnFlags |= (byte) ((info.ReturnType != Types.Void ? 1 : 0) << 4);
                fnFlags |= (byte) ((info.Arguments?.Length > 0 ? 1 : 0) << 5);
                writer.Write(fnFlags);
                if ((fnFlags & 16) == 16)
                    writer.Write((byte) info.ReturnType);
                if ((fnFlags & 32) == 32)
                {
                    writer.Write((byte) info.Arguments.Length);
                    for (int i = 0; i < info.Arguments.Length; i++)
                        writer.Write((byte) info.Arguments[i]);
                }

                for (int i = 0; i < info.Instructions.Length; i++)
                    WriteInstruction(info.Instructions[i], writer);
            }
        }

        Console.WriteLine("Done!");

        return ms.ToArray();
    }

    private void WriteInstruction(Instruction instruction, BinaryWriter writer)
    {
        writer.Write((byte) instruction.OpCode);
        switch (instruction.Type)
        {
            case Types.Void:
                break;
            case Types.I8:
                writer.Write((sbyte) instruction.Object);
                break;
            case Types.I16:
                writer.Write((short) instruction.Object);
                break;
            case Types.I32:
                writer.Write((int) instruction.Object);
                break;
            case Types.I64:
                writer.Write((long) instruction.Object);
                break;
            case Types.U8:
                writer.Write((byte) instruction.Object);
                break;
            case Types.U16:
                writer.Write((ushort) instruction.Object);
                break;
            case Types.U32:
                writer.Write((uint) instruction.Object);
                break;
            case Types.U64:
                writer.Write((ulong) instruction.Object);
                break;
            case Types.F32:
                writer.Write((float) instruction.Object);
                break;
            case Types.F64:
                writer.Write((double) instruction.Object);
                break;
            case Types.String:
                writer.Write((int) instruction.Object);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private Exception Error(string message, int line)
    {
        return new Exception("Line " + line + ": " + message);
    }
}