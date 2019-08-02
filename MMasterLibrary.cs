using System;

namespace MMaster
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MMasterLibrary : Attribute
    {
        public MMasterLibrary(string helpPrompt = "", string callName = "")
        {
            HelpPrompt = helpPrompt;
            CallName = callName;
        }

        public string HelpPrompt { get; }
        public string CallName { get; }
    }
}