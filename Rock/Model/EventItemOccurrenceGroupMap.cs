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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents the linkage between event campus, registration instance, and group.
    /// </summary>
    [RockDomain( "Event" )]
    [Table( "EventItemOccurrenceGroupMap" )]
    [DataContract]
    public partial class EventItemOccurrenceGroupMap : Model<EventItemOccurrenceGroupMap>
    {

        /// <summary>
        /// Gets or sets the event item occurrence identifier.
        /// </summary>
        /// <value>
        /// The event item occurrence identifier.
        /// </value>
        [DataMember]
        public int? EventItemOccurrenceId { get; set; }

        /// <summary>
        /// Gets or sets the registration instance identifier.
        /// </summary>
        /// <value>
        /// The registration instance identifier.
        /// </value>
        [DataMember]
        public int? RegistrationInstanceId { get; set; }

        /// <summary>
        /// Gets or sets the group identifier.
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

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the event item occurrence.
        /// </summary>
        /// <value>
        /// The event item occurrence.
        /// </value>
        [LavaInclude]
        public virtual EventItemOccurrence EventItemOccurrence { get; set; }

        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        [DataMember]
        public virtual Group Group { get; set; }

        /// <summary>
        /// Gets or sets the registration instance.
        /// </summary>
        /// <value>
        /// The registration instance.
        /// </value>
        [DataMember]
        public virtual RegistrationInstance RegistrationInstance { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return ToString( false, true, true );
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <param name="includeEventItem">if set to <c>true</c> [include event item].</param>
        /// <param name="includeRegistrationInstance">if set to <c>true</c> [include registration instance].</param>
        /// <param name="includeGroup">if set to <c>true</c> [include group].</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public string ToString( bool includeEventItem, bool includeRegistrationInstance, bool includeGroup )
        { 
            var parts = new List<string>();

            if ( includeEventItem && EventItemOccurrence != null )
            {
                parts.Add( EventItemOccurrence.ToString() );
            }

            if ( includeRegistrationInstance && RegistrationInstance != null )
            {
                parts.Add( RegistrationInstance.ToString() );
            }

            if ( includeGroup && Group != null )
            {
                parts.Add( Group.ToString() );
            }

            return parts.AsDelimited( " - " );
        }

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
        }
    }

    #endregion
}