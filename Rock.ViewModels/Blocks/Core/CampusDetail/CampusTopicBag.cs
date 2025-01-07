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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Core.CampusDetail
{
    /// <summary>
    /// Class CampusTopicBag
    /// </summary>
    public class CampusTopicBag
    {
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>The unique identifier.</value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the Campus Topics.
        /// The Campus Topics is a Defined Value.
        /// </summary>
        /// <value>The Campus Topic.</value>
        public ListItemBag Type { get; set; }

        /// <summary>
        /// Gets or sets the Email.
        /// </summary>
        /// <value>The Email.</value>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets if the Campus Topic is Public.
        /// </summary>
        /// <value>The boolean value whether the Campus Topic is Public or not.</value>
        public bool? IsPublic { get; set; }
    }
}