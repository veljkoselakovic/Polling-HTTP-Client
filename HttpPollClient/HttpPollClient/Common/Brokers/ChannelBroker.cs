using System.Threading.Channels;

namespace HttpPollClient.Common.Brokers
{
    public sealed class ChannelBroker<T> : IMessageBroker<T>
    {
        public static ChannelBroker<T> Instance { get; } = new ChannelBroker<T>();
        private readonly Channel<T> _channel;
        private readonly ChannelWriter<T> _writer;
        private readonly ChannelReader<T> _reader;

        private ChannelBroker()
        {
            var options = new UnboundedChannelOptions
            {
                SingleReader = false,
                SingleWriter = false,
            };
            _channel = Channel.CreateUnbounded<T>(options);
            _writer = _channel.Writer;
            _reader = _channel.Reader;
        }

        public async Task<T> ConsumeMessageAsync()
        {
            return await _reader.ReadAsync();
        }

        public bool ProduceMessage(T message)
        {
            return _writer.TryWrite(message);
        }
    }
}
