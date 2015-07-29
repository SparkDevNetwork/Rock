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
using System.Web;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a connection opportunity
    /// </summary>
    [Table( "ConnectionOpportunity" )]
    [DataContract]
    public partial class ConnectionOpportunity : Model<ConnectionOpportunity>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [Required]
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the public.
        /// </summary>
        /// <value>
        /// The name of the public.
        /// </value>
        [Required]
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
        public string PublicName { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the photo identifier.
        /// </summary>
        /// <value>
        /// The photo identifier.
        /// </value>
        [DataMember]
        public int? PhotoId { get; set; }

        /// <summary>
        /// Gets or sets the connection type identifier.
        /// </summary>
        /// <value>
        /// The connection type identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int ConnectionTypeId { get; set; }

        /// <summary>
        /// Gets or sets the group type identifier.
        /// </summary>
        /// <value>
        /// The group type identifier.
        /// </value>
        [Required]
        [DataMember]
        public int GroupTypeId { get; set; }

        /// <summary>
        /// Gets or sets the connector group identifier.
        /// </summary>
        /// <value>
        /// The connector group identifier.
        /// </value>
        [DataMember]
        public int? ConnectorGroupId { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        [DataMember]
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [Required]
        [DataMember]
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the group member role identifier.
        /// </summary>
        /// <value>
        /// The group member role identifier.
        /// </value>
        [DataMember]
        public int? GroupMemberRoleId { get; set; }

        /// <summary>
        /// Gets or sets the group member status.
        /// </summary>
        /// <value>
        /// The group member status.
        /// </value>
        [DataMember]
        public GroupMemberStatus GroupMemberStatus { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use all groups of type].
        /// </summary>
        /// <value>
        /// <c>true</c> if [use all groups of type]; otherwise, <c>false</c>.
        /// </value>
        [Required]
        [DataMember]
        public bool UseAllGroupsOfType { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the type of the connection.
        /// </summary>
        /// <value>
        /// The type of the connection.
        /// </value>
        public virtual ConnectionType ConnectionType { get; set; }

        /// <summary>
        /// Gets or sets the connector group.
        /// </summary>
        /// <value>
        /// The connector group.
        /// </value>
        public virtual Group ConnectorGroup { get; set; }

        /// <summary>
        /// Gets or sets the type of the group.
        /// </summary>
        /// <value>
        /// The type of the group.
        /// </value>
        public virtual GroupType GroupType { get; set; }

        /// <summary>
        /// Gets or sets the group member role.
        /// </summary>
        /// <value>
        /// The group member role.
        /// </value>
        public virtual GroupTypeRole GroupMemberRole { get; set; }

        /// <summary>
        /// Gets the URL of the Opportunity's photo.
        /// </summary>
        /// <value>
        /// URL of the photo
        /// </value>
        [NotMapped]
        public virtual string PhotoUrl
        {
            get
            {
                return ConnectionOpportunity.GetPhotoUrl( this.PhotoId );
            }
            private set { }
        }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.BinaryFile"/> that contains the Opportunity's photo.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.BinaryFile"/> that contains the Opportunity's photo.
        /// </value>
        public virtual BinaryFile Photo { get; set; }

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.ConnectionOpportunityGroup">ConnectionOpportunityGroups</see> who are associated with the ConnectionOpportunity.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.ConnectionOpportunityGroup">ConnectionOpportunityGroups</see> who are associated with the ConnectionOpportunity.
        /// </value>
        public virtual ICollection<ConnectionOpportunityGroup> ConnectionOpportunityGroups
        {
            get { return _connectionOpportunityGroups ?? ( _connectionOpportunityGroups = new Collection<ConnectionOpportunityGroup>() ); }
            set { _connectionOpportunityGroups = value; }
        }

        private ICollection<ConnectionOpportunityGroup> _connectionOpportunityGroups;

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.ConnectionWorkflow">ConnectionWorkflows</see> who are associated with the ConnectionOpportunity.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.ConnectionWorkflow">ConnectionWorkflows</see> who are associated with the ConnectionOpportunity.
        /// </value>
        public virtual ICollection<ConnectionWorkflow> ConnectionWorkflows
        {
            get { return _connectionWorkflows ?? ( _connectionWorkflows = new Collection<ConnectionWorkflow>() ); }
            set { _connectionWorkflows = value; }
        }

        private ICollection<ConnectionWorkflow> _connectionWorkflows;

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.ConnectionRequest">ConnectionRequests</see> who are associated with the ConnectionOpportunity.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.ConnectionRequest">ConnectionRequests</see> who are associated with the ConnectionOpportunity.
        /// </value>
        public virtual ICollection<ConnectionRequest> ConnectionRequests
        {
            get { return _connectionRequests ?? ( _connectionRequests = new Collection<ConnectionRequest>() ); }
            set { _connectionRequests = value; }
        }

        private ICollection<ConnectionRequest> _connectionRequests;

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.ConnectionOpportunityGroupCampus">ConnectionOpportunityGroupCampuses</see> who are associated with the ConnectionOpportunity.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.ConnectionOpportunityGroupCampus">ConnectionOpportunityGroupCampuses</see> who are associated with the ConnectionOpportunity.
        /// </value>
        public virtual ICollection<ConnectionOpportunityGroupCampus> ConnectionOpportunityGroupCampuses
        {
            get { return _connectionOpportunityGroupCampuses ?? ( _connectionOpportunityGroupCampuses = new Collection<ConnectionOpportunityGroupCampus>() ); }
            set { _connectionOpportunityGroupCampuses = value; }
        }

        private ICollection<ConnectionOpportunityGroupCampus> _connectionOpportunityGroupCampuses;

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.ConnectionOpportunityCampus">ConnectionOpportunityCampuses</see> who are associated with the ConnectionOpportunity.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.ConnectionOpportunityCampus">ConnectionOpportunityCampuses</see> who are associated with the ConnectionOpportunity.
        /// </value>
        public virtual ICollection<ConnectionOpportunityCampus> ConnectionOpportunityCampuses
        {
            get { return _connectionOpportunityCampuses ?? ( _connectionOpportunityCampuses = new Collection<ConnectionOpportunityCampus>() ); }
            set { _connectionOpportunityCampuses = value; }
        }

        private ICollection<ConnectionOpportunityCampus> _connectionOpportunityCampuses;

        #endregion

        #region Methods

        /// <summary>
        /// Gets the photo URL.
        /// </summary>
        /// <param name="photoId">The photo identifier.</param>
        /// <param name="maxWidth">The maximum width.</param>
        /// <param name="maxHeight">The maximum height.</param>
        /// <returns></returns>
        public static string GetPhotoUrl( int? photoId, int? maxWidth = null, int? maxHeight = null )
        {
            string virtualPath = String.Empty;
            if ( photoId.HasValue )
            {
                string widthHeightParams = string.Empty;
                if ( maxWidth.HasValue )
                {
                    widthHeightParams += string.Format( "&maxwidth={0}", maxWidth.Value );
                }

                if ( maxHeight.HasValue )
                {
                    widthHeightParams += string.Format( "&maxheight={0}", maxHeight.Value );
                }

                virtualPath = String.Format( "~/GetImage.ashx?id={0}" + widthHeightParams, photoId );
            }
            else
            {
                virtualPath = "~/Assets/Images/no-picture.svg?";
            }

            if ( System.Web.HttpContext.Current == null )
            {
                return virtualPath;
            }
            else
            {
                return VirtualPathUtility.ToAbsolute( virtualPath );
            }
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// ConnectionOpportunity Configuration class.
    /// </summary>
    public partial class ConnectionOpportunityConfiguration : EntityTypeConfiguration<ConnectionOpportunity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionOpportunityConfiguration" /> class.
        /// </summary>
        public ConnectionOpportunityConfiguration()
        {
            this.HasOptional( p => p.GroupMemberRole ).WithMany().HasForeignKey( p => p.GroupMemberRoleId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.ConnectorGroup ).WithMany().HasForeignKey( p => p.ConnectorGroupId ).WillCascadeOnDelete( false );
            this.HasRequired( p => p.GroupType ).WithMany().HasForeignKey( p => p.GroupTypeId ).WillCascadeOnDelete( false );
            this.HasRequired( p => p.ConnectionType ).WithMany( p => p.ConnectionOpportunities ).HasForeignKey( p => p.ConnectionTypeId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.Photo ).WithMany().HasForeignKey( p => p.PhotoId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}