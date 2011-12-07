using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web;

namespace Rock.Api
{
    public class RockServiceAuthorizationManager  : ServiceAuthorizationManager
    {
        protected override bool CheckAccessCore( OperationContext operationContext )
        {
            return base.CheckAccessCore( operationContext );
        }
    }
}