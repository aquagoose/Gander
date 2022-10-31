namespace Gander.Parser;

public enum OpCode : byte
{
    Nop,
    
    Ld_I8,
    Ld_I16,
    Ld_I32,
    Ld_I64,
    
    Ld_U8,
    Ld_U16,
    Ld_U32,
    Ld_U64,
    
    Ld_F32,
    Ld_F64,
    
    LD_Str,
    
    Conv_I8,
    Conv_I16,
    Conv_I32,
    Conv_I64,
    
    Conv_U8,
    Conv_U16,
    Conv_U32,
    Conv_U64,
    
    Conv_F32,
    Conv_F64,
    
    Conv_Str,
    
    Add,
    Sub,
    Div,
    Mul,
    
    Sttmp_0,
    Sttmp_1,
    Sttmp_2,
    Sttmp_3,
    Sttmp_4,
    Sttmp_5,
    Sttmp_6,
    Sttmp_7,
    Sttmp_8,
    Sttmp_9,
    
    Ldtmp_0,
    Ldtmp_1,
    Ldtmp_2,
    Ldtmp_3,
    Ldtmp_4,
    Ldtmp_5,
    Ldtmp_6,
    Ldtmp_7,
    Ldtmp_8,
    Ldtmp_9,
    
    Ldarg_0,
    Ldarg_1,
    Ldarg_2,
    Ldarg_3,
    Ldarg_4,
    Ldarg_5,
    Ldarg_6,
    Ldarg_7,
    Ldarg_8,
    Ldarg_9,
    
    Call,
    Call_I,
    CallStd,
    
    Create,
    Create_C,
    
    Stvar,
    Stvar_I,
    
    Br
}