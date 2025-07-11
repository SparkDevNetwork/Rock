﻿// <copyright>
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

using System.Collections.Generic;

namespace Rock.ViewModels.Controls
{
    /// <summary>
    /// Describes the settings that will be used to initialize an Obsidian
    /// UI component. This is used by various parts of Rock to allow an
    /// Obsidian UI component to handle some aspect of a C# component when
    /// it needs to render a custom UI.
    /// </summary>
    public class DynamicComponentDefinitionBag
    {
        /// <summary>
        /// The URL of an obsidian component to display.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The static options that will be sent to the component to help it render
        /// correctly. These will not be sent back to the server.
        /// </summary>
        public Dictionary<string, string> Options { get; set; }

        /// <summary>
        /// The security grant that will be provided to the component. This
        /// will allow any UI controls to make API requests that require
        /// additional permissions. These are not automatically renewed so it
        /// is suggested that you create the token with an expiration of atleast
        /// a few hours to cover then walking away while editing something.
        /// </summary>
        public string SecurityGrantToken { get; set; }
    }
}
