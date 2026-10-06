namespace HerrGeneral.SampleApplication.Bank.WriteSide.Card;

/// <summary>
/// Strongly-typed identifier for bank cards.
/// </summary>
public readonly record struct CardNumber(string Value)
{
    public override string ToString() => Value;

    public static implicit operator string(CardNumber number) => number.Value;
    public static implicit operator CardNumber(string value) => new(value);
}
