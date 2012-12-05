//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.ModelConfiguration;

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

        /// <summary>
        /// Gets or sets the frequency.
        /// </summary>
        /// <value>
        /// The frequency.
        /// </value>
        public ScheduleFrequency Frequency { get; set; }

        /// <summary>
        /// Gets or sets the frequency qualifier.
        /// </summary>
        /// <value>
        /// The frequency qualifier.
        /// </value>
        [MaxLength(100)]
        public string FrequencyQualifier { get; set; }

        /// <summary>
        /// Gets or sets the start time.
        /// </summary>
        /// <value>
        /// The start time.
        /// </value>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time.
        /// </summary>
        /// <value>
        /// The end time.
        /// </value>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Gets or sets the check in start time.
        /// </summary>
        /// <value>
        /// The check in start time.
        /// </value>
        public DateTime CheckInStartTime { get; set; }

        /// <summary>
        /// Gets or sets the check in end time.
        /// </summary>
        /// <value>
        /// The check in end time.
        /// </value>
        public DateTime CheckInEndTime { get; set; }

        /// <summary>
        /// Gets or sets the effective start date.
        /// </summary>
        /// <value>
        /// The effective start date.
        /// </value>
        public DateTimeOffset EffectiveStartDate { get; set; }

        /// <summary>
        /// Gets or sets the effective end date.
        /// </summary>
        /// <value>
        /// The effective end date.
        /// </value>
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
