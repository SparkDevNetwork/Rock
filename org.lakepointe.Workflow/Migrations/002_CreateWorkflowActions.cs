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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;

namespace org.lakepointe.Migrations
{

    [MigrationNumber( 2, "1.8.0" )]
    public class CreateWorkflowActions : Migration
    {
        private const string AddGroupsByAbsoluteGradeAndAge = "9b6540f3-e1c1-46aa-9092-9488614d78a0";
        private const string CustomCalculateLastAttended = "8838594e-6d06-455f-872c-c68ec9871f70";

        public override void Up()
        {
            RockMigrationHelper.UpdateEntityType( "org.lakepointe.Workflow.Action.CheckIn.AddGroupsByAbsoluteGradeAndAge", "Add Groups By Absolute Grade And Age", "org.lakepointe.Workflow.Action.CheckIn.AddGroupsByAbsoluteGradeAndAge, org.lakepointe.Workflow, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", false, true, AddGroupsByAbsoluteGradeAndAge );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( AddGroupsByAbsoluteGradeAndAge, "a75dfc58-7a1b-4799-bf31-451b2bbe38ff", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "920c3903-0bdc-4262-9fcb-1a9a1a4f861c" );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( AddGroupsByAbsoluteGradeAndAge, "1edafded-dfe6-4334-b019-6eecba89e05a", "Active", "Active", "Should Service be used?", 0, @"False", "78f5593b-7283-491f-9058-fd7ffb748733" );

            RockMigrationHelper.UpdateEntityType( "org.lakepointe.Workflow.Action.CheckIn.CustomCalculateLastAttended", "Custom Calculate Last Attended", "org.lakepointe.Workflow.Action.CheckIn.CustomCalculateLastAttended, org.lakepointe.Workflow, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", false, true, CustomCalculateLastAttended );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( CustomCalculateLastAttended, "a75dfc58-7a1b-4799-bf31-451b2bbe38ff", "AutoSelectDaysBack", "AutoSelectDaysBack", "The number of days to look back for previous check-in location.", 0, @"", "03774109-08db-4df9-a396-8fbfef010c93" );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( CustomCalculateLastAttended, "a75dfc58-7a1b-4799-bf31-451b2bbe38ff", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "c23dab69-6924-48cf-b422-ae59e1f22f87" );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( CustomCalculateLastAttended, "1edafded-dfe6-4334-b019-6eecba89e05a", "Active", "Active", "Should Service be used?", 0, @"False", "473bb4e1-7d07-427c-83ad-3763e133dcb6" );
        }

        public override void Down()
        {
        }
    }
}
