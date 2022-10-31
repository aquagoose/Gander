using System.IO;
using Gander;
using Gander.Parser;

IlToAsm asm = new IlToAsm();

const string code = @"
:class implements(none) MyProgram

    :fn entrypoint i32 main()

        ld.i32      5
        ld.i32      6
        add
        sttmp.0
        ld.str      ""5 + 6 = ""
        ldtmp.0
        ld.str      ""!""

        ld.i32      3
        callstd     concat
        callstd     stdout
        callstd     endl

        ld.i32      5
        call        MyProgram::myfunc(i32)

        create.c    SomeClass
        sttmp.0

        ld.str      ""Here is some class I just created: ""
        callstd     stdout
        callstd     endl

        ldtmp.0
        ld.str      ""String 1""
        ld.str      ""String 2""
        call.i      SomeClass::stuff(str,str)

        ld.i32 0

    :fnend

    :fn void myfunc(i32)

        ld.str      ""This is a function. The number was: ""
        ldarg.0
        ld.str      "".""
        ld.i32      3
        callstd     concat
        callstd     stdout
        callstd     endl

    :fnend

:clend

:class instance implements(none) SomeClass

    :var private instance f64 _myField

    :fn constructor init()

        ld.f64      129.234
        stvar.i     _myField

    :fnend

    :fn instance void stuff(str,str)

        :lbl        loop
        ldarg.0
        ldarg.1
        ld.i32      2
        callstd     concat
        callstd     stdout
        callstd     endl
        br          loop

    :fnend

:clend
";

File.WriteAllBytes("/home/ollie/Documents/stuff.gnd", asm.Process(code));