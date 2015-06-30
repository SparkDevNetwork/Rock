// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Collections.ObjectModel;
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
    [Table( "EventItemCampusGroupMap" )]
    [DataContract]
    public partial class EventItemCampusGroupMap : Model<EventItemCampusGroupMap>
    {
        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.EventItem"/> that this EventItemCampus is associated with. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.EventItem"/> that the EventItemCampus is associated with.
        /// </value>
        [DataMember( IsRequired = true )]
        public int? EventItemCampusId { get; set; }

        /// <summary>
        /// Gets or sets the registration instance identifier.
        /// </summary>
        /// <value>
        /// The registration instance identifier.
        /// </value>
        [DataMember]
        public int? RegistrationInstanceId { get; set; }        
        
        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Campus"/> that this EventItemCampus is associated with. when null it is a church-wide event.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Campus"/> that the EventItemCampus is associated with.
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
        /// Gets or sets the event item campus.
        /// </summary>
        /// <value>
        /// The event item campus.
        /// </value>
        [DataMember]
        public virtual EventItemCampus EventItemCampus { get; set; }

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

            if ( includeEventItem && EventItemCampus != null )
            {
                parts.Add( EventItemCampus.ToString() );
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
    /// EventItemCampus Configuration class.
    /// </summary>
    public partial class EventItemCampusGroupMapConfiguration : EntityTypeConfiguration<EventItemCampusGroupMap>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventItemCampusConfiguration" /> class.
        /// </summary>
        public EventItemCampusGroupMapConfiguration()
        {
            this.HasOptional( p => p.EventItemCampus ).WithMany( e => e.Linkages ).HasForeignKey( p => p.EventItemCampusId ).WillCascadeOnDelete( true );
            this.HasOptional( p => p.RegistrationInstance ).WithMany( r => r.Linkages ).HasForeignKey( p => p.RegistrationInstanceId ).WillCascadeOnDelete( true );
            this.HasOptional( p => p.Group ).WithMany( g => g.Linkages ).HasForeignKey( p => p.GroupId ).WillCascadeOnDelete( true );
        }
    }

    #endregion
}