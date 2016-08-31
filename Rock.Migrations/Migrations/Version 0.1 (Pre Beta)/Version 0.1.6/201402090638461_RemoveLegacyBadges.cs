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
    public partial class RemoveLegacyBadges : Rock.Migrations.RockMigration3
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {

            Sql(@"

                -- delete era badges
                  DELETE FROM [PersonBadge]
                  WHERE [Guid] = '44E4D9CD-8143-4333-8C66-89DF95B9F580'

                  DELETE FROM [PersonBadge]
                  WHERE [Guid] = '452CF317-D3A1-49B5-84B1-4206DDADC653'

                  DECLARE @eraAttendanceType int = (SELECT [Id] FROM [EntityType] WHERE [Guid] = 'D046A808-06A2-4131-88AF-A97166840E97')
                  DELETE FROM [Attribute] WHERE [EntityTypeId] = @eraAttendanceType

                  DECLARE @eraType int = (SELECT [Id] FROM [EntityType] WHERE [Guid] = '5EE996CB-EA8A-4EEF-88D2-F11510AEE5CD')
                  DELETE FROM [Attribute] WHERE [EntityTypeId] = @eraType

                  DELETE FROM [EntityType]
                  WHERE [Guid] = 'D046A808-06A2-4131-88AF-A97166840E97'

                -- delete disc type badge
                  DELETE FROM [EntityType]
                  WHERE [Guid] = '5EE996CB-EA8A-4EEF-88D2-F11510AEE5CD'

                  DECLARE @discType int = (SELECT [Id] FROM [EntityType] WHERE [Guid] = '208A2FE8-9CC8-4608-891A-A62CE29BE05B')
                  DELETE FROM [Attribute] WHERE [EntityTypeId] = @discType

                  DELETE FROM [EntityType]
                  WHERE [Guid] = '208A2FE8-9CC8-4608-891A-A62CE29BE05B'

                -- delete next step badge
                  DECLARE @nextstepType int = (SELECT [Id] FROM [EntityType] WHERE [Guid] = '13E3B42C-7E7F-41A6-940B-FA0DDC5E647F')
                  DELETE FROM [Attribute] WHERE [EntityTypeId] = @nextstepType

                  DELETE FROM [EntityType]
                  WHERE [Guid] = '13E3B42C-7E7F-41A6-940B-FA0DDC5E647F'

                  DELETE FROM [Attribute] WHERE [EntityTypeId] = @nextstepType

                -- delete serving
                  DECLARE @servingType int = (SELECT [Id] FROM [EntityType] WHERE [Guid] = '7ADBA2D4-663F-4039-AA04-E0AEC81B5A21')
                  DELETE FROM [Attribute] WHERE [EntityTypeId] = @servingType
  
                  DELETE FROM [EntityType]
                  WHERE [Guid] = '7ADBA2D4-663F-4039-AA04-E0AEC81B5A21'
   
                -- delete baptism badge
                  DECLARE @baptizedType int = (SELECT [Id] FROM [EntityType] WHERE [Guid] = '66B1421C-B33E-40AF-B0DD-D758759E999A')
                  DELETE FROM [Attribute] WHERE [EntityTypeId] = @baptizedType

                  DELETE FROM [EntityType]
                  WHERE [Guid] = '66B1421C-B33E-40AF-B0DD-D758759E999A'

                -- remove record status badge
                  DELETE FROM [PersonBadge] WHERE [Guid] = 'A999125F-E2B8-48AD-AA25-DF94147E65C2'

                  DECLARE @RecordStatusType int = (SELECT [Id] FROM [EntityType] WHERE [Guid] = '09E8CD24-BE34-4DC0-8B43-A7ACE0549CE0')
                  DELETE FROM [Attribute] WHERE [EntityTypeId] = @RecordStatusType

                  DELETE FROM [EntityType]
                  WHERE [Guid] = '09E8CD24-BE34-4DC0-8B43-A7ACE0549CE0'

                -- delete connection status
                  DELETE FROM [PersonBadge] WHERE [Guid] = '43B4800D-B995-459E-82FC-5FD575F9E348'

                  DECLARE @ConnectionStatusType int = (SELECT [Id] FROM [EntityType] WHERE [Guid] = '3D093330-D547-454B-8956-B76D8F9B8536')
                  DELETE FROM [Attribute] WHERE [EntityTypeId] = @ConnectionStatusType

                  DELETE FROM [EntityType]
                  WHERE [Guid] = '3D093330-D547-454B-8956-B76D8F9B8536'


            ");

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
