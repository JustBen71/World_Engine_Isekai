using System.Text;

namespace Isekai.Engine.Diagnostics;

/// <summary>
/// Writes nested engine traces to a text file such as debug.log.
/// </summary>
public sealed class FileTraceLogger : ITraceLogger, IDisposable
{
    private readonly object _syncRoot = new();
    private readonly StreamWriter _writer;
    private int _depth;
    private bool _isDisposed;

    /// <summary>
    /// Creates a file trace logger.
    /// </summary>
    public FileTraceLogger(string filePath = "debug.log", int level = EngineLogLevels.FullTrace, bool append = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var directory = Path.GetDirectoryName(Path.GetFullPath(filePath));
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        Level = level;
        _writer = new StreamWriter(filePath, append, Encoding.UTF8)
        {
            AutoFlush = true
        };
    }

    /// <inheritdoc />
    public int Level { get; }

    /// <inheritdoc />
    public bool IsEnabled(int level) => Level >= level;

    /// <inheritdoc />
    public IDisposable BeginScope(string name, int level = EngineLogLevels.FullTrace)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (!IsEnabled(level))
        {
            return EmptyScope.Instance;
        }

        lock (_syncRoot)
        {
            ThrowIfDisposed();
            WriteLine($"{name} => {{");
            _depth++;
        }

        return new TraceScope(this, name);
    }

    /// <inheritdoc />
    public void Write(string message, int level = EngineLogLevels.FullTrace)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (!IsEnabled(level))
        {
            return;
        }

        lock (_syncRoot)
        {
            ThrowIfDisposed();
            WriteLine(message);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        lock (_syncRoot)
        {
            if (_isDisposed)
            {
                return;
            }

            _writer.Dispose();
            _isDisposed = true;
        }
    }

    private void EndScope(string name)
    {
        lock (_syncRoot)
        {
            ThrowIfDisposed();
            _depth = Math.Max(0, _depth - 1);
            WriteLine($"}} <= {name}");
        }
    }

    private void WriteLine(string line)
    {
        _writer.WriteLine($"{new string(' ', _depth * 2)}{line}");
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
    }

    /// <summary>
    /// Closes one trace block when disposed.
    /// </summary>
    private sealed class TraceScope : IDisposable
    {
        private readonly FileTraceLogger _logger;
        private readonly string _name;
        private bool _isDisposed;

        public TraceScope(FileTraceLogger logger, string name)
        {
            _logger = logger;
            _name = name;
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _logger.EndScope(_name);
            _isDisposed = true;
        }
    }

    /// <summary>
    /// Disposable scope used when a log level is disabled.
    /// </summary>
    private sealed class EmptyScope : IDisposable
    {
        public static EmptyScope Instance { get; } = new();

        public void Dispose()
        {
        }
    }
}
