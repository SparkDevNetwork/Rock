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
using System.Linq;
using System.Reflection;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Sets an entity property.
    /// </summary>
    [ActionCategory( "Utility" )]
    [Description( "Sets an entity property." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Entity Property Set" )]

    [EntityTypeField( "Entity Type", false, "The type of Entity.", true, "", 0, "EntityType" )]
    [WorkflowTextOrAttribute( "Entity Id or Guid", "Entity Attribute", "The id or guid of the entity. <span class='tip tip-lava'></span>", true, "", "", 1, "EntityIdGuid" )]
    [WorkflowTextOrAttribute( "Property Name", "Property Name Attribute", "The name of the property to set. <span class='tip tip-lava'></span>", true, "", "", 2, "PropertyName" )]
    [WorkflowTextOrAttribute( "Property Value", "Property Value Attribute", "The value to set. <span class='tip tip-lava'></span>", false, "", "", 3, "PropertyValue" )]
    [CustomDropdownListField( "Empty Value Handling", "How to handle empty property values.", "IGNORE^Ignore empty values,EMPTY^Set to empty,NULL^Set to NULL", true, "", "", 4, "EmptyValueHandling" )]
    public class SetEntityProperty : ActionComponent
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

            // Get the property settings
            string propertyName = GetAttributeValue( action, "PropertyName", true ).ResolveMergeFields( mergeFields );
            string propertyValue = GetAttributeValue( action, "PropertyValue", true ).ResolveMergeFields( mergeFields );
            string emptyValueHandling = GetAttributeValue( action, "EmptyValueHandling" );

            if ( emptyValueHandling == "IGNORE" && String.IsNullOrWhiteSpace( propertyValue ) )
            {
                action.AddLogEntry( "Skipping empty value." );
                return true;
            }

            PropertyInfo propInf = entityObject.GetType().GetProperty( propertyName, BindingFlags.Public | BindingFlags.Instance );

            if ( propInf == null )
            {
                errorMessages.Add( string.Format( "Property does not exist ('{0}')!", propertyName ) );
                return false;
            }

            if ( !propInf.CanWrite )
            {
                errorMessages.Add( string.Format( "Property is not writable ('{0}')!", entityIdGuidString ) );
                return false;
            }

            try
            {
                propInf.SetValue( entityObject, ConvertObject( propertyValue, propInf.PropertyType, emptyValueHandling == "NULL" ), null );
            }
            catch ( Exception ex ) when ( ex is InvalidCastException || ex is FormatException || ex is OverflowException )
            {
                errorMessages.Add( string.Format( "Could not convert property value ('{0}')! {1}", propertyValue, ex.Message ) );
                return false;
            }

            if ( !entityObject.IsValid )
            {
                errorMessages.Add( string.Format( "Invalid property value ('{0}')! {1}", propertyValue, entityObject.ValidationResults.Select( r => r.ErrorMessage ).ToList().AsDelimited( " " ) ) );
                return false;
            }

            try
            {
                _rockContext.SaveChanges();
            }
            catch ( Exception ex )
            {
                errorMessages.Add( string.Format( "Could not save value ('{0}')! {1}", propertyValue, ex.Message ) );
                return false;
            }

            action.AddLogEntry( string.Format( "Set '{0}' property to '{1}'.", propertyName, propertyValue ) );

            return true;
        }

        /// <summary>
        /// Converts a string to the specified type of object.
        /// </summary>
        /// <param name="theObject">The string to convert.</param>
        /// <param name="objectType">The type of object desired.</param>
        /// <param name="tryToNull">If empty strings should return as null.</param>
        /// <returns></returns>
        private static object ConvertObject( string theObject, Type objectType, bool tryToNull = true )
        {
            if ( objectType.IsEnum )
            {
                return string.IsNullOrWhiteSpace( theObject ) ? null : Enum.Parse( objectType, theObject, true );
            }

            Type underType = Nullable.GetUnderlyingType( objectType );
            if ( underType == null ) // not nullable
            {
                return Convert.ChangeType( theObject, objectType );
            }

            if ( tryToNull && string.IsNullOrWhiteSpace( theObject ) )
            {
                return null;
            }
            return Convert.ChangeType( theObject, underType );
        }
    }
}
