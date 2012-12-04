//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RangeValue<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RangeValue{T}" /> class.
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        public RangeValue(T from, T to)
        {
            this.From = from;
            this.To = to;
        }

        /// <summary>
        /// Gets or sets from.
        /// </summary>
        /// <value>
        /// From.
        /// </value>
        public T From { get; set; }

        /// <summary>
        /// Gets or sets to.
        /// </summary>
        /// <value>
        /// To.
        /// </value>
        public T To { get; set; }
    }
}