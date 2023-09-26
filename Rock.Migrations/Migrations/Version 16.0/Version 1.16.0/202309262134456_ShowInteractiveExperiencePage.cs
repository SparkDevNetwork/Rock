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
    public partial class ShowInteractiveExperiencePage : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            //  Show the "Interactive Experiences" Page in the menu
            Sql( "UPDATE [Page] SET [DisplayInNavWhen] = 0 WHERE [Guid] = '0C1C9100-C2B0-44DA-840D-DDD50B970FE2'" );

            // Add a page route to the "Interactive Experiences" page
            Sql( @"
                DECLARE @PageId INT = (SELECT [Id] FROM [Page] WHERE [Guid] = '0C1C9100-C2B0-44DA-840D-DDD50B970FE2')
                IF @PageId IS NOT NULL
                    AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'web/interactive-experiences') 
                    AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [Guid] = '4EE4F9D5-518B-4889-9C63-8EE2076E17E2')
                BEGIN
                    INSERT INTO [PageRoute] ([IsSystem],[PageId],[Route],[Guid])
                    VALUES( 1, @PageId, 'web/interactive-experiences', '4EE4F9D5-518B-4889-9C63-8EE2076E17E2' )
                END");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( "UPDATE [Page] SET [DisplayInNavWhen] = 2 WHERE [Guid] = '0C1C9100-C2B0-44DA-840D-DDD50B970FE2'" );
        }
    }
}
