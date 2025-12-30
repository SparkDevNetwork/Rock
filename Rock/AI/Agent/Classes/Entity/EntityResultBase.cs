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
using System.Text.Json.Serialization;

using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Classes.Entity
{
    /// <summary>
    /// Result model for a person's profile.
    /// </summary>
    internal class EntityResultBase
    {
        /// <summary>
        /// The phone number id. This will not be show in the JSON output.
        /// </summary>
        [JsonIgnore]
        internal int Id { get; set; }

        /// <summary>
        /// Internal identifier of the phone number.
        /// </summary>
        public string IdKey
        {
            get { return Id.AsIdKey(); }
        }

        /// <summary>
        /// Gets or sets the date and time that the entity was created.
        /// </summary>
        public DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the date and time that the entity was last modified.
        /// </summary>
        public DateTime? ModifiedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the person who created the entity.
        /// </summary>
        public PersonResult CreatedByPerson { get; set; }

        /// <summary>
        /// Gets or sets the person who last modified the entity.
        /// </summary>
        public PersonResult ModifiedByPerson { get; set; }

        /// <summary>
        /// Attributes of the defined value.
        /// </summary>
        public List<AttributeResult> Attributes { get; set; }

        #region Public Methods

        /// <summary>
        /// Sanitizes the entity for security by checking attribute security.
        /// </summary>
        /// <param name="currentPerson"></param>
        /// <returns>True if the security checks passed; false if the checks failed.</returns>
        public virtual bool SanitizeForSecurity( Person currentPerson )
        {
            // Default logic (very basic, override in subclasses for custom behavior)
            //if ( currentPerson == null )
            //{
            //    return false;
            //}

            // Remove attributes the current person does not have view access to.
            CheckAttributeSecurity( currentPerson );

            // Currently there is need to prevent the security from passing.
            return true;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Removes any attributes that the current person does not have view access to.
        /// </summary>
        /// <param name="currentPerson"></param>
        private void CheckAttributeSecurity( Person currentPerson )
        {
            if ( Attributes != null )
            {
                for ( int i = Attributes.Count - 1; i >= 0; i-- )
                {
                    var isAllowedViewAccess = AttributeCache.Get( Attributes[i].Id )?.IsAuthorized( Authorization.VIEW, currentPerson ) ?? false;

                    if ( !isAllowedViewAccess )
                    {
                        Attributes.RemoveAt( i );
                    }
                }
            }
        }

        #endregion
    }
}
