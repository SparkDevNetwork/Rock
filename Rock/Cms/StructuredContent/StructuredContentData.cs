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

namespace Rock.Cms.StructuredContent
{
    /// <summary>
    /// The save data structure for EditorJS.
    /// </summary>
    public class StructuredContentData
    {
        /// <summary>
        /// Gets or sets the time in JavaScript milliseconds that the save data
        /// was generated.
        /// </summary>
        /// <value>
        /// The time in JavaScript milliseconds.
        /// </value>
        public long Time { get; set; }

        /// <summary>
        /// Gets or sets the blocks.
        /// </summary>
        /// <value>
        /// The blocks.
        /// </value>
        public List<StructuredContentBlock> Blocks { get; set; } = new List<StructuredContentBlock>();

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public string Version { get; set; }
    }
}
