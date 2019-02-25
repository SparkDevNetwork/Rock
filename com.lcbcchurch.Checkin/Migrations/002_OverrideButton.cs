// <copyright>
// Copyright by LCBC Church
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock;
using Rock.Plugin;
namespace com.lcbcchurch.Checkin.Migrations
{
    [MigrationNumber( 2, "1.0.14" )]
    public class OverrideButton : Migration
    {
        public override void Up()
        {
            // Add new entity attributes
            RockMigrationHelper.AddEntityAttribute( "Rock.Model.Device", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "", "", "Allow Override On Start?", "", "", 1008, "False", "548EB4FC-7467-4E0A-AB5E-1F900FD38A7B", "AllowOverrideOnStart" );

            //Add new block to Welcome Page
        }
        public override void Down()
        {
           
        }
    }
}
