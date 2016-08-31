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
    public partial class CommunicationMedium : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
    DELETE [EntityType]
    WHERE [Name] LIKE 'Rock.Communication.Medium.%'
    UPDATE [EntityType] SET 
          [Name] = REPLACE( [Name], 'Channel', 'Medium' )
        , [AssemblyName] = REPLACE( [AssemblyName], 'Channel', 'Medium' )
    WHERE [Name] LIKE 'Rock.Communication.Channel.%'

    UPDATE [Page] SET
	      [InternalName] = 'Communication Mediums'
	    , [PageTitle] = 'Communication Mediums'
	    , [BrowserTitle] = 'Communication Mediums'
    WHERE [Guid] = '6FF35C53-F89F-4601-8543-2E2328C623F8'

    UPDATE [AttributeValue] SET
	    [Value] = 'Rock.Communication.MediumContainer, Rock'
    WHERE [Value] = 'Rock.Communication.ChannelContainer, Rock'
" );

            RenameColumn(table: "dbo.Communication", name: "ChannelEntityTypeId", newName: "MediumEntityTypeId");
            RenameColumn( table: "dbo.Communication", name: "ChannelDataJson", newName: "MediumDataJson" );
            RenameIndex( table: "dbo.Communication", name: "IX_ChannelEntityTypeId", newName: "IX_MediumEntityTypeId" );

            RenameColumn( table: "dbo.CommunicationTemplate", name: "ChannelEntityTypeId", newName: "MediumEntityTypeId" );
            RenameColumn( table: "dbo.CommunicationTemplate", name: "ChannelDataJson", newName: "MediumDataJson" );
            RenameIndex(table: "dbo.CommunicationTemplate", name: "IX_ChannelEntityTypeId", newName: "IX_MediumEntityTypeId");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
    UPDATE [EntityType] SET 
          [Name] = REPLACE( [Name], 'Medium', 'Channel' )
        , [AssemblyName] = REPLACE( [AssemblyName], 'Medium', 'Channel' )
    WHERE [Name] LIKE 'Rock.Communication.Medium.%'

    UPDATE [Page] SET
	      [InternalName] = 'Communication Channels'
	    , [PageTitle] = 'Communication Channels'
	    , [BrowserTitle] = 'Communication Channels'
    WHERE [Guid] = '6FF35C53-F89F-4601-8543-2E2328C623F8'

    UPDATE [AttributeValue] SET
	    [Value] = 'Rock.Communication.ChannelContainer, Rock'
    WHERE [Value] = 'Rock.Communication.MediumContainer, Rock'
" );

            RenameIndex(table: "dbo.CommunicationTemplate", name: "IX_MediumEntityTypeId", newName: "IX_ChannelEntityTypeId");
            RenameColumn( table: "dbo.CommunicationTemplate", name: "MediumDataJson", newName: "ChannelDataJson" );
            RenameColumn( table: "dbo.CommunicationTemplate", name: "MediumEntityTypeId", newName: "ChannelEntityTypeId" );

            RenameIndex( table: "dbo.Communication", name: "IX_MediumEntityTypeId", newName: "IX_ChannelEntityTypeId" );
            RenameColumn( table: "dbo.Communication", name: "MediumDataJson", newName: "ChannelDataJson" );
            RenameColumn( table: "dbo.Communication", name: "MediumEntityTypeId", newName: "ChannelEntityTypeId" );
        }
    }
}
