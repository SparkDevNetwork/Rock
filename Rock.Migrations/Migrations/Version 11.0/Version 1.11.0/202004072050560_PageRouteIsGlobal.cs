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
    public partial class PageRouteIsGlobal : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.PageRoute", "IsGlobal", c => c.Boolean(nullable: false));
            UpdateSystemDialogPageRoutes();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.PageRoute", "IsGlobal");
        }

        /// <summary>
        /// Updates the IsGlobal setting to 1 for system dialog page routes.
        /// </summary>
        private void UpdateSystemDialogPageRoutes()
        {
            Sql( @"UPDATE [PageRoute]
                SET [IsGlobal] = 1
                WHERE [PageId] IN ( 
	                SELECT [Id]
	                FROM [Page]
	                WHERE [Guid] IN (
		                '9F36531F-C1B5-4E23-8FA3-18B6DAFF1B0B',	--Zone Blocks
		                'F0B34893-9550-4864-ADB4-EE860E4E427C',	--Block Properties
		                '86D5E33E-E351-4CA5-9925-849C6C467257',	--Manage Security
		                'D58F205E-E9CC-4BD9-BC79-F3DA86F6E346',	--Child Pages
		                '37759B50-DB4A-440D-A83B-4EF3B4727B1E',	--Page Properties
		                '8A97CC93-3E93-4286-8440-E5217B65A904',	--System Information
		                'A9188D7A-80D9-4865-9C77-9F90E992B65C',	--Short Link
		                '4A4995CA-24F6-4D33-B861-A24274F53AA6',	--HtmlEditor RockFileBrowser Plugin Frame
		                '1FC09F0D-72F2-44E6-9D16-2884F9AF33DD',	--HtmlEditor RockMergeField Plugin Frame
		                'DEB88EA2-D0CE-47B2-9EB3-FDDDAC2C3389'	--HtmlEditor RockAssetManager Plugin Frame
	                )
                )" );
        }
    }
}
