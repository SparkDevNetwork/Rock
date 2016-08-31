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
    public partial class GroupDetailLimitGroupTypeOption : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attrib for BlockType: Group Detail:Limit to Group Types that are shown in navigation
            AddBlockTypeAttribute( "582BEEA1-5B27-444D-BC0A-F60CEB053981", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Limit to Group Types that are shown in navigation", "LimitToShowInNavigationGroupTypes", "", "", 3, @"False", "62B0099E-B1A3-4468-B821-B96AB088A861" );

            // Attrib Value for Block:GroupDetailRight, Attribute:Limit to Group Types that are shown in navigation Page: Group Viewer, Site: Rock RMS
            AddBlockAttributeValue( "88344FE3-737E-4741-A38D-D2D3A1653818", "62B0099E-B1A3-4468-B821-B96AB088A861", @"True" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Group Detail:Limit to Group Types that are shown in navigation
            DeleteAttribute( "62B0099E-B1A3-4468-B821-B96AB088A861" );
        }
    }
}
