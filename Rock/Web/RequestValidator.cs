using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Util;

namespace Rock.Web
{
    public class RequestValidator : System.Web.Util.RequestValidator
    {
        protected override bool IsValidRequestString( HttpContext context, string value, RequestValidationSource requestValidationSource, string collectionKey, out int validationFailureIndex )
        {
            if ( requestValidationSource == RequestValidationSource.Form )
            {
                // TODO: For now do not validate form values.  Eventually should provide way for just specific controls to be ignored
                validationFailureIndex = -1;
                return true;
            }
            else
            {
                return base.IsValidRequestString( context, value, requestValidationSource, collectionKey, out validationFailureIndex );
            }
        }
    }
}
