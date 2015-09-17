using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.Composition;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Workflow;

namespace com.reallifeministries.RockExtensions.Workflow.Action
{
    /// <summary>
    /// Appends or Prepends to an Attribute Value
    /// </summary>
    [Description( "Appends or Prepends to an Attribute Value" )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Append/Prepend to Attribute" )]

    [WorkflowAttribute( "Attribute", "The attribute to update the value of.",true,"","",0 )]
    [BooleanField( "Prepend", "Check if you want to add content before existing value. (Default is after)",false,"",1 )]
    [CodeEditorField( "Value", "Formatted content to append or prepend. <span class='tip tip-lava'></span>", Rock.Web.UI.Controls.CodeEditorMode.Html, Rock.Web.UI.Controls.CodeEditorTheme.Rock, 200, false, "", "", 2 )]
    [CodeEditorField("Separator", "Content to place between values",Rock.Web.UI.Controls.CodeEditorMode.Html,Rock.Web.UI.Controls.CodeEditorTheme.Rock,200,false,"","",3)]
    public class AppendPrependAttributeValue : ActionComponent
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
            Guid guid = GetAttributeValue( action, "Attribute" ).AsGuid();
            if (!guid.IsEmpty())
            {
                var attribute = AttributeCache.Read( guid, rockContext );
                if ( attribute != null )
                {
                    string existingValue = action.GetWorklowAttributeValue( guid );
                    string value = GetAttributeValue( action, "Value" );
                    string separator;

                    if (String.IsNullOrEmpty( existingValue ))
                    {
                        separator = "";
                    }
                    else
                    {
                        separator = GetAttributeValue( action, "Separator" );
                    }

                    var mergeFields = GetMergeFields( action );
                    
                    value = value.ResolveMergeFields( mergeFields );
                    
                    string valueAction;
                    string newValue;

                    if (GetAttributeValue(action,"Prepend").AsBoolean()) 
                    {
                        valueAction = "Prepended";
                        newValue = string.Format("{1}{2}{0}", existingValue, value, separator);
                    } else {
                        valueAction = "Appended";
                        newValue = string.Format("{0}{2}{1}", existingValue, value, separator);
                    }

                    if ( attribute.EntityTypeId == new Rock.Model.Workflow().TypeId )
                    {
                        action.Activity.Workflow.SetAttributeValue( attribute.Key, newValue );
                        action.AddLogEntry( string.Format( "{2} '{0}' attribute to '{1}'.", attribute.Name, newValue, valueAction ) );
                    }
                    else if ( attribute.EntityTypeId == new Rock.Model.WorkflowActivity().TypeId )
                    {
                        action.Activity.SetAttributeValue( attribute.Key, newValue );
                        action.AddLogEntry( string.Format( "{2} '{0}' attribute to '{1}'.", attribute.Name, newValue, valueAction ) );
                    }
                }
            }
            return true;
        }
    }
}
