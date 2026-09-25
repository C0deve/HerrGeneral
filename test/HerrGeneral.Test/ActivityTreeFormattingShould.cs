using FakeItEasy;
using HerrGeneral.Core.Diagnostics;
using Microsoft.Extensions.Logging;

namespace HerrGeneral.Tracing;

public class ActivityTreeFormattingShould
{
    private class SampleCommand;
    private class SampleCommandHandler;
    private class CardPaymentAuthorizedEvent;
    private class DebitAccountOnCardPayment;
    private class AccountDebitedEvent;
    private class ApplyOverdraftFeeOnAccountDebited;
    private class FeeAppliedEvent;
    private class CardTransactionsView;
    private class AccountBalanceView;
    private class AccountStatementView;
    private class SendSmsConfirmation;
    private class SendOverdraftWarning;
    private class AccountCreated;
    private class CreateBankCardOnAccountCreated;
    private class BankCardCreated;
    private class MoneyDeposited;
    private class MoneyWithdrawn;

    [Fact]
    public void FormatCompactTreeWithoutRedundantEmptyLines()
    {
        var collector = new ActivityTreeCollector();
        collector.StartHandlingCommand("DepositMoney", typeof(SampleCommandHandler), 2);

        var evtCreated = new AccountCreated();
        var evtBankCard = new BankCardCreated();
        var evtDeposited = new MoneyDeposited();
        var evtWithdrawn = new MoneyWithdrawn();

        collector.RegisterRootWriteSideEvent(evtCreated);
        collector.PublishEventOnWriteSide(evtCreated);
        collector.RecordWriteSideHandler(typeof(CreateBankCardOnAccountCreated), typeof(AccountCreated), TimeSpan.FromMilliseconds(13.7), [evtBankCard], null);

        collector.RegisterRootWriteSideEvent(evtDeposited);
        collector.PublishEventOnWriteSide(evtDeposited);

        collector.RegisterRootWriteSideEvent(evtWithdrawn);
        collector.PublishEventOnWriteSide(evtWithdrawn);

        collector.StopHandlingCommand("DepositMoney", TimeSpan.FromMilliseconds(0.1));

        var formatted = ActivityTreeFormatter.Format(collector);
        var lines = formatted.Split(["\r\n", "\n"], StringSplitOptions.None);

        lines.ShouldContain("CMD [DepositMoney] (thread #2) .................................... [OK] (0.1ms)");
        lines.ShouldContain(" |");
        lines.ShouldContain(" \\--> (cmd) SampleCommandHandler (0.1ms)");
        lines.ShouldContain("       |");
        lines.ShouldContain("       |-- (evt) AccountCreated");
        lines.ShouldContain("       |    \\--> (wr) CreateBankCardOnAccountCreated (13.7ms)");
        lines.ShouldContain("       |          \\-- (evt) BankCardCreated");
        lines.ShouldContain("       |");
        lines.ShouldContain("       |-- (evt) MoneyDeposited");
        lines.ShouldContain("       \\-- (evt) MoneyWithdrawn");

        // Verify there is no separator between MoneyDeposited and MoneyWithdrawn
        var idxDeposited = Array.FindIndex(lines, l => l.Contains("MoneyDeposited"));
        var idxWithdrawn = Array.FindIndex(lines, l => l.Contains("MoneyWithdrawn"));
        idxWithdrawn.ShouldBe(idxDeposited + 1);

        // Verify there is no separator between AccountCreated and its handler
        var idxAccountCreated = Array.FindIndex(lines, l => l.Contains("AccountCreated"));
        var idxHandler = Array.FindIndex(lines, l => l.Contains("CreateBankCardOnAccountCreated"));
        idxHandler.ShouldBe(idxAccountCreated + 1);

        // Verify there is no separator between handler and child event
        var idxBankCardCreated = Array.FindIndex(lines, l => l.Contains("BankCardCreated"));
        idxBankCardCreated.ShouldBe(idxHandler + 1);
    }

