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
using Rock.Lava;

namespace Rock.Model
{
    /// <summary>
    /// Represents the linkage between event campus, registration instance, and group.
    /// </summary>
    [RockDomain( "Event" )]
    [Table( "EventItemOccurrenceGroupMap" )]
    [DataContract]
    [CodeGenerateRest( DisableEntitySecurity = true )]
    [Rock.SystemGuid.EntityTypeGuid( "1479D2B7-65C0-4E98-9E70-0848422FA00C")]
    public partial class EventItemOccurrenceGroupMap : Model<EventItemOccurrenceGroupMap>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.EventItemOccurrence" /> identifier.
        /// </summary>
        /// <value>
        /// The event item occurrence identifier.
        /// </value>
        [DataMember]
        public int? EventItemOccurrenceId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.RegistrationInstance" /> identifier.
        /// </summary>
        /// <value>
        /// The registration instance identifier.
        /// </value>
        [DataMember]
        public int? RegistrationInstanceId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Group" /> identifier.
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        [DataMember]
        [FieldType( Rock.SystemGuid.FieldType.GROUP )]
        public int? GroupId { get; set; }


        /// <summary>
        /// Gets or sets the name of the public.
        /// </summary>
        /// <value>
        /// The name of the public.
        /// </value>
        [MaxLength(200)]
        [DataMember]
        public string PublicName { get; set; }

        /// <summary>
        /// Gets or sets the URL slug.
        /// </summary>
        /// <value>
        /// The URL slug.
        /// </value>
        [MaxLength( 200 )]
        [DataMember]
        public string UrlSlug { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Campus"/> the event will be tied to.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Campus"/> the event occured
        /// </value>
        [DataMember]
        [FieldType(Rock.SystemGuid.FieldType.CAMPUS)]
        public int? CampusId { get; set; }

        #endregion
        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.EventItemOccurrence" />.
        /// </summary>
        /// <value>
        /// The event item occurrence.
        /// </value>
        [LavaVisible]
        public virtual EventItemOccurrence EventItemOccurrence { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Group" />.
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        [DataMember]
        public virtual Group Group { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.RegistrationInstance" />.
        /// </summary>
        /// <value>
        /// The registration instance.
        /// </value>
        [DataMember]
        public virtual RegistrationInstance RegistrationInstance { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Campus"/> the event will be tied to.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Campus"/> where the <see cref="Rock.Model.Person"/> attended.
        /// </value>
        [DataMember]
        public virtual Campus Campus { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// EventItemOccurrenceGroupMap Configuration class.
    /// </summary>
    public partial class EventItemOccurrenceGroupMapConfiguration : EntityTypeConfiguration<EventItemOccurrenceGroupMap>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventItemOccurrenceGroupMapConfiguration" /> class.
        /// </summary>
        public EventItemOccurrenceGroupMapConfiguration()
        {
            this.HasOptional( p => p.EventItemOccurrence ).WithMany( e => e.Linkages ).HasForeignKey( p => p.EventItemOccurrenceId ).WillCascadeOnDelete( true );
            this.HasOptional( p => p.RegistrationInstance ).WithMany( r => r.Linkages ).HasForeignKey( p => p.RegistrationInstanceId ).WillCascadeOnDelete( true );
            this.HasOptional( p => p.Group ).WithMany( g => g.Linkages ).HasForeignKey( p => p.GroupId ).WillCascadeOnDelete( true );
            this.HasOptional( p => p.Campus ).WithMany().HasForeignKey( c => c.CampusId ).WillCascadeOnDelete( true );
        }
    }

    #endregion
}