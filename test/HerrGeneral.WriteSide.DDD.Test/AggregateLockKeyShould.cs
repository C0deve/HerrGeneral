using HerrGeneral.Core.WriteSide;
using HerrGeneral.DDD;
using HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing;
using HerrGeneral.WriteSide.DDD.Test.Data.WriteSide.TheThing.Command;
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

    [Fact]
    public void Change_command_key_matches_lock_key_attribute_and_is_cached()
    {
        var id = Guid.NewGuid();
        var changeCmd = new ChangeTheThing("newName", id);

        var key = CommandConcurrencyLimiter.ExtractKey(changeCmd);

        key.ShouldBe((typeof(TheThing).FullName, (object)id));
        changeCmd.Key.ShouldBe((typeof(TheThing).FullName, (object)id));
        ReferenceEquals(changeCmd.Key, changeCmd.Key).ShouldBeTrue();
    }
}
