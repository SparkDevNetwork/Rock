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
using System;
using System.Collections.Generic;

namespace Rock.Lava
{
    /// <summary>
    /// Defines the properties of a Lava Dynamic Shortcode.
    /// A dynamic shortcode creates an element in a Lava document from a parameterized template that is defined at runtime.
    /// </summary>
    public class DynamicShortcodeDefinition
    {
        #region Constructors

        public DynamicShortcodeDefinition()
        {
            this.Parameters = new Dictionary<string, string>();
            this.EnabledLavaCommands = new List<string>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// The name of the shortcode.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The markup that contains the template that will be inserted in place of the shortcode tag during the rendering process.
        /// </summary>
        public string TemplateMarkup { get; set; }

        /// <summary>
        /// The set of parameter names and values that are injected into the shortcode template.
        /// </summary>
        public Dictionary<string, string> Parameters { get; set; }

        /// <summary>
        /// The set of Lava Commands specifically enabled for this shortcode.
        /// </summary>
        public List<string> EnabledLavaCommands { get; set; }

        /// <summary>
        /// The type of Lava document element that this shortcode is substituted for, either an inline tag or a block element.
        /// </summary>
        public LavaShortcodeTypeSpecifier ElementType { get; set; }

        #endregion
    }
}