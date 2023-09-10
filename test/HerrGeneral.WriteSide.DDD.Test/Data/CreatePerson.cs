namespace HerrGeneral.WriteSide.DDD.Test.Data;

public record CreatePerson : CreateAggregate<Person>
{
    public class Handler: CreateAggregateHandler<Person,CreatePerson>
    {
        public Handler(CtorParams @params) : base(@params)
        {
        }

        protected override Person Handle(CreatePerson command, Guid aggregateId) => new(aggregateId);
    }
}