using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using church.ccv.Hr.Data;

namespace church.ccv.Hr.Model
{
    /// <summary>
    /// NOTE: Table is populated on-demand. If the CurrentDate isn’t a TimeCardPayPeriod yet, a row will be created. 
    /// </summary>
    [Table( "_church_ccv_Hr_TimeCardPayPeriod" )]
    [DataContract]
    public class TimeCardPayPeriod : Model<TimeCardPayPeriod>, Rock.Data.IRockEntity
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the start date of the Pay Period
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the DateTime where the PayPeriod ends. NOTE: The displayed EndDate will be EndDate.AddDays(-1) since humans think of this as a 24 hour date vs a point in time.
        /// </summary>
        /// <value>
        /// The end date.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime EndDate { get; set; }

        #endregion

        #region Virtual Properties

        #endregion

        #region methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format( "{0} - {1}", StartDate.ToShortDateString(), EndDate.AddDays( -1 ).ToShortDateString() );
        }

        #endregion
    }
}