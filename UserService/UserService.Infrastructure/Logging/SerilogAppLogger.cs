using Microsoft.Extensions.Logging;
using UserService.Domain.ILogging;

namespace UserService.Infrastructure.Logging;

public sealed class SerilogAppLogger<T> : IAppLogger<T>
{
    private readonly ILogger<T> _logger;

    public SerilogAppLogger(ILogger<T> logger)
    {
        _logger = logger;
    }

    public void LogInformation(string message, params object[] args)
        => _logger.LogInformation(message, args);

    public void LogWarning(string message, params object[] args)
        => _logger.LogWarning(message, args);

    public void LogError(Exception exception, string message, params object[] args)
        => _logger.LogError(exception, message, args);
}