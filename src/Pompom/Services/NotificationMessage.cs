namespace Pompom.Services;

internal sealed record NotificationMessage(
    string Title,
    string Body,
    NotificationSound Sound);
