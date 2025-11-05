namespace LetsServiceBus.Services;

public interface IServiceBusService
{
    Task SendMessageAsync(string message);
    Task<List<string>> ReceiveMessagesAsync(int maxMessages = 10);
}
