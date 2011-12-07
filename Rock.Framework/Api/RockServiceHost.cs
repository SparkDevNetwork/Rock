using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Web;

namespace Rock.Api
{
    public class RockServiceHost : ServiceHost
    {
        protected override void OnOpening()
        {
            ServiceCredentials sc = new ServiceCredentials();
            sc.UserNameAuthentication.UserNamePasswordValidationMode = System.ServiceModel.Security.UserNamePasswordValidationMode.MembershipProvider;
            sc.UserNameAuthentication.MembershipProvider.
            this.Description.Behaviors.
            base.OnOpening();
        }
    }
}