    [Fact]
    public void FormatNominalCausalTreeAccurately()
    {
        var collector = new ActivityTreeCollector();
        collector.StartHandlingCommand("ProcessCardPayment", typeof(SampleCommandHandler), 12);
        collector.StartUnitOfWork(TimeSpan.FromMilliseconds(0.1));

        var evt1 = new CardPaymentAuthorizedEvent();
        var evt2 = new AccountDebitedEvent();
        var evt3 = new FeeAppliedEvent();

        collector.RegisterRootWriteSideEvent(evt1);
        collector.PublishEventOnWriteSide(evt1);
        collector.RecordWriteSideHandler(typeof(DebitAccountOnCardPayment), typeof(CardPaymentAuthorizedEvent), TimeSpan.FromMilliseconds(2.3), [evt2], null);

        collector.PublishEventOnWriteSide(evt2);
        collector.RecordWriteSideHandler(typeof(ApplyOverdraftFeeOnAccountDebited), typeof(AccountDebitedEvent), TimeSpan.FromMilliseconds(1.4), [evt3], null);

        collector.PublishEventOnWriteSide(evt3);

        collector.CommitUnitOfWork(TimeSpan.FromMilliseconds(1.1));

        collector.RecordSyncProjection(typeof(CardPaymentAuthorizedEvent), typeof(CardTransactionsView), TimeSpan.FromMilliseconds(1.8), null);
        collector.RecordSyncProjection(typeof(AccountDebitedEvent), typeof(AccountBalanceView), TimeSpan.FromMilliseconds(2.1), null);
        collector.RecordSyncProjection(typeof(FeeAppliedEvent), typeof(AccountStatementView), TimeSpan.FromMilliseconds(1.2), null);

        collector.RecordPostTransaction(typeof(CardPaymentAuthorizedEvent), typeof(SendSmsConfirmation), "side-effect", TimeSpan.FromMilliseconds(2.4), null);
        collector.RecordPostTransaction(typeof(FeeAppliedEvent), typeof(SendOverdraftWarning), "side-effect", TimeSpan.FromMilliseconds(2.0), null);

        collector.StopHandlingCommand("ProcessCardPayment", TimeSpan.FromMilliseconds(16.2));

        var formatted = ActivityTreeFormatter.Format(collector);

        formatted.ShouldContain("CMD [ProcessCardPayment] (thread #12)");
        formatted.ShouldContain("[OK] (16.2ms)");
        formatted.ShouldContain("\\--> (cmd) SampleCommandHandler");
        formatted.ShouldContain("|-- (evt) CardPaymentAuthorizedEvent");
        formatted.ShouldContain("\\--> (wr) DebitAccountOnCardPayment (2.3ms)");
        formatted.ShouldContain("\\-- (evt) AccountDebitedEvent");
        formatted.ShouldContain("\\--> (wr) ApplyOverdraftFeeOnAccountDebited (1.4ms)");
        formatted.ShouldContain("\\-- (evt) FeeAppliedEvent");
        formatted.ShouldContain("\\== [TX COMMIT] (1.1ms)");
        formatted.ShouldContain("+-- [SYNC PROJECTIONS] (Read-Side)");
        formatted.ShouldContain("CardPaymentAuthorizedEvent   ===> CardTransactionsView     (1.8ms)");
        formatted.ShouldContain("AccountDebitedEvent          ===> AccountBalanceView       (2.1ms)");
        formatted.ShouldContain("FeeAppliedEvent              ===> AccountStatementView     (1.2ms)");
        formatted.ShouldContain("\\-- [POST TRANSACTION] (Side Effects & Outbox)");
        formatted.ShouldContain("CardPaymentAuthorizedEvent   ===> SendSmsConfirmation      (2.4ms)");
        formatted.ShouldContain("FeeAppliedEvent              ===> SendOverdraftWarning     (2.0ms)");
    }

