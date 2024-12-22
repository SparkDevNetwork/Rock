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
    public partial class AddStartAndEndDateToAdaptiveModule : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.AdaptiveMessageAdaptation", "StartDate", c => c.DateTime(storeType: "date"));
            AddColumn("dbo.AdaptiveMessageAdaptation", "EndDate", c => c.DateTime(storeType: "date"));
            AddColumn("dbo.AdaptiveMessage", "StartDate", c => c.DateTime(storeType: "date"));
            AddColumn("dbo.AdaptiveMessage", "EndDate", c => c.DateTime(storeType: "date"));

            Sql( @"
DECLARE @PageId INT = (SELECT [Id] FROM [Page] WHERE [Guid] = '73112D38-E051-4452-AEF9-E473EEDD0BCB')
UPDATE [Block] SET [PageId] = @PageId WHERE [Guid] = '9912C605-6699-4484-B88B-469171F2F693'

UPDATE [Block] SET [PageId] = @PageId WHERE [Guid] = 'F10D62AB-46D0-4C01-9A34-FAE2BE8CFDB6'

UPDATE [Block] SET [PageId] = @PageId WHERE [Guid] = '859B5FE9-9068-40EC-B7AD-78598BEDC6AA'

UPDATE [Page] SET [PageTitle] = 'Message Adaptation Detail', [BrowserTitle] = 'Message Adaptation Detail' WHERE [Guid] = 'FE12A90C-C20F-4F23-A1B1-528E0C5FDA83'

UPDATE [Page] SET [ParentPageId] = @PageId WHERE [Guid] = 'FE12A90C-C20F-4F23-A1B1-528E0C5FDA83'

DELETE FROM [Page] WHERE [Guid] = 'BEC30E90-0434-43C4-B839-09E11775E497'
" );
            RockMigrationHelper.AddBlockAttributeValue( "2DBFA85E-BA20-4FF2-8372-80688C8B9CD1", "F9E1AD87-CA8A-49DF-84AC-8F18895FCB87", @"73112d38-e051-4452-aef9-e473eedd0bcb" );
            RockMigrationHelper.AddBlockAttributeValue( "9912C605-6699-4484-B88B-469171F2F693", "D2596ADF-4455-42A4-848F-6DFD816C2867", @"fas fa-comment-alt" );
            RockMigrationHelper.DeleteAttribute( "8562A1F2-2942-4721-9585-18FC3E385F5B" );
            RockMigrationHelper.AddBlockAttributeValue( "859B5FE9-9068-40EC-B7AD-78598BEDC6AA", "E4F286BC-9338-4E17-ABFF-4578793B7A54", @"fe12a90c-c20f-4f23-a1b1-528e0c5fda83" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.AdaptiveMessage", "EndDate");
            DropColumn("dbo.AdaptiveMessage", "StartDate");
            DropColumn("dbo.AdaptiveMessageAdaptation", "EndDate");
            DropColumn("dbo.AdaptiveMessageAdaptation", "StartDate");
        }
    }
}
