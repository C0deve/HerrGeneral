using HerrGeneral.Core.WriteSide;
using HerrGeneral.DDD;
using HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing;
using Shouldly;

namespace HerrGeneral.WriteSide.DDD.Test;

public class AggregateLockKeyShould
{
    private record CustomDddCommand([property: AggregateLockKey<TheThing>] Guid TargetId);

    [Fact]
    public void Attribute_has_target_aggregate_type()
    {
        var attr = new AggregateLockKeyAttribute<TheThing>();
        attr.KeyGroup.ShouldBe(typeof(TheThing).FullName);
    }

    [Fact]
    public void ExtractKey_qualifies_key_with_aggregate_type()
    {
        var id = Guid.NewGuid();
        var cmd = new CustomDddCommand(id);

        var key = CommandConcurrencyLimiter.ExtractKey(cmd);

        key.ShouldBe((typeof(TheThing).FullName, (object)id));
    }
}
