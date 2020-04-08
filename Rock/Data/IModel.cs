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
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

using Rock.Model;

namespace Rock.Data
{
    /// <summary>
    /// Interface for all models
    /// </summary>
    public interface IModel : IEntity
    {
        /// <summary>
        /// Gets or sets the created date time.
        /// </summary>
        /// <value>
        /// The created date time.
        /// </value>
        DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the modified date time.
        /// </summary>
        /// <value>
        /// The modified date time.
        /// </value>
        DateTime? ModifiedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the created by person alias identifier.
        /// </summary>
        /// <value>
        /// The created by person alias identifier.
        /// </value>
        int? CreatedByPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the modified by person alias identifier.
        /// </summary>
        /// <value>
        /// The modified by person alias identifier.
        /// </value>
        int? ModifiedByPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the created by person alias.
        /// </summary>
        /// <value>
        /// The created by person alias.
        /// </value>
        PersonAlias CreatedByPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the modified by person alias.
        /// </summary>
        /// <value>
        /// The modified by person alias.
        /// </value>
        PersonAlias ModifiedByPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the custom sort value.
        /// </summary>
        /// <value>
        /// The custom sort value.
        /// </value>
        object CustomSortValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the ModifiedByPersonAliasId value has already been 
        /// updated to reflect who/when model was updated. If this value is false (default) the framework will update 
        /// the value with the current user when the model is saved. Set this value to true if this automatic
        /// update should not be done.
        /// </summary>
        /// <value>
        /// <c>false</c> if rock should set the ModifiedByPersonAliasId to current user when saving model; otherwise, <c>true</c>.
        /// </value>
        bool ModifiedAuditValuesAlreadyUpdated { get; set; }

        /// <summary>
        /// Method that will be called on an entity immediately before the item is saved
        /// </summary>
        void PreSaveChanges( Rock.Data.DbContext dbContext, DbEntityEntry entry );

        /// <summary>
        /// Method that will be called on an entity immediately after the item is saved
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        void PostSaveChanges( Rock.Data.DbContext dbContext );

        /// <summary>
        /// Pres the save changes.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entry">The entry.</param>
        /// <param name="state">The state.</param>
        void PreSaveChanges( Rock.Data.DbContext dbContext, DbEntityEntry entry, EntityState state );

        /// <summary>
        /// Gets the Created By audit HTML details.
        /// </summary>
        /// <param name="rootUrl">The root URL.</param>
        /// <returns>An HTML fragment with details about who created the model and when it was created.</returns>
        string GetCreatedAuditHtml( string rootUrl );

        /// <summary>
        /// Gets the Last Modified By audit HTML details.
        /// </summary>
        /// <param name="rootUrl">The root URL.</param>
        /// <returns>An HTML fragment with details about who modified the model and when it was modified.</returns>
        string GetModifiedAuditHtml( string rootUrl );
    }
}
