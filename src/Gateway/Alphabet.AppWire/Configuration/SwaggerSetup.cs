using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Alphabet.AppWire.Configuration;

/// <summary>
/// Configures Swagger generation.
/// </summary>
public sealed class SwaggerSetup : IConfigureOptions<SwaggerGenOptions>
{
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
            var relativePath = apiDescription.RelativePath?.ToLowerInvariant() ?? string.Empty;
            return relativePath switch
            {
                var path when path.Contains("/communications") => ["Communication Module"],
                var path when path.Contains("/products") => ["Product Module"],
                var path when path.Contains("/auth") || path.Contains("/admin") => ["Identity Module"],
                _ => ["Alphabet API"]
            };
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
