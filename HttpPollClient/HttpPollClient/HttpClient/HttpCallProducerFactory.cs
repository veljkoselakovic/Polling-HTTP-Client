namespace HttpPollClient.HttpClient
{
    using HttpPollClient.Common.Brokers;

    public sealed class HttpCallProducerFactory
    {
        public static HttpCallProducer CreateDefaultWithChannelBroker()
        {
            return new HttpCallProducer(ChannelBroker<HttpRequestMessage>.Instance);
        }
    }
}
