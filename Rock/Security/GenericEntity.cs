using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rock.Security
{
    public class GenericEntity : ISecured
    {
        private string _authEntity = "";
        private int id = 0;

        public GenericEntity( string authEntity )
        {
            _authEntity = authEntity;
        }

        public string AuthEntity
        {
            get { return _authEntity; }
        }

        public int Id
        {
            get { return 0; }
        }

        public ISecured ParentAuthority
        {
            get { return null; }
        }

        public List<string> SupportedActions
        {
            get { return new List<string>() { "View", "Edit", "Configure" }; }
        }

        public bool Authorized( string action, System.Web.Security.MembershipUser user )
        {
            return Security.Authorization.Authorized( this, action, user );
        }

        public bool DefaultAuthorization( string action )
        {
            return action == "View";
        }
    }
}