using System;
using System.Text;
using ConsumerAPI.Contexts;
using ConsumerAPI.Entities;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ConsumerAPI.Services.Consumer;

public class ConsumerService : IConsumerService
{
    readonly string _hostName = "localhost";
    readonly string _queueName = "product-queue";
    readonly IServiceScopeFactory _serviceProvider;
    private IConnection? _connection;
    private IChannel? _channel;

    public ConsumerService(IServiceScopeFactory serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    private async Task InitalizeAsync()
    {
        var factory = new ConnectionFactory() { HostName = _hostName };
        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();
        await _channel.QueueDeclareAsync(queue: _queueName, exclusive: false, autoDelete: false);
    }

    public async Task ConsumeAsync()
    {
        if (_connection is null || _channel is null)
            await InitalizeAsync();
    
        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (sender, ea) =>
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);
            var message = JsonConvert.DeserializeObject<Message>(json);

            await ProcessMessageAsync(message);
        };


        await _channel.BasicConsumeAsync(queue: _queueName, autoAck: true, consumer: consumer);
    }

    private async Task ProcessMessageAsync(Message message)
    {
        switch (message.Action.ToLower())
        {
            case "create":
                await CreateProductAsync(message.Data);
                break;
            case "update":
                await UpdateProductAsync(message.Data);
                break;
            case "delete":
                await DeleteProductAsync(message.Data);
                break;
            default:
                break;
        }
    }

    private async Task CreateProductAsync(Product product)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Products.AddAsync(product);
        await context.SaveChangesAsync();
    }

    private async Task UpdateProductAsync(Product product)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var existingProduct = await context.Products.FindAsync(product.Id);
        if (existingProduct is not null)
        {
            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.Price = product.Price;
            existingProduct.Picture = product.Picture;
            existingProduct.Stock = product.Stock;
            context.Products.Update(existingProduct);
            await context.SaveChangesAsync();
        }
    }

    private async Task DeleteProductAsync(Product product)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var existingProduct = await context.Products.FindAsync(product.Id);
        if (existingProduct is not null)
        {
            context.Products.Remove(existingProduct);
            await context.SaveChangesAsync();
        }
    }
}

public class Message
{
    public string Action { get; set; }
    public Product Data { get; set; }
}
