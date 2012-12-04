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
using System.Data.Spatial;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// CheckInSchedule EF Model.
    /// </summary>
    public partial class Schedule : Model<Schedule>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the Schedule name.
        /// </summary>
        /// <value>
        /// File Name.
        /// </value>
        [Required]
        [AlternateKey]
        [MaxLength( 50 )]
        public string Name { get; set; }
        
        public ScheduleFrequency Frequency { get; set; }

        [MaxLength(100)]
        public string FrequencyQualifier { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime CheckInStartTime { get; set; }
        public DateTime CheckInEndTime { get; set; }
        public DateTimeOffset EffectiveStartDate { get; set; }
        public DateTimeOffset EffectiveEndDate { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets the dto.
        /// </summary>
        /// <returns></returns>
        public override IDto Dto
        {
            get { return null; } // return this.ToDto(); }
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
            return this.Name;
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
        public static Schedule Read( int id )
        {
            return Read<Schedule>( id );
        }

        /// <summary>
        /// Static method to return an object based on the GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        public static Schedule Read( Guid guid )
        {
            return Read<Schedule>( guid );
        }

        #endregion

    }

    #region Entity Configuration
    
    /// <summary>
    /// File Configuration class.
    /// </summary>
    public partial class ScheduleConfiguration : EntityTypeConfiguration<Schedule>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleConfiguration"/> class.
        /// </summary>
        public ScheduleConfiguration()
        {
        }
    }

    #endregion

    #region Enumerations

    /// <summary>
    /// The frequency type
    /// </summary>
    public enum ScheduleFrequency
    {
        /// <summary>
        /// Daily
        /// </summary>
        Daily = 0,

        /// <summary>
        /// Weekly
        /// </summary>
        Weekly = 1,

        /// <summary>
        /// Monthly
        /// </summary>
        Monthly = 2,

        /// <summary>
        /// One Time
        /// </summary>
        OneTime = 3
    }

    #endregion


}
