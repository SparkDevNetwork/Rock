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
    public partial class BenevolenceUpdates2 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateBinaryFileType( Rock.SystemGuid.EntityType.STORAGE_PROVIDER_DATABASE, "Benevolence Request Documents", "Related documents for benevolence requests.", "fa fa-files-o", Rock.SystemGuid.BinaryFiletype.BENEVOLENCE_REQUEST_DOCUMENTS, false, true );

            // add security to the document
            RockMigrationHelper.AddSecurityAuthForBinaryFileType( Rock.SystemGuid.BinaryFiletype.BENEVOLENCE_REQUEST_DOCUMENTS, 0, "View", true, Rock.SystemGuid.Group.GROUP_BENEVOLENCE, Model.SpecialRole.None, "3EF0EE1E-A2F5-0C95-48AA-3B1FD2A6E5A1" );
            RockMigrationHelper.AddSecurityAuthForBinaryFileType( Rock.SystemGuid.BinaryFiletype.BENEVOLENCE_REQUEST_DOCUMENTS, 1, "View", true, Rock.SystemGuid.Group.GROUP_ADMINISTRATORS, Model.SpecialRole.None, "4D486E0B-FD09-61A6-463C-10022C0C68AA" );
            RockMigrationHelper.AddSecurityAuthForBinaryFileType( Rock.SystemGuid.BinaryFiletype.BENEVOLENCE_REQUEST_DOCUMENTS, 2, "View", false, "", Model.SpecialRole.AllUsers, "7A7A6C2A-5032-07AD-428F-3695F726E6A7" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteSecurityAuth( "3EF0EE1E-A2F5-0C95-48AA-3B1FD2A6E5A1" );
            RockMigrationHelper.DeleteSecurityAuth( "4D486E0B-FD09-61A6-463C-10022C0C68AA" );
            RockMigrationHelper.DeleteSecurityAuth( "7A7A6C2A-5032-07AD-428F-3695F726E6A7" );
        }
    }
}
