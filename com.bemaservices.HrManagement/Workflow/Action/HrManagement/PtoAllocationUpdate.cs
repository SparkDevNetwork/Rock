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
using System.Linq;
using System.Reflection;
using com.bemaservices.HrManagement.Model;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Workflow;

namespace com.bemaservices.HrManagement.Workflow.Action
{
    /// <summary>
    /// Sets an entity property.
    /// </summary>
    [ActionCategory( "BEMA Services > HR Management" )]
    [Description( "Adds or Updates a PTO Allocation." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "PTO Allocation Update" )]

    // Existing PtoAllocation
    [WorkflowAttribute( "Existing Pto Allocation", "The Pto Allocation to update.",
        false, "", "", 0, PTO_ALLOCATION_ATTRIBUTE_KEY, new string[] { "com.bemaservices.HrManagement.Field.Types.PtoAllocationFieldType" } )]

    // Input Fields
    [WorkflowAttribute( "Person", "The person or an attribute that contains the person of the Pto Allocation. <span class='tip tip-lava'></span>",
        true, "", "", 1, PERSON_KEY, new string[] { "Rock.Field.Types.PersonFieldType" } )]
    [WorkflowAttribute( "Pto Type", "The Pto Type or an attribute that contains the Pto Type of the Pto Allocation. <span class='tip tip-lava'></span>",
        true, "", "", 2, PTO_TYPE_KEY, new string[] { "com.bemaservices.HrManagement.Field.Types.PtoTypeFieldType" } )]
    [WorkflowTextOrAttribute( "Start Date", "Attribute Value", "The start date or an attribute that contains the start date of the Pto Allocation. <span class='tip tip-lava'></span>",
        true, "", "", 3, STARTDATE_KEY, new string[] { "Rock.Field.Types.DateFieldType" } )]
    [WorkflowTextOrAttribute( "End Date", "Attribute Value", "The end date or an attribute that contains the end date of the Pto Allocation. <span class='tip tip-lava'></span>",
        false, "", "", 4, ENDDATE_KEY, new string[] { "Rock.Field.Types.DateFieldType" } )]
    [WorkflowTextOrAttribute( "Hours", "Attribute Value", "The hours or an attribute that contains the hours of the Pto Allocation. <span class='tip tip-lava'></span>",
        true, "", "", 5, HOURS_KEY, new string[] { "Rock.Field.Types.DecimalFieldType" } )]
    [EnumField( "Source", "The source of the Pto Request", typeof( PtoAllocationSourceType ),
        true, "Manual", "", 6, SOURCE_TYPE_KEY )]

    public class PtoAllocationUpdate : ActionComponent
    {
        private const string PTO_ALLOCATION_ATTRIBUTE_KEY = "PTO_ALLOCATION_ATTRIBUTE_KEY";
        private const string STARTDATE_KEY = "STARTDATE_KEY";
        private const string ENDDATE_KEY = "ENDDATE_KEY";
        private const string HOURS_KEY = "HOURS_KEY";
        private const string PTO_TYPE_KEY = "PTO_TYPE_KEY";
        private const string PERSON_KEY = "PERSON_KEY";
        private const string SOURCE_TYPE_KEY = "SOURCE_TYPE_KEY";

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
            Person person = null;
            PtoType ptoType = null;

            Guid? ptoAllocationGuid = null;
            PtoAllocation ptoAllocation = null;
            var ptoAllocationService = new PtoAllocationService( rockContext );

            // get the existing Pto Allocation
            Guid ptoAllocationAttributeGuid = GetAttributeValue( action, PTO_ALLOCATION_ATTRIBUTE_KEY ).AsGuid();

            if ( !ptoAllocationAttributeGuid.IsEmpty() )
            {
                ptoAllocationGuid = action.GetWorklowAttributeValue( ptoAllocationAttributeGuid ).AsGuidOrNull();

                if ( ptoAllocationGuid.HasValue )
                {
                    ptoAllocation = ptoAllocationService.Get( ptoAllocationGuid.Value );
                }
            }

