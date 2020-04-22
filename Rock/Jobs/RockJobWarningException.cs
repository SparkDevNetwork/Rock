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
        public RockJobWarningException( string message ) : base( message ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockJobWarningException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (<see langword="Nothing" /> in Visual Basic) if no inner exception is specified.</param>
        public RockJobWarningException( string message, Exception innerException ) : base( message, innerException ) { }
    }
}
