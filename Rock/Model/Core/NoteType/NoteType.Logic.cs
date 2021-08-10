﻿// <copyright>
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
using System.Data.Entity;

using Rock.Lava;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class NoteType
    {
        #region Properties

        /// <summary>
        /// Gets or sets an optional CSS class to include for the note
        /// </summary>
        /// <value>
        /// The CSS class.
        /// </value>
        [RockObsolete( "1.8" )]
        [Obsolete( "No Longer Supported", true )]
        [NotMapped]
        [LavaHidden]
        public string CssClass
        {
            get
            {
                return null;
            }

            set
            {
                //
            }
        }

        /// <summary>
        /// A dictionary of actions that this class supports and the description of each.
        /// </summary>
        public override Dictionary<string, string> SupportedActions
        {
            get
            {
                var supportedActions = base.SupportedActions;
                supportedActions.AddOrReplace( Rock.Security.Authorization.APPROVE, "The roles and/or users that have access to approve notes." );
                return supportedActions;
            }
        }

        #endregion

        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return NoteTypeCache.Get( this.Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            NoteTypeCache.UpdateCachedEntity( this.Id, entityState );
            NoteTypeCache.RemoveEntityNoteTypes();
        }

        #endregion
    }
}
