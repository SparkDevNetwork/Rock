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
    public partial class MiscUIChanges : Rock.Migrations.RockMigration3
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // page using wrong layout, probably missed when we added the fullwidthpanel
            Sql(@"UPDATE [Page]
                    SET [LayoutId] = 13
                    WHERE [Guid] = 'BDD7B906-4D42-43C0-8DBB-B89A566734D8'");

            // unset the cleanup job as a system job to allow admins to set the configuration
            Sql(@"UPDATE [ServiceJob]
                    SET [IsSystem] = 0
                    WHERE [Guid] = '1A8238B1-038A-4295-9FDE-C6D93002A5D7'");

            // delete two left over attributes from when we deleted the 'video class' group type
            Sql(@"DELETE FROM [Attribute]
                    WHERE [Guid] in ('C5E3B8B6-F5F7-4304-9BD5-90FAAE39830F', '5315301B-7829-4A67-BDF6-5C133BBF1826')");

            // add attribute categories to two org settings that are missing them
            Sql(@"INSERT INTO [AttributeCategory] ([AttributeId], [CategoryId])
                    VALUES (519,5)");
            Sql(@"INSERT INTO [AttributeCategory] ([AttributeId], [CategoryId])
                    VALUES (544,9)");

            // delete two org settings for sendgrid that are not being used and should be transport settings
            Sql(@"DELETE FROM [Attribute]
                    WHERE [Guid] in ('E49FE2ED-F67A-4E60-B297-A7C5220C056C', 'A96616C8-EFC1-4E7D-A6E1-76FCA2E5AB52')");

            // move data filters next to data transformation on the systems settings page since they are similar in function
            Sql(@"UPDATE [Page]
                    SET [Order] = 19
                    WHERE [Guid] = '9C569E6B-F745-40E4-B91B-A518CD6C2922'");
            Sql(@"UPDATE [Page]
                    SET [Order] = 24
                    WHERE [Guid] = '21DA6141-0A03-4F00-B0A8-3B110FBE2438'");

            // shortened long page name
            Sql(@"UPDATE [Page]
                    SET [InternalName] = 'Ad Types Detail', [PageTitle] = 'Ad Types Detail', [BrowserTitle] = 'Ad Types Detail'
                    WHERE [Guid] = '36826974-C613-48F2-877E-460C4EC90CCE'");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
