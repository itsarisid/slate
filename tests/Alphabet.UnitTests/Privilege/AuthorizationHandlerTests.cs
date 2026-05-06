using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Common.Interfaces.Privilege;
using Alphabet.Application.Features.Privilege.Dtos;
using Alphabet.Application.Results;
using Alphabet.Infrastructure.Security;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Moq;
using Xunit;

namespace Alphabet.UnitTests.Privilege;

public sealed class AuthorizationHandlerTests
{
    [Fact]
    public async Task Handler_Should_Succeed_When_User_Has_Required_Privilege()
    {
        var currentUser = new Mock<ICurrentUserService>();
        var privilegeService = new Mock<IPrivilegeService>();
        currentUser.SetupGet(x => x.UserId).Returns(Guid.NewGuid());
        privilegeService
            .Setup(x => x.CheckPrivilegeAsync(It.IsAny<Guid>(), "user.view", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PrivilegeCheckResultDto>.Success(new PrivilegeCheckResultDto(true, "Role: Admin", null, null)));

        var handler = new PrivilegeAuthorizationHandler(privilegeService.Object, currentUser.Object);
        var requirement = new PrivilegeRequirement(["user.view"], false);
        var context = new AuthorizationHandlerContext([requirement], new System.Security.Claims.ClaimsPrincipal(), null);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }
}
