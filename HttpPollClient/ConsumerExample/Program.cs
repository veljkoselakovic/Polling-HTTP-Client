using HttpPollClient.Common;
using HttpPollClient.HttpClient;

async static Task SendLoopAsync(HttpCallProducer producer, HttpRequestMessage message)
{
    for (int i = 0; i < 3; i++)
    {
        producer.ProduceMessage(message);
        await Task.Delay(1000);
    }
}

HttpCallConsumer consumer = HttpCallConsumerFactory.CreateDefaultWithChannelBroker();
List<HttpCallProducer> producers = new List<HttpCallProducer>();

for (int i = 0; i < 10; i++)
{
    producers.Add(HttpCallProducerFactory.CreateDefaultWithChannelBroker());
}

producers.ForEach(producer =>
{
    _ = SendLoopAsync(producer, new HttpRequestMessage(HttpMethod.Get, "https://example.com"));
});

consumer.Start();

await Task.Delay(20000);

consumer.Stop();
await Task.Delay(4000);