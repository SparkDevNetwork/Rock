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
    public partial class AddPageTrackingGlobalAttrib : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddGlobalAttribute("1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "", "", "Enable Page View Tracking", "Tracks each page that is viewed in Rock.", 0 , "true", "950096BE-1F3C-49B8-81F9-757D27C90310");
        
            // set attribute category
            Sql( @"
                 INSERT INTO [AttributeCategory]
	                ([AttributeId], [CategoryId])
                 VALUES (
	                (SELECT [ID]FROM [Attribute] WHERE [GUID] = '950096BE-1F3C-49B8-81F9-757D27C90310')
	                , 5
                )
            " );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"DELETE FROM [Attribute] WHERE [Guid] = '950096BE-1F3C-49B8-81F9-757D27C90310'" );
        }
    }
}
