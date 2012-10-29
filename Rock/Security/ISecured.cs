//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.Collections.Generic;
using System.Linq;

namespace Rock.Security
{
    /// <summary>
    /// Represents a securable object.  Note each ISecured object must also expose a static Read(int id) method if the object
    /// types will be used in a grid with a SecureField column
    /// </summary>
    public interface ISecured
    {
        /// <summary>
        /// The auth entity. Classes that implement the <see cref="ISecured"/> interface should return
        /// a value that is unique across all <see cref="ISecured"/> classes.  Typically this is the 
        /// qualified name of the class. 
        /// </summary>
        string TypeName { get; }

        /// <summary>
        /// The Id
        /// </summary>
        int Id { get; }

        /// <summary>
        /// A parent authority.  If a user is not specifically allowed or denied access to 
        /// this object, Rock will check access to the parent authority specified by this property.
        /// </summary>
        ISecured ParentAuthority { get; }

        /// <summary>
        /// A list of actions that this class supports.
        /// </summary>
        List<string> SupportedActions { get; }

        /// <summary>
        /// Return <c>true</c> if the user is authorized to perform the selected action on this object.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns>
        ///   <c>true</c> if the specified action is authorized; otherwise, <c>false</c>.
        /// </returns>
        bool IsAuthorized( string action, Rock.Crm.Person person );

        /// <summary>
        /// If a user or role is not specifically allowed or denied to perform the selected action,
        /// return <c>true</c> if they should be allowed anyway or <c>false</c> if not.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>

        bool IsAllowedByDefault( string action );

        /// <summary>
        /// Finds the AuthRule records associated with the current object.
        /// </summary>
        /// <returns></returns>
        IQueryable<AuthRule> FindAuthRules();

    }
}