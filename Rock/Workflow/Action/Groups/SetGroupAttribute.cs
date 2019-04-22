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

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Sets an attribute's value to the selected group 
    /// </summary>
    [ActionCategory( "Groups" )]
    [Description( "Sets a group attribute." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Group Attribute Set" )]

    [WorkflowAttribute( "Group", "The attribute containing the group whose attribute will be set.", true, "", "", 0, null,
        new string[] { "Rock.Field.Types.GroupFieldType", "Rock.Field.Types.GroupFieldType" } )]

    [TextField( "Group Attribute Key", "The attribute key to use for the group attribute.", true, "", "", 2 )]

    [WorkflowTextOrAttribute( "Text Value", "Attribute Value", "The text or attribute to set the value from. <span class='tip tip-lava'></span>", false, "", "", 4, "AttributeValue" )]
    public class SetGroupAttribute : ActionComponent
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

            Guid? groupGuid = null;
            string attributeValue = string.Empty;
            string attributeKey = string.Empty;

            // get the group attribute
            Guid groupAttributeGuid = GetAttributeValue( action, "Group" ).AsGuid();

            if ( !groupAttributeGuid.IsEmpty() )
            {
                groupGuid = action.GetWorklowAttributeValue( groupAttributeGuid ).AsGuidOrNull();

                if ( !groupGuid.HasValue )
                {
                    errorMessages.Add( "The group could not be found!" );
                }
            }

            // get group attribute value
            attributeValue = GetAttributeValue( action, "AttributeValue" );
            Guid guid = attributeValue.AsGuid();
            if ( guid.IsEmpty() )
            {
                attributeValue = attributeValue.ResolveMergeFields( GetMergeFields( action ) );
            }
            else
            {
                var workflowAttributeValue = action.GetWorklowAttributeValue( guid );

                if ( workflowAttributeValue != null )
                {
                    attributeValue = workflowAttributeValue;
                }
            }

            // get attribute key
            attributeKey = GetAttributeValue( action, "GroupAttributeKey" ).Replace( " ", "" );

            // set attribute
            if ( groupGuid.HasValue )
            {
                var qry = new GroupService( rockContext ).Queryable()
                                .Where( m => m.Guid == groupGuid );

                foreach ( var group in qry.ToList() )
                {
                    group.LoadAttributes( rockContext );
                    if ( group.Attributes.ContainsKey( attributeKey ) )
                    {
                        var attribute = group.Attributes[attributeKey];
                        Rock.Attribute.Helper.SaveAttributeValue( group, attribute, attributeValue, rockContext );
                    }
                    else
                    {
                        action.AddLogEntry( string.Format( "The group attribute {0} does not exist!", attributeKey ) );
                        break;
                    }
                }
            }

            errorMessages.ForEach( m => action.AddLogEntry( m, true ) );

            return true;
        }
    }
}