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
    public partial class BlogLiquidRollupItems : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {

            // add blog liquid templates
            Sql( @"DECLARE @AttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '8026FEA1-35C1-41CF-9D09-E8B1DB6CBDA8')
                  DECLARE @BlockId int = (SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = 'C146309D-E282-4FD5-94D9-CD4D0853AF09')
  
                  UPDATE [AttributeValue]
	                SET [Value] = '{% include ''BlogItemDetail'' %}'
	                WHERE [AttributeId] = @AttributeId AND [EntityId] = @BlockId AND [ModifiedDateTime] IS NULL" );

            Sql( @"DECLARE @AttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '8026FEA1-35C1-41CF-9D09-E8B1DB6CBDA8')
                  DECLARE @BlockId int = (SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = 'AE2D8454-AE86-40DF-A57A-C9F30B8AB50F')

                  UPDATE [AttributeValue]
	                SET [Value] = '{% include ''BlogItemList'' %}'
	                WHERE [AttributeId] = @AttributeId AND [EntityId] = @BlockId AND [ModifiedDateTime] IS NULL" );
            
            // process workflows job editable
            Sql( @"UPDATE [ServiceJob]
            SET [IsSystem] = 0
            WHERE [Guid] = '35EABBDB-1EFA-46F1-86D4-4199FFA2D9A7'" );
            
            // staff profile security
            // Staff Users
            RockMigrationHelper.AddSecurityAuthForBlock( "B5C1FDB6-0224-43E4-8E26-6B2EAF86253A", 0, "Edit", true, "2C112948-FF4C-46E7-981A-0257681EADF4", Model.SpecialRole.None, "59F07D32-B66B-4A85-84F5-7A0594293CA9" ); // Bio
            RockMigrationHelper.AddSecurityAuthForBlock( "4CC50BE8-72ED-43E0-8D11-7E2A590453CC", 0, "Edit", true, "2C112948-FF4C-46E7-981A-0257681EADF4", Model.SpecialRole.None, "05854901-73DE-4737-BA53-FB1267190817" ); // Family Members
            RockMigrationHelper.AddSecurityAuthForBlock( "D5CD2A87-D34E-42E0-A58A-5FFC9B72F136", 0, "Edit", true, "2C112948-FF4C-46E7-981A-0257681EADF4", Model.SpecialRole.None, "2851A6B9-768D-4F99-BB74-30E73A9BCC90" ); // Known Relationships
            RockMigrationHelper.AddSecurityAuthForBlock( "59C7EA79-2073-4EA9-B439-7E74F06E8F5B", 0, "Edit", true, "2C112948-FF4C-46E7-981A-0257681EADF4", Model.SpecialRole.None, "6A47DA72-EEF4-44D4-B8A7-E371ABF7F387" ); // Person Detail
            RockMigrationHelper.AddSecurityAuthForBlock( "E3A7A9E0-9321-4153-928F-94AB0B576A3B", 0, "Edit", true, "2C112948-FF4C-46E7-981A-0257681EADF4", Model.SpecialRole.None, "7EE72B4F-980C-449C-A65A-CBD87C059F28" ); // Family Detail

            // Staff-Like Users
            RockMigrationHelper.AddSecurityAuthForBlock( "B5C1FDB6-0224-43E4-8E26-6B2EAF86253A", 1, "Edit", true, "300BA2C8-49A3-44BA-A82A-82E3FD8C3745", Model.SpecialRole.None, "7808DD7C-9404-4200-A5F6-03822EBDE639" ); // Bio
            RockMigrationHelper.AddSecurityAuthForBlock( "4CC50BE8-72ED-43E0-8D11-7E2A590453CC", 1, "Edit", true, "300BA2C8-49A3-44BA-A82A-82E3FD8C3745", Model.SpecialRole.None, "CC74981D-3DB2-4E3A-896F-DF08357D2388" ); // Family Members
            RockMigrationHelper.AddSecurityAuthForBlock( "D5CD2A87-D34E-42E0-A58A-5FFC9B72F136", 1, "Edit", true, "300BA2C8-49A3-44BA-A82A-82E3FD8C3745", Model.SpecialRole.None, "6FF76BD0-4395-4C30-97A7-F73F3A0A363E" ); // Known Relationships
            RockMigrationHelper.AddSecurityAuthForBlock( "59C7EA79-2073-4EA9-B439-7E74F06E8F5B", 1, "Edit", true, "300BA2C8-49A3-44BA-A82A-82E3FD8C3745", Model.SpecialRole.None, "32937D01-0F34-41A0-94F0-9351D8A43051" ); // Person Detail
            RockMigrationHelper.AddSecurityAuthForBlock( "E3A7A9E0-9321-4153-928F-94AB0B576A3B", 1, "Edit", true, "300BA2C8-49A3-44BA-A82A-82E3FD8C3745", Model.SpecialRole.None, "950C674A-8685-4FD3-9A15-4BA0B84C3745" ); // Family Detail
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
