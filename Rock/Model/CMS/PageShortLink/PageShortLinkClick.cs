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

using Rock.Data;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Rock.Model
{
    /// <summary>
    /// Represents a list of clicks for a particular short link in Rock.
    /// </summary>
    [RockDomain("CMS")]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid("339EA577-0532-4402-A78C-F4D4267AECBF")]
    public class PageShortLinkClickList
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the identifier for the PageShortLink.
        /// </summary>
        [Required]
        [DataMember(IsRequired = true)]
        public int PageShortLinkId { get; set; }

        /// <summary>
        /// Gets or sets the date and time of the interaction.
        /// </summary>
        [Required]
        [DataMember(IsRequired = true)]
        public DateTime InteractionDateTime { get; set; }

        /// <summary>
        /// Gets or sets the identifier for the person.
        /// </summary>
        [DataMember]
        public int? PersonId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.PersonAlias"/> entity that is associated with the person.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.PersonAlias"/> that is associated with the person.
        /// </value>
        [ForeignKey("PersonId")]  // Define the foreign key relationship
        [DataMember]
        public virtual PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the application.
        /// </summary>
        [DataMember]
        public string Application { get; set; }

        /// <summary>
        /// Gets or sets the type of the client.
        /// </summary>
        [DataMember]
        public string ClientType { get; set; }

        /// <summary>
        /// Gets or sets the operating system.
        /// </summary>
        [DataMember]
        public string OperatingSystem { get; set; }

        /// <summary>
        /// Gets or sets the UTM source.
        /// </summary>
        [DataMember]
        public string Source { get; set; }

        #endregion Entity Properties
    }
}
