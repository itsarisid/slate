namespace Alphabet.Infrastructure.Options;

/// <summary>
/// Core asset management settings.
/// </summary>
public sealed class AssetManagementSettings
{
    public const string SectionName = "AssetManagementSettings";

    public string AssetTagPrefix { get; init; } = "AST";

    public int AssetTagDigits { get; init; } = 6;

    public string DefaultCurrency { get; init; } = "USD";

    public bool EnableQrCode { get; init; } = true;

    public bool EnableBarcode { get; init; } = true;

    public int RetentionDays { get; init; } = 2555;

    public int LowStockThreshold { get; init; } = 5;

    public bool AutoGenerateAssetTag { get; init; } = true;

    public bool RequireApprovalForAssignment { get; init; }

    public bool RequireApprovalForDisposal { get; init; } = true;

    public int MaxBulkImportSize { get; init; } = 1000;
}

/// <summary>
/// Workflow behavior settings for asset management.
/// </summary>
public sealed class AssetWorkflowSettings
{
    public const string SectionName = "AssetWorkflowSettings";

    public bool EnableAutoEscalation { get; init; } = true;

    public int EscalationCheckIntervalMinutes { get; init; } = 60;

    public int DefaultTimeoutHours { get; init; } = 48;

    public int MaxConcurrentWorkflows { get; init; } = 100;
}

/// <summary>
/// Depreciation calculation settings.
/// </summary>
public sealed class AssetDepreciationSettings
{
    public const string SectionName = "AssetDepreciationSettings";

    public string DefaultMethod { get; init; } = "StraightLine";

    public decimal DefaultRate { get; init; } = 20.0m;

    public decimal SalvagePercentage { get; init; } = 10.0m;
}

/// <summary>
/// Notification settings for the asset module.
/// </summary>
public sealed class AssetNotificationSettings
{
    public const string SectionName = "AssetNotificationSettings";

    public bool AssignmentConfirmation { get; init; } = true;

    public int[] ReturnReminderDays { get; init; } = [3, 1, 0];

    public int WarrantyExpiryWarningDays { get; init; } = 30;

    public int MaintenanceReminderDays { get; init; } = 14;

    public int OverdueEscalationDays { get; init; } = 7;
}

/// <summary>
/// Barcode and QR code generation settings.
/// </summary>
public sealed class AssetBarcodeSettings
{
    public const string SectionName = "AssetBarcodeSettings";

    public string Format { get; init; } = "QRCode";

    public int Size { get; init; } = 300;

    public bool IncludeUrl { get; init; } = true;

    public string BaseUrl { get; init; } = "https://assets.alphabet.local";
}

/// <summary>
/// Import/export settings for asset data.
/// </summary>
public sealed class AssetImportExportSettings
{
    public const string SectionName = "AssetImportExportSettings";

    public string[] SupportedFormats { get; init; } = ["csv", "xlsx", "json"];

    public int MaxFileSizeMb { get; init; } = 10;

    public int ExportChunkSize { get; init; } = 500;
}
