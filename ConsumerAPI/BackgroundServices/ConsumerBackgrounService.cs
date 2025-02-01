using System;
using ConsumerAPI.Services.Consumer;

namespace ConsumerAPI.BackgroundServices;

public class ConsumerBackgrounService : BackgroundService
{
    readonly IConsumerService _consumerService;

    public ConsumerBackgrounService(IConsumerService consumerService)
    {
        _consumerService = consumerService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _consumerService.ConsumeAsync();
            await Task.Delay(500, stoppingToken);
        }
    }
}
