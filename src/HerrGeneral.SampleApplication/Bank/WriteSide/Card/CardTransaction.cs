namespace HerrGeneral.SampleApplication.Bank.WriteSide.Card;

public record CardTransaction(decimal Amount, string Description, DateTime Timestamp);