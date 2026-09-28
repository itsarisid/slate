// Bare integer enums in swagger.json: JobType(4), ScheduleType(3),
// ExecutionStatus(6), RetryBackoffType(3). Names below are best-guess,
// derived from the fields present on JobConfigurationDto (url/method suggest
// Http; storedProcedureName suggests StoredProcedure; handlerType/methodName
// suggest an internal handler; sourcePath/destinationPath/archivePath
// suggest a file operation). Match the *integer values* against your real
// enums if they already exist.

namespace Alphabet.Domain.Entities
{
    public enum JobType
    {
        HttpCallback = 1,
        StoredProcedure = 2,
        InternalHandler = 3,
        FileOperation = 4
    }

    public enum ScheduleType
    {
        Cron = 1,
        Interval = 2,
        OneTime = 3
    }

    public enum ExecutionStatus
    {
        Pending = 1,
        Running = 2,
        Succeeded = 3,
        Failed = 4,
        Cancelled = 5,
        TimedOut = 6
    }

    public enum RetryBackoffType
    {
        Fixed = 1,
        Linear = 2,
        Exponential = 3
    }
}
