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
using Rock.Workflow;
using Rock;

using org.newpointe.ExtraActions;
using Rock.Field;

namespace org.newpointe.WorkflowEntities
{
    [ActionCategory( "Extra Actions" )]
    [Description( "Sets multiple properties of an entity" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Set Entity Properties" )]

    [WorkflowAttribute( "Entity", "The entity to set the property of.", true, "", "", 1 )]
    [CustomDropdownListField( "Empty Value Handling", "How to handle empty property values", "IGNORE^Ignore empty values,EMPTY^Leave empty values empty,NULL^Set empty values to NULL (where possible)", true, "", "", 2 )]
    [KeyValueListField( "Entity Properties", "The properties to set on the entity. You can use <span class='tip tip-lava'></span>, but you'll need to use ! instead of |", true, "", "Property", "Value", "", "", "", 3 )]
    class SetEntityProperties : ActionComponent
    {
        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            Guid workflowAttributeGuid = GetAttributeValue( action, "Entity" ).AsGuid();
            Guid entityGuid = action.GetWorklowAttributeValue( workflowAttributeGuid ).AsGuid();

            if ( !entityGuid.IsEmpty() )
            {
                IEntityFieldType entityField = AttributeCache.Read( workflowAttributeGuid ).FieldType.Field as IEntityFieldType;
                if ( entityField == null )
                {
                    errorMessages.Add( "Attribute Type is not an Entity." );
                    return false;
                }

                IEntity entityObject = entityField.GetEntity( entityGuid.ToString(), rockContext );

                if ( entityObject == null )
                {
                    errorMessages.Add( "Failed to get entity." );
                    return false;
                }

                var propertyValues = GetAttributeValue( action, "EntityProperties" ).Replace( " ! ", " | " ).ResolveMergeFields( GetMergeFields( action ) ).TrimEnd( '|' ).Split( '|' ).Select( p => p.Split( '^' ) ).Select( p => new { Name = p[0], Value = p[1] } );

                foreach ( var prop in propertyValues )
                {
                    PropertyInfo propInf = entityObject.GetType().GetProperty( prop.Name, BindingFlags.Public | BindingFlags.Instance );
                    if ( null != propInf && propInf.CanWrite )
                    {
                        if ( !( GetAttributeValue( action, "EmptyValueHandling" ) == "IGNORE" && string.IsNullOrWhiteSpace( prop.Value ) ) )
                        {
                            try
                            {
                                propInf.SetValue( entityObject, ObjectConverter.ConvertObject( prop.Value, propInf.PropertyType, GetAttributeValue( action, "EmptyValueHandling" ) == "NULL" ), null );
                            }
                            catch ( Exception ex ) when ( ex is InvalidCastException || ex is FormatException || ex is OverflowException )
                            {
                                errorMessages.Add( "Invalid Property Value: " + prop.Name + ": " + ex.Message );
                            }
                        }
                    }
                    else
                    {
                        errorMessages.Add( "Invalid Property: " + prop.Name );
                    }
                }

                rockContext.SaveChanges();
                return true;
            }

            errorMessages.Add( "Invalid Entity Attribute" );
            return false;
        }
    }
}
