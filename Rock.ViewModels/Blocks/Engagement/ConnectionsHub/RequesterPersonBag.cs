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

using Rock.Enums.Connection;
using Rock.Model;
using Rock.ViewModels.Blocks.Core.DefinedValueList;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Core.Grid;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Engagement.ConnectionsHub
{
    /// <summary>
    /// Represents the profile data for the person who submitted a connection request, used to populate the requester section of the detail panel.
    /// </summary>
    public class RequesterPersonBag
    {
        /// <summary>
        /// Gets or sets the hashed identifier key.
        /// </summary>
        /// <value>The hashed identifier key.</value>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the nick name.
        /// </summary>
        /// <value>The nick name.</value>
        public string NickName { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        /// <value>The last name.</value>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the photo URL.
        /// </summary>
        /// <value>The photo URL.</value>
        public string PhotoUrl { get; set; }

        /// <summary>
        /// Gets or sets the connection status.
        /// </summary>
        /// <value>The connection status.</value>
        public string ConnectionStatus { get; set; }

        /// <summary>
        /// Gets or sets the age of the requester.
        /// </summary>
        public int? Age { get; set; }

        /// <summary>
        /// Gets or sets the display value of the requester's gender.
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// Gets or sets the display value of the requester's marital status.
        /// </summary>
        public string MaritalStatus { get; set; }
    }
}
