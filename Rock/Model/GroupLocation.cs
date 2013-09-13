//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
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
    /// Represents a <see cref="Rock.Model.Location"/> that is associated with a <see cref="Rock.Model.Group"/>
    /// </summary>
    /// <remarks>
    /// In RockChMS a <see cref="Rock.Model.Group"/> is defined any party or collection of <see cref="Rock.Model.Person">Persons</see>.  Examples of GroupLocaitons
    /// could include a Person/Family's address, a Business' address, a church campus, a room where a Bible study meets.  Pretty much, it is any place where a 
    /// group of people meet or are located. 
    /// </remarks>
    [Table( "GroupLocation" )]
    [DataContract]
    public partial class GroupLocation : Model<GroupLocation>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Group"/> that that is associated with this GroupLocation. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Group"/> that this GroupLocation is associated with.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Location"/> that is associated with this GroupLocation. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> referencing the Id of the <see cref="Rock.Model.Location"/> that is associated with this GroupLocation. 
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int LocationId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the GroupLocationType <see cref="Rock.Model.DefinedValue"/> that is used to identify the type of <see cref="Rock.Model.Location"/>
        /// that this is.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> referencing the Id of the GroupLocaitonType <see cref="Rock.Model.DefinedValue"/> that identifies the type of location that this is.
        /// If a GroupLocationType <see cref="Rock.Model.DefinedValue"/> is not associated with this GroupLocation this value will be null.
        /// </value>
        /// <remarks>
        /// Examples of 
        /// </remarks>
        [DataMember]
        public int? GroupLocationTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the <see cref="Rock.Model.Location"/> referenced by this GroupLocation is the mailing address/location for the <see cref="Rock.Model.Group"/>.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this is the mailing address/location for this <see cref="Rock.Model.Group"/>.
        /// </value>
        public bool IsMailing { get; set; }

        //TODO: Document
        /// <summary>
        /// Gets or sets a flag indicating if this is the mappable location for this 
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is location; otherwise, <c>false</c>.
        /// </value>
        public bool IsLocation { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Group"/> that is associated with this GroupLocation
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Group"/> that is associated with this GroupLocation.
        /// </value>
        [DataMember]
        public virtual Group Group { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Location"/> that is associated with this GroupLocation.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Location"/> that is associated with this GroupLocation.
        /// </value>
        [DataMember]
        public virtual Location Location { get; set; }

        /// <summary>
        /// Gets or sets the Location Type <see cref="Rock.Model.DefinedValue"/> of this GroupLocation.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.DefinedValue"/> that represents the type of this GroupLocation.
        /// </value>
        [DataMember]
        public virtual DefinedValue LocationTypeValue { get; set; }

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.Schedule">Schedules</see> that are associated with this GroupLocation.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.Scheudles"/> that are associated with this GroupLocation.
        /// </value>
        public virtual ICollection<Schedule> Schedules
        {
            get { return _schedules ?? ( _schedules = new Collection<Schedule>() ); }
            set { _schedules = value; }
        }
        private ICollection<Schedule> _schedules;

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" />  that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Group.ToString() + " at " + Location.ToString();
        }

        #endregion

    }
    
    #region Entity Configuration

    /// <summary>
    /// GroupLocation Configuration class
    /// </summary>
    public partial class GroupLocationConfiguration : EntityTypeConfiguration<GroupLocation>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupLocationConfiguration"/> class.
        /// </summary>
        public GroupLocationConfiguration()
        {
            this.HasRequired( t => t.Group ).WithMany( t => t.GroupLocations ).HasForeignKey( t => t.GroupId );
            this.HasRequired( t => t.Location ).WithMany( l => l.GroupLocations).HasForeignKey( t => t.LocationId );
            this.HasOptional( t => t.LocationTypeValue ).WithMany().HasForeignKey( t => t.GroupLocationTypeValueId ).WillCascadeOnDelete( false );
            this.HasMany( t => t.Schedules ).WithMany().Map( t => { t.MapLeftKey( "GroupLocationId" ); t.MapRightKey( "ScheduleId" ); t.ToTable( "GroupLocationSchedule" ); } );
        }
    }

    #endregion

}