//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

namespace Rock.Model
{
    /// <summary>
    /// Represents a Range of objects in RockChMS.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RangeValue<T>
    {

        /// <summary>
        /// Initializes a new instanceof the <see cref="RangeValue{T}"/> class.
        /// </summary> 
        /// <param name="from">The From/minimum value of the range.</param>
        /// <param name="to">The To/maximum value of the range.</param>
        public RangeValue(T from, T to)
        {
            this.From = from;
            this.To = to;
        }

        /// <summary>
        /// Gets or sets the From/mininimum value of the range.
        /// </summary>
        /// <value>
        /// The from/minimum value of the range.
        /// </value>
        public T From { get; set; }

        /// <summary>
        /// Gets or sets To/maximum value of the range.
        /// </summary>
        /// <value>
        /// The to/maximum value of the range.
        /// </value>
        public T To { get; set; }
    }
}