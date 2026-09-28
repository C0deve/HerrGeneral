namespace HerrGeneral.Test.BuilderExtension;

public record SampleModuleService(string Value);

public static class SampleModuleExtensions
{
    public static IHerrGeneralBuilder UseSampleModule(this IHerrGeneralBuilder builder, string optionValue)
    {
        builder.Properties["SampleModuleOption"] = optionValue;
        builder.Services.AddSingleton(new SampleModuleService(optionValue));
        return builder;
    }

    public static ConfigurationBuilder UseSampleModule(this ConfigurationBuilder builder, string optionValue)
    {
        ((IHerrGeneralBuilder)builder).UseSampleModule(optionValue);
        return builder;
    }
}

public class BuilderExtensionShould(ITestOutputHelper output)
{
    public record CustomCommand(string Payload);
    public record CustomEvent(string Payload);

    public class CustomCommandHandler : ICommandHandler<CustomCommand, Unit>
    {
        public (IReadOnlyList<object> Events, Unit Result) Handle(CustomCommand command) =>
            ([new CustomEvent(command.Payload)], Unit.Default);
    }

    [Fact]
    public void Allow_custom_extensions_to_access_services_and_properties()
    {
        var services = new ServiceCollection();
        var builder = new ConfigurationBuilder(services);

        builder.UseSampleModule("MyConfigValue");

        builder.Properties["SampleModuleOption"].ShouldBe("MyConfigValue");

        var sp = services.BuildServiceProvider();
        var moduleService = sp.GetService<SampleModuleService>();
        moduleService.ShouldNotBeNull();
        moduleService.Value.ShouldBe("MyConfigValue");
    }

    [Fact]
    public async Task Support_custom_registration_policy_via_builder()
    {
        var services = new ServiceCollection()
            .AddHerrGeneralTestLogger(output)
            .AddSingleton<CustomCommandHandler>()
            .AddHerrGeneral(cfg => cfg
                .UseSampleModule("TestOption")
                .ScanWriteSideOn(typeof(CustomCommandHandler).Assembly, typeof(CustomCommandHandler).Namespace!));

        var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<Mediator>();

        var result = await mediator.Send(new CustomCommand("Hello Extension"));
        result.IsSuccess.ShouldBeTrue();

        var moduleService = sp.GetService<SampleModuleService>();
        moduleService.ShouldNotBeNull();
        moduleService.Value.ShouldBe("TestOption");
    }
}
