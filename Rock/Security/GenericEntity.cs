using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rock.Security
{
    /// <summary>
    /// A generic ISecured entity
    /// </summary>
    public class GenericEntity : ISecured
    {
        private string _authEntity = "";

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericEntity"/> class.
        /// </summary>
        /// <param name="authEntity">The auth entity.</param>
        public GenericEntity( string authEntity )
        {
            _authEntity = authEntity;
        }

        /// <summary>
        /// The auth entity. Classes that implement the <see cref="ISecured"/> interface should return
        /// a value that is unique across all <see cref="ISecured"/> classes.  Typically this is the
        /// qualified name of the class.
        /// </summary>
        public string EntityTypeName
        {
            get { return _authEntity; }
        }

        /// <summary>
        /// The Id
        /// </summary>
        public int Id
        {
            get { return 0; }
        }

        /// <summary>
        /// A parent authority.  If a user is not specifically allowed or denied access to
        /// this object, Rock will check access to the parent authority specified by this property.
        /// </summary>
        public ISecured ParentAuthority
        {
            get { return null; }
        }

        /// <summary>
        /// A list of actions that this class supports.
        /// </summary>
        public List<string> SupportedActions
        {
            get { return new List<string>() { "View", "Edit", "Configure" }; }
        }

        /// <summary>
        /// Return <c>true</c> if the user is authorized to perform the selected action on this object.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns>
        ///   <c>true</c> if the specified action is authorized; otherwise, <c>false</c>.
        /// </returns>
        public bool IsAuthorized( string action, Rock.Crm.Person person )
        {
            return Security.Authorization.Authorized( this, action, person );
        }

        /// <summary>
        /// If a user or role is not specifically allowed or denied to perform the selected action,
        /// return <c>true</c> if they should be allowed anyway or <c>false</c> if not.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public bool IsAllowedByDefault( string action )
        {
            return action == "View";
        }

        public IQueryable<AuthRule> FindAuthRules()
        {
            return Authorization.FindAuthRules( this );
        }
    }
}