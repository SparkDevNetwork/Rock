// <copyright>
// Copyright by Central Christian Church
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace com.centralaz.Workflow.Migrations
{
    [MigrationNumber( 1, "1.5.0" )]
    public class AddCheckInWorkflowActions : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            // Filter Groups By First Match
            RockMigrationHelper.UpdateEntityType( "com.centralaz.Workflow.Action.CheckIn.FilterGroupsByFirstMatch", "8A7C593C-CAB8-4407-A93A-6C455AE70943", false, true );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "8A7C593C-CAB8-4407-A93A-6C455AE70943", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "A56348F4-9CE7-469B-B815-D18FA3ABA19A" ); // com.centralaz.Workflow.Action.CheckIn.FilterGroupsByFirstMatch:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "8A7C593C-CAB8-4407-A93A-6C455AE70943", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Remove", "Remove", "Select 'Yes' if groups should be be removed. Select 'No' if they should just be marked as excluded.", 0, @"True", "86BEAADE-1048-42D7-9E4A-73300606D478" ); // com.centralaz.Workflow.Action.CheckIn.FilterGroupsByFirstMatch:Remove
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "8A7C593C-CAB8-4407-A93A-6C455AE70943", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Ascending Group Order", "UseAscendingGroupOrder", "Select 'Yes' if groups should be ordered as seen on screen (from top to bottom); choose false otherwise.", 0, @"True", "D13BB4BB-0715-4AA4-AFE1-AEDF51D3274C" ); // com.centralaz.Workflow.Action.CheckIn.FilterGroupsByFirstMatch:Use Ascending Group Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "8A7C593C-CAB8-4407-A93A-6C455AE70943", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "9CF3609C-159C-4E09-A023-AC2789E8546C" ); // com.centralaz.Workflow.Action.CheckIn.FilterGroupsByFirstMatch:Order

            // Filter Groups By Age Or Grade (currently not in the Workflow)
            RockMigrationHelper.UpdateEntityType( "com.centralaz.Workflow.Action.CheckIn.FilterGroupsByAgeOrGrade", "10E2CF3E-2758-48CE-BB74-5FFD1E3EC8FD", false, true );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "10E2CF3E-2758-48CE-BB74-5FFD1E3EC8FD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "92D70E9B-DB29-4266-82D9-A3BFB70D70B8" ); // com.centralaz.Workflow.Action.CheckIn.FilterGroupsByAgeOrGrade:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "10E2CF3E-2758-48CE-BB74-5FFD1E3EC8FD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Remove", "Remove", "Select 'Yes' if groups should be be removed.  Select 'No' if they should just be marked as excluded.", 0, @"True", "2CA19CBA-4E7E-4711-A3B0-37DA6AA12883" ); // com.centralaz.Workflow.Action.CheckIn.FilterGroupsByAgeOrGrade:Remove
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "10E2CF3E-2758-48CE-BB74-5FFD1E3EC8FD", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Group Age Range Attribute", "GroupAgeRangeAttribute", "Select the attribute used to define the age range of the group", 2, @"43511B8F-71D9-423A-85BF-D1CD08C1998E", "C103692D-F0C1-4C03-B41A-F29B111D04A0" ); // com.centralaz.Workflow.Action.CheckIn.FilterGroupsByAgeOrGrade:Group Age Range Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "10E2CF3E-2758-48CE-BB74-5FFD1E3EC8FD", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "5C1231A0-89C9-41AF-AA2F-4FE30B808030" ); // com.centralaz.Workflow.Action.CheckIn.FilterGroupsByAgeOrGrade:Order

        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteEntityType( "8A7C593C-CAB8-4407-A93A-6C455AE70943" );
            RockMigrationHelper.DeleteEntityType( "10E2CF3E-2758-48CE-BB74-5FFD1E3EC8FD" );
        }
    }
}
