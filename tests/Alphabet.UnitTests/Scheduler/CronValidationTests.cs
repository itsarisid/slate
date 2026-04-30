using Alphabet.Infrastructure.Scheduler;
using FluentAssertions;
using Xunit;

namespace Alphabet.UnitTests.Scheduler;

public sealed class CronValidationTests
{
    [Theory]
    [InlineData("0 8 * * *")]
    [InlineData("*/5 * * * *")]
    public void IsValid_Should_Accept_Expected_Expressions(string expression)
    {
        var validator = new CronExpressionValidator();
        validator.IsValid(expression).Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("0 8 * *")]
    [InlineData("bad cron")]
    public void IsValid_Should_Reject_Invalid_Expressions(string expression)
    {
        var validator = new CronExpressionValidator();
        validator.IsValid(expression).Should().BeFalse();
    }
}
