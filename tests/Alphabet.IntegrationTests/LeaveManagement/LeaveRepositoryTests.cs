using Alphabet.Domain.Entities;
using Alphabet.Domain.Models;
using Alphabet.Infrastructure.Persistence.Context;
using Alphabet.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Alphabet.IntegrationTests.LeaveManagement;

public sealed class LeaveRepositoryTests
{
    [Fact]
    public async Task GetLeaveTypesAsync_Should_Return_Active_Leave_Types()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"leave-repository-{Guid.NewGuid()}")
            .Options;

        await using var dbContext = new AppDbContext(options);
        var repository = new LeaveRepository(dbContext);

        await repository.AddLeaveTypeAsync(CreateLeaveType("Annual Leave", "ANNUAL", true), CancellationToken.None);
        await repository.AddLeaveTypeAsync(CreateLeaveType("Legacy Leave", "LEGACY", false), CancellationToken.None);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        var result = await repository.GetLeaveTypesAsync(true, null, null, CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("ANNUAL", result[0].Code);
    }

    private static LeaveType CreateLeaveType(string name, string code, bool isActive)
        => LeaveType.Create(
            name,
            code,
            $"{name} description",
            "#2F80ED",
            null,
            true,
            21,
            30,
            0.5m,
            null,
            false,
            null,
            true,
            5,
            3,
            false,
            null,
            true,
            new LeaveEligibilityRules(0, false, [], []),
            [],
            false,
            [],
            new LeaveAutoApproveRules(false, 0, 0),
            isActive);
}
