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
    public partial class RemoveAuditDetail : Rock.Migrations.RockMigration1
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Delete the Audit Information List attribute value
            DeleteBlockAttributeValue( "73C2CC25-6D78-4B08-B4B1-95254C3F66BF", "E585CC84-2E3E-4EE6-A1AC-F5E94E7A87AF" );
            // Delete the Audit Information List attribute
            DeleteBlockAttribute( "E585CC84-2E3E-4EE6-A1AC-F5E94E7A87AF" );
            // Delete the Audit Information Detail block
            DeleteBlock( "D0067699-EAA7-49E0-9C58-3A7B070862EF" );
            // Delete the Audit Information Detail block type
            DeleteBlockType( "0B1409F3-7A09-4573-A995-B98599A28CB8" );
            // Delete the Audit Information Detail page
            DeletePage( "25A45244-11A7-42BD-BC10-D81963A720B2" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Add a page for the Audit Information Detail
            AddPage( "4D7F3953-0BD9-4B4B-83F9-5FCC6B2BBE30", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Audit Information Detail", "Detail page for managing the audit information.", "25A45244-11A7-42BD-BC10-D81963A720B2", "fa fa-check" );
            // Add a block type for the Audit Information Detail
            AddBlockType( "Core - Audit Information Detail", "Audit Information Detail", "~/Blocks/Core/AuditInformationDetail.ascx", "0B1409F3-7A09-4573-A995-B98599A28CB8" );
            // Add a block for the Audit Information Detail
            AddBlock( "25A45244-11A7-42BD-BC10-D81963A720B2", "", "0B1409F3-7A09-4573-A995-B98599A28CB8", "Audit Information Detail", "Main", 0, "D0067699-EAA7-49E0-9C58-3A7B070862EF" );
            // Add a block attribute for the Audit Information List block type
            AddBlockTypeAttribute( "D3B7C96B-DF1F-40AF-B09F-AB468E0E726D", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, "", "E585CC84-2E3E-4EE6-A1AC-F5E94E7A87AF" );
            // Add a attribute value for the Audit Information List block attribute
            AddBlockAttributeValue( "73C2CC25-6D78-4B08-B4B1-95254C3F66BF", "E585CC84-2E3E-4EE6-A1AC-F5E94E7A87AF", @"25A45244-11A7-42BD-BC10-D81963A720B2" );
        }
    }
}
