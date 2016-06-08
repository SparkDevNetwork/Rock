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
    public partial class DefaultMaritalStatus : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attrib for BlockType: Add Family:Child Marital Status
            AddBlockTypeAttribute( "DE156975-597A-4C55-A649-FE46712F91C3", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Child Marital Status", "ChildMaritalStatus", "", "The marital status to use for children in the family.", 4, @"F19FC180-FE8F-4B72-A59C-8013E3B0EB0D", "34B56BE9-FF33-4F64-A4B2-A098EC5250FB" );
            // Attrib for BlockType: Add Family:Adult Marital Status
            AddBlockTypeAttribute( "DE156975-597A-4C55-A649-FE46712F91C3", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Adult Marital Status", "AdultMaritalStatus", "", "The default marital status for adults in the family.", 3, @"", "815D526D-671A-48B0-8988-9264D65BAB26" );
            // Attrib Value for Block:Add Family, Attribute:Adult Marital Status Page: New Family, Site: Rock RMS
            AddBlockAttributeValue( "613536BE-86BC-4755-B815-807C236B92E6", "815D526D-671A-48B0-8988-9264D65BAB26", @"d9cfd343-6a56-45f6-9e26-3269ba4fbc02" );
            // Attrib Value for Block:Add Family, Attribute:Child Marital Status Page: New Family, Site: Rock RMS
            AddBlockAttributeValue( "613536BE-86BC-4755-B815-807C236B92E6", "34B56BE9-FF33-4F64-A4B2-A098EC5250FB", @"f19fc180-fe8f-4b72-a59c-8013e3b0eb0d" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Add Family:Adult Marital Status
            DeleteAttribute( "815D526D-671A-48B0-8988-9264D65BAB26" );
            // Attrib for BlockType: Add Family:Child Marital Status
            DeleteAttribute( "34B56BE9-FF33-4F64-A4B2-A098EC5250FB" );
        }
    }
}
