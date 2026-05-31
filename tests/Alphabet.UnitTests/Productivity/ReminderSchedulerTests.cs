using Alphabet.Domain.Entities;
using Alphabet.Infrastructure.Services;
using FluentAssertions;
using Hangfire;
using Hangfire.States;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Alphabet.UnitTests.Productivity;

public sealed class ReminderSchedulerTests
{
    [Fact]
    public async Task ScheduleAsync_Should_Create_Background_Job()
    {
        var client = new FakeBackgroundJobClient();
        var scheduler = new ReminderSchedulerService(client, NullLogger<ReminderSchedulerService>.Instance);
        var reminder = Reminder.Create(
            Guid.NewGuid(),
            "Doctor",
            "Annual checkup",
            DateTimeOffset.UtcNow.AddMinutes(5),
            Alphabet.Domain.Enums.ReminderType.Once,
            null,
            null,
            null,
            true,
            false,
            true,
            10,
            null,
            null,
            ["Email"],
            null);

        await scheduler.ScheduleAsync(reminder, CancellationToken.None);

        client.CreatedJobs.Should().BeGreaterThan(0);
    }

    private sealed class FakeBackgroundJobClient : IBackgroundJobClient
    {
        public int CreatedJobs { get; private set; }

        public string Create(Hangfire.Common.Job job, IState state)
        {
            CreatedJobs++;
            return Guid.NewGuid().ToString("N");
        }

        public bool ChangeState(string jobId, IState state, string expectedState) => true;
    }
}
