using Alphabet.Common.Models;

namespace Alphabet.Modules.CommunicationModule.Api.Resource;

public static class ApiResource
{
    public static EndpointDetails SendCommunication => new()
    {
        Endpoint = "/send",
        Name = "SendCommunication",
        Summary = "Sends a message through one or more configured channels.",
        Description = "Dispatches the provided subject and body through Email, Sms, Push, InApp, and Webhook channels based on the request payload and enabled configuration."
    };

    public static EndpointDetails GetCommunicationConfiguration => new()
    {
        Endpoint = "/configuration",
        Name = "GetCommunicationConfiguration",
        Summary = "Gets the active communication module configuration.",
        Description = "Returns the enabled channels, default channel, and diagnostic settings used by the communication module."
    };
}
