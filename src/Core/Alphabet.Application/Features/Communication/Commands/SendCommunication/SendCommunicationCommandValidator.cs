using FluentValidation;

namespace Alphabet.Application.Features.Communication.Commands.SendCommunication;

/// <summary>
/// Validates communication dispatch requests before processing.
/// </summary>
public sealed class SendCommunicationCommandValidator : AbstractValidator<SendCommunicationCommand>
{
    private static readonly HashSet<string> SupportedChannels =
    [
        "email",
        "sms",
        "push",
        "inapp",
        "webhook"
    ];

    public SendCommunicationCommandValidator()
    {
        RuleFor(x => x.Subject)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Body)
            .NotEmpty()
            .MaximumLength(4000);

        RuleFor(x => x.Channels)
            .NotEmpty()
            .Must(channels => channels.All(channel => SupportedChannels.Contains(channel.Trim().ToLowerInvariant())))
            .WithMessage("Channels must be one or more of: Email, Sms, Push, InApp, Webhook.");

        RuleFor(x => x.EmailAddress)
            .NotEmpty()
            .EmailAddress()
            .When(x => x.Channels.Any(channel => channel.Equals("Email", StringComparison.OrdinalIgnoreCase)));

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .When(x => x.Channels.Any(channel => channel.Equals("Sms", StringComparison.OrdinalIgnoreCase)));

        RuleFor(x => x.PushToken)
            .NotEmpty()
            .When(x => x.Channels.Any(channel => channel.Equals("Push", StringComparison.OrdinalIgnoreCase)));

        RuleFor(x => x.UserId)
            .NotEmpty()
            .When(x => x.Channels.Any(channel => channel.Equals("InApp", StringComparison.OrdinalIgnoreCase)));

        RuleFor(x => x.WebhookUrl)
            .NotEmpty()
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .When(x => x.Channels.Any(channel => channel.Equals("Webhook", StringComparison.OrdinalIgnoreCase)))
            .WithMessage("WebhookUrl must be a valid absolute URL.");
    }
}
