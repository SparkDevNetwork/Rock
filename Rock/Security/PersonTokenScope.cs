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

using System;
using System.Collections.Generic;
using System.Threading;

using Rock.Model;

namespace Rock.Security
{
    /// <summary>
    /// Limits person token creation to specific people until the scope is disposed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The restriction follows the current async execution context, including tasks started while the scope is active.
    /// Nested scopes intersect, so an inner scope never allows a person that an outer scope blocks.
    /// </para>
    /// <para>
    /// Blocked creation returns <c>TokenProhibited</c>, which every token consumer already rejects.
    /// </para>
    /// </remarks>
    internal sealed class PersonTokenScope : IDisposable
    {
        #region Fields

        private static readonly AsyncLocal<PersonTokenScope> _currentScope = new AsyncLocal<PersonTokenScope>();

        private readonly PersonTokenScope _parentScope;

        private readonly HashSet<int> _allowedPersonIds;

        private bool _isDisposed;

        #endregion

        #region Constructors

        private PersonTokenScope( IEnumerable<int> allowedPersonIds )
        {
            _allowedPersonIds = new HashSet<int>( allowedPersonIds );
            _parentScope = _currentScope.Value;
            _currentScope.Value = this;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Begins a scope that allows tokens only for the specified person.
        /// </summary>
        /// <param name="person">The person who may receive tokens, or <c>null</c> to allow no one.</param>
        /// <returns>The active scope; disposing it ends the restriction.</returns>
        public static PersonTokenScope RestrictTo( Person person )
        {
            return new PersonTokenScope( person != null ? new[] { person.Id } : Array.Empty<int>() );
        }

        /// <summary>
        /// Determines whether the active scopes allow a token to be created for the specified person.
        /// </summary>
        /// <param name="personId">The identifier of the person the token would represent.</param>
        /// <returns><c>true</c> if no active scope blocks the person; otherwise <c>false</c>.</returns>
        public static bool IsTokenAllowed( int personId )
        {
            for ( var scope = _currentScope.Value; scope != null; scope = scope._parentScope )
            {
                if ( !scope._allowedPersonIds.Contains( personId ) )
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Ends the restriction and reinstates the scope that was active when this one began.
        /// </summary>
        public void Dispose()
        {
            if ( _isDisposed )
            {
                return;
            }

            _isDisposed = true;
            _currentScope.Value = _parentScope;
        }

        #endregion
    }
}
