namespace HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing.Command;

/// <summary>
/// Command to delete all TheThing aggregates
/// </summary>
public record DeleteAllTheThings
{
    public Guid Id { get; } = Guid.NewGuid();
    
    /// <summary>
    /// Handler for deleting all TheThing aggregates
    /// </summary>
    public class Handler(IAggregateRepository<TheThing> repository, TheThingTracker tracker) : IChangeMultiHandler<TheThing, DeleteAllTheThings>
    {
        /// <summary>
        /// Handles the DeleteAllTheThings command by deleting all existing TheThing aggregates
        /// </summary>
        /// <param name="command">The delete command</param>
        /// <returns>Success result</returns>
        public IEnumerable<TheThing> Handle(DeleteAllTheThings command) => 
            tracker.All()
                .Select(repository.Get)
                .Select(aggregate => aggregate!.Delete(command.Id));
    }
}
