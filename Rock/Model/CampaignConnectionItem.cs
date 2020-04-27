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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Rock
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CampaignItem"/> class.
    /// </summary>
    public class CampaignItem
    {
        /// <summary>
        /// Gets or sets the Guid of the campaign connection. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Guid"/> representing the Guid of the campaign connection.
        /// </value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this campaign connection is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this campaign connection is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the entity set identifier.
        /// </summary>
        /// <value>
        /// The entity set identifier.
        /// </value>
        public int EntitySetId { get; set; }

        /// <summary>
        /// Gets or sets the connection type identifier.
        /// </summary>
        /// <value>
        /// The connection type identifier.
        /// </value>
        public Guid ConnectionTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the opportunity identifier.
        /// </summary>
        /// <value>
        /// The opportunity identifier.
        /// </value>
        public Guid OpportunityGuid { get; set; }

        /// <summary>
        /// Gets or sets the request comments lava template.
        /// </summary>
        /// <value>
        /// The the request comments lava template.
        /// </value>
        public string RequestCommentsLavaTemplate { get; set; }

        /// <summary>
        /// Gets or sets the dataview identifier.
        /// </summary>
        /// <value>
        /// The dataview identifier.
        /// </value>
        public Guid DataViewGuid { get; set; }

        /// <summary>
        /// Gets or sets the optout Group identifier.
        /// </summary>
        /// <value>
        /// The optout Group identifier.
        /// </value>
        public Guid? OptOutGroupGuid { get; set; }

        /// <summary>
        /// Gets or sets the family limits
        /// </summary>
        /// <value>
        /// The Family Limits
        /// </value>
        public FamilyLimits FamilyLimits { get; set; }

        /// <summary>
        /// Gets or sets the option for creating connection requests.
        /// </summary>
        /// <value>
        /// The create connection request option.
        /// </value>
        public CreateConnectionRequestOptions CreateConnectionRequestOption { get; set; }

        /// <summary>
        /// Gets or sets the daily limit assigned
        /// </summary>
        /// <value>
        /// The daily limit assigned
        /// </value>
        public int? DailyLimitAssigned { get; set; }

        /// <summary>
        /// Gets or sets the days between connection
        /// </summary>
        /// <value>
        /// The days between connection
        /// </value>
        public int DaysBetweenConnection { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the previous connection should be preffered.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the previous connection should be preffered; otherwise, <c>false</c>.
        /// </value>
        public bool PreferPreviousConnector { get; set; }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name ?? base.ToString();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [JsonConverter( typeof( StringEnumConverter ) )]
    public enum FamilyLimits
    {
        /// <summary>
        /// Head Of House
        /// </summary>
        [Description( "Limit to Head of House" )]
        HeadOfHouse,

        /// <summary>
        /// Everyone in Data View
        /// </summary>
        [Description( "Everyone in Data View" )]
        Everyone
    }

    /// <summary>
    /// 
    /// </summary>
    [JsonConverter( typeof( StringEnumConverter ) )]
    public enum CreateConnectionRequestOptions
    {
        /// <summary>
        /// As Needed
        /// </summary>
        [Description( "As Needed" )]
        AsNeeded = 0,

        /// <summary>
        /// All at Once
        /// </summary>
        [Description( "All at Once" )]
        AllAtOnce = 1,
    }
}
