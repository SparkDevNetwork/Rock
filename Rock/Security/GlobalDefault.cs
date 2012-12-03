//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rock.Security
{
    /// <summary>
    /// A generic ISecured entity
    /// </summary>
    public class GlobalDefault : ISecured
    {
        /// <summary>
        /// Gets the Entity Type ID for this entity.
        /// </summary>
        /// <value>
        /// The type id.
        /// </value>
        public int TypeId
        {
            get
            {
                // Read should never return null since it will create entity type if it doesn't exist
                return Rock.Web.Cache.EntityTypeCache.Read( this.TypeName ).Id;
            }
        }
        /// <summary>
        /// The auth entity. Classes that implement the <see cref="ISecured"/> interface should return
        /// a value that is unique across all <see cref="ISecured"/> classes.  Typically this is the
        /// qualified name of the class.
        /// </summary>
        public string TypeName
        {
            get { return this.GetType().FullName; }
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

        /// <summary>
        /// Finds the AuthRule records associated with the current object.
        /// </summary>
        /// <returns></returns>
        public IQueryable<AuthRule> FindAuthRules()
        {
            return Authorization.FindAuthRules( this );
        }

        /// <summary>
        /// Reads the specified id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static GlobalDefault Read( int id )
        {
            return new GlobalDefault();
        }
    }
}