    [Fact]
    public void FormatFailureWithRollbackAndSkippedSections()
    {
        var collector = new ActivityTreeCollector();
        collector.StartHandlingCommand("TransferFunds", typeof(SampleCommandHandler), 14);
        collector.StartUnitOfWork(TimeSpan.FromMilliseconds(0.1));

        var evt1 = new CardPaymentAuthorizedEvent();
        collector.RegisterRootWriteSideEvent(evt1);
        collector.PublishEventOnWriteSide(evt1);

        var ex = new InvalidOperationException("Account balance cannot be negative.");
        collector.RecordWriteSideHandler(typeof(DebitAccountOnCardPayment), typeof(CardPaymentAuthorizedEvent), TimeSpan.FromMilliseconds(2.8), null, ex);

        collector.RollbackUnitOfWork(TimeSpan.FromMilliseconds(0.4));
        collector.StopHandlingCommand("TransferFunds", TimeSpan.FromMilliseconds(6.4));

        var formatted = ActivityTreeFormatter.Format(collector);

        formatted.ShouldContain("CMD [TransferFunds] (thread #14)");
        formatted.ShouldContain("[FAILED] (6.4ms)");
        formatted.ShouldContain("\\--> (wr) DebitAccountOnCardPayment");
        formatted.ShouldContain("[ERR]");
        formatted.ShouldContain("EXCEPTION: InvalidOperationException (2.8ms)");
        formatted.ShouldContain("Message : \"Account balance cannot be negative.\"");
        formatted.ShouldContain("\\== [TX ROLLBACK] (0.4ms)");
        formatted.ShouldContain("+--x [SYNC PROJECTIONS] (Skipped: Transaction aborted)");
        formatted.ShouldContain("\\--x [POST TRANSACTION] (Skipped: Transaction aborted)");
    }

    [Fact]
    public async Task ProduceCausalTreeTraceWhenCommandExecutedViaMediator()
    {
        var unitOfWork = A.Fake<IUnitOfWork>();
        var loggerProvider = new InMemoryLoggerProvider();
        var services = new ServiceCollection()
            .AddScoped<IUnitOfWork>(_ => unitOfWork)
            .AddLogging(builder => builder.AddProvider(loggerProvider).SetMinimumLevel(LogLevel.Information))
            .AddHerrGeneral(cfg =>
                cfg.ScanWriteSideOn(typeof(E2ECommand).Assembly, typeof(E2ECommand).Namespace!)
                   .ScanReadSideOn(typeof(E2ECommand).Assembly, typeof(E2ECommand).Namespace!));

        var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<Mediator>();

        var result = await mediator.Send(new E2ECommand());
        result.IsSuccess.ShouldBeTrue();

        var log = loggerProvider.Messages.FirstOrDefault(m => m.Contains("CMD [E2ECommand]"));
        log.ShouldNotBeNull();
        log.ShouldContain("CMD [E2ECommand]");
        log.ShouldContain("[OK]");
        log.ShouldContain("\\--> (cmd) Handler");
        log.ShouldContain("|-- (evt) E2ERootEvent");
        log.ShouldContain("\\--> (wr) E2EWriteSideHandler");
        log.ShouldContain("\\-- (evt) E2EChildEvent");
        log.ShouldContain("\\== [TX COMMIT]");
        log.ShouldContain("E2ERootEvent                 ===> E2EProjection");
    }

    [Fact]
    public async Task ProduceOriginFromDomainCodeWhenCommandHandlerFails()
    {
        var unitOfWork = A.Fake<IUnitOfWork>();
        var loggerProvider = new InMemoryLoggerProvider();
        var services = new ServiceCollection()
            .AddScoped<IUnitOfWork>(_ => unitOfWork)
            .AddLogging(builder => builder.AddProvider(loggerProvider).SetMinimumLevel(LogLevel.Information))
            .AddHerrGeneral(cfg =>
                cfg.ScanWriteSideOn(typeof(E2EFailingCommand).Assembly, typeof(E2EFailingCommand).Namespace!)
                   .ScanReadSideOn(typeof(E2EFailingCommand).Assembly, typeof(E2EFailingCommand).Namespace!));

        var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<Mediator>();

        var result = await mediator.Send(new E2EFailingCommand());
        result.IsSuccess.ShouldBeFalse();

        var log = loggerProvider.Messages.FirstOrDefault(m => m.Contains("CMD [E2EFailingCommand]"));
        log.ShouldNotBeNull();
        log.ShouldContain("CMD [E2EFailingCommand]");
        log.ShouldContain("[FAILED]");
        log.ShouldContain("EXCEPTION: InvalidOperationException");
        log.ShouldContain("Message : \"Command business error\"");
        log.ShouldContain($"Origin  : {typeof(ActivityTreeFormattingShould).FullName}.E2EDomain.ThrowDomainError()");
        log.ShouldNotContain("HerrGeneral.Core.WriteSide.CommandPipeline");
    }

