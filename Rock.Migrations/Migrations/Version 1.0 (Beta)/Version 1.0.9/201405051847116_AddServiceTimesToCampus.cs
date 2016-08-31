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
    public partial class AddServiceTimesToCampus : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddEntityAttribute("Rock.Model.Campus", "73B02051-0D38-4AD9-BF81-A2D477DE4F70", "", "", "Service Times", "", "The service times for the campus.  The Key (left column) should be the service title, for example 'First Service'.  The Value (right column) should be the actual service time, like '9:30am'.", 0, "", "F5810A78-D2E2-4017-8B2A-73AE83B6725E");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteAttribute( "F5810A78-D2E2-4017-8B2A-73AE83B6725E" );
        }
    }
}
