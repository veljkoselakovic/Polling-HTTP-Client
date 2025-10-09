using HttpPollClient.Common.Brokers;

namespace HttpPollClient.Common
{
    public sealed class ProducerFactory
    {
        public static Producer<T> CreateProducer<T>(IMessageBroker<T> messageBroker)
        {
            Producer<T> producer = new Producer<T>(messageBroker);
            return producer;
        }

        public static Producer<string> CreateProducerWithConcurrentQueueBroker()
        {
            IMessageBroker<string> messageBroker = ChannelBroker<string>.Instance;
            Producer<string> producer = new Producer<string>(messageBroker);
            return producer;
        }
    }
}
