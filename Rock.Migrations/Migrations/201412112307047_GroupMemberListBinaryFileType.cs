// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class GroupMemberListBinaryFileType : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            //Group Member List_PersonProfile Up Migration

            // Deletes GroupMemberDetail PersonProfile Attribute
            RockMigrationHelper.DeleteBlockAttribute( "15E2C1EA-B0A1-469F-AC25-45C93FEC8140" );
            // Adds GroupMemberList PersonProfile Attribute
            RockMigrationHelper.AddBlockTypeAttribute( "88B7EFA9-7419-4D05-9F88-38B936E61EDD", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Profile Page", "PersonProfilePage", "", "The person profile page to link to.", 2, "", "9E139BB9-D87C-4C9F-A241-DC4620AD340B", false );
            // Sets GroupMemberList PersonProfile Attribute Value
            RockMigrationHelper.AddBlockAttributeValue( "BA0F3E7D-1C3A-47CB-9058-893DBAA35B89", "9E139BB9-D87C-4C9F-A241-DC4620AD340B", "08dbd8a5-2c35-4146-b4a8-0f7652348b25,7e97823a-78a8-4e8e-a337-7a20f2da9e52" );
            // Security role members
            RockMigrationHelper.AddBlockAttributeValue( "E71D3062-286A-49D2-A0BB-84B385EFAD50", "9E139BB9-D87C-4C9F-A241-DC4620AD340B", "08dbd8a5-2c35-4146-b4a8-0f7652348b25,7e97823a-78a8-4e8e-a337-7a20f2da9e52" );
            // Application Group Members
            RockMigrationHelper.AddBlockAttributeValue( "AB47ABE2-B9BB-4C89-B35A-ABCECA2098C6", "9E139BB9-D87C-4C9F-A241-DC4620AD340B", "08dbd8a5-2c35-4146-b4a8-0f7652348b25,7e97823a-78a8-4e8e-a337-7a20f2da9e52" );
            // Photo Request Members
            RockMigrationHelper.AddBlockAttributeValue( "B99901FD-E852-4FCF-8F9B-0870984D59AE", "9E139BB9-D87C-4C9F-A241-DC4620AD340B", "08dbd8a5-2c35-4146-b4a8-0f7652348b25,7e97823a-78a8-4e8e-a337-7a20f2da9e52" );

            // BinaryFileType ScanSettings Up Migration
            AddColumn( "dbo.BinaryFileType", "MaxWidth", c => c.Int() );
            AddColumn( "dbo.BinaryFileType", "MaxHeight", c => c.Int() );
            AddColumn( "dbo.BinaryFileType", "PreferredFormat", c => c.Int( nullable: false, defaultValue: -1 ) );
            AddColumn( "dbo.BinaryFileType", "PreferredResolution", c => c.Int( nullable: false, defaultValue: -1 ) );
            AddColumn( "dbo.BinaryFileType", "PreferredColorDepth", c => c.Int( nullable: false, defaultValue: -1 ) );
            AddColumn( "dbo.BinaryFileType", "PreferredRequired", c => c.Boolean( nullable: false ) );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // BinaryFileType ScanSettings Down Migration
            DropColumn( "dbo.BinaryFileType", "PreferredRequired" );
            DropColumn( "dbo.BinaryFileType", "PreferredColorDepth" );
            DropColumn( "dbo.BinaryFileType", "PreferredResolution" );
            DropColumn( "dbo.BinaryFileType", "PreferredFormat" );
            DropColumn( "dbo.BinaryFileType", "MaxHeight" );
            DropColumn( "dbo.BinaryFileType", "MaxWidth" );

            // Group Member List_PersonProfile Down Migration

            // Deletes GroupMemberList PersonProfile Attribute
            RockMigrationHelper.DeleteBlockAttribute( "9E139BB9-D87C-4C9F-A241-DC4620AD340B" );
            // Creates and sets a value to the GroupMemberDetail PersonProfile Attribute
            RockMigrationHelper.AddBlockTypeAttribute( "AAE2E5C3-9279-4AB0-9682-F4D19519D678", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Profile Page", "PersonProfilePage", "", "The person profile page to link to.", 0, "", "15E2C1EA-B0A1-469F-AC25-45C93FEC8140", false );
            RockMigrationHelper.AddBlockAttributeValue( "C66D11C8-DA55-40EA-925C-C9D7AC71F879", "15E2C1EA-B0A1-469F-AC25-45C93FEC8140", "08dbd8a5-2c35-4146-b4a8-0f7652348b25,7e97823a-78a8-4e8e-a337-7a20f2da9e52" );
        }
    }
}
