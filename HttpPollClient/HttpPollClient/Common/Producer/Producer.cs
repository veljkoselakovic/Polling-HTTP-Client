namespace HttpPollClient.Common
{
    public class Producer<T>
    {
        private readonly IMessageBroker<T> _messageBroker;

        public Producer(IMessageBroker<T> messageBroker)
        {
            _messageBroker = messageBroker;
        }

        public void ProduceMessage(T message)
        {
            _messageBroker.ProduceMessage(message);
        }
    }
}
