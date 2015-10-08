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
    public partial class Benevolence : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            //Defined Types
            RockMigrationHelper.AddDefinedType( "Financial", "Benevolence Request Status", "The status of a benevolence request.", "2787B088-D607-4D69-84FF-850A6891EE34", @"" );
            RockMigrationHelper.AddDefinedType( "Financial", "Benevolence Result Type", "The type of response given to a benevolence request.", "35FC0225-3DAC-48B4-BDF7-AFDE104FB60E", @"" );
            RockMigrationHelper.AddDefinedValue( "2787B088-D607-4D69-84FF-850A6891EE34", "Pending", "", "67B24629-62A9-436A-A98C-30A454642153", false );
            RockMigrationHelper.AddDefinedValue( "2787B088-D607-4D69-84FF-850A6891EE34", "Approved", "", "18D3A2DA-F2BA-49AE-83EB-7E60DCD18A3B", false );
            RockMigrationHelper.AddDefinedValue( "2787B088-D607-4D69-84FF-850A6891EE34", "Denied", "", "3720671E-DA48-405F-A6D5-5E2D47436F9A", false );
            RockMigrationHelper.AddDefinedValue( "35FC0225-3DAC-48B4-BDF7-AFDE104FB60E", "Provided Financial Assistance", "", "4FEEDE75-0663-47E5-B1D0-A9639FAF197B", false );
            RockMigrationHelper.AddDefinedValue( "35FC0225-3DAC-48B4-BDF7-AFDE104FB60E", "Provided Gift Card", "", "78CE59CC-3E3B-47DE-8116-7B898319DE9E", false );
            RockMigrationHelper.AddDefinedValue( "35FC0225-3DAC-48B4-BDF7-AFDE104FB60E", "Provided Services", "", "C4C36847-0E86-4667-A28D-CB88D3BDEB7E", false );
            RockMigrationHelper.AddDefinedValue( "35FC0225-3DAC-48B4-BDF7-AFDE104FB60E", "Referred to Counseling", "", "43066522-55A2-4A2B-8340-17E927EC1B8D", false );
            RockMigrationHelper.AddDefinedValue( "35FC0225-3DAC-48B4-BDF7-AFDE104FB60E", "Referred to Food Pantry", "", "C70395A7-7F71-4F14-A599-BB4635ABFF2D", false );
            RockMigrationHelper.AddDefinedValue( "35FC0225-3DAC-48B4-BDF7-AFDE104FB60E", "Unable to Make Contact", "", "CDF5D13E-4636-489C-85F9-95D42EF11895", false );

            //Model
            CreateTable(
                "dbo.BenevolenceRequest",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    FirstName = c.String( nullable: false, maxLength: 50 ),
                    LastName = c.String( nullable: false, maxLength: 50 ),
                    Email = c.String( maxLength: 254 ),
                    RequestedByPersonAliasId = c.Int(),
                    RequestText = c.String( nullable: false ),
                    RequestDateTime = c.DateTime( nullable: false ),
                    HomePhoneNumber = c.String(),
                    CellPhoneNumber = c.String(),
                    WorkPhoneNumber = c.String(),
                    CaseWorkerPersonAliasId = c.Int(),
                    GovernmentId = c.String(),
                    RequestStatusValueId = c.Int( nullable: false ),
                    ResultSummary = c.String(),
                    ConnectionStatusValueId = c.Int(),
                    LocationId = c.Int(),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.String( maxLength: 50 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.PersonAlias", t => t.CaseWorkerPersonAliasId )
                .ForeignKey( "dbo.DefinedValue", t => t.ConnectionStatusValueId )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.Location", t => t.LocationId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.RequestedByPersonAliasId )
                .ForeignKey( "dbo.DefinedValue", t => t.RequestStatusValueId )
                .Index( t => t.RequestedByPersonAliasId )
                .Index( t => t.CaseWorkerPersonAliasId )
                .Index( t => t.RequestStatusValueId )
                .Index( t => t.ConnectionStatusValueId )
                .Index( t => t.LocationId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.BenevolenceResult",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    BenevolenceRequestId = c.Int( nullable: false ),
                    ResultTypeValueId = c.Int( nullable: false ),
                    Amount = c.Decimal( precision: 18, scale: 2 ),
                    ResultSummary = c.String(),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.String( maxLength: 50 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.BenevolenceRequest", t => t.BenevolenceRequestId )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .ForeignKey( "dbo.DefinedValue", t => t.ResultTypeValueId )
                .Index( t => t.BenevolenceRequestId )
                .Index( t => t.ResultTypeValueId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            // Add Benevolence Blocks
            RockMigrationHelper.UpdateBlockType( "Benevolence Request List", "Block used to list Benevolence Requests.", "~/Blocks/Finance/BenevolenceRequestList.ascx", "Finance", "3131C55A-8753-435F-85F3-DF777EFBD1C8" );
            RockMigrationHelper.AddBlockTypeAttribute( "3131C55A-8753-435F-85F3-DF777EFBD1C8", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "E2C90243-A79A-4DAD-9301-07F6DF095CDB" );
            RockMigrationHelper.AddBlockTypeAttribute( "3131C55A-8753-435F-85F3-DF777EFBD1C8", "F4399CEF-827B-48B2-A735-F7806FCFE8E8", "Case Worker Group", "CaseWorkerGroup", "", "The group to draw case workers from", 0, @"26E7148C-2059-4F45-BCFE-32230A12F0DC", "576E31E0-EE40-4A89-93AE-5CCF1F45D21F" );

            RockMigrationHelper.UpdateBlockType( "Benevolence Request Detail", "Block for users to create, edit, and view benevolence requests.", "~/Blocks/Finance/BenevolenceRequestDetail.ascx", "Finance", "34275D0E-BC7E-4A9C-913E-623D086159A1" );
            RockMigrationHelper.AddBlockTypeAttribute( "34275D0E-BC7E-4A9C-913E-623D086159A1", "F4399CEF-827B-48B2-A735-F7806FCFE8E8", "Case Worker Group", "CaseWorkerGroup", "", "The group to draw case workers from", 0, @"26E7148C-2059-4F45-BCFE-32230A12F0DC", "89EA176C-2CEB-46F4-AACC-6AF55F5C42B0" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "89EA176C-2CEB-46F4-AACC-6AF55F5C42B0" );
            RockMigrationHelper.DeleteBlockType( "34275D0E-BC7E-4A9C-913E-623D086159A1" );
            RockMigrationHelper.DeleteAttribute( "576E31E0-EE40-4A89-93AE-5CCF1F45D21F" );
            RockMigrationHelper.DeleteAttribute( "E2C90243-A79A-4DAD-9301-07F6DF095CDB" );
            RockMigrationHelper.DeleteBlockType( "3131C55A-8753-435F-85F3-DF777EFBD1C8" );

            DropForeignKey( "dbo.BenevolenceRequest", "RequestStatusValueId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.BenevolenceRequest", "RequestedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.BenevolenceRequest", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.BenevolenceRequest", "LocationId", "dbo.Location" );
            DropForeignKey( "dbo.BenevolenceRequest", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.BenevolenceRequest", "ConnectionStatusValueId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.BenevolenceRequest", "CaseWorkerPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.BenevolenceResult", "ResultTypeValueId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.BenevolenceResult", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.BenevolenceResult", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.BenevolenceResult", "BenevolenceRequestId", "dbo.BenevolenceRequest" );
            DropIndex( "dbo.BenevolenceResult", new[] { "Guid" } );
            DropIndex( "dbo.BenevolenceResult", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.BenevolenceResult", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.BenevolenceResult", new[] { "ResultTypeValueId" } );
            DropIndex( "dbo.BenevolenceResult", new[] { "BenevolenceRequestId" } );
            DropIndex( "dbo.BenevolenceRequest", new[] { "Guid" } );
            DropIndex( "dbo.BenevolenceRequest", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.BenevolenceRequest", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.BenevolenceRequest", new[] { "LocationId" } );
            DropIndex( "dbo.BenevolenceRequest", new[] { "ConnectionStatusValueId" } );
            DropIndex( "dbo.BenevolenceRequest", new[] { "RequestStatusValueId" } );
            DropIndex( "dbo.BenevolenceRequest", new[] { "CaseWorkerPersonAliasId" } );
            DropIndex( "dbo.BenevolenceRequest", new[] { "RequestedByPersonAliasId" } );
            DropTable( "dbo.BenevolenceResult" );
            DropTable( "dbo.BenevolenceRequest" );

            RockMigrationHelper.DeleteDefinedValue( "18D3A2DA-F2BA-49AE-83EB-7E60DCD18A3B" );
            RockMigrationHelper.DeleteDefinedValue( "3720671E-DA48-405F-A6D5-5E2D47436F9A" );
            RockMigrationHelper.DeleteDefinedValue( "43066522-55A2-4A2B-8340-17E927EC1B8D" );
            RockMigrationHelper.DeleteDefinedValue( "4FEEDE75-0663-47E5-B1D0-A9639FAF197B" );
            RockMigrationHelper.DeleteDefinedValue( "67B24629-62A9-436A-A98C-30A454642153" );
            RockMigrationHelper.DeleteDefinedValue( "78CE59CC-3E3B-47DE-8116-7B898319DE9E" );
            RockMigrationHelper.DeleteDefinedValue( "C4C36847-0E86-4667-A28D-CB88D3BDEB7E" );
            RockMigrationHelper.DeleteDefinedValue( "C70395A7-7F71-4F14-A599-BB4635ABFF2D" );
            RockMigrationHelper.DeleteDefinedValue( "CDF5D13E-4636-489C-85F9-95D42EF11895" );
            RockMigrationHelper.DeleteDefinedType( "2787B088-D607-4D69-84FF-850A6891EE34" );
            RockMigrationHelper.DeleteDefinedType( "35FC0225-3DAC-48B4-BDF7-AFDE104FB60E" ); //Defined Types
        }
    }
}
