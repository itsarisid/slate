using Alphabet.Common.Models;

namespace Alphabet.Modules.SystemLogModule.Api.Resource;

/// <summary>
/// Provides documentation metadata for system log endpoints.
/// </summary>
public static class ApiResource
{
    /// <summary>
    /// Gets the endpoint metadata for listing log files.
    /// </summary>
    public static EndpointDetails ListSystemLogs => new()
    {
        Endpoint = "/",
        Name = "ListSystemLogs",
        Summary = "Lists the available system log files.",
        Description = "Returns the rolling Serilog files stored in the application's logs directory. Administrator access is required."
    };

    /// <summary>
    /// Gets the endpoint metadata for reading a log file.
    /// </summary>
    public static EndpointDetails GetSystemLog => new()
    {
        Endpoint = "/{fileName}",
        Name = "GetSystemLog",
        Summary = "Gets the latest entries from a system log file.",
        Description = "Returns the tail of a selected .log file. The optional lines parameter defaults to 200 and is capped at 1000."
    };
}
