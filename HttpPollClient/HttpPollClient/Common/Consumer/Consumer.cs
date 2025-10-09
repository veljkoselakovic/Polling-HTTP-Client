namespace HttpPollClient.Common
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using HttpPollClient.Extension;

    public class Consumer<T> : IStartable, IDisposable
    {
        private bool _isRunning;
        private List<Task> _internalConsumers;
        private CancellationTokenSource? _consumerCancellationTokenSource;

        private IMessageBroker<T>? _messageBroker;
        private List<Func<T, Task<T>>> _messageProcessorPipeline;

        private SemaphoreSlim _maxConcurrentMessageLimiter;
        private uint _concurrencyLevel;

        private bool _disposed;

        public Consumer()
        {
            _isRunning = false;
            _internalConsumers = new List<Task>();
            _consumerCancellationTokenSource = null;

            _messageBroker = null;
            _messageProcessorPipeline = new List<Func<T, Task<T>>>();

            _maxConcurrentMessageLimiter = new SemaphoreSlim(3);
            _concurrencyLevel = 1;
        }

        public Consumer<T> SetMessageBroker(IMessageBroker<T> messageBroker)
        {
            _messageBroker = messageBroker;
            return this;
        }

        public Consumer<T> SetMaxConcurrentTasks(int maxValue)
        {
            _maxConcurrentMessageLimiter = new SemaphoreSlim(maxValue);
            return this;
        }

        public Consumer<T> SetConcurrencyLevel(uint concurrencyLevel)
        {
            _concurrencyLevel = concurrencyLevel;
            return this;
        }

        public Consumer<T> AddMessageProcessorToPipeline(Func<T, Task<T>> messageProcessor)
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
                Console.Write("Canceled");
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
                T message = await _messageBroker.ConsumeMessageAsync();
                await ProcessMessagePipelineAsync(message);
            }
        }

        private async Task ProcessMessagePipelineAsync(T message)
        {
            foreach (var processor in _messageProcessorPipeline)
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
            Console.WriteLine(reason);
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
