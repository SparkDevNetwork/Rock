using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rock.Cms.Security
{
    public interface ISecured
    {
        string AuthEntity { get; }
        int Id { get; }
        ISecured ParentAuthority { get; }
        List<string> SupportedActions { get; }

        bool Authorized( string action, System.Web.Security.MembershipUser user );
        bool DefaultAuthorization( string action );
    }
}