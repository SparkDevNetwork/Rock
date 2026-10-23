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
using System.ComponentModel.DataAnnotations.Schema;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;

using Rock.Data;
using Rock.Model;

namespace Rock.Core.EntitySearch
{
    /// <summary>
    /// Expression visitor that rejects entity search expressions that use
    /// entity members which create sign-in tokens or other credentials.
    /// </summary>
    internal sealed class RestrictedMemberExpressionVisitor : ExpressionVisitor
    {
        /*
            10/9/2026 - MSE

            When entity security is enabled the search expressions are run in
            memory against the entity objects, so they can read properties that
            are not database columns. Some of those properties create a new
            sign-in token or credential each time they are read. The entity
            indexer is also rejected because it can read those properties by
            name. Results that are evaluated later, such as when they are
            serialized, come from this same expression, so checking the
            expression covers them too.

            Reason: Prevent entity searches from creating credentials for other people.
        */

        /// <summary>
        /// The names of entity properties that create a sign-in token or other
        /// credential when they are read.
        /// </summary>
        private static readonly HashSet<string> _restrictedMemberNames = new HashSet<string>( StringComparer.OrdinalIgnoreCase )
        {
            "ImpersonationParameter",
            "UrlEncodedKey",
            "EncryptedKey",
            "AliasEncryptedKey",
            "ConfirmationCode",
            "ConfirmationCodeEncoded"
        };

        /// <summary>
        /// The names of entity properties that only create a sign-in token when
        /// they are read from a <see cref="Person"/>. On other entities they
        /// are an encrypted identifier.
        /// </summary>
        private static readonly HashSet<string> _personOnlyMemberNames = new HashSet<string>( StringComparer.OrdinalIgnoreCase )
        {
            "UrlEncodedKey",
            "EncryptedKey"
        };

        /// <summary>
        /// Throws an exception if the expression uses a restricted member.
        /// </summary>
        /// <param name="expression">The expression to check.</param>
        public static void Validate( Expression expression )
        {
            new RestrictedMemberExpressionVisitor().Visit( expression );
        }

        /// <inheritdoc/>
        protected override Expression VisitMember( MemberExpression node )
        {
            if ( node.Member is PropertyInfo property && IsRestrictedProperty( property, node.Expression?.Type ) )
            {
                throw new Exception( $"The '{property.Name}' property can not be used in an entity search." );
            }

            return base.VisitMember( node );
        }

        /// <inheritdoc/>
        protected override Expression VisitMethodCall( MethodCallExpression node )
        {
            if ( IsRestrictedMethod( node.Method, node.Object?.Type ) )
            {
                throw new Exception( $"The '{node.Method.Name}' method can not be used in an entity search." );
            }

            return base.VisitMethodCall( node );
        }

        /// <inheritdoc/>
        protected override Expression VisitIndex( IndexExpression node )
        {
            if ( node.Indexer != null && IsEntityType( node.Indexer.DeclaringType ) )
            {
                throw new Exception( "The entity indexer can not be used in an entity search." );
            }

            return base.VisitIndex( node );
        }

        /// <inheritdoc/>
        protected override Expression VisitDynamic( DynamicExpression node )
        {
            // Dynamic member access is used when the value is typed as object,
            // so the actual type is not known until it runs.
            if ( node.Binder is GetIndexBinder
                || ( node.Binder is GetMemberBinder getMemberBinder && _restrictedMemberNames.Contains( getMemberBinder.Name ) )
                || ( node.Binder is InvokeMemberBinder invokeMemberBinder && _restrictedMemberNames.Contains( invokeMemberBinder.Name ) ) )
            {
                throw new Exception( "The expression can not be used in an entity search." );
            }

            return base.VisitDynamic( node );
        }

        /// <summary>
        /// Determines whether the property is a restricted property or an
        /// indexer declared on an entity type.
        /// </summary>
        /// <param name="property">The property to check.</param>
        /// <param name="instanceType">The type of the object the property is read from.</param>
        /// <returns><c>true</c> if the property is restricted; otherwise <c>false</c>.</returns>
        private static bool IsRestrictedProperty( PropertyInfo property, Type instanceType )
        {
            if ( !IsEntityType( property.DeclaringType ) )
            {
                return false;
            }

            if ( property.GetIndexParameters().Length > 0 )
            {
                return true;
            }

            if ( !_restrictedMemberNames.Contains( property.Name ) )
            {
                return false;
            }

            // A real database column with the same name is not restricted.
            if ( !property.DeclaringType.IsInterface && property.GetCustomAttribute<NotMappedAttribute>( true ) == null )
            {
                return false;
            }

            if ( _personOnlyMemberNames.Contains( property.Name ) )
            {
                return CouldBePerson( instanceType ?? property.DeclaringType );
            }

            return true;
        }

        /// <summary>
        /// Determines whether the method is the getter of a restricted property
        /// or an indexer declared on an entity type.
        /// </summary>
        /// <param name="method">The method to check.</param>
        /// <param name="instanceType">The type of the object the method is called on.</param>
        /// <returns><c>true</c> if the method is restricted; otherwise <c>false</c>.</returns>
        private static bool IsRestrictedMethod( MethodInfo method, Type instanceType )
        {
            if ( !method.IsSpecialName || !method.Name.StartsWith( "get_" ) || !IsEntityType( method.DeclaringType ) )
            {
                return false;
            }

            // Property getters and the indexer are called as methods named
            // get_Name and get_Item.
            if ( method.Name == "get_Item" )
            {
                return true;
            }

            var property = method.DeclaringType.GetProperty( method.Name.Substring( 4 ), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly );

            return property != null && IsRestrictedProperty( property, instanceType );
        }

        /// <summary>
        /// Determines whether a value of the type could be a <see cref="Person"/>.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns><c>true</c> if the value could be a <see cref="Person"/>; otherwise <c>false</c>.</returns>
        private static bool CouldBePerson( Type type )
        {
            return typeof( Person ).IsAssignableFrom( type ) || type.IsAssignableFrom( typeof( Person ) );
        }

        /// <summary>
        /// Determines whether the type is an entity type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns><c>true</c> if the type is an entity type; otherwise <c>false</c>.</returns>
        private static bool IsEntityType( Type type )
        {
            return type != null && typeof( IEntity ).IsAssignableFrom( type );
        }
    }
}
