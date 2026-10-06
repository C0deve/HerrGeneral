namespace HerrGeneral.SampleApplication.Bank.WriteSide.Account;

/// <summary>
/// Strongly-typed identifier for bank accounts.
/// </summary>
public readonly record struct AccountNumber(string Value)
{
    public override string ToString() => Value;

    public static implicit operator string(AccountNumber number) => number.Value;
    public static implicit operator AccountNumber(string value) => new(value);
}
