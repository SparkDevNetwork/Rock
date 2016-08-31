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
    public partial class ExceptionFilter : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddGlobalAttribute( "73B02051-0D38-4AD9-BF81-A2D477DE4F70", "", "", "Email Exceptions Filter", "Before sending the exception notification email, Rock can evaluate the current client's HTTP Server variables and ignore any exceptions from clients that have server variable values containing the values configured here.", 0, "HTTP_USER_AGENT^Googlebot|", "120E6F4C-030E-4FA8-B1B1-119FAC77CC2C" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "73B02051-0D38-4AD9-BF81-A2D477DE4F70" );
        }
    }
}
