// Interface para abstração do sistema de mensageria
// Permite trocar a implementação do RabbitMQ sem afetar o código dos serviços

namespace SharedLibrary.Interfaces
{
    // Contrato principal para publicação e consumo de mensagens
    public interface IMessageBus
    {
        // Publica uma mensagem em uma fila específica
        Task PublishAsync<T>(string queueName, T message) where T : class;
        
        // Configura um consumidor para uma fila específica
        Task SubscribeAsync<T>(string queueName, Func<T, Task> handler) where T : class;
        
        // Encerra conexões e libera recursos
        Task DisconnectAsync();
    }

    // Interface para serviços que precisam processar mensagens
    public interface IMessageHandler<T> where T : class
    {
        Task HandleAsync(T message);
    }
}