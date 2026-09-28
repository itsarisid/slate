// NOTE: swagger.json only exposes these as bare integer enums (1..N), with no
// string names. The member names below are my best-guess mapping based on the
// module's domain (asset management) and field usage. Reconcile the *values*
// against whatever your real enum already uses if one exists — the integers
// must match, the names are cosmetic.

namespace Alphabet.Domain.Entities
{
    public enum AssetStatus
    {
        Active = 1,
        InUse = 2,
        UnderMaintenance = 3,
        InStorage = 4,
        Retired = 5,
        Disposed = 6,
        Lost = 7
    }

    public enum AssetCondition
    {
        New = 1,
        Excellent = 2,
        Good = 3,
        Fair = 4,
        Poor = 5
    }

    public enum AssetAssignmentType
    {
        Permanent = 1,
        Temporary = 2,
        Loan = 3,
        Pool = 4
    }

    public enum AssetLocationType
    {
        Building = 1,
        Floor = 2,
        Room = 3,
        Warehouse = 4,
        Site = 5
    }

    public enum AssetMaintenancePriority
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }

    public enum AssetMaintenanceType
    {
        Preventive = 1,
        Corrective = 2,
        Inspection = 3
    }
}