            // get person alias guid
            Guid personAliasGuid = Guid.Empty;
            string personAttribute = GetAttributeValue( action, PERSON_KEY );

            Guid guid = personAttribute.AsGuid();
            if ( !guid.IsEmpty() )
            {
                var attribute = AttributeCache.Get( guid, rockContext );
                if ( attribute != null )
                {
                    string value = action.GetWorklowAttributeValue( guid );
                    personAliasGuid = value.AsGuid();
                }

                if ( personAliasGuid != Guid.Empty )
                {
                    person = new PersonAliasService( rockContext ).Queryable()
                                    .Where( p => p.Guid.Equals( personAliasGuid ) )
                                    .Select( p => p.Person )
                                    .FirstOrDefault();
                }
                else
                {
                    errorMessages.Add( "The person could not be found!" );
                }
            }


            // get pto type guid
            Guid ptoTypeGuid = Guid.Empty;
            string ptoTypeAttribute = GetAttributeValue( action, PTO_TYPE_KEY );

            Guid ptoTypeAttributeGuid = ptoTypeAttribute.AsGuid();
            if ( !ptoTypeAttributeGuid.IsEmpty() )
            {
                var attribute = AttributeCache.Get( ptoTypeAttributeGuid, rockContext );
                if ( attribute != null )
                {
                    string value = action.GetWorklowAttributeValue( ptoTypeAttributeGuid );
                    ptoTypeGuid = value.AsGuid();
                }

                if ( ptoTypeGuid != Guid.Empty )
                {
                    ptoType = new PtoTypeService( rockContext ).Queryable()
                                    .Where( p => p.Guid.Equals( ptoTypeGuid ) )
                                    .FirstOrDefault();
                }
                else
                {
                    errorMessages.Add( "The pto type could not be found!" );
                }
            }

            var mergeFields = GetMergeFields( action );
            var startDate = GetAttributeValue( action, STARTDATE_KEY, true ).ResolveMergeFields( mergeFields ).AsDateTime();
            if ( !startDate.HasValue )
            {
                errorMessages.Add( "The start date could not be found!" );
            }

            var hours = GetAttributeValue( action, HOURS_KEY, true ).ResolveMergeFields( mergeFields ).AsDecimalOrNull();
            if ( !hours.HasValue )
            {
                errorMessages.Add( "The hours could not be found!" );
            }

            var endDate = GetAttributeValue( action, ENDDATE_KEY, true ).ResolveMergeFields( mergeFields ).AsDateTime();
            var sourceType = GetAttributeValue( action, SOURCE_TYPE_KEY, true ).ResolveMergeFields( mergeFields ).ConvertToEnum<PtoAllocationSourceType>( PtoAllocationSourceType.Manual );

            if ( person != null && ptoType != null && hours.HasValue && startDate.HasValue )
            {

                if ( ptoAllocation == null )
                {
                    ptoAllocation = new PtoAllocation();
                    ptoAllocationService.Add( ptoAllocation );
                }

                if ( ptoAllocation != null )
                {
                    ptoAllocation.PtoType = ptoType;
                    ptoAllocation.PtoTypeId = ptoType.Id;
                    ptoAllocation.PersonAlias = person.PrimaryAlias;
                    ptoAllocation.PersonAliasId = person.PrimaryAliasId.Value;
                    ptoAllocation.StartDate = startDate.Value;
                    ptoAllocation.EndDate = endDate;
                    ptoAllocation.Hours = hours.Value;
                    ptoAllocation.PtoAllocationSourceType = sourceType;
                    
                    rockContext.SaveChanges();
                }
                else
                {
                    errorMessages.Add( "The Pto Allocation could not be created!" );
                }
            }

            errorMessages.ForEach( m => action.AddLogEntry( m, true ) );

            return true;
        }

    }
}
