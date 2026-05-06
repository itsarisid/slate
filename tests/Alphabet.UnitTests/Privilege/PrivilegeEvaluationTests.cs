using Alphabet.Application.Common.Interfaces;
using Alphabet.Domain.Entities.Privilege;
using Alphabet.Domain.Enums;
using Alphabet.Domain.Interfaces;
using Alphabet.Domain.Interfaces.Privilege;
using Alphabet.Infrastructure.Options;
using Alphabet.Infrastructure.Repositories.Privilege;
using Alphabet.Infrastructure.Security;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Alphabet.UnitTests.Privilege;

public sealed class PrivilegeEvaluationTests
{
    [Fact]
    public async Task HasPrivilegeAsync_Should_Return_True_When_Role_Assigns_Privilege()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var privilege = Alphabet.Domain.Entities.Privilege.Privilege.Create("user.view", "View Users", null, null, "User", ["Read"], false, null, "system");
        var repository = new Mock<IPrivilegeRepository>();
        var auditRepository = new Mock<IPrivilegeAuditRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var currentUser = new Mock<ICurrentUserService>();
        var cacheService = new Mock<Alphabet.Application.Common.Interfaces.ICacheService>();

        repository.Setup(x => x.GetUserRoleIdsAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync([roleId]);
        repository.Setup(x => x.GetUserRoleNamesAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(["Admin"]);
        repository.Setup(x => x.GetRoleByIdAsync(roleId, It.IsAny<CancellationToken>())).ReturnsAsync(new IdentityRole<Guid> { Id = roleId, Name = "Admin" });
        repository.Setup(x => x.GetRolePrivilegesAsync(roleId, It.IsAny<CancellationToken>())).ReturnsAsync([RolePrivilege.Create(roleId, privilege.Id, "system", null)]);
        repository.Setup(x => x.GetPrivilegesByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([privilege]);
        repository.Setup(x => x.GetRolePolicyAssignmentsAsync(roleId, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        repository.Setup(x => x.GetPoliciesByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        repository.Setup(x => x.GetUserPolicyAssignmentsAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        repository.Setup(x => x.GetUserPrivilegesAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        repository.Setup(x => x.GetPrivilegesByNamesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([privilege]);

        var service = CreateService(repository, auditRepository, unitOfWork, currentUser, cacheService);

        var result = await service.HasPrivilegeAsync(userId, ["user.view"], false, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasPrivilegeAsync_Should_Return_False_When_Direct_Deny_Overrides_Role_Assignment()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var privilege = Alphabet.Domain.Entities.Privilege.Privilege.Create("user.delete", "Delete Users", null, null, "User", ["Delete"], false, null, "system");
        var repository = new Mock<IPrivilegeRepository>();
        var auditRepository = new Mock<IPrivilegeAuditRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var currentUser = new Mock<ICurrentUserService>();
        var cacheService = new Mock<Alphabet.Application.Common.Interfaces.ICacheService>();

        repository.Setup(x => x.GetUserRoleIdsAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync([roleId]);
        repository.Setup(x => x.GetUserRoleNamesAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(["Admin"]);
        repository.Setup(x => x.GetRoleByIdAsync(roleId, It.IsAny<CancellationToken>())).ReturnsAsync(new IdentityRole<Guid> { Id = roleId, Name = "Admin" });
        repository.Setup(x => x.GetRolePrivilegesAsync(roleId, It.IsAny<CancellationToken>())).ReturnsAsync([RolePrivilege.Create(roleId, privilege.Id, "system", null)]);
        repository.Setup(x => x.GetPrivilegesByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([privilege]);
        repository.Setup(x => x.GetRolePolicyAssignmentsAsync(roleId, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        repository.Setup(x => x.GetPoliciesByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        repository.Setup(x => x.GetUserPolicyAssignmentsAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        repository.Setup(x => x.GetUserPrivilegesAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([UserPrivilege.Create(userId, privilege.Id, PrivilegeEffect.Deny, "admin@alphabet.local", null, "Temporary restriction")]);
        repository.Setup(x => x.GetPrivilegesByNamesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync([privilege]);

        var service = CreateService(repository, auditRepository, unitOfWork, currentUser, cacheService);

        var result = await service.HasPrivilegeAsync(userId, [privilege.Name], false, CancellationToken.None);

        result.Should().BeFalse();
    }

    private static PrivilegeService CreateService(
        Mock<IPrivilegeRepository> repository,
        Mock<IPrivilegeAuditRepository> auditRepository,
        Mock<IUnitOfWork> unitOfWork,
        Mock<ICurrentUserService> currentUser,
        Mock<Alphabet.Application.Common.Interfaces.ICacheService> cacheService)
    {
        currentUser.SetupGet(x => x.Email).Returns("admin@alphabet.local");
        currentUser.SetupGet(x => x.IpAddress).Returns("127.0.0.1");

        return new PrivilegeService(
            repository.Object,
            auditRepository.Object,
            unitOfWork.Object,
            currentUser.Object,
            CreateUserManager(),
            CreateRoleManager(),
            new PrivilegeCacheRepository(cacheService.Object),
            Options.Create(new PrivilegeSettings { CacheEnabled = false }),
            NullLogger<PrivilegeService>.Instance);
    }

    private static UserManager<Alphabet.Domain.Entities.ApplicationUser> CreateUserManager()
    {
        var store = new Mock<IUserStore<Alphabet.Domain.Entities.ApplicationUser>>();
        return new UserManager<Alphabet.Domain.Entities.ApplicationUser>(
            store.Object,
            Mock.Of<IOptions<IdentityOptions>>(),
            Mock.Of<IPasswordHasher<Alphabet.Domain.Entities.ApplicationUser>>(),
            [],
            [],
            Mock.Of<ILookupNormalizer>(),
            new IdentityErrorDescriber(),
            Mock.Of<IServiceProvider>(),
            Mock.Of<ILogger<UserManager<Alphabet.Domain.Entities.ApplicationUser>>>());
    }

    private static RoleManager<IdentityRole<Guid>> CreateRoleManager()
    {
        var store = new Mock<IRoleStore<IdentityRole<Guid>>>();
        return new RoleManager<IdentityRole<Guid>>(
            store.Object,
            [],
            Mock.Of<ILookupNormalizer>(),
            new IdentityErrorDescriber(),
            Mock.Of<ILogger<RoleManager<IdentityRole<Guid>>>>());
    }
}
