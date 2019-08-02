using System;

namespace MMaster
{
    [AttributeUsage(AttributeTargets.Method)]
    public class MMasterCommand : Attribute
    {
        public string HelpPrompt { get; }
        public string CallName { get; }

        public MMasterCommand(string helpPrompt = "", string callName = "")
        {
            this.HelpPrompt = helpPrompt;
            this.CallName = callName;
        }
    }
}