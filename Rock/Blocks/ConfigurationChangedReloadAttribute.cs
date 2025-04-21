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

using Rock.Enums.Cms;

namespace Rock.Blocks
{
    /// <summary>
    /// Specifies how the block should be reloaded automatically when the settings
    /// have been changed in the UI. If this attribute is not specified then
    /// <see cref="BlockReloadMode.None"/> is assumed.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class )]
    public class ConfigurationChangedReloadAttribute : System.Attribute
    {
        /// <summary>
        /// Gets the reload mode for the block.
        /// </summary>
        /// <value>The reload mode.</value>
        public BlockReloadMode ReloadMode { get; }

        /// <summary>
        /// Initializes a new instance of the ConfigurationChangedReloadAttribute
        /// class with the specified reload mode.
        /// </summary>
        /// <param name="reloadMode">The block reload mode to use.</param>
        public ConfigurationChangedReloadAttribute( BlockReloadMode reloadMode )
        {
            ReloadMode = reloadMode;
        }
    }
}
