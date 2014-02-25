using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Web
{
    /// <summary>
    /// Exception for when a file upload fails.  We don't want these to go to the standard error page, this is why we have a special exception class
    /// </summary>
    public class FileUploadException : WebFaultException<string>
    {
        public FileUploadException( string detail, System.Net.HttpStatusCode statusCode ) : base( detail, statusCode ) 
        {
        }
    }
}
