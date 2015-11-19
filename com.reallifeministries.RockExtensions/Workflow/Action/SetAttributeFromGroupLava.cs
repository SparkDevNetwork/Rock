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
    
    [Description( "Set an attribute on the workflow with a group attribute (Lava)" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Set Workflow Attribute With Group Attribute (Lava)" )]

    [WorkflowAttribute( "GroupAttribute", "The workflow attribute containing the group.", true, "", "", 0, null,
        new string[] { "Rock.Field.Types.GroupFieldType" } )]
    [WorkflowAttribute( "Attribute", "The workflow attribute you will be setting", true)]
    [TextField("Lava", "Lava to use when setting the attribute.  Normal Workflow merge fields will be available, as well as: {{Group}} which will be the Group model corresponding to the selected Group Attribute",true)]
    public class SetAttributeFromGroupLava : ActionComponent
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
            
            var groupAttribute =  GetAttributeValue( action, "GroupAttribute" );
            Guid groupAttrGuid = groupAttribute.AsGuid();
            
            if (!groupAttrGuid.IsEmpty())
            {
                var groupAttributeInst = AttributeCache.Read( groupAttrGuid, rockContext );
                if (groupAttributeInst != null)
                {
                    string attributeGroupValue = action.GetWorklowAttributeValue( groupAttrGuid );
                    Guid groupGuid = attributeGroupValue.AsGuid();
                    
                    var attrAttribute = GetAttributeValue( action, "Attribute" );
                    Guid attributeGuid = attrAttribute.AsGuid();

                    if (!attributeGuid.IsEmpty())
                    {
                        // Get the attribute
                        var attributeInst = AttributeCache.Read( attributeGuid, rockContext );
                        if ( attributeInst != null )
                        {
                            var group = (new GroupService( rockContext )).Get( groupGuid );
                            if (group != null)
                            {
                                group.LoadAttributes();
                                var mergeFields = GetMergeFields( action );
                                mergeFields.Add( "Group", group );

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
                                errorMessages.Add( string.Format( "Group cannot be found: {0}", groupGuid ) );
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
                    errorMessages.Add( string.Format( "Group attribute could not be found for '{0}'!", groupAttrGuid.ToString() ) );
                }
            } 
            else
            {
                errorMessages.Add( string.Format( "Selected group attribute ('{0}') was not a valid Guid!", groupAttribute ) );
            }

            return true;
        }
            
    }
}
