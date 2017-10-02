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
using System.ComponentModel;
using System.ComponentModel.Composition;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Sets an entity attribute.
    /// </summary>
    [ActionCategory( "Utility" )]
    [Description( "Sets an entity attribute." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Entity Attribute Set" )]

    [EntityTypeField( "Entity Type", false, "The type of Entity.", true, "", 0, "EntityType" )]
    [WorkflowTextOrAttribute( "Entity Id or Guid", "Entity Attribute", "The id or guid of the entity. <span class='tip tip-lava'></span>", true, "", "", 1, "EntityIdGuid" )]
    [WorkflowTextOrAttribute( "Attribute Key", "Attribute Key Attribute", "The key of the attribute to set. <span class='tip tip-lava'></span>", true, "", "", 2, "AttributeKey" )]
    [WorkflowTextOrAttribute( "Attribute Value", "Attribute Value Attribute", "The value to set. <span class='tip tip-lava'></span>", false, "", "", 3, "AttributeValue" )]
    public class SetEntityAttribute : ActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            // Get the entity type
            EntityTypeCache entityType = null;
            var entityTypeGuid = GetAttributeValue( action, "EntityType" ).AsGuidOrNull();
            if ( entityTypeGuid.HasValue )
            {
                entityType = EntityTypeCache.Read( entityTypeGuid.Value );
            }
            if ( entityType == null )
            {
                errorMessages.Add( string.Format( "Entity Type could not be found for selected value ('{0}')!", entityTypeGuid.ToString() ) );
                return false;
            }

            var mergeFields = GetMergeFields( action );
            RockContext _rockContext = new RockContext();

            // Get the entity
            EntityTypeService entityTypeService = new EntityTypeService( _rockContext );
            IEntity entityObject = null;
            string entityIdGuidString = GetAttributeValue( action, "EntityIdGuid", true ).ResolveMergeFields( mergeFields ).Trim();
            var entityGuid = entityIdGuidString.AsGuidOrNull();
            if ( entityGuid.HasValue )
            {
                entityObject = entityTypeService.GetEntity( entityType.Id, entityGuid.Value );
            }
            else
            {
                var entityId = entityIdGuidString.AsIntegerOrNull();
                if ( entityId.HasValue )
                {
                    entityObject = entityTypeService.GetEntity( entityType.Id, entityId.Value );
                }
            }

            if ( entityObject == null )
            {
                errorMessages.Add( string.Format( "Entity could not be found for selected value ('{0}')!", entityIdGuidString ) );
                return false;
            }

            var entityWithAttributes = entityObject as IHasAttributes;
            if ( entityWithAttributes == null )
            {
                errorMessages.Add( string.Format( "Entity does not support attributes ('{0}')!", entityIdGuidString ) );
                return false;
            }

            // Get the property settings
            string attributeKey = GetAttributeValue( action, "AttributeKey", true ).ResolveMergeFields( mergeFields );
            string attributeValue = GetAttributeValue( action, "AttributeValue", true ).ResolveMergeFields( mergeFields );

            entityWithAttributes.LoadAttributes(_rockContext);
            entityWithAttributes.SetAttributeValue( attributeKey, attributeValue );

            try
            {
                entityWithAttributes.SaveAttributeValue( attributeKey, _rockContext );
            }
            catch ( Exception ex )
            {
                errorMessages.Add( string.Format( "Could not save value ('{0}')! {1}", attributeValue, ex.Message ) );
                return false;
            }

            action.AddLogEntry( string.Format( "Set '{0}' attribute to '{1}'.", attributeKey, attributeValue ) );

            return true;
        }
    }
}
