using Alphabet.Modules.PrivilegeModule.Api;
using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Alphabet.IntegrationTests.Privilege;

public sealed class PrivilegeIntegrationTests
{
    [Fact]
    public void MapPrivilegeModule_Should_Register_Without_Throwing()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddRouting();
        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
        }).AddApiExplorer();

        var app = builder.Build();

        var exception = Record.Exception(() => app.MapPrivilegeModule());

        Assert.Null(exception);
    }
}
