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
    public partial class RockRename : Rock.Migrations.RockMigration1
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
UPDATE [Page] SET [Description] = 'Main Rock Administration Homepage' WHERE [Guid] = '20f97a93-7949-4c2a-8a5e-c756fe8585ca'
UPDATE [Page] SET [Description] = 'Used to organized the various settings and configuration pages for the Rock system.' WHERE [Guid] = '550a898c-edea-48b5-9c58-b20ec13af13b' 
UPDATE [Page] SET [Description] = 'The items below represent general configuration settings for Rock. Please use caution when making these changes as improper values could cause the system to become unresponsive.' WHERE [Guid] = '0b213645-fa4e-44a5-8e4c-b2d8ef054985'
UPDATE [Page] SET [Description] = 'Displays status and performance information for the currently running instance of Rock' WHERE [Guid] = '8a97cc93-3e93-4286-8440-e5217b65a904'
UPDATE [BinaryFile] SET [FileName] = 'staff-directory.png' WHERE [Guid] = 'ca708a3a-d3b0-4f41-9fb4-ea74b5c79815'
UPDATE [BlockType] SET [Description] ='Displays status and performance information for the currently running instance of Rock' WHERE [Guid] = 'de08efd7-4cf9-4bd5-9f72-c0151fd08523'
UPDATE [DefinedValue] SET [Description] = REPLACE([Description], 'rockchms.com', 'rockrms.com') WHERE [Guid] = 'b1ce8eca-6584-45dc-a022-40490fc753e8'
UPDATE [DefinedValue] SET [Description] = REPLACE([Description], 'rockchms.com', 'rockrms.com') WHERE [Guid] = 'b0e46522-921f-47aa-b548-f0072f22b903'
UPDATE [Attribute] SET [DefaultValue] = REPLACE([DefaultValue], 'rockchms.com/F/rockchms', 'rockrms.com/F/rock') WHERE [Guid] = '306e7e7c-9416-4098-9c25-488380b940a5'
UPDATE [Attribute] SET [Description] = REPLACE([Description], 'rockchms.com', 'rockrms.com'), [DefaultValue] = REPLACE([DefaultValue], '/RockChMS', '/Rock') WHERE [GUID] IN ('06e0e3fc-9a1c-43af-8b3b-c760f9951012','49ad7ad6-9bac-4743-b1e8-b917f6271924')
UPDATE [HtmlContent] SET [Content] = REPLACE([Content], 'Rock ChMS', 'Rock') WHERE [Guid] = 'f76b1c2c-baf1-45e1-844c-459417fefc69'
UPDATE [HtmlContent] SET [Content] = REPLACE([Content], 'rockchms.com', 'sparkdevnetwork.com') WHERE [Guid] IN ('62aa523d-dc9a-444d-b95b-6ae24ee13124','0af1d334-13f8-4799-9158-304b9fda235f')
UPDATE [HtmlContent] SET [Content] = REPLACE([Content], 'rockchms.com', 'rockrms.com') WHERE [Guid] IN ('9471748b-2690-41a1-a623-08105a2ee72c','007ea905-d5d3-4dc5-ad0b-2c1e3935e452','ca44278d-7a6f-45cf-96bc-b24ab35cedb0')
UPDATE [HtmlContent] SET [Content] = REPLACE([Content], 'Rock ChMS', 'Rock') WHERE [Guid] IN ('9471748b-2690-41a1-a623-08105a2ee72c','b89605cf-c111-4a1b-9228-29a78ea9da25','7ef64acf-a66e-4086-900b-6832ec65cc9c','ca44278d-7a6f-45cf-96bc-b24ab35cedb0')
UPDATE [EmailTemplate] SET [Body] = REPLACE([Body], 'Rock ChMS', 'Rock') WHERE [Guid] = '75cb0a4a-b1c5-4958-adeb-8621bd231520'
UPDATE [EmailTemplate] SET [Body] = REPLACE([Body], 'occurred in the Rock', 'occurred') WHERE [Guid] = '75cb0a4a-b1c5-4958-adeb-8621bd231520'
UPDATE [Site] SET [Description] = REPLACE([Description], 'Rock ChMS', 'Rock'), [Theme] = REPLACE([Theme], 'RockChMS', 'Rock') WHERE [Guid] = 'c2d29296-6a87-47a9-a753-ee4e9159c4c4'
UPDATE [AttributeValue] SET [Value] = REPLACE([Value], 'Rock-ChMS', 'Rock') WHERE [Guid] IN ('a1cc351d-ba4e-4a9a-88e5-765194b87097','21701986-9bdb-41e4-8e6d-3f186f61a573')
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
