// <copyright>
// Copyright by BEMA Information Technologies
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
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Workflow;


namespace com.bemaservices.WorkflowExtensions.Workflow.Action
{
    /// <summary>
    /// Sets an entity attribute.
    /// </summary>
    [ActionCategory( "BEMA Services > Workflow Extensions" )]
    [Description( "Sets entity attributes." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Entity Attributes Set" )]

    [EntityTypeField( "Entity Type", false, "The type of Entity.", true, "", 0, "EntityType" )]
    [WorkflowTextOrAttribute( "Entity Id or Guid", "Entity Attribute", "The id or guid of the entity. <span class='tip tip-lava'></span>", true, "", "", 1, "EntityIdGuid" )]
    [MatrixField( "F9267429-44ED-46AB-8A9D-A8FF904DD056", "Set Entity Attributes", "", false, "", 2, "Matrix" )]
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
                entityType = EntityTypeCache.Get( entityTypeGuid.Value );
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

            var entityWithAttributes = entityObject as Rock.Attribute.IHasAttributes;
            if ( entityWithAttributes == null )
            {
                errorMessages.Add( string.Format( "Entity does not support attributes ('{0}')!", entityIdGuidString ) );
                return false;
            }

            // Get the property settings


            entityWithAttributes.LoadAttributes( _rockContext );

            var attributeMatrixGuid = GetAttributeValue( action, "Matrix" ).AsGuid();
            var attributeMatrix = new AttributeMatrixService( rockContext ).Get( attributeMatrixGuid );
            if ( attributeMatrix != null )
            {
                foreach ( AttributeMatrixItem attributeMatrixItem in attributeMatrix.AttributeMatrixItems )
                {
                    attributeMatrixItem.LoadAttributes();

                    string attributeKey = attributeMatrixItem.GetMatrixAttributeValue( action, "AttributeKey", true ).ResolveMergeFields( mergeFields );
                    string attributeValue = attributeMatrixItem.GetMatrixAttributeValue( action, "Value", true ).ResolveMergeFields( mergeFields );

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
                }
            }
            return true;
        }
    }
}
