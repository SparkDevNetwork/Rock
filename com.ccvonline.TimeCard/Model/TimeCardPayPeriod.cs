using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using com.ccvonline.TimeCard.Data;

namespace com.ccvonline.TimeCard.Model
{
    /// <summary>
    /// NOTE: Table is populated on-demand. If the CurrentDate isn’t a TimeCardPayPeriod yet, a row will be created. 
    /// </summary>
    [Table( "_com_ccvonline_TimeCard_TimeCardPayPeriod" )]
    [DataContract]
    public class TimeCardPayPeriod : NamedModel<TimeCardPayPeriod>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        /// <value>
        /// The end date.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime? EndDate { get; set; }

        #endregion

        #region Virtual Properties
        #endregion
    }
}
