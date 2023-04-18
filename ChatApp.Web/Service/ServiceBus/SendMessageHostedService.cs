using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using ChatApp.Web.Configuration;
using ChatApp.Web.Service.Messages;

namespace ChatApp.Web.Service.ServiceBus;

public class SendMessageHostedService : IHostedService
{
    private readonly IMessageService _messageService;
    private readonly IMessageSerializer _messageSerializer;
    private readonly ServiceBusProcessor _processor;

    public SendMessageHostedService(
        ServiceBusClient serviceBusClient, 
        IMessageService messageService,
        IMessageSerializer messageSerializer,
        IOptions<ServiceBusSettings> options)
    {
        _messageService = messageService;
        _messageSerializer = messageSerializer;
        _processor = serviceBusClient.CreateProcessor(options.Value.SendMessageQueueName);
        
        // add handler to process messages
        _processor.ProcessMessageAsync += MessageHandler;

        // add handler to process any errors
        _processor.ProcessErrorAsync += ErrorHandler;
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return _processor.StartProcessingAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return _processor.StopProcessingAsync(cancellationToken);
    }
    
    private async Task MessageHandler(ProcessMessageEventArgs args)
    {
        string data = args.Message.Body.ToString();
        Console.WriteLine($"Received: {data}");

        var message = _messageSerializer.DeserializeMessage(data);
        await _messageService.PostMessageToConversation(message, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

        await args.CompleteMessageAsync(args.Message);
    }

    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        Console.WriteLine(args.Exception.ToString());
        return Task.CompletedTask;
    }
}