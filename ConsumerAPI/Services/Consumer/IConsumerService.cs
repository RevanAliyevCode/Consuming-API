using System;

namespace ConsumerAPI.Services.Consumer;

public interface IConsumerService
{
    Task ConsumeAsync();
}
