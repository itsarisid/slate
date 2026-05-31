using Alphabet.Application.Features.Communication;
using Alphabet.Application.Features.Communication.Commands.SendCommunication;
using Alphabet.Application.Features.Communication.Queries.GetCommunicationConfiguration;
using Alphabet.Common.Extensions;
using Alphabet.Modules.CommunicationModule.Api.Resource;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Alphabet.Modules.CommunicationModule.Api;

/// <summary>
/// Maps communication endpoints for sending alerts, notifications, and user messages.
/// </summary>
public static class CommunicationModuleEndpoints
{
    /// <summary>
    /// Registers communication endpoints for the module.
    /// </summary>
    public static IEndpointRouteBuilder MapCommunicationModule(this IEndpointRouteBuilder endpoints)
    {
        var versionSet = endpoints.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        var group = endpoints.MapGroup("api/v{version:apiVersion}/communications")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(new ApiVersion(1, 0))
            .WithTags("Communication Module")
            .RequireAuthorization("AdminOnly");

        group.MapPost(
                ApiResource.SendCommunication.Endpoint,
                async Task<Results<Ok<CommunicationBatchResponseDto>, BadRequest<ProblemDetails>>> (
                    [FromBody] SendCommunicationCommand command,
                    [FromServices] ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(command, cancellationToken);

                    if (result.IsFailure || result.Value is null)
                    {
                        return TypedResults.BadRequest(new ProblemDetails
                        {
                            Title = "Communication dispatch failed",
                            Detail = result.Error
                        });
                    }

                    return TypedResults.Ok(result.Value);
                })
            .Produces<CommunicationBatchResponseDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .WithDocumentation(ApiResource.SendCommunication);

        group.MapGet(
                ApiResource.GetCommunicationConfiguration.Endpoint,
                async Task<Results<Ok<CommunicationConfigurationDto>, BadRequest<ProblemDetails>>> (
                    [FromServices] ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new GetCommunicationConfigurationQuery(), cancellationToken);

                    if (result.IsFailure || result.Value is null)
                    {
                        return TypedResults.BadRequest(new ProblemDetails
                        {
                            Title = "Communication configuration could not be loaded",
                            Detail = result.Error
                        });
                    }

                    return TypedResults.Ok(result.Value);
                })
            .Produces<CommunicationConfigurationDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .WithDocumentation(ApiResource.GetCommunicationConfiguration);

        return endpoints;
    }
}
