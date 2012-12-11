//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// GroupLocation POCO class.
    /// </summary>
    [Table( "GroupLocation" )]
    public partial class GroupLocation : Model<GroupLocation>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the group id.
        /// </summary>
        /// <value>
        /// The group id.
        /// </value>
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the location id.
        /// </summary>
        /// <value>
        /// The location id.
        /// </value>
        public int LocationId { get; set; }

        /// <summary>
        /// Gets or sets the location type.
        /// </summary>
        /// <value>
        /// The location type.
        /// </value>
        public int? LocationTypeValueId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        public virtual Group Group { get; set; }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>
        /// The location.
        /// </value>
        public virtual Location Location { get; set; }

        /// <summary>
        /// Gets or sets the Location Type.
        /// </summary>
        /// <value>
        /// A <see cref="Model.DefinedValue"/> object.
        /// </value>
        public virtual DefinedValue LocationTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the schedules.
        /// </summary>
        /// <value>
        /// The schedules.
        /// </value>
        public virtual ICollection<Schedule> Schedules
        {
            get { return _schedules ?? ( _schedules = new Collection<Schedule>() ); }
            set { _schedules = value; }
        }
        private ICollection<Schedule> _schedules;

        /// <summary>
        /// Gets the dto.
        /// </summary>
        /// <returns></returns>
        public override IDto Dto
        {
            get { return this.ToDto(); }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Group.ToString() + " at " + Location.ToString();
        }

        #endregion

        #region Private Methods

        #endregion

        #region Static Methods

        /// <summary>
        /// Static Method to return an object based on the id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static GroupLocation Read( int id )
        {
            return Read<GroupLocation>( id );
        }

        /// <summary>
        /// Static method to return an object based on the GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        public static GroupLocation Read( Guid guid )
        {
            return Read<GroupLocation>( guid );
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
            this.HasOptional( t => t.LocationTypeValue ).WithMany().HasForeignKey( t => t.LocationTypeValueId ).WillCascadeOnDelete( false );
            this.HasMany( t => t.Schedules ).WithMany().Map( t => { t.MapLeftKey( "GroupLocationId" ); t.MapRightKey( "ScheduleId" ); t.ToTable( "GroupLocationSchedule" ); } );
        }
    }

    #endregion

}