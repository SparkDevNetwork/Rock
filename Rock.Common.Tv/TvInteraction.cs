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

namespace Rock.Common.Tv
{
    /// <summary>
    /// Defines some interaction the user made with a part of the system. You
    /// can either specify the AppId+Pageguid or ChannelGuid+ComponentName, but
    /// you cannot mix and match.
    /// </summary>
    public class TvInteraction
    {
        /// <summary>
        /// Gets or sets the GUID.
        /// </summary>
        /// <value>The GUID.</value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the app identifier.
        /// </summary>
        /// <value>The app identifier.</value>
        public int? AppId { get; set; }

        /// <summary>
        /// Gets or sets the page GUID.
        /// </summary>
        /// <value>The page GUID.</value>
        public Guid? PageGuid { get; set; }

        /// <summary>
        /// Gets or sets the channel identifier.
        /// </summary>
        /// <value>The channel identifier.</value>
        public int? ChannelId { get; set; }

        /// <summary>
        /// Guid of the channel
        /// </summary>
        public Guid? ChannelGuid { get; set; }

        /// <summary>
        /// Gets or sets the name of the component.
        /// </summary>
        /// <value>The name of the component.</value>
        public string ComponentName { get; set; }

        /// <summary>
        /// Gets or sets the component identifier.
        /// </summary>
        /// <value>
        /// The component identifier.
        /// </value>
        public int? ComponentId { get; set; }

        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        /// <value>
        /// The entity identifier.
        /// </value>
        public int? EntityId { get; set; }

        /// <summary>
        /// Gets or sets the date time.
        /// </summary>
        /// <value>The date time.</value>
        public DateTimeOffset DateTime { get; set; }

        /// <summary>
        /// Gets or sets the operation.
        /// </summary>
        /// <value>The operation.</value>
        public string Operation { get; set; }

        /// <summary>
        /// Gets or sets the summary.
        /// </summary>
        /// <value>The summary.</value>
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>The data.</value>
        public string Data { get; set; }

        /// <summary>
        /// Gets or sets the component entity identifier.
        /// </summary>
        /// <value>
        /// The component entity identifier.
        /// </value>
        public int? ComponentEntityId { get; set; }
        /// <summary>
        /// Gets or sets the related entity type identifier.
        /// </summary>
        /// <value>
        /// The related entity type identifier.
        /// </value>
        public int? RelatedEntityTypeId { get; set; }
        /// <summary>
        /// Gets or sets the related entity identifier.
        /// </summary>
        /// <value>
        /// The related entity identifier.
        /// </value>
        public int? RelatedEntityId { get; set; }
        /// <summary>
        /// Gets or sets the channel custom1.
        /// </summary>
        /// <value>
        /// The channel custom1.
        /// </value>
        public string ChannelCustom1 { get; set; }
        /// <summary>
        /// Gets or sets the channel custom2.
        /// </summary>
        /// <value>
        /// The channel custom2.
        /// </value>
        public string ChannelCustom2 { get; set; }
        /// <summary>
        /// Gets or sets the channel custom indexed1.
        /// </summary>
        /// <value>
        /// The channel custom indexed1.
        /// </value>
        public string ChannelCustomIndexed1 { get; set; }

        /// <summary>
        /// Gets or sets the interaction time to serve.
        /// </summary>
        /// <value>
        /// The interaction time to serve.
        /// </value>
        public double InteractionTimeToServe { get; set; }
    }
}
