using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Workflow;



namespace com.reallifeministries.RockExtensions.Workflow.Action
{
   
    [Description( "Set an attribute on the workflow with a person attribute (Lava)" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Set Workflow Attribute With Person Attribute (Lava)" )]

    [WorkflowAttribute( "PersonAttribute", "The workflow attribute containing the person.", true, "", "", 0, null,
        new string[] { "Rock.Field.Types.PersonFieldType" } )]
    [WorkflowAttribute( "Attribute", "The workflow attribute you will be setting", true)]
    [TextField("Lava", "Lava to use when setting the attribute.  Normal Workflow merge fields will be available, as well as: {{Person}} which will be the Person model corresponding to the selected Person Attribute",true)]
    public class SetAttributeFromPersonLava : ActionComponent
    {
         /// <summary>
        /// Executes the specified workflow action.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();
            
            var personAttribute =  GetAttributeValue( action, "PersonAttribute" );
            Guid personAttrGuid = personAttribute.AsGuid();
            
            if (!personAttrGuid.IsEmpty())
            {
                var personAttributeInst = AttributeCache.Read( personAttrGuid, rockContext );
                if (personAttributeInst != null)
                {
                    string attributePersonValue = action.GetWorklowAttributeValue( personAttrGuid );
                    Guid personAliasGuid = attributePersonValue.AsGuid();
                    
                    var attrAttribute = GetAttributeValue( action, "Attribute" );
                    Guid attributeGuid = attrAttribute.AsGuid();

                    if (!attributeGuid.IsEmpty())
                    {
                        // Get the attribute
                        var attributeInst = AttributeCache.Read( attributeGuid, rockContext );
                        if ( attributeInst != null )
                        {
                            var personAlias = (new PersonAliasService( rockContext )).Get( personAliasGuid );
                            if (personAlias != null)
                            {
                                personAlias.Person.LoadAttributes();
                                var mergeFields = GetMergeFields( action );
                                mergeFields.Add( "Person", personAlias.Person );

                                string value = GetAttributeValue( action, "Lava" );

                                value = value.ResolveMergeFields( mergeFields );

                                if (attributeInst.EntityTypeId == new Rock.Model.Workflow().TypeId)
                                {
                                    action.Activity.Workflow.SetAttributeValue( attributeInst.Key, value );
                                    action.AddLogEntry( string.Format( "Set '{0}' attribute to '{1}'.", attributeInst.Name, value ) );
                                }
                                else if (attributeInst.EntityTypeId == new Rock.Model.WorkflowActivity().TypeId)
                                {
                                    action.Activity.SetAttributeValue( attributeInst.Key, value );
                                    action.AddLogEntry( string.Format( "Set '{0}' attribute to '{1}'.", attributeInst.Name, value ) );
                                }
                            }
                            else
                            {
                                errorMessages.Add( string.Format( "PersonAlias cannot be found: {0}", personAliasGuid ) );
                            }
                        }
                        else
                        {
                            errorMessages.Add( string.Format( "Workflow attribute could not be found for '{0}'!", attributeGuid.ToString() ) );
                        }
                    }
                    else
                    {
                        errorMessages.Add( string.Format( "Selected attribute ('{0}') was not a valid Guid!", attrAttribute ) );
                    }
                    
                }
                else
                {
                    errorMessages.Add( string.Format( "Person attribute could not be found for '{0}'!", personAttrGuid.ToString() ) );
                }
            } 
            else
            {
                errorMessages.Add( string.Format( "Selected person attribute ('{0}') was not a valid Guid!", personAttribute ) );
            }

            return true;
        }
            
    }
}
