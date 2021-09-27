using System;

namespace Rock.Bus
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class BusException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BusException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public BusException( string message )
            : base( message )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BusException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (<see langword="Nothing" /> in Visual Basic) if no inner exception is specified.</param>
        public BusException( string message, Exception innerException)
            : base (message, innerException)
        {
        }
    }
}
