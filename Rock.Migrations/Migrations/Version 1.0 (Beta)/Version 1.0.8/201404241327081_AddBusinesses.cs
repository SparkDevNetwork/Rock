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
    public partial class AddBusinesses : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // **************************************** BUSINESS LIST ****************************************

            // The business list page
            AddPage( "18C9E5C3-3E28-4AA3-84F6-78CD4EA2DD3C", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Businesses", "A list of businesses associated with your organization", "F4DF4899-2D44-4997-BA9B-9D2C64958A20", "fa fa-briefcase" );

            // The business list block type
            AddBlockType( "Business List", "Lists the businesses that are associated with your organization", "~/Blocks/Finance/BusinessList.ascx", "Finance", "65A2186E-570C-48FD-9565-E2495DD8E5CE" );

            // The business list block
            AddBlock( "F4DF4899-2D44-4997-BA9B-9D2C64958A20", "", "65A2186E-570C-48FD-9565-E2495DD8E5CE", "Business List", "Main", "", "", 0, "04E68378-E2F6-465B-925A-D8B124858C44" );

            // **************************************** BUSINESS DETAIL ****************************************

            // The business detail page
            AddPage( "F4DF4899-2D44-4997-BA9B-9D2C64958A20", "195BCD57-1C10-4969-886F-7324B6287B75", "Business Detail", "Detailed information about this business", "D2B43273-C64F-4F57-9AAE-9571E1982BAC", "fa fa-briefcase" );

            // The business detail block type
            AddBlockType( "Business Detail", "Used viewing and editing a businesses information", "~/Blocks/Finance/BusinessDetail.ascx", "Finance", "2F1B1A70-6AD7-4758-B4A8-20E3B90698C2" );

            // The business detail block
            AddBlock( "D2B43273-C64F-4F57-9AAE-9571E1982BAC", "", "2F1B1A70-6AD7-4758-B4A8-20E3B90698C2", "Business Detail", "Main", "", "", 0, "77AB2D30-FCBE-45E9-9757-401AE2676A7F" );

            // Create the Detail Page attribute on the Business List page
            AddBlockTypeAttribute( "65A2186E-570C-48FD-9565-E2495DD8E5CE", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, "", "7A42B562-C343-4279-87A6-2A76066CD0C9" );

            // Set "Detail Page" attribute on Business List page to the Business Detail page
            AddBlockAttributeValue( "04E68378-E2F6-465B-925A-D8B124858C44", "7A42B562-C343-4279-87A6-2A76066CD0C9", "D2B43273-C64F-4F57-9AAE-9571E1982BAC" );

            Sql( @"
                INSERT INTO [GroupTypeRole]
                   ([IsSystem]
                   ,[GroupTypeId]
                   ,[Name]
                   ,[Description]
                   ,[Order]
                   ,[IsLeader]
                   ,[Guid])
                VALUES
                   (0
                   ,11
                   ,'Business Contact'
                   ,'Role to identify a person as a contact for a business.'
                   ,0
                   ,0
                   ,'102E6AF5-62C2-4767-B473-C9C228D75FB6')" );

            Sql( @"
                INSERT INTO [GroupTypeRole]
                   ([IsSystem]
                   ,[GroupTypeId]
                   ,[Name]
                   ,[Description]
                   ,[Order]
                   ,[IsLeader]
                   ,[Guid])
                VALUES
                   (0
                   ,11
                   ,'Business'
                   ,'A role to identify the business a person owns.'
                   ,0
                   ,0
                   ,'7FC58BB2-7C1E-4C5C-B2B3-4738258A0BE0')" );

            Sql( @"
                INSERT INTO [GroupTypeRole]
                   ([IsSystem]
                   ,[GroupTypeId]
                   ,[Name]
                   ,[Description]
                   ,[Order]
                   ,[IsLeader]
                   ,[Guid])
                VALUES
                   (0
                   ,11
                   ,'Principle'
                   ,'A role to identify the owner of a business.'
                   ,0
                   ,0
                   ,'27198949-FAD3-4BD6-820C-FEB98AA61E7D')" );

            Sql( @"
                DECLARE @AttributeId int
                SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'C91148D9-D663-493A-86E8-5000BD281852')

                DECLARE @EntityId int
                SET @EntityId = (SELECT [Id] FROM [GroupTypeRole] WHERE [Guid] = '7FC58BB2-7C1E-4C5C-B2B3-4738258A0BE0')

                INSERT INTO [AttributeValue]
                    ([IsSystem]
                    ,[AttributeId]
                    ,[EntityId]
                    ,[Order]
                    ,[Value]
                    ,[Guid])
                VALUES
                    (0
                    ,@AttributeId
                    ,@EntityId
                    ,0
                    ,'27198949-FAD3-4BD6-820C-FEB98AA61E7D'
                    ,'4659F638-9BD0-4ADA-BAB1-6E9BD0F6BF73')" );

            Sql( @"
                DECLARE @AttributeId int
                SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'C91148D9-D663-493A-86E8-5000BD281852')

                DECLARE @EntityId int
                SET @EntityId = (SELECT [Id] FROM [GroupTypeRole] WHERE [Guid] = '27198949-FAD3-4BD6-820C-FEB98AA61E7D')

                INSERT INTO [AttributeValue]
                    ([IsSystem]
                    ,[AttributeId]
                    ,[EntityId]
                    ,[Order]
                    ,[Value]
                    ,[Guid])
                VALUES
                    (0
                    ,@AttributeId
                    ,@EntityId
                    ,0
                    ,'7FC58BB2-7C1E-4C5C-B2B3-4738258A0BE0'
                    ,'79F1B549-4B3D-45CE-A697-CDD6A8608D45')" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