    [Fact]
    public async Task ProduceOriginFromDomainCodeWhenEventHandlerFails()
    {
        var unitOfWork = A.Fake<IUnitOfWork>();
        var loggerProvider = new InMemoryLoggerProvider();
        var services = new ServiceCollection()
            .AddScoped<IUnitOfWork>(_ => unitOfWork)
            .AddLogging(builder => builder.AddProvider(loggerProvider).SetMinimumLevel(LogLevel.Information))
            .AddHerrGeneral(cfg =>
                cfg.ScanWriteSideOn(typeof(E2EFailingEventCommand).Assembly, typeof(E2EFailingEventCommand).Namespace!)
                   .ScanReadSideOn(typeof(E2EFailingEventCommand).Assembly, typeof(E2EFailingEventCommand).Namespace!));

        var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<Mediator>();

        var result = await mediator.Send(new E2EFailingEventCommand());
        result.IsSuccess.ShouldBeFalse();

        var log = loggerProvider.Messages.FirstOrDefault(m => m.Contains("CMD [E2EFailingEventCommand]"));
        log.ShouldNotBeNull();
        log.ShouldContain("CMD [E2EFailingEventCommand]");
        log.ShouldContain("[FAILED]");
        log.ShouldContain("[ERR]");
        log.ShouldContain("EXCEPTION: InvalidOperationException");
        log.ShouldContain("Message : \"Event handler business error\"");
        log.ShouldContain($"Origin  : {typeof(ActivityTreeFormattingShould).FullName}.E2EDomain.ThrowEventHandlerDomainError()");
        log.ShouldContain("\\== [TX ROLLBACK]");
        log.ShouldNotContain("HerrGeneral.Core.WriteSide.EventHandlerPipeline");
    }

    public record E2ECommand
    {
        public class Handler : ICommandHandler<E2ECommand, Unit>
        {
            public (IReadOnlyList<object> Events, Unit Result) Handle(E2ECommand command) =>
                ((IReadOnlyList<object>)[new E2ERootEvent()], Unit.Default);
        }
    }

    public record E2EFailingCommand
    {
        public class Handler : ICommandHandler<E2EFailingCommand, Unit>
        {
            public (IReadOnlyList<object> Events, Unit Result) Handle(E2EFailingCommand command)
            {
                E2EDomain.ThrowDomainError();
                return (Array.Empty<object>(), Unit.Default);
            }
        }
    }

    public record E2EFailingEventCommand
    {
        public class Handler : ICommandHandler<E2EFailingEventCommand, Unit>
        {
            public (IReadOnlyList<object> Events, Unit Result) Handle(E2EFailingEventCommand command) =>
                ((IReadOnlyList<object>)[new E2EFailingEvent()], Unit.Default);
        }
    }

    public static class E2EDomain
    {
        public static void ThrowDomainError() =>
            throw new InvalidOperationException("Command business error");

        public static void ThrowEventHandlerDomainError() =>
            throw new InvalidOperationException("Event handler business error");
    }

    public record E2ERootEvent;
    public record E2EChildEvent;
    public record E2EFailingEvent;

    public class E2EWriteSideHandler : IEventHandler<E2ERootEvent>
    {
        public IReadOnlyList<object> Handle(E2ERootEvent @event) =>
            [new E2EChildEvent()];
    }

    public class E2EFailingWriteSideHandler : IEventHandler<E2EFailingEvent>
    {
        public IReadOnlyList<object> Handle(E2EFailingEvent @event)
        {
            E2EDomain.ThrowEventHandlerDomainError();
            return Array.Empty<object>();
        }
    }

    public class E2EProjection : IHandleSyncProjection<E2ERootEvent>
    {
        public void Handle(E2ERootEvent @event) { }
    }

    private class InMemoryLoggerProvider : ILoggerProvider
    {
        public List<string> Messages { get; } = new();

        public ILogger CreateLogger(string categoryName) => new CustomLogger(this);
        public void Dispose() { }

        private class CustomLogger(InMemoryLoggerProvider provider) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, System.Exception? exception, Func<TState, System.Exception?, string> formatter)
            {
                provider.Messages.Add(formatter(state, exception));
            }
        }
    }
}
