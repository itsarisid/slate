namespace Alphabet.Domain.Enums;

/// <summary>
/// Supported scheduler job types.
/// </summary>
public enum JobType
{
    HttpCall = 1,
    StoredProcedure = 2,
    CodeExecution = 3,
    FileOperation = 4
}
