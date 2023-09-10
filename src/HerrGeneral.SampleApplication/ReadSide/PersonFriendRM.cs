using HerrGeneral.ReadSide;
using HerrGeneral.SampleApplication.WriteSide;

namespace HerrGeneral.SampleApplication.ReadSide;

public record PersonFriendRM(Guid PersonId, string Person, string Friend)
{
    public class PersonFriendRMRepository : IEventHandler<FriendChanged>
    {
        private readonly Dictionary<Guid, PersonFriendRM> _personFriends = new();
        public Task Handle(FriendChanged notification, CancellationToken cancellationToken)
        {
            _personFriends[notification.AggregateId] = new PersonFriendRM(notification.AggregateId, notification.Person, notification.FriendName);
            return Task.CompletedTask;
        }

        public PersonFriendRM Get(Guid personId) => _personFriends[personId];
    }    
}


