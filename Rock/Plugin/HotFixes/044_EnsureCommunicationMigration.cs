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
using System;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 44, "1.7.0" )]
    [RockObsolete( "1.7" )]
    [Obsolete( "The Communication.MediumDataJson and CommunicationTemplate.MediumDataJson fields will be removed in Rock 1.10. So the plugin will be changed to do nothing starting in Rock 1.10" )]
    public class EnsureCommunicationMigration : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        
        public override void Up()
        {
            // There was a bug in one of the v7.0 Rock Updates that might have prevented UpdateCommunicationRecords, so make sure that UpdateCommunicationRecords ran.  
            //Rock.Jobs.MigrateCommunicationMediumData.UpdateCommunicationRecords( true, 50, null );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}
