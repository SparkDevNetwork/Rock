// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.IO;
using System.Linq;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// The data access/service class for the <see cref="Rock.Model.History"/> entity. This inherits from the Service class
    /// </summary>
    public partial class HistoryService
    {
        /// <summary>
        /// Saves a list of history messages.
        /// </summary>
        /// <param name="modelType">Type of the model.</param>
        /// <param name="categoryGuid">The category unique identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="changes">The changes.</param>
        /// <param name="CurrentPersonId">The current person identifier.</param>
        public void SaveChanges (Type modelType, Guid categoryGuid, int entityId, List<string> changes, int? CurrentPersonId )
        {
            SaveChanges( modelType, categoryGuid, entityId, changes, null, null, null, CurrentPersonId );
        }

        /// <summary>
        /// Saves the changes.
        /// </summary>
        /// <param name="modelType">Type of the model.</param>
        /// <param name="categoryGuid">The category unique identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="changes">The changes.</param>
        /// <param name="caption">The caption.</param>
        /// <param name="relatedModelType">Type of the related model.</param>
        /// <param name="relatedEntityId">The related entity identifier.</param>
        /// <param name="CurrentPersonId">The current person identifier.</param>
        public void SaveChanges( Type modelType, Guid categoryGuid, int entityId, List<string> changes, string caption, Type relatedModelType, int? relatedEntityId, int? CurrentPersonId )
        {

            var entityType = EntityTypeCache.Read(modelType);
            var category = CategoryCache.Read(categoryGuid);
            var creationDate = DateTime.Now;

            int? relatedEntityTypeId = null;
            if (relatedModelType != null)
            {
                var relatedEntityType = EntityTypeCache.Read(relatedModelType);
                if (relatedModelType != null)
                {
                    relatedEntityTypeId = relatedEntityType.Id;
                }
            }

            if (entityType != null && category != null)
            {
                foreach ( string message in changes.Where( m => m != null && m != "" ) )
                {
                    var history = new History();
                    history.EntityTypeId = entityType.Id;
                    history.CategoryId = category.Id;
                    history.EntityId = entityId;
                    history.Caption = caption;
                    history.Summary = message;
                    history.RelatedEntityTypeId = relatedEntityTypeId;
                    history.RelatedEntityId = relatedEntityId;
                    history.CreatedByPersonId = CurrentPersonId;
                    history.CreationDateTime = creationDate;
                    Add( history, CurrentPersonId );
                    Save( history, CurrentPersonId );
                }
            }
        }
    }
}


