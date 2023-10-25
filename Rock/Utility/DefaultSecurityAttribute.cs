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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Utility
{
    /// <summary>
    /// Defines the default security to apply to the type when it is first
    /// created in the database.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <strong>This is an internal API</strong> that supports the Rock
    ///         infrastructure and not subject to the same compatibility standards
    ///         as public APIs. It may be changed or removed without notice in any
    ///         release and should therefore not be directly used in any plug-ins.
    ///     </para>
    /// </remarks>
    [AttributeUsage( AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = false )]
    [RockInternal( "1.17" )]
    public class DefaultSecurityAttribute : System.Attribute
    {
        #region Properties

        /// <summary>
        /// Gets the security action.
        /// </summary>
        /// <value>The security action.</value>
        public string Action { get; }

        /// <summary>
        /// Gets the special role to be matched.
        /// </summary>
        /// <value>The special role to be matched.</value>
        public SpecialRole SpecialRole { get; }

        /// <summary>
        /// Gets the group unique identifier.
        /// </summary>
        /// <value>The group unique identifier.</value>
        public Guid? GroupGuid { get; }

        /// <summary>
        /// Gets the order of the rule.
        /// </summary>
        /// <value>The order of the rule.</value>
        public int Order { get; }

        /// <summary>
        /// Gets a value indicating whether this rule allows access.
        /// </summary>
        /// <value><c>true</c> if this rule allows access; otherwise, <c>false</c>.</value>
        public bool IsAllowed { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultSecurityAttribute"/> class.
        /// </summary>
        public DefaultSecurityAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultSecurityAttribute"/> class.
        /// </summary>
        /// <param name="action">The security action, such as "View" or "Edit".</param>
        /// <param name="role">The special role to match.</param>
        /// <param name="order">The order of this rule.</param>
        /// <param name="isAllowed">if set to <c>true</c> then access will be allowed.</param>
        public DefaultSecurityAttribute( string action, SpecialRole role, int order, bool isAllowed )
        {
            Action = action;
            SpecialRole = role;
            Order = order;
            IsAllowed = isAllowed;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultSecurityAttribute"/> class.
        /// </summary>
        /// <param name="action">The security action, such as "View" or "Edit".</param>
        /// <param name="groupGuid">The unique identifier of the group as a string.</param>
        /// <param name="order">The order of this rule.</param>
        /// <param name="isAllowed">if set to <c>true</c> then access will be allowed.</param>
        public DefaultSecurityAttribute( string action, string groupGuid, int order, bool isAllowed )
        {
            Action = action;
            GroupGuid = groupGuid.AsGuidOrNull();
            Order = order;
            IsAllowed = isAllowed;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the authorization rule that should be saved to the database.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="rockContext">The rock context to use when looking up group identifiers.</param>
        /// <returns>A new instance of <see cref="Auth"/>.</returns>
        internal Auth GetAuth( int entityTypeId, int? entityId, RockContext rockContext )
        {
            if ( !IsValid() )
            {
                throw new Exception( "Default Security Attribute is not valid." );
            }

            var auth = new Auth
            {
                EntityTypeId = entityTypeId,
                EntityId = entityId,
                Action = Action,
                SpecialRole = SpecialRole,
                Order = Order,
                AllowOrDeny = IsAllowed ? "A" : "D"
            };

            if ( GroupGuid.HasValue )
            {
                var groupId = new GroupService( rockContext ).GetId( GroupGuid.Value );

                if ( !groupId.HasValue )
                {
                    throw new Exception( $"Unable to find group '{GroupGuid}' when creating default authorization rule." );
                }

                auth.GroupId = groupId.Value;
            }

            return auth;
        }

        /// <summary>
        /// Creates the default auth rules defined on the type for the specified
        /// entity type and entity. This does not delete any existing rules but
        /// will skip any rules that already exist.
        /// </summary>
        /// <param name="type">The type to use when looking for <see cref="DefaultSecurityAttribute"/> decorations.</param>
        /// <param name="entityTypeId">The identifier of the entity type.</param>
        /// <param name="entityId">The identifier of the entity to add the auth rules to.</param>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "1.17" )]
        public static bool CreateAuthRules( Type type, int entityTypeId, int entityId )
        {
            return CreateAuthRules( type.GetCustomAttributes<DefaultSecurityAttribute>(), entityTypeId, entityId );
        }

        /// <summary>
        /// Creates the default auth rules defined on the method for the specified
        /// entity type and entity. This does not delete any existing rules but
        /// will skip any rules that already exist.
        /// </summary>
        /// <param name="method">The method to use when looking for <see cref="DefaultSecurityAttribute"/> decorations.</param>
        /// <param name="entityTypeId">The identifier of the entity type.</param>
        /// <param name="entityId">The identifier of the entity to add the auth rules to.</param>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "1.17" )]
        public static bool CreateAuthRules( MethodInfo method, int entityTypeId, int entityId )
        {
            return CreateAuthRules( method.GetCustomAttributes<DefaultSecurityAttribute>(), entityTypeId, entityId );
        }

        /// <summary>
        /// Creates the specified default auth rules. This does not delete any
        /// existing rules but will skip any rules that already exist.
        /// </summary>
        /// <param name="defaultSecurityAttributes">The list of default rules to create.</param>
        /// <param name="entityTypeId">The identifier of the entity type.</param>
        /// <param name="entityId">The identifier of the entity to add the auth rules to.</param>
        internal static bool CreateAuthRules( IEnumerable<DefaultSecurityAttribute> defaultSecurityAttributes, int entityTypeId, int entityId )
        {
            var ruleAttributes = defaultSecurityAttributes.ToList();

            if ( ruleAttributes.Count == 0 )
            {
                return false;
            }

            using ( var rockContext = new RockContext() )
            {
                var authService = new AuthService( rockContext );
                var existingAuths = authService.Queryable()
                    .Where( a => a.EntityTypeId == entityTypeId && a.EntityId == entityId )
                    .ToList();

                foreach ( var ruleAttribute in ruleAttributes )
                {
                    if ( ruleAttribute.IsDefault() )
                    {
                        continue;
                    }

                    var auth = ruleAttribute.GetAuth( entityTypeId, entityId, rockContext );

                    // Look for an existing auth rule. This handles cases where
                    // this method completed successfully, but then the EntityType
                    // record couldn't be updated. That scenario would cause this
                    // method to be called again even though the rules already
                    // exist.
                    var existingAuth = existingAuths
                        .Where( a => a.Action == auth.Action
                            && a.SpecialRole == auth.SpecialRole
                            && a.GroupId == auth.GroupId
                            && a.Order == auth.Order )
                        .FirstOrDefault();

                    if ( existingAuth == null )
                    {
                        authService.Add( auth );
                    }
                }

                // Disable pre-post processing because this might be happening
                // very early in Rock startup process before everything is ready.
                rockContext.SaveChanges( new SaveChangesArgs { DisablePrePostProcessing = true } );
            }

            return true;
        }

        /// <summary>
        /// Returns true if this instance is valid.
        /// </summary>
        /// <returns><c>true</c> if this instance is valid; otherwise, <c>false</c>.</returns>
        public bool IsValid()
        {
            if ( IsDefault() )
            {
                return true;
            }

            if ( Action.IsNullOrWhiteSpace() )
            {
                return false;
            }

            if ( SpecialRole == SpecialRole.None && !GroupGuid.HasValue )
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether this instance indicates that the default security
        /// should be applied.
        /// </summary>
        /// <returns><c>true</c> if default security should be applied; otherwise, <c>false</c>.</returns>
        public bool IsDefault()
        {
            return Action.IsNullOrWhiteSpace() && SpecialRole == SpecialRole.None && !GroupGuid.HasValue;
        }

        #endregion
    }
}
