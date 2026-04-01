using Application.Interfaces.Logging;
using Serilog;
namespace Application.Services.Logging;

public class LoggerManager : ILoggerManager
{
    public void LogInfo(string messageTemplate, params object[] propertyValues)
    {
        Log.Information(messageTemplate, propertyValues);
    }

    public void LogWarn(string messageTemplate, params object[] propertyValues)
    {
        Log.Warning(messageTemplate, propertyValues);
    }

    public void LogDebug(string messageTemplate, params object[] propertyValues)
    {
        Log.Debug(messageTemplate, propertyValues);
    }

    public void LogError(string messageTemplate, params object[] propertyValues)
    {
        Log.Error(messageTemplate, propertyValues);
    }
}