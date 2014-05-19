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
namespace com.ccvonline.Residency.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class ResidentProjectPageAttrib : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attrib for BlockType: com .ccvonline - Residency Competency Person Project Assignment Detail:Resident Project Page
            AddBlockTypeAttribute( "8A5FB3E3-4147-4DE0-9CAE-20974ADD5E70", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Resident Project Page", "ResidentProjectPage", "", "", 0, "", "499D97C8-EABC-4D98-B3F0-BDD2377B434C" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: com .ccvonline - Residency Competency Person Project Assignment Detail:Resident Project Page
            DeleteAttribute( "499D97C8-EABC-4D98-B3F0-BDD2377B434C" );
        }
    }
}
