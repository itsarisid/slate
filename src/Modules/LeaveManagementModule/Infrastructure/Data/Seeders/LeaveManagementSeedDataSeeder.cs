using Alphabet.Domain.Entities;
using Alphabet.Domain.Enums;
using Alphabet.Domain.Interfaces;
using Alphabet.Domain.Interfaces.LeaveManagement;
using Alphabet.Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Alphabet.Infrastructure.Data.Seeders;

/// <summary>
/// Seeds baseline leave policies and approval chains.
/// </summary>
public static class LeaveManagementSeedDataSeeder
{
    /// <summary>
    /// Seeds leave management reference data.
    /// </summary>
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ILeaveRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Alphabet.LeaveSeed");

        var annual = await EnsureLeaveTypeAsync(repository, "Annual Leave", "ANNUAL", "Paid annual leave entitlement.", 21, true, true, CancellationToken.None);
        await EnsureLeaveTypeAsync(repository, "Sick Leave", "SICK", "Medical leave with optional attachment requirement.", 10, true, true, CancellationToken.None);
        await EnsureLeaveTypeAsync(repository, "Unpaid Leave", "UNPAID", "Unpaid discretionary leave.", 0, false, true, CancellationToken.None);

        var chains = await repository.GetApprovalChainsAsync(CancellationToken.None);
        if (!chains.Any(x => x.Code == "DEFAULT-LEAVE"))
        {
            await repository.AddApprovalChainAsync(
                ApprovalChain.Create(
                    "Default Leave Approval",
                    "DEFAULT-LEAVE",
                    "Manager then HR approval for standard leave requests.",
                    new ApprovalChainApplicability([annual.Id], [], [], [], 0, null),
                    [
                        new ApprovalLevelDefinition(1, "Manager Approval", LeaveApproverType.RoleBased, "Manager", 1, 48, true, 48, "HR", false, true, new Dictionary<string, string>()),
                        new ApprovalLevelDefinition(2, "HR Approval", LeaveApproverType.Hr, "HR", 1, 48, true, 48, "Admin", false, true, new Dictionary<string, string>())
                    ],
                    2,
                    false,
                    false,
                    true),
                CancellationToken.None);
        }

        await unitOfWork.SaveChangesAsync(CancellationToken.None);
        logger.LogInformation("Leave management seed data is ready.");
    }

    private static async Task<LeaveType> EnsureLeaveTypeAsync(
        ILeaveRepository repository,
        string name,
        string code,
        string description,
        decimal defaultDays,
        bool isPaid,
        bool requiresApproval,
        CancellationToken cancellationToken)
    {
        var existing = await repository.GetLeaveTypeByCodeAsync(code, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var leaveType = LeaveType.Create(
            name,
            code,
            description,
            "#2F80ED",
            null,
            isPaid,
            defaultDays,
            30,
            0.5m,
            null,
            requiresApproval,
            null,
            true,
            5,
            3,
            false,
            null,
            true,
            new LeaveEligibilityRules(0, false, [], []),
            [],
            code == "SICK",
            code == "SICK" ? ["pdf", "jpg", "png"] : [],
            new LeaveAutoApproveRules(false, 0, 0),
            true);

        await repository.AddLeaveTypeAsync(leaveType, cancellationToken);
        return leaveType;
    }
}
