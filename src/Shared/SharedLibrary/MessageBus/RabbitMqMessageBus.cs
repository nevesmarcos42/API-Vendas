// Implementação do MessageBus utilizando RabbitMQ
// Fornece funcionalidades de publicação e consumo de mensagens

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SharedLibrary.Interfaces;
using System.Text;
using System.Text.Json;

namespace SharedLibrary.MessageBus
{
    public class RabbitMqMessageBus : IMessageBus, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger<RabbitMqMessageBus> _logger;
        private readonly string _connectionString;

        public RabbitMqMessageBus(IConfiguration configuration, ILogger<RabbitMqMessageBus> logger)
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("RabbitMQ") ?? "amqp://localhost";

            try
            {
                var factory = new ConnectionFactory
                {
                    Uri = new Uri(_connectionString),
                    ClientProvidedName = "MicroservicesVendas"
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _logger.LogInformation("Conexão com RabbitMQ estabelecida com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao conectar com RabbitMQ: {ConnectionString}", _connectionString);
                throw;
            }
        }

        // Publica uma mensagem em uma fila específica
        public async Task PublishAsync<T>(string queueName, T message) where T : class
        {
            try
            {
                // Declara a fila caso não exista
                _channel.QueueDeclare(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                // Serializa a mensagem para JSON
                var messageBody = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(messageBody);

                // Configura propriedades da mensagem para persistência
                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true;
                properties.MessageId = Guid.NewGuid().ToString();
                properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                // Publica a mensagem
                _channel.BasicPublish(
                    exchange: "",
                    routingKey: queueName,
                    basicProperties: properties,
                    body: body);

                _logger.LogInformation("Mensagem publicada na fila {QueueName}: {MessageType}", 
                    queueName, typeof(T).Name);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao publicar mensagem na fila {QueueName}", queueName);
                throw;
            }
        }

        // Configura um consumidor para uma fila específica
        public async Task SubscribeAsync<T>(string queueName, Func<T, Task> handler) where T : class
        {
            try
            {
                // Declara a fila caso não exista
                _channel.QueueDeclare(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                // Configura QoS para processar uma mensagem por vez
                _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                // Cria o consumidor
                var consumer = new EventingBasicConsumer(_channel);
                
                consumer.Received += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var messageContent = Encoding.UTF8.GetString(body);

                    try
                    {
                        // Deserializa e processa a mensagem
                        var message = JsonSerializer.Deserialize<T>(messageContent);
                        if (message != null)
                        {
                            await handler(message);
                            
                            // Confirma o processamento da mensagem
                            _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                            
                            _logger.LogInformation("Mensagem processada com sucesso da fila {QueueName}: {MessageType}", 
                                queueName, typeof(T).Name);
                        }
                        else
                        {
                            _logger.LogWarning("Mensagem nula recebida da fila {QueueName}", queueName);
                            _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao processar mensagem da fila {QueueName}: {MessageContent}", 
                            queueName, messageContent);
                        
                        // Rejeita a mensagem e a move para dead letter queue se configurada
                        _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                    }
                };

                // Inicia o consumo
                _channel.BasicConsume(
                    queue: queueName,
                    autoAck: false,
                    consumer: consumer);

                _logger.LogInformation("Consumidor configurado para a fila {QueueName}: {MessageType}", 
                    queueName, typeof(T).Name);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao configurar consumidor para a fila {QueueName}", queueName);
                throw;
            }
        }

        // Encerra conexões e libera recursos
        public async Task DisconnectAsync()
        {
            try
            {
                _channel?.Close();
                _connection?.Close();
                
                _logger.LogInformation("Conexão com RabbitMQ encerrada");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao encerrar conexão com RabbitMQ");
                throw;
            }
        }

        // Implementação do IDisposable
        public void Dispose()
        {
            try
            {
                _channel?.Dispose();
                _connection?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao liberar recursos do RabbitMQ");
            }
        }
    }
}