// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

namespace Rock.Lava
{
    /// <summary>
    /// Constants defined for the Lava library.
    /// </summary>
    public static class Constants
    {
        public static class Messages
        {
            public const string NotAuthorizedMessage = "The Lava command '{0}' is not configured for this template.";
        }

        public static class ContextKeys
        {
            public const string LavaEngineInstance = "LavaEngineInstance";
            public const string SourceTemplateElements = "SourceElements";
            public const string SourceTemplateStatements = "Statements";
        }

        // A suffix that is added to shortcode elements to avoid naming collisions with other tags and blocks.
        // Note that a suffix is used because the closing tag of a Liquid language element requires the "end" prefix.
        // Also, the suffix must match a regular expression word character, either A to Z or "_" to be compatible with the DotLiquid engine parser.
        public static string ShortcodeInternalNameSuffix = "_";
    }
}