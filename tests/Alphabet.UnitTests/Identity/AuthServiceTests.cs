using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Features.Identity.Commands;
using Alphabet.Application.Features.Identity.Dtos;
using Alphabet.Application.Results;
using FluentAssertions;
using Moq;
using Xunit;

namespace Alphabet.UnitTests.Identity;

public sealed class AuthServiceTests
{
    [Fact]
    public async Task Register_Handler_Should_Return_User_When_Service_Succeeds()
    {
        var identityService = new Mock<IIdentityService>();
        identityService
            .Setup(x => x.RegisterAsync("user@example.com", "SecurePass123!", "John", "Doe", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UserDto>.Success(new UserDto(Guid.NewGuid(), "user@example.com", "John", "Doe", false)));

        var handler = new RegisterCommandHandler(identityService.Object);
        var result = await handler.Handle(new RegisterCommand("user@example.com", "SecurePass123!", "SecurePass123!", "John", "Doe"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }
}
