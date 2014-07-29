using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class ChartClickArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the date time value.
        /// </summary>
        /// <value>
        /// The date time value.
        /// </value>
        public DateTime DateTimeValue { get; set; }

        /// <summary>
        /// Gets or sets the y value.
        /// </summary>
        /// <value>
        /// The y value.
        /// </value>
        public decimal? YValue { get; set; }

        /// <summary>
        /// Gets or sets the series identifier.
        /// </summary>
        /// <value>
        /// The series identifier.
        /// </value>
        public string SeriesId { get; set; }
    }
}
