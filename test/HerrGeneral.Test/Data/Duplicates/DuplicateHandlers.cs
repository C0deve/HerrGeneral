using HerrGeneral.WriteSide;

namespace HerrGeneral.Test.Data.Duplicates;

public record DuplicateCmd;

public class DuplicateHandlerA : ICommandHandler<DuplicateCmd, Unit>
{
    public (IReadOnlyList<object> Events, Unit Result) Handle(DuplicateCmd command) => ([], Unit.Default);
}

public class DuplicateHandlerB : ICommandHandler<DuplicateCmd, Unit>
{
    public (IReadOnlyList<object> Events, Unit Result) Handle(DuplicateCmd command) => ([], Unit.Default);
}
