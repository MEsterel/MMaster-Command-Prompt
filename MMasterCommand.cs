using System;

namespace MMaster
{
    [AttributeUsage(AttributeTargets.Method)]
    public class MMasterCommand : Attribute
    {
        public readonly bool RequireAdminRights;
        public readonly string HelpPrompt;

        public MMasterCommand(string helpPrompt = "", bool requireAdminRights = false)
        {
            this.RequireAdminRights = requireAdminRights;
            this.HelpPrompt = helpPrompt;
        }
    }
}