using HerrGeneral.Core.Registration;
using HerrGeneral.Scanner.Test.AnotherNamespace;
using Shouldly;

// ReSharper disable once CheckNamespace
namespace HerrGeneral.Scanner.Test
{
    public class ScannerShould
    {
        [Fact]
        public void Get_all_concrete_class() =>
            Core.Registration.Scanner.Scan([
                        new ScanParam(typeof(Command1).Assembly)
                    ],
                    [typeof(ICommandHandler<>)])
                [typeof(ICommandHandler<>)]
                .ShouldBe([
                    typeof(Command3.Command1Handler),
                    typeof(Command1.Command1Handler),
                    typeof(Command2Handler),
                ]);

        [Fact]
        public void Get_all_concrete_class_filtered_by_namespace() =>
            HerrGeneral.Core.Registration.Scanner.Scan([
                        new ScanParam(typeof(Command1).Assembly,
                            typeof(Command3).Namespace!)
                    ],
                    [typeof(ICommandHandler<>)])
                [typeof(ICommandHandler<>)]
                .ShouldBe([
                    typeof(Command3.Command1Handler)
                ]);
        
    }

    internal interface ICommandHandler<in T> 
    {
        IEnumerable<object> Handle(T command);
    }
        
    internal record Command1
    {
        public class Command1Handler : ICommandHandler<Command1>
        {
            public IEnumerable<object> Handle(Command1 command) => [];
        }
    }

    internal record Command2;

    internal abstract class Command2HandlerBase : ICommandHandler<Command2>
    {
        public IEnumerable<object> Handle(Command2 command) => [];
    }

    internal class Command2Handler : Command2HandlerBase;

    namespace AnotherNamespace
    {
        internal record Command3
        {
            public class Command1Handler : ICommandHandler<Command3>
            {
                public IEnumerable<object> Handle(Command3 command) => [];
            }
        }
    }
}

