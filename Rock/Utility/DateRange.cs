using System;

namespace Rock
{
    /// <summary>
    /// 
    /// </summary>
    public class DateRange
    {
        /// <summary>
        /// Gets or sets the start.
        /// </summary>
        /// <value>
        /// The start.
        /// </value>
        public DateTime? Start { get; set; }

        /// <summary>
        /// Gets or sets the end.
        /// </summary>
        /// <value>
        /// The end.
        /// </value>
        public DateTime? End { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format( "{0} to {1}", Start.HasValue ? Start.Value.ToString( "f" ) : null, End.HasValue ? End.Value.ToString( "f" ) : null );
        }
    }
}
