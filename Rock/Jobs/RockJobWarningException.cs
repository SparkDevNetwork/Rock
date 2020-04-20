using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Jobs
{
    /// <summary>
    /// This is exception type is used by rock jobs to show warnings instead of errors.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class RockJobWarningException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RockJobWarningException"/> class.
        /// </summary>
        public RockJobWarningException() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockJobWarningException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public RockJobWarningException(string message) : base(message) { }
    }
}
