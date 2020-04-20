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
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class PendingPeopleDataViewRecordTypeFilter : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Create Rock.Reporting.DataFilter.PropertyFilter RecordType DataViewFilter for DataView: Pending People
            Sql( @"
            IF NOT EXISTS (SELECT * FROM DataViewFilter where [Guid] = '61185692-E5D3-40CD-A7CB-03FE51BE492B') BEGIN    
                DECLARE
                    @ParentDataViewFilterId int = (select Id from DataViewFilter where [Guid] = '9EB9455C-ADBA-435B-869B-264178484944'),
                    @DataViewFilterEntityTypeId int = (select Id from EntityType where [Guid] = '03F0D6AC-D181-48B6-B4BC-1F2652B55323')

                INSERT INTO [DataViewFilter] (ExpressionType, ParentId, EntityTypeId, Selection, [Guid]) 
                values (0,@ParentDataViewFilterId,@DataViewFilterEntityTypeId,'[""Property_RecordTypeValueId"",""36CF10D6-C695-413D-8E7C-4546EFEF385E""]','61185692-E5D3-40CD-A7CB-03FE51BE492B')
            END");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Delete RecordType DataViewFilter for DataView: Pending People
            Sql( @"DELETE FROM DataViewFilter where [Guid] = '61185692-E5D3-40CD-A7CB-03FE51BE492B'");
        }
    }
}
