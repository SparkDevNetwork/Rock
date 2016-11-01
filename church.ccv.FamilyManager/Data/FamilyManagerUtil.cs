// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using System.Data.Entity;
using church.ccv.FamilyManager.Models;

namespace church.ccv.FamilyManager
{
    // Class containing functions related to launching the Mobile App
    public static class Launch
    {
        // the attribute Id for the Family Manager's version
        const int FamilyManagerVersionAttributeId = 0;//29469;

        // Returns the startup data for the Mobile App
        public static CoreData BuildCoreData()
        {
            RockContext rockContext = new RockContext();

            CoreData coreData = new CoreData();

            // setup the campuses
            coreData.Campuses = new List<Campus>( );
            foreach (CampusCache campus in CampusCache.All(false))
            {
                // only include campuses with a location
                if( campus.Location.Longitude.HasValue && campus.Location.Latitude.HasValue )
                {
                    Campus campusModel = new Campus();
                    campusModel.Guid = campus.Guid;
                    campusModel.Id = campus.Id;
                    campusModel.Name = campus.Name;
                
                    campusModel.Location = new Location( );
                    campusModel.Location.SetLocationPointFromLatLong( campus.Location.Latitude.Value, campus.Location.Longitude.Value );

                    coreData.Campuses.Add( campusModel );
                }
            }

            // get needed defined types
            DefinedTypeService definedTypeService = new DefinedTypeService( rockContext );

            coreData.MaritalStatus = definedTypeService.GetByGuid( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS.AsGuid( ) ).DefinedValues.ToList( );
            coreData.SchoolGrades = definedTypeService.GetByGuid( Rock.SystemGuid.DefinedType.SCHOOL_GRADES.AsGuid( ) ).DefinedValues.ToList( );
            coreData.SourceOfVisit = definedTypeService.GetByGuid( Guids.SOURCE_OF_VISIT_DEFINED_TYPE.AsGuid( ) ).DefinedValues.ToList( );
            
            // get needed attributes
            AttributeService attributeService = new AttributeService( rockContext );
            coreData.FirstTimeVisit = attributeService.Get( Guids.FIRST_TIME_VISIT_DEFINED_TYPE.AsGuid( ) );

            // here, we can control what attributes Family Manager will display!
            coreData.PersonAttributes = new List<PersonAttribute>( );
                coreData.PersonAttributes.Add( new PersonAttribute( )
                {
                    Filter = PersonAttribute.FamilyRole.Child,
                    Required = false,
                    Attribute = attributeService.Get( 676 )
                });

                coreData.PersonAttributes.Add( new PersonAttribute( )
                {
                    Filter = PersonAttribute.FamilyRole.Child,
                    Required = false,
                    Attribute = attributeService.Get( 715 )
                });

                coreData.PersonAttributes.Add( new PersonAttribute( )
                {
                    Filter = PersonAttribute.FamilyRole.Child,
                    Required = false,
                    Attribute = attributeService.Get( 1055 )
                });

                coreData.PersonAttributes.Add( new PersonAttribute( )
                {
                    Filter = PersonAttribute.FamilyRole.Child,
                    Required = false,
                    Attribute = attributeService.Get( 1068)
                });
            

            // get needed group type roles
            GroupTypeRoleService groupTypeRoleService = new GroupTypeRoleService( rockContext );
            coreData.FamilyMember_Adult_GroupRole = groupTypeRoleService.Get( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid( ) );
            coreData.FamilyMember_Child_GroupRole = groupTypeRoleService.Get( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid( ) );

            coreData.CanCheckIn_GroupRole = groupTypeRoleService.Get( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_CAN_CHECK_IN.AsGuid( ) );
            coreData.AllowedCheckInBy_GroupRole = groupTypeRoleService.Get( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_ALLOW_CHECK_IN_BY.AsGuid( ) );
            
            return coreData;
        }
    }

    // Common reusable functions for supporting the Mobile App
    public static class Util
    {
        public static void LaunchWorkflow( RockContext rockContext, WorkflowType workflowType, GroupMember groupMember )
        {
            try
            {
                List<string> workflowErrors;
                var workflow = Workflow.Activate( workflowType, workflowType.Name );
                new WorkflowService( rockContext ).Process( workflow, groupMember, out workflowErrors );
            }
            catch (Exception ex)
            {
                ExceptionLogService.LogException( ex, null );
            }
        }
    }
}
