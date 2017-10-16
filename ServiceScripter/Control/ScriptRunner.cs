using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Fun;
using Fun.Extensions;
using MoreLinq;
using ServiceScripter.Properties;
using ServiceScripter.Scripts;

namespace ServiceScripter.Control
{
    public class ScriptRunner
    {
        private readonly ServiceRepository _repository;

        private static int MaxAttempts =>
            Settings.Default.MaxRetriesPerAction;

        private static TimeSpan RetryInterval =>
            TimeSpan.FromMilliseconds(Settings.Default.RetryIntervalMilliseconds);

        public ScriptRunner(
            ServiceRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<Unit>> Run(Script script)
        {
            return await ValidateScript(script)
                .MapAsync(async _ =>
                {
                    foreach (var action in script.Actions)
                    {
                        var result = await PerformAction(action);
                        if (!result.HasValue)
                        {
                            return result;
                        }
                    }

                    return Unit.Value.AsResult();
                });
        }

        private Task<Result<Unit>> ValidateScript(Script script)
        {
            return Result.TryAsync(() => _repository.GetServices())
                .MapAsync(services =>
                {
                    var serviceNames = services
                        .Select(s => s.ServiceName)
                        .ToImmutableList();

                    var requestedNames = script.Actions
                        .Select(a => a.ServiceName);

                    var notFound = requestedNames
                        .Except(serviceNames)
                        .ToImmutableList();

                    return Result
                        .Assert(!notFound.Any(),
                            () => new Exception("The following services were not found: " +
                                                $"[{notFound.ToDelimitedString(", ")}]"))
                        .AsTask();
                });
        }

        private async Task<Result<Unit>> PerformAction(ScriptAction action)
        {
            Console.WriteLine($"Attempting to {action.ToString()}");
            switch (action.ActionType)
            {
                case ScriptActionType.Start:
                    return await _repository.RetryStartService(action.ServiceName, MaxAttempts, RetryInterval);
                case ScriptActionType.Stop:
                    return await _repository.RetryStopService(action.ServiceName, MaxAttempts, RetryInterval);
                case ScriptActionType.Reset:
                    return new Exception("Reset actions are not primitive.").AsError<Unit>();
                default:
                    return new Exception("Invalid enum value.").AsError<Unit>();
            }
        }
    }
}
