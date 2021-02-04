using System;
using System.Collections.Generic;
using System.Text;

namespace Rock.Lava
{
    public static class Constants
    {
        public static class Messages
        {
            public const string NotAuthorizedMessage = "The Lava command '{0}' is not configured for this template.";
        }

        public static class ContextKeys
        {
            //public const string SourceTemplateText = "SourceText";
            public const string SourceTemplateElements = "SourceElements";
            public const string SourceTemplateStatements = "Statements";

        }
    }
}
