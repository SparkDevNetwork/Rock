using System;

namespace Rock.Slingshot
{
    public class UploadedPersonCSVInvalidException : Exception
    {
        public UploadedPersonCSVInvalidException( string message ) : base( message )
        {
        }
    }
}
