using System.Threading.Tasks;

namespace WirexApp.Infrastructure.Messaging
{
    public interface IMessageBus
    {
        Task PublishAsync<T>(string topic, T message) where T : class;

        Task PublishAsync<T>(string topic, string key, T message) where T : class;
    }
}
