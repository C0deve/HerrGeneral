﻿using HerrGeneral.WriteSide.DDD;

namespace HerrGeneral.SampleApplication.WriteSide;

public record SetFriend : Change<Person>
{
    private readonly string _friend;

    public SetFriend(Guid aggregateId, string friend) : base(aggregateId) => _friend = friend;

    public class Handler : ChangeHandler<Person,SetFriend>
    {
        public Handler(CtorParams ctorParams) : base(ctorParams)
        {
        }

        protected override Person Handle(Person aggregate, SetFriend command) => 
            aggregate.SetFriend(command._friend, command.Id);
    }
}