using System.ComponentModel.DataAnnotations.Schema;

namespace Alphabet.Domain.ValueObjects;

/// <summary>
/// Represents depreciation calculation results.
/// </summary>
[NotMapped]
public sealed record DepreciationSnapshot(
    decimal OriginalCost,
    decimal CurrentValue,
    decimal DepreciationRate,
    string DepreciationMethod,
    decimal AccumulatedDepreciation,
    decimal SalvageValue,
    decimal DepreciationYearToDate);
