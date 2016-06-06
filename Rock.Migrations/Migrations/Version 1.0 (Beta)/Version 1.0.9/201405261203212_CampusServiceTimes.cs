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
    public partial class CampusServiceTimes : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.DeleteAttribute( "F5810A78-D2E2-4017-8B2A-73AE83B6725E" );

            AddColumn( "dbo.Campus", "ServiceTimes", c => c.String( maxLength: 500 ) );

            Sql( @"
    UPDATE [Attribute]
    SET [DefaultValue] = 'ascx, ashx, aspx, ascx.cs, ashx.cs, aspx.cs, cs, aspx.cs, php, exe, dll'
    WHERE [Guid] = '9FFB15C1-AA53-4FBA-A480-64C9B348C5E5'

    UPDATE [Page]
    SET [InternalName] = 'REST Keys',
    [PageTitle] = 'REST Keys',
    [BrowserTitle] = 'REST Keys'
    WHERE [Guid] = '881AB1C2-4E00-4A73-80CC-9886B3717A20'

    UPDATE [Page]
    SET [InternalName] = 'REST Key Detail',
    [PageTitle] = 'REST Key Detail',
    [BrowserTitle] = 'REST Key Detail'
    WHERE [Guid] = '594692AA-5647-4F9A-9488-AADB990FDE56'

    DECLARE @LayoutId int = (SELECT [Id] FROM [Layout] WHERE [Guid] = '195BCD57-1C10-4969-886F-7324B6287B75')
    UPDATE [Page] SET [LayoutId] = @LayoutId WHERE [Guid] = '64E16878-D5AE-40A5-94FE-C2E8BE62DF61'
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.Campus", "ServiceTimes");
        }
    }
}
