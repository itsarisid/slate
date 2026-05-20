using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Alphabet.AppWire.Configuration;

/// <summary>
/// Configures Swagger generation.
/// </summary>
public sealed class SwaggerSetup : IConfigureOptions<SwaggerGenOptions>
{
    /// <summary>
    /// Configure.
    /// </summary>
    public void Configure(SwaggerGenOptions options)
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Alphabet API",
            Version = "v1",
            Description = "Production-ready Clean Architecture Web API template."
        });

        options.TagActionsBy(apiDescription =>
        {
            // Respect the WithTags() metadata set on minimal API endpoint groups.
            if (apiDescription.ActionDescriptor.EndpointMetadata
                    .OfType<Microsoft.AspNetCore.Http.Metadata.ITagsMetadata>()
                    .FirstOrDefault() is { } tagsMetadata && tagsMetadata.Tags.Count > 0)
            {
                return tagsMetadata.Tags.ToList();
            }

            return ["Alphabet API"];
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme.",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        });

        options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecuritySchemeReference("Bearer", null),
                []
            }
        });

        var xmlFile = $"{typeof(Program).Assembly.GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }
    }
}
