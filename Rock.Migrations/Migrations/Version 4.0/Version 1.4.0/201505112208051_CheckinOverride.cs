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
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class CheckinOverride : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // add attribute
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "011E9F5A-60D4-4FF5-912A-290881E37EAF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Override", "Override", "Used to enable age/grade override.", 0, "False", "66EF6CB1-1A96-2F81-4534-3BCA5C33D4CD" );
        
            // add action filters for this attribute
            Sql( @"
                  --Filter GroupTypes by Age
                  UPDATE [WorkflowActionType]
                   SET [CriteriaAttributeGuid] = '66EF6CB1-1A96-2F81-4534-3BCA5C33D4CD'
		                , [CriteriaComparisonType] = 1
		                , [CriteriaValue] = 'False'
	                WHERE [Guid] = 'F01D08DB-5AAE-417B-85D2-3FC7DAB44CBC'

                  -- Filter GroupTypes by Grade
                  UPDATE [WorkflowActionType]
                   SET [CriteriaAttributeGuid] = '66EF6CB1-1A96-2F81-4534-3BCA5C33D4CD'
		                , [CriteriaComparisonType] = 1
		                , [CriteriaValue] = 'False'
	                WHERE [Guid] = 'BB3C824F-3B77-4C52-A173-ACCE9F60BA3E'

                  -- Filter Groups by Age
                  UPDATE [WorkflowActionType]
                   SET [CriteriaAttributeGuid] = '66EF6CB1-1A96-2F81-4534-3BCA5C33D4CD'
		                , [CriteriaComparisonType] = 1
		                , [CriteriaValue] = 'False'
	                WHERE [Guid] = 'BB45E6E1-C39A-42A2-B988-490382DB7977'

                  -- Filter Groups by Grade
                  UPDATE [WorkflowActionType]
                   SET [CriteriaAttributeGuid] = '66EF6CB1-1A96-2F81-4534-3BCA5C33D4CD'
		                , [CriteriaComparisonType] = 1
		                , [CriteriaValue] = 'False'
	                WHERE [Guid] = '6D8317FB-8AAB-4533-A472-01FA572478D7'" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // delete attribute
            RockMigrationHelper.DeleteAttribute( "66EF6CB1-1A96-2F81-4534-3BCA5C33D4CD" );

            // remove action filter
            // add action filters for this attribute
            Sql( @"
                  --Filter GroupTypes by Age
                  UPDATE [WorkflowActionType]
                   SET [CriteriaAttributeGuid] = null
		                , [CriteriaValue] = ''
	                WHERE [Guid] = 'F01D08DB-5AAE-417B-85D2-3FC7DAB44CBC'

                  -- Filter GroupTypes by Grade
                  UPDATE [WorkflowActionType]
                   SET [CriteriaAttributeGuid] = null
		                , [CriteriaValue] = 'False'
	                WHERE [Guid] = 'BB3C824F-3B77-4C52-A173-ACCE9F60BA3E'

                  -- Filter Groups by Age
                  UPDATE [WorkflowActionType]
                   SET [CriteriaAttributeGuid] = null
		                , [CriteriaValue] = 'False'
	                WHERE [Guid] = 'BB45E6E1-C39A-42A2-B988-490382DB7977'

                  -- Filter Groups by Grade
                  UPDATE [WorkflowActionType]
                   SET [CriteriaAttributeGuid] =  null
		                , [CriteriaValue] = 'False'
	                WHERE [Guid] = '6D8317FB-8AAB-4533-A472-01FA572478D7'" );
        }
    }
}
