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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents an instance of when a <see cref="Rock.Model.Person">Person's</see> person detail data was viewed in Rock.  Includes data on who was viewed, the person who viewed their record, and when/where their record
    /// was viewed.
    /// </summary>
    [RockDomain( "CRM" )]
    [Table( "PersonViewed" )]
    [NotAudited]
    [DataContract]
    public partial class PersonViewed : Entity<PersonViewed>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Person"/> that was the viewer.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Person"/> who was the viewer.
        /// </value>
        [DataMember]
        public int? ViewerPersonAliasId { get; set; }
        
        /// <summary>
        /// Gets or sets the Id of the Target/Viewed <see cref="Rock.Model.Person"/>.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the Target/Viewed <see cref="Rock.Model.Person"/> 
        /// </value>
        [DataMember]
        public int? TargetPersonAliasId { get; set; }
        
        /// <summary>
        /// Gets or sets the Date and Time that the that the person was viewed.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the time that the person was viewed.
        /// </value>
        [DataMember]
        public DateTime? ViewDateTime { get; set; }
        
        /// <summary>
        /// Gets or sets the IP address of the computer/device that requested the page view.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the IP address of the computer/device that requested the page view.
        /// </value>
        [MaxLength( 25 )]
        [DataMember]
        public string IpAddress { get; set; }
        
        /// <summary>
        /// Gets or sets the source of the view (site id or application name)
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the source of the View.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string Source { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Person"/> entity of the viewer.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.Person"/> entity representing the viewer.
        /// </value>
        [LavaInclude]
        public virtual PersonAlias ViewerPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Person"/> entity of the individual who was viewed.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.Person"/> entity representing the person who was viewed.
        /// </value>
        [LavaInclude]
        public virtual PersonAlias TargetPersonAlias { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format( "{0} viewed ",
                ( ( ViewerPersonAlias != null && ViewerPersonAlias.Person != null ) ? ViewerPersonAlias.Person.ToStringSafe() : ViewerPersonAliasId.ToString() ),
                ( ( TargetPersonAlias != null && TargetPersonAlias.Person != null ) ? TargetPersonAlias.Person.ToStringSafe() : TargetPersonAliasId.ToString() ) );
        }

        #endregion

    }

    #region Entity Configuration
    
    /// <summary>
    /// Person Viewed Configuration class.
    /// </summary>
    public partial class PersonViewedConfiguration : EntityTypeConfiguration<PersonViewed>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonViewedConfiguration"/> class.
        /// </summary>
        public PersonViewedConfiguration()
        {
            this.HasOptional( p => p.ViewerPersonAlias ).WithMany().HasForeignKey( p => p.ViewerPersonAliasId ).WillCascadeOnDelete(false);
            this.HasOptional( p => p.TargetPersonAlias ).WithMany().HasForeignKey( p => p.TargetPersonAliasId ).WillCascadeOnDelete(false);
        }
    }

    #endregion

}
