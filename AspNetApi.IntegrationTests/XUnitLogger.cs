using Xunit.Abstractions;

namespace AspNetApi.Tests
{
    /// <summary>
    /// <see cref="XunitLogger"/> provider.
    /// </summary>
    public class XunitLoggerProvider : ILoggerProvider
    {
        private readonly ITestOutputHelper _output;

        public XunitLoggerProvider(ITestOutputHelper output)
        {
            _output = output;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new XunitLogger(_output);
        }

        public void Dispose() { }
    }

    /// <summary>
    /// Outputs messages logged by <see cref="ILogger"/>> to <see cref="ITestOutputHelper"/>.
    /// </summary>
    public class XunitLogger : ILogger
    {
        private readonly ITestOutputHelper _output;

        public XunitLogger(ITestOutputHelper output)
        {
            _output = output;
        }

        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => true; // Change to filter levels as needed

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (formatter != null)
            {
                var message = formatter(state, exception);
                if (message != null)
                {
                    _output.WriteLine($"[{logLevel}] {message}"); // Format your message as needed
                }
            }
        }
    }

}
