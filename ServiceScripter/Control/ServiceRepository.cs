using System;
using System.Collections.Immutable;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using Fun;
using Fun.Extensions;

namespace ServiceScripter.Control
{
    public class ServiceRepository
    {
        public Task<Result<ImmutableList<ServiceController>>> GetServices()
        {
            return Result.TryAsync(() =>
                ServiceController.GetServices()
                    .ToImmutableList()
                    .AsTask());
        }

        public Task<Result<ServiceController>> GetService(string name)
        {
            return Result.TryAsync(() =>
                ServiceController.GetServices()
                    .Single(s => s.ServiceName == name)
                    .AsTask());
        }

        public Task<Result<Unit>> RetryStartService(string name,
            int maxAttempts, TimeSpan retryInterval)
        {
            const string alreadyRunning = "An instance of the service is already running";

            return Retry.GetAsync(
                    getValue: () => GetService(name)
                        .MapAsync(s =>
                        {
                            if (s.Status == ServiceControllerStatus.Running)
                            {
                                return true;
                            }
                            else
                            {
                                s.Start();
                                return false;
                            }
                        }),
                    predicate: result =>
                        (result.HasValue && result.Value)
                        || (!result.HasValue && result.Error.InnerException.Message == alreadyRunning),
                    getErrorMessage: result => result.HasValue 
                        ? "Timed out without getting an acceptable value."
                        : "Timed out with exception.",
                    maxAttempts: maxAttempts,
                    interval: retryInterval)
                .IgnoreAsync()
                .CatchAsync(er => er.InnerException.Message == alreadyRunning,
                    er => Unit.Value.AsResult());
        }

        public Task<Result<Unit>> RetryStopService(string name,
            int maxAttempts, TimeSpan retryInterval)
        {
            return Retry.GetAsync(
                    getValue: () => GetService(name)
                        .MapAsync(s =>
                        {
                            if (s.Status == ServiceControllerStatus.Stopped)
                            {
                                return true;
                            }
                            else
                            {
                                s.Stop();
                                return false;
                            }
                        }),
                    predicate: result => result.HasValue && result.Value,
                    getErrorMessage: result => result.HasValue
                        ? "Timed out without getting an acceptable value."
                        : "Timed out with exception.",
                    maxAttempts: maxAttempts,
                    interval: retryInterval)
                .IgnoreAsync();
        }
    }
}
