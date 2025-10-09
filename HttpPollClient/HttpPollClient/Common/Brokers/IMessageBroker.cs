namespace HttpPollClient.Common
{
    public interface IMessageBroker<T>
    {
        public abstract Task<T> ConsumeMessageAsync();
        public abstract bool ProduceMessage(T message);
    }
}
