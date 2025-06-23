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
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;

namespace Rock.Workflow.Action.WorkflowControl
{
    /// <summary>
    /// Sets the CampusId of the workflow
    /// </summary>
    [ActionCategory( "Workflow Control" )]
    [Description( "Sets the workflow campus to the given campus attribute or primary campus of a given person attribute." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Workflow Set Campus" )]

    #region Block Attributes

    [WorkflowAttribute( "Campus",
        Key = AttributeKey.Campus,
        Description = "Workflow attribute that contains the Campus to use to set the workflow's campus. If both Person and Campus are provided, Campus takes precedence over the Person's Campus. If Campus is not provided, the Person's primary Campus will be used.",
        IsRequired = false,
        Order = 0,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.CampusFieldType" } )]

    [WorkflowAttribute( "Person",
        Key = AttributeKey.Person,
        Description = "Workflow attribute that contains the person to use to set the workflow's campus.",
        IsRequired = false,
        Order = 1,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.PersonFieldType" } )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "6C59E23A-EF4D-4392-8B06-7FC21278DD9F")]
    public class SetWorkflowCampus : ActionComponent
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string Person = "Person";
            public const string Campus = "Campus";
        }

        #endregion Attribute Keys

        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns><see cref="bool"/></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            var person = GetPersonFromAttributeValue( action, AttributeKey.Person, true, rockContext );
            var campus = GetEntityFromAttributeValue<Campus>( action, AttributeKey.Campus, true, rockContext );

            if ( person == null && campus == null )
            {
                return HandleError( "Neither a Person nor a Campus was provided. You must provide at least one of these.", errorMessages );
            }

            if ( campus == null )
            {
                // look for the Campus on the Person entity first
                campus = person.PrimaryCampus;

                if ( campus == null )
                {
                    // if the Person's PrimaryCampus was not defined, grab the Campus from the Person's primary family
                    var family = person.GetFamily( rockContext );

                    string msg = string.Format( "Could not find {0}'s Campus.", person.FullName );

                    if ( family == null )
                    {
                        return HandleError( msg, errorMessages );
                    }

                    campus = family.Campus;
                    if ( campus == null )
                    {
                        return HandleError( msg, errorMessages );
                    }
                }
            }

            action.Activity.Workflow.CampusId = campus.Id;
            action.AddLogEntry( string.Format( "Set Workflow Campus to '{0}' (ID: {1}).", campus.Name, campus.Id ) );

            return true;
        }

        private bool HandleError( string msg, List<string> errorMessages )
        {
            errorMessages.Add( msg );

            return false;
        }
    }
}
