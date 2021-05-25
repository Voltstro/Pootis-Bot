using System;
using Quartz.Logging;

namespace Pootis_Bot.Jobs
{
    internal class QuartzLogger : ILogProvider
    {
        public Logger GetLogger(string name)
        {
            return (level, func, exception, parameters) =>
            {
                if (func != null)
                {
                    if(level == LogLevel.Info)
                        Logging.Logger.Info($"Quartz: {func()}", parameters);
                }
                return true;
            }; 
        }

        public IDisposable OpenNestedContext(string message)
        {
            throw new NotImplementedException();
        }

        public IDisposable OpenMappedContext(string key, object value, bool destructure = false)
        {
            throw new NotImplementedException();
        }
    }
}