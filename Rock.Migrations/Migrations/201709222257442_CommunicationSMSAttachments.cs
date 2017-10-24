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
    public partial class CommunicationSMSAttachments : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.CommunicationAttachment", "CommunicationType", c => c.Int(nullable: false));
            AddColumn("dbo.CommunicationTemplateAttachment", "CommunicationType", c => c.Int(nullable: false));

            // default any existing CommunicationAttachment records to a CommunicationType of 'Email'
            Sql( "UPDATE [CommunicationAttachment] SET [CommunicationType] = 1" );

            // default any existing CommunicationTemplateAttachment records to a CommunicationType of 'Email'
            Sql( "UPDATE [CommunicationTemplateAttachment] SET [CommunicationType] = 1" );

            // JE: Add LifeStageLevel to MetaPersonicxLifestageCluster/Group
            AddColumn( "dbo.MetaPersonicxLifestageCluster", "LifeStageLevel", c => c.String( maxLength: 50 ) );
            AddColumn( "dbo.MetaPersonicxLifestageGroup", "LifeStageLevel", c => c.String( maxLength: 50 ) );

            // MP: Add IsActive to CommunicationTemplate
            AddColumn( "dbo.CommunicationTemplate", "IsActive", c => c.Boolean( nullable: false ) );
            Sql( "UPDATE [CommunicationTemplate] SET [IsActive] = 1" );

            // JE: Add default communication template (and update Blank template)
            Sql( MigrationSQL._201709222257442_CommunicationSMSAttachments_AddCommunicationTemplates );

            // JE: Default Communication List
            Sql( @"DECLARE @DataViewId int = (SELECT TOP 1 [Id] FROM [DataView] WHERE [Guid] = '0DA5F82F-CFFE-45AF-B725-49B3899A1F72')
	DECLARE @ListGroupTypeId int = (SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = 'D1D95777-FFA3-CBB3-4A6D-658706DAED33')

	INSERT INTO [Group]
	([GroupTypeId], [Name], [IsActive], [Guid], [SyncDataViewId], [IsSystem], [IsSecurityRole], [Order])
	VALUES
	(@ListGroupTypeId, 'Adult Members and Attendees', 1,'D3DC9A8E-43D9-43AB-BB48-94788F4B1A42', @DataViewId, 0, 0, 0)
" );
           
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.CommunicationTemplateAttachment", "CommunicationType");
            DropColumn("dbo.CommunicationTemplate", "IsActive");
            DropColumn("dbo.CommunicationAttachment", "CommunicationType");
            DropColumn("dbo.MetaPersonicxLifestageGroup", "LifeStageLevel");
            DropColumn("dbo.MetaPersonicxLifestageCluster", "LifeStageLevel");
        }
    }
}
