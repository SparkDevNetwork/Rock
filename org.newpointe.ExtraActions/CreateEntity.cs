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

namespace org.newpointe.WorkflowEntities
{
    [ActionCategory( "Extra Actions" )]
    [Description( "Creates an entity" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Create Entity" )]

    [EntityTypeField( "Entity Type", "The type of entity to create." )]
    [CustomDropdownListField( "Empty Value Handling", "How to handle empty property values", "IGNORE^Ignore empty values,EMPTY^Leave empty values empty,NULL^Set empty values to NULL (where possible)", true, "", "", 1 )]
    [KeyValueListField( "Entity Properties", "The properties to create the entity with.  You can use <span class='tip tip-lava'></span>, but you'll need to use ! instead of |", true, "", "Property", "Value", "", "", "", 2 )]
    [WorkflowAttribute( "Entity Attribute", "The attribute to save the entity to.", false, "", "", 3 )]
    class CreateEntity : ActionComponent
    {
        public override bool Execute( RockContext rockContext, WorkflowAction action, object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            EntityTypeCache cachedEntityType = EntityTypeCache.Read( GetAttributeValue( action, "EntityType" ).AsGuid() );

            if ( cachedEntityType != null )
            {
                Type entityType = cachedEntityType.GetEntityType();
                var newEntity = (IEntity)Activator.CreateInstance( entityType );

                var propertyValues = GetAttributeValue( action, "EntityProperties" ).Replace( " ! ", " | " ).ResolveMergeFields( GetMergeFields( action ) ).TrimEnd( '|' ).Split( '|' ).Select( p => p.Split( '^' ) ).Select( p => new { Name = p[0], Value = p[1] } );
                foreach ( var prop in propertyValues )
                {
                    PropertyInfo propInf = entityType.GetProperty( prop.Name, BindingFlags.Public | BindingFlags.Instance );
                    if ( null != propInf && propInf.CanWrite )
                    {
                        if ( !( GetAttributeValue( action, "EmptyValueHandling" ) == "IGNORE" && string.IsNullOrWhiteSpace( prop.Value ) ) )
                        {
                            try
                            {
                                propInf.SetValue( newEntity, ObjectConverter.ConvertObject( prop.Value, propInf.PropertyType, GetAttributeValue( action, "EmptyValueHandling" ) == "NULL" ), null );
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

                rockContext.Set( entityType ).Add( newEntity );
                rockContext.SaveChanges();

                // If request attribute was specified, requery the request and set the attribute's value
                Guid? entityAttributeGuid = GetAttributeValue( action, "EntityAttribute" ).AsGuidOrNull();
                if ( entityAttributeGuid.HasValue )
                {
                    newEntity = (IEntity)rockContext.Set( entityType ).Find( new[] { newEntity.Id } );
                    if ( newEntity != null )
                    {
                        SetWorkflowAttributeValue( action, entityAttributeGuid.Value, newEntity.Guid.ToString() );
                    }
                }


                return true;

            }

            errorMessages.Add( "Invalid Entity Attribute" );
            return false;
        }
    }
}
