using System.Collections.Generic;
using System.Collections.Immutable;

namespace ServiceScripter.Scripts
{
    public class Script
    {
        public IImmutableList<ScriptAction> Actions { get; }

        public Script(IEnumerable<ScriptAction> actions)
        {
            Actions = actions.ToImmutableList();
        }
    }
}
