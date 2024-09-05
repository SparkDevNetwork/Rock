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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a topic associated with a campus
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "CampusTopic" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "0FFDCB0B-B435-4E66-9085-2750534E706A" )]
    public class CampusTopic : Model<CampusTopic>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the campus type value identifier.
        /// </summary>
        /// <value>
        /// The topic type value identifier.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.TOPIC_TYPE )]
        public int TopicTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the Email
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing the campus's email address.
        /// </value>
        [MaxLength( 254 )]
        [DataMember]
        [RegularExpression( @"\s*(?:[A-Za-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[A-Za-z0-9!#$%&'*+/=?^_`{|}~-]+)*|""(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*"")@(?:(?:[A-Za-z0-9](?:[A-Za-z0-9-]*[A-Za-z0-9])?\.)+[A-Za-z0-9](?:[A-Za-z0-9-]*[A-Za-z0-9])?|\[(?:(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9]))\.){3}(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9])|[A-Za-z0-9-]*[A-Za-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])\s*", ErrorMessage = "The Email address is invalid" )]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the us public
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> indicating if the topic is public or not
        /// </value>
        [DataMember]
        public bool IsPublic { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Campus"/> associated with this topic.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> that represents the Id of the campus. If none exists, this value is null.
        /// </value>
        public int CampusId { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Campus"/> that is associated with this topic
        /// </summary>
        [DataMember]
        public virtual Campus Campus { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.DefinedValue"/> representing the topic type.
        /// </summary>
        /// <value>
        /// A <see cref="DefinedValue"/> object representing the topic type
        /// </value>
        [DataMember]
        public virtual DefinedValue TopicTypeValue { get; set; }

        #endregion
    }

    /// <summary>
    /// CampusTopic configuration class
    /// </summary>
    public class CampusTopicConfiguration : EntityTypeConfiguration<CampusTopic>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CampusTopicConfiguration"/> class
        /// </summary>
        public CampusTopicConfiguration()
        {
            this.HasRequired( t => t.Campus ).WithMany( t => t.CampusTopics ).HasForeignKey( t => t.CampusId );
            this.HasRequired( t => t.TopicTypeValue ).WithMany().HasForeignKey( t => t.TopicTypeValueId ).WillCascadeOnDelete( false );
        }
    }
}
