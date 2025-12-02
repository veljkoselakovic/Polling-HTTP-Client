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
        private bool _isRunning;
        private readonly List<Task> _internalConsumers;
        private CancellationTokenSource? _consumerCancellationTokenSource;

        private IMessageBroker<TMessage>? _messageBroker;
        private readonly List<Func<TMessage, Task<TMessage>>> _messageProcessorPipeline;

        private SemaphoreSlim _maxConcurrentMessageLimiter;
        private uint _concurrencyLevel;

        private IMetricsTracker? _metricsTracker;

        private bool _disposed;

        public Consumer()
        {
            _isRunning = false;
            _internalConsumers = [];
            _consumerCancellationTokenSource = null;

            _messageBroker = null;
            _messageProcessorPipeline = [];

            _maxConcurrentMessageLimiter = new SemaphoreSlim(3);
            _concurrencyLevel = 1;
        }

        public Consumer<TMessage> SetMessageBroker(IMessageBroker<TMessage> messageBroker)
        {
            _messageBroker = messageBroker;
            return this;
        }

        public Consumer<TMessage> SetMaxConcurrentTasks(int maxValue)
        {
            _maxConcurrentMessageLimiter = new SemaphoreSlim(maxValue);
            return this;
        }

        public Consumer<TMessage> SetConcurrencyLevel(uint concurrencyLevel)
        {
            _concurrencyLevel = concurrencyLevel;
            return this;
        }

        public Consumer<TMessage> SetMetricsTracker(IMetricsTracker metricsTracker)
        {
            _metricsTracker = metricsTracker;
            return this;
        }

        public Consumer<TMessage> AddMessageProcessorToPipeline(Func<TMessage, Task<TMessage>> messageProcessor)
        {
            _messageProcessorPipeline.Add(messageProcessor);
            return this;
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
