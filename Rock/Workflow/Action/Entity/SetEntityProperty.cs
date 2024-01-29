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
    [ActionCategory( "Entity" )]
    [Description( "Sets an entity property." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Entity Property Set" )]

    [EntityTypeField(
        "Entity Type",
        Description = "The type of Entity.",
        IsRequired = true,
        Order = 0,
        IncludeGlobalAttributeOption = false,
        Key = AttributeKey.EntityType )]
    [WorkflowTextOrAttribute(
        "Entity Id or Guid",
        "Entity Attribute",
        Description = "The id or guid of the entity. <span class='tip tip-lava'></span>",
        IsRequired = true,
        Order = 1,
        Key = AttributeKey.EntityIdGuid )]
    [WorkflowTextOrAttribute(
        "Property Name",
        "Property Name Attribute",
        Description = "The name of the property to set. <span class='tip tip-lava'></span>",
        IsRequired = true,
        Order = 2,
        Key = AttributeKey.PropertyName )]
    [WorkflowTextOrAttribute(
        "Property Value",
        "Property Value Attribute",
        Description = "The value to set. <span class='tip tip-lava'></span>",
        IsRequired = false,
        Order = 3,
        Key = AttributeKey.PropertyValue )]
    [CustomDropdownListField(
        "Empty Value Handling",
        Description = "How to handle empty property values.",
        ListSource = "IGNORE^Ignore empty values,EMPTY^Set to empty,NULL^Set to NULL",
        IsRequired = true,
        Order = 4,
        Key = AttributeKey.EmptyValueHandling )]
    [Rock.SystemGuid.EntityTypeGuid( "2B3502EA-5531-4345-AA01-23AE273F0B6F")]
    public class SetEntityProperty : ActionComponent
    {

        #region Attribute Keys

        private static class AttributeKey
        {
            public const string EntityType = "EntityType";
            public const string EntityIdGuid = "EntityIdGuid";
            public const string PropertyName = "PropertyName";
            public const string PropertyValue = "PropertyValue";
            public const string EmptyValueHandling = "EmptyValueHandling";
        }

        #endregion

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
            var entityTypeGuid = GetAttributeValue( action, AttributeKey.EntityType ).AsGuidOrNull();
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
            string entityIdGuidString = GetAttributeValue( action, AttributeKey.EntityIdGuid, true ).ResolveMergeFields( mergeFields ).Trim();
            var entityId = entityIdGuidString.AsIntegerOrNull();
            if ( entityId.HasValue )
            {
                entityObject = entityTypeService.GetEntity( entityType.Id, entityId.Value );
            }
            else
            {
                var entityGuid = entityIdGuidString.AsGuidOrNull();
                if ( entityGuid.HasValue )
                {
                    entityObject = entityTypeService.GetEntity( entityType.Id, entityGuid.Value );
                }
            }

            if ( entityObject == null )
            {
                var value = GetActionAttributeValue( action, AttributeKey.EntityIdGuid );
                entityObject = action.GetEntityFromAttributeValue( value, rockContext );
            }

            if ( entityObject == null )
            {
                errorMessages.Add( string.Format( "Entity could not be found for selected value ('{0}')!", entityIdGuidString ) );
                return false;
            }

            // Get the property settings
            string propertyName = GetAttributeValue( action, AttributeKey.PropertyName, true ).ResolveMergeFields( mergeFields );
            string propertyValue = GetAttributeValue( action, AttributeKey.PropertyValue, true ).ResolveMergeFields( mergeFields );
            string emptyValueHandling = GetAttributeValue( action, AttributeKey.EmptyValueHandling );

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

            // C# SetProperty can't take a string representation of a Guid (Fixes: #3183)
            if ( objectType.Name == "Guid" )
            {
                return theObject.AsGuidOrNull();
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
            
            if ( underType.IsEnum )
            {
                return string.IsNullOrWhiteSpace( theObject ) ? null : Enum.Parse( underType, theObject, true );
            }

            return Convert.ChangeType( theObject, underType );
        }
    }
}
