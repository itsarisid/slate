using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Common.Interfaces.LeaveManagement;
using Alphabet.Application.Features.LeaveManagement.Commands;
using Alphabet.Domain.Entities;
using Alphabet.Domain.Enums;
using Alphabet.Domain.Interfaces;
using Alphabet.Domain.Interfaces.LeaveManagement;
using Alphabet.Domain.Models;
using FluentAssertions;
using Moq;
using Xunit;

namespace Alphabet.UnitTests.LeaveManagement;

public sealed class SubmitLeaveRequestCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Submit_Leave_Request_When_Balance_Is_Available()
    {
        var userId = Guid.NewGuid();
        var leaveType = CreateLeaveType();
        var balance = LeaveBalance.Create(userId, leaveType.Id, 2026, 10, 10);
        var repository = CreateRepository(userId, leaveType, balance);
        var unitOfWork = new Mock<IUnitOfWork>();
        var currentUser = CreateCurrentUser(userId);
        var calendar = new Mock<ILeaveCalendarService>();
        var approverResolver = new Mock<ILeaveApproverResolver>();
        var notifications = new Mock<ILeaveNotificationService>();

        calendar
            .Setup(x => x.CalculateLeaveDaysAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<LeavePartialDays>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);
        unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new SubmitLeaveRequestCommandHandler(
            repository.Object,
            calendar.Object,
            approverResolver.Object,
            notifications.Object,
            currentUser.Object,
            unitOfWork.Object);

        var result = await handler.Handle(CreateCommand(leaveType.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Status.Should().Be(LeaveRequestStatus.Approved.ToString());
        balance.Remaining.Should().Be(8);
        balance.Taken.Should().Be(2);
        repository.Verify(x => x.AddRequestAsync(It.IsAny<LeaveRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Balance_Is_Insufficient()
    {
        var userId = Guid.NewGuid();
        var leaveType = CreateLeaveType();
        var balance = LeaveBalance.Create(userId, leaveType.Id, 2026, 1, 1);
        var repository = CreateRepository(userId, leaveType, balance);
        var unitOfWork = new Mock<IUnitOfWork>();
        var currentUser = CreateCurrentUser(userId);
        var calendar = new Mock<ILeaveCalendarService>();

        calendar
            .Setup(x => x.CalculateLeaveDaysAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<LeavePartialDays>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        var handler = new SubmitLeaveRequestCommandHandler(
            repository.Object,
            calendar.Object,
            Mock.Of<ILeaveApproverResolver>(),
            Mock.Of<ILeaveNotificationService>(),
            currentUser.Object,
            unitOfWork.Object);

        var result = await handler.Handle(CreateCommand(leaveType.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Insufficient leave balance.");
        repository.Verify(x => x.AddRequestAsync(It.IsAny<LeaveRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static SubmitLeaveRequestCommand CreateCommand(Guid leaveTypeId)
        => new(
            leaveTypeId,
            new DateOnly(2026, 6, 1),
            new DateOnly(2026, 6, 2),
            new LeavePartialDays(LeaveDayPart.Full, LeaveDayPart.Full),
            "Family travel",
            [],
            null,
            null,
            true,
            false);

    private static LeaveType CreateLeaveType()
        => LeaveType.Create(
            "Annual Leave",
            "ANNUAL",
            "Annual entitlement",
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
            true);

    private static Mock<ICurrentUserService> CreateCurrentUser(Guid userId)
    {
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.UserId).Returns(userId);
        currentUser.Setup(x => x.Roles).Returns(["Employee"]);
        currentUser.Setup(x => x.IpAddress).Returns("127.0.0.1");
        currentUser.Setup(x => x.UserAgent).Returns("UnitTest");
        return currentUser;
    }

    private static Mock<ILeaveRepository> CreateRepository(Guid userId, LeaveType leaveType, LeaveBalance balance)
    {
        var repository = new Mock<ILeaveRepository>();
        repository.Setup(x => x.GetLeaveTypeByIdAsync(leaveType.Id, It.IsAny<CancellationToken>())).ReturnsAsync(leaveType);
        repository.Setup(x => x.GetBalanceAsync(userId, leaveType.Id, 2026, It.IsAny<CancellationToken>())).ReturnsAsync(balance);
        repository.Setup(x => x.HasOverlappingRequestAsync(userId, It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), null, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        repository.Setup(x => x.GetBlackoutPeriodsAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
        repository.Setup(x => x.GetApplicableApprovalChainAsync(leaveType.Id, It.IsAny<decimal>(), It.IsAny<CancellationToken>())).ReturnsAsync((ApprovalChain?)null);
        repository.Setup(x => x.AddRequestAsync(It.IsAny<LeaveRequest>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        repository.Setup(x => x.AddActivityAsync(It.IsAny<LeaveActivityLog>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        return repository;
    }
}
