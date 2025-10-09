namespace HttpPollClient.HttpClient
{
    using HttpPollClient.Common;

    public sealed class HttpCallProducer : Producer<HttpRequestMessage>
    {
        public HttpCallProducer(IMessageBroker<HttpRequestMessage> messageBroker)
            : base(messageBroker) { }
    }
}