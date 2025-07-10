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
using System.Linq;

using Rock.AI.Agent;

namespace Rock.Model
{
    public partial class AIAgentSession
    {
        /// <summary>
        /// The cached session context data. This is loaded on demand by either
        /// <see cref="GetSessionContext(string)"/> or <see cref="SetSessionContext(string, SessionContext)"/>.
        /// </summary>
        private Dictionary<string, SessionContext> _sessionContextData;

        #region Methods

        private void LoadSessionContextData()
        {
            _sessionContextData = this.GetAdditionalSettings<Dictionary<string, SessionContext>>( typeof( SessionContext ).Name );

            if ( _sessionContextData.Count == 0 )
            {
                return;
            }

            // Look for any expired contexts and remove them.
            var expiredKeys = new List<string>();

            foreach ( var kvp in _sessionContextData )
            {
                if ( kvp.Value.ExpireDateTime < RockDateTime.Now )
                {
                    expiredKeys.Add( kvp.Key );
                }
            }

            if ( expiredKeys.Count > 0 )
            {
                foreach ( var key in expiredKeys )
                {
                    _sessionContextData.Remove( key );
                }

                this.SetAdditionalSettings( typeof( SessionContext ).Name, _sessionContextData );
            }
        }

        /// <summary>
        /// Gets the session context data for the specified key.
        /// </summary>
        /// <param name="key">The key that identifies the session context.</param>
        /// <returns>An instance of <see cref="SessionContext"/> or <c>null</c> if it was not found or was expired.</returns>
        internal SessionContext GetSessionContext( string key )
        {
            if ( _sessionContextData == null )
            {
                LoadSessionContextData();
            }

            return _sessionContextData.TryGetValue( key, out var data )
                ? data
                : null;
        }

        /// <summary>
        /// Gets all of the session context data for this session.
        /// </summary>
        /// <returns>A dictionary of <see cref="SessionContext"/> values that are still valid.</returns>
        internal Dictionary<string, SessionContext> GetSessionContextDictionary()
        {
            if ( _sessionContextData == null )
            {
                LoadSessionContextData();
            }

            // Return as a new dictionary to avoid external modifications.
            return new Dictionary<string, SessionContext>( _sessionContextData );
        }

        /// <summary>
        /// Sets the session context data for the specified key. This will update
        /// the value, but it won't be persisted to the database until you call
        /// SaveChanges.
        /// </summary>
        /// <param name="key">The key that identifies the session context.</param>
        /// <param name="content">The session context that represents the data, or <c>null</c> to remove the context data.</param>
        internal void SetSessionContext( string key, SessionContext content )
        {
            if ( _sessionContextData == null )
            {
                LoadSessionContextData();
            }

            if ( content != null )
            {
                _sessionContextData[key] = content;
            }
            else
            {
                _sessionContextData.Remove( key );
            }

            this.SetAdditionalSettings( typeof( SessionContext ).Name, _sessionContextData );
        }

        #endregion
    }
}
