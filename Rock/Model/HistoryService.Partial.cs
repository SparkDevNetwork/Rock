//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
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
        /// <param name="relatedModelType">Type of the related model.</param>
        /// <param name="relatedModelId">The related model identifier.</param>
        /// <param name="CurrentPersonId">The current person identifier.</param>
        public void SaveChanges( Type modelType, Guid categoryGuid, int entityId, List<string> changes, string caption, Type relatedModelType, int? relatedEntityId, int? CurrentPersonId )
        {

            var entityType = EntityTypeCache.Read(modelType);
            var category = CategoryCache.Read(categoryGuid);

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
                    history.CreationDateTime = DateTime.Now;
                    Add( history, CurrentPersonId );
                    Save( history, CurrentPersonId );
                }
            }
        }
    }
}


