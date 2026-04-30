using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Common.Interfaces.Scheduler;
using Alphabet.Application.Features.Scheduler.Commands;
using Alphabet.Application.Features.Scheduler.Dtos;
using Alphabet.Domain.Entities;
using Alphabet.Domain.Enums;
using Alphabet.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace Alphabet.UnitTests.Scheduler;

public sealed class JobManagementTests
{
    [Fact]
    public async Task CreateJob_Should_Return_Success()
    {
        var jobRepository = new Mock<IJobRepository>();
        var historyRepository = new Mock<IRepository<JobHistory>>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var schedulerService = new Mock<ISchedulerService>();
        var currentUserService = new Mock<ICurrentUserService>();

        currentUserService.SetupGet(x => x.Email).Returns("admin@alphabet.local");
        schedulerService.Setup(x => x.ScheduleJobAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>())).ReturnsAsync("scheduler-job:test");
        schedulerService.Setup(x => x.GetJobStatusAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>())).ReturnsAsync("Active");

        var handler = new CreateJobCommandHandler(jobRepository.Object, historyRepository.Object, unitOfWork.Object, schedulerService.Object, currentUserService.Object);

        var result = await handler.Handle(
            new CreateJobCommand(
                "Daily report",
                "Creates a report every morning",
                JobType.HttpCall,
                ScheduleType.Cron,
                "0 8 * * *",
                null,
                null,
                new JobConfigurationDto("https://example.com/reports", "POST", null, null, 30, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null),
                new RetryPolicyDto(3, 60, RetryBackoffType.Fixed, null, null),
                300,
                "UTC",
                true,
                ["reports"],
                null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be("Daily report");
    }
}
