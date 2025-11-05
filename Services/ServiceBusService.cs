using Azure.Identity;
using Azure.Messaging.ServiceBus;

namespace LetsServiceBus.Services;

public class ServiceBusService : IServiceBusService
{
    private readonly ServiceBusClient _serviceBusClient;
    private readonly string _queueName;
    private readonly ILogger<ServiceBusService> _logger;

    public ServiceBusService(IConfiguration configuration, ILogger<ServiceBusService> logger)
    {
        _logger = logger;
        
        var serviceBusNamespace = configuration["ServiceBus:Namespace"] 
            ?? throw new InvalidOperationException("ServiceBus:Namespace is not configured");
        
        _queueName = configuration["ServiceBus:QueueName"] 
            ?? throw new InvalidOperationException("ServiceBus:QueueName is not configured");

        // Use DefaultAzureCredential for authentication
        _serviceBusClient = new ServiceBusClient(serviceBusNamespace, new DefaultAzureCredential());
        
        _logger.LogInformation("ServiceBusService initialized for namespace: {Namespace}, queue: {QueueName}", 
            serviceBusNamespace, _queueName);
    }

    public async Task SendMessageAsync(string message)
    {
        try
        {
            var sender = _serviceBusClient.CreateSender(_queueName);
            var serviceBusMessage = new ServiceBusMessage(message);
            
            await sender.SendMessageAsync(serviceBusMessage);
            
            _logger.LogInformation("Message sent to Service Bus queue: {QueueName}", _queueName);
            
            await sender.DisposeAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to Service Bus");
            throw;
        }
    }

    public async Task<List<string>> ReceiveMessagesAsync(int maxMessages = 10)
    {
        var messages = new List<string>();
        
        try
        {
            var receiver = _serviceBusClient.CreateReceiver(_queueName, new ServiceBusReceiverOptions
            {
                ReceiveMode = ServiceBusReceiveMode.PeekLock
            });

            var receivedMessages = await receiver.ReceiveMessagesAsync(maxMessages, TimeSpan.FromSeconds(5));

            foreach (var message in receivedMessages)
            {
                messages.Add(message.Body.ToString());
                
                // Complete the message to remove it from the queue
                await receiver.CompleteMessageAsync(message);
                
                _logger.LogInformation("Message received and completed from queue: {QueueName}", _queueName);
            }

            await receiver.DisposeAsync();
            
            _logger.LogInformation("Retrieved {Count} messages from Service Bus queue: {QueueName}", 
                messages.Count, _queueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error receiving messages from Service Bus");
            throw;
        }

        return messages;
    }
}
