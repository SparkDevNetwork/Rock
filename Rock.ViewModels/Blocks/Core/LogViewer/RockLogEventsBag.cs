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

namespace Rock.ViewModels.Blocks.Core.LogViewer
{
    /// <summary>
    /// Describes the Rock Log Events
    /// </summary>
    public class RockLogEventsBag
    {
        /// <summary>
        /// Gets or sets the DateTime
        /// </summary>
        public DateTimeOffset DateTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Level { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Exception { get; set; }
    }
}
