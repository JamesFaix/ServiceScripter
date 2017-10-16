namespace ServiceScripter.Scripts
{
    public class ScriptAction
    {
        public string ServiceName { get; }

        public ScriptActionType ActionType { get; }

        public ScriptAction(ScriptActionType type, string serviceName)
        {
            ActionType = type;
            ServiceName = serviceName;
        }

        public override string ToString() =>
            $"{ActionType} {ServiceName}";
    }
}
