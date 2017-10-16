using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fun;
using Newtonsoft.Json.Linq;
using ServiceScripter.Extensions;
using ServiceScripter.Scripts;
using static System.Console;

namespace ServiceScripter.Control
{
    public class ScriptParser
    {
        private static int MaxScriptLength => Properties.Settings.Default.MaxScriptLength;

        public ScriptParser()
        {
        }

        public Result<Script> Parse(string json)
        {
            return Result.Try(() =>
                {
#if DEBUG
                    WriteLine();
                    WriteLine("Input JSON:");
                    WriteLine();
                    WriteLine(json);
                    WriteLine();
#endif
                    return json;
                })
                .Map(ParseJson)
                .Assert(script => script.Actions.Count <= MaxScriptLength,
                    () => new Exception($"Scripts cannot have more than {MaxScriptLength} actions."))
                .Do(script =>
                {
                    WriteLine("Parsed script:");
                    WriteLine();
                    WriteLine(DisplayScript(script));
                })
                .Map(ExpandResets)
                .Do(script =>
                {
#if DEBUG
                    WriteLine("Expanded script:");
                    WriteLine();
                    WriteLine(DisplayScript(script));
#endif
                });
        }

        private Result<Script> ParseJson(string json)
        {
            return Result.Try(() =>
            {
                var script = JObject.Parse(json);

                var actions = script.Property("Actions").Value as JArray;

                var mapActions = actions.AsJEnumerable()
                    .Cast<JObject>()
                    .Select(j =>
                    {
                        var actionType = j.Property("ActionType").Value.ToString().ParseEnum<ScriptActionType>();
                        var serviceName = j.Property("ServiceName").Value.ToString();

                        return new ScriptAction(actionType, serviceName);
                    });

                return new Script(mapActions);
            });
        }

        private Script ExpandResets(Script script)
        {
            var actions = new List<ScriptAction>(script.Actions.Count);

            foreach (var a in script.Actions)
            {
                if (a.ActionType == ScriptActionType.Reset)
                {
                    actions.Add(new ScriptAction(ScriptActionType.Stop, a.ServiceName));
                    actions.Add(new ScriptAction(ScriptActionType.Start, a.ServiceName));
                }
                else
                {
                    actions.Add(a);
                }
            }

            return new Script(actions);
        }

        private string DisplayScript(Script script)
        {
            var sb = new StringBuilder();
            foreach (var a in script.Actions)
            {
                var type = a.ActionType.ToString().PadRight(10);
                sb.AppendLine($"{type}{a.ServiceName}");
            }
            return sb.ToString();
        }
    }
}
