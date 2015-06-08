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
    /// Represents a connection opportunity campus
    /// </summary>
    [Table( "ConnectionOpportunityCampus" )]
    [DataContract]
    public partial class ConnectionOpportunityCampus : Model<ConnectionOpportunityCampus>
    {

        #region Entity Properties

        [Required]
        [DataMember]
        public int? ConnectionOpportunityId { get; set; }

        [Required]
        [DataMember]
        public int? CampusId { get; set; }

        [DataMember]
        public int? ConnectorGroupId { get; set; }

        #endregion

        #region Virtual Properties

        [DataMember]
        public virtual ConnectionOpportunity ConnectionOpportunity { get; set; }

        [DataMember]
        public virtual Campus Campus { get; set; }

        [DataMember]
        public virtual Group ConnectorGroup { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// ConnectionOpportunityCampus Configuration class.
    /// </summary>
    public partial class ConnectionOpportunityCampusConfiguration : EntityTypeConfiguration<ConnectionOpportunityCampus>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionOpportunityCampusConfiguration" /> class.
        /// </summary>
        public ConnectionOpportunityCampusConfiguration()
        {
            this.HasRequired( p => p.ConnectionOpportunity ).WithMany().HasForeignKey( p => p.ConnectionOpportunityId ).WillCascadeOnDelete( false );
            this.HasRequired( p => p.Campus ).WithMany().HasForeignKey( p => p.CampusId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.ConnectorGroup ).WithMany().HasForeignKey( p => p.ConnectorGroupId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}