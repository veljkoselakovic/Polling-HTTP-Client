namespace HttpPollClient.Common
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using HttpPollClient.Common.Metrics;
    using HttpPollClient.Extension;

    public class Consumer<TMessage> : IStartable, IDisposable
    {
        private readonly List<Task> _internalConsumers;

        private readonly IMessageBroker<TMessage>? _messageBroker;
        private readonly List<Func<TMessage, Task<TMessage>>> _messageProcessorPipeline;

        private readonly SemaphoreSlim _maxConcurrentMessageLimiter;
        private readonly uint _concurrencyLevel;

        private readonly IMetricsTracker? _metricsTracker;

        private bool _isRunning;
        private CancellationTokenSource? _consumerCancellationTokenSource;
        private bool _disposed;

        public Consumer(IMessageBroker<TMessage>? messageBroker,
                        List<Func<TMessage, Task<TMessage>>> messageProcessorPipeline,
                        SemaphoreSlim maxConcurrentMessageLimiter,
                        uint concurrencyLevel,
                        IMetricsTracker? metricsTracker)
        {
            _isRunning = false;
            _internalConsumers = [];
            _disposed = false;

            _messageBroker = messageBroker;
            _messageProcessorPipeline = messageProcessorPipeline;
            _maxConcurrentMessageLimiter = maxConcurrentMessageLimiter;
            _concurrencyLevel = concurrencyLevel;
            _metricsTracker = metricsTracker;
        }

        public Consumer()
        {
        }

        public void Start()
        {
            _isRunning = true;
            _consumerCancellationTokenSource = new CancellationTokenSource();
            for (int i = 0; i < _concurrencyLevel; i++)
            {
                _internalConsumers.Add(RunAsync());
            }
        }

        private async Task RunAsync()
        {
            if (_consumerCancellationTokenSource == null)
            {
                throw new InvalidOperationException("CancellationTokenSource is not initialized.");
            }
            try
            {
                while (_isRunning)
                {
                    _consumerCancellationTokenSource.Token.ThrowIfCancellationRequested();
                    await ConsumeAndProcessMessageAsync();
                }
            }
            catch (OperationCanceledException)
            {
                _metricsTracker?.Log("Canceled");
            }
        }

        private async Task ConsumeAndProcessMessageAsync()
        {
            if (_messageBroker == null)
            {
                throw new InvalidOperationException("MessageBroker is not initialized.");
            }

            using (await _maxConcurrentMessageLimiter.WaitAsyncWithAutoReleaser())
            {
                TMessage message = await _messageBroker.ConsumeMessageAsync();
                await ProcessMessagePipelineAsync(message);
            }
        }

        private async Task ProcessMessagePipelineAsync(TMessage message)
        {
            foreach (Func<TMessage, Task<TMessage>> processor in _messageProcessorPipeline)
            {
                message = await processor(message);
            }
        }

        public void Stop()
        {
            _isRunning = false;
            _internalConsumers.Clear();
        }

        /**
         * Force stops and keeps _internalConsumers + _isRunning in their states
         */
        public void ForceStop(string reason)
        {
            _consumerCancellationTokenSource?.Cancel();
            _metricsTracker?.Log($"Consumer force stopped: {reason}");
            // Log reason
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects)
                    _consumerCancellationTokenSource?.Dispose();
                    _maxConcurrentMessageLimiter?.Dispose();
                    _internalConsumers.Clear();
                }

                // Free unmanaged resources (unmanaged objects) and override finalizer
                // Set large fields to null
                _disposed = true;
            }
        }

        ~Consumer()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
