using HerrGeneral.Core.Registration;
using HerrGeneral.Scanner.Test.AnotherNamespace;
using Shouldly;

// ReSharper disable once CheckNamespace
namespace HerrGeneral.Scanner.Test
{
    public class ScannerShould
    {
        [Fact]
        public void GetAllConcreteClass() =>
            Core.Registration.Scanner.Scan([
                        new ScanParam(typeof(Command1).Assembly)
                    ],
                    [typeof(ICommandHandler<>)])
                [typeof(ICommandHandler<>)]
                .ShouldBe([
                    typeof(Command3.Command3Handler),
                    typeof(Command1.Command1Handler),
                    typeof(Command2Handler),
                ]);

        [Fact]
        public void GetAllConcreteClassFilteredByNamespace() =>
            HerrGeneral.Core.Registration.Scanner.Scan([
                        new ScanParam(typeof(Command1).Assembly,
                            typeof(Command3).Namespace!)
                    ],
                    [typeof(ICommandHandler<>)])
                [typeof(ICommandHandler<>)]
                .ShouldBe([
                    typeof(Command3.Command3Handler)
                ]);
        
        [Fact]
        public void GetAllConcreteClassFilteredByParentNamespace() =>
            HerrGeneral.Core.Registration.Scanner.Scan([
                        new ScanParam(typeof(Command1).Assembly,
                            typeof(Command1).Namespace!)
                    ],
                    [typeof(ICommandHandler<>)])
                [typeof(ICommandHandler<>)]
                .ShouldBe([
                    typeof(Command3.Command3Handler),
                    typeof(Command1.Command1Handler),
                    typeof(Command2Handler),
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
            public class Command3Handler : ICommandHandler<Command3>
            {
                public IEnumerable<object> Handle(Command3 command) => [];
            }
        }
    }
}

