using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Features.Communication.Commands.SendCommunication;
using FluentAssertions;
using Moq;

namespace Alphabet.UnitTests.Application.Communication;

public sealed class SendCommunicationCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Successful_Channel_Results()
    {
        var communicationService = new Mock<ICommunicationService>();
        communicationService
            .Setup(service => service.SendAsync(It.IsAny<CommunicationDispatchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new CommunicationDispatchResult("Email", true, "Email queued.", DateTimeOffset.UtcNow),
                new CommunicationDispatchResult("Sms", false, "SMS provider unavailable.", DateTimeOffset.UtcNow)
            ]);

        var handler = new SendCommunicationCommandHandler(communicationService.Object);
        var command = new SendCommunicationCommand(
            "Important update",
            "Please review the latest account activity.",
            ["Email", "Sms"],
            "user@example.com",
            "+97455555555",
            "user-1",
            null,
            null,
            false);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TotalChannelsRequested.Should().Be(2);
        result.Value.SuccessfulChannels.Should().Be(1);
        result.Value.FailedChannels.Should().Be(1);
    }
}
