using System.Text;

namespace HerrGeneral.Contracts.WriteSide;

public interface ICommand<out TResult>
{
    Guid Id { get; }
    DateTime ExecutionDate { get; }

    StringBuilder Log(StringBuilder sb);
}