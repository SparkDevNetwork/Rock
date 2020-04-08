// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.Collections.Generic;

using Rock.Data;
using Rock.Model;

namespace Rock.Security
{
    /// <summary>
    /// Represents a securable object.  Note each ISecured object must also expose a static Read(int id) method if the object
    /// types will be used in a grid with a SecureField column
    /// </summary>
    public interface ISecured
    {
        /// <summary>
        /// Gets the Entity Type ID for this entity.
        /// </summary>
        /// <value>
        /// The type id.
        /// </value>
        int TypeId { get; }

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
        /// An optional parent authority to check prior to checking main ParentAuthority.  
        /// i.e for Groups, the GroupType is a checked prior to the main parent
        /// authority of parent group type, entity, globaldefault. 
        /// 
        /// NOTE: The ParentAuthorityPre is only checked for the root entity, for example 
        /// if the entity specified by the Parentuthority value has a ParentAuthorityPre value,
        /// that will not be checked.
        /// </summary>
        /// <value>
        /// The parent authority pre.
        /// </value>
        ISecured ParentAuthorityPre { get; }

        /// <summary>
        /// A dictionary of actions that this class supports and the description of each.
        /// </summary>
        Dictionary<string, string> SupportedActions { get; }

        /// <summary>
        /// Return <c>true</c> if the user is authorized to perform the selected action on this object.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns>
        ///   <c>true</c> if the specified action is authorized; otherwise, <c>false</c>.
        /// </returns>
        bool IsAuthorized( string action, Rock.Model.Person person );

        /// <summary>
        /// If a user or role is not specifically allowed or denied to perform the selected action,
        /// return <c>true</c> if they should be allowed anyway or <c>false</c> if not.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        bool IsAllowedByDefault( string action );

        /// <summary>
        /// Determines whether the specified action is private (Only the current user has access).
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns>
        ///   <c>true</c> if the specified action is private; otherwise, <c>false</c>.
        /// </returns>
        bool IsPrivate( string action, Person person );

        /// <summary>
        /// Makes the action on the current entity private (Only the current user will have access).
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        void MakePrivate( string action, Person person, RockContext rockContext = null );

        /// <summary>
        /// If action on the current entity is private, removes security that made it private.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        void MakeUnPrivate( string action, Person person, RockContext rockContext = null );
    }
}