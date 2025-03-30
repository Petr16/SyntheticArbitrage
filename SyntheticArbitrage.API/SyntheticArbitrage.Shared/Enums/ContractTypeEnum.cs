namespace SyntheticArbitrage.Shared.Enums;

public enum ContractTypeEnum
{
    Perpetual,
    CurrentQuarter,
    NextQuarter
}

public static class ContractTypeExtensions
{
    public static string ToTypeString(this ContractTypeEnum contractType)
    {
        switch(contractType)
        {
            case ContractTypeEnum.Perpetual: return "PERPETUAL";
            case ContractTypeEnum.CurrentQuarter: return "CURRENT_QUARTER";
            case ContractTypeEnum.NextQuarter: return "NEXT_QUARTER";
            default: throw new ArgumentOutOfRangeException(nameof(contractType), "Unsupported contract type");
        };
    }
}

