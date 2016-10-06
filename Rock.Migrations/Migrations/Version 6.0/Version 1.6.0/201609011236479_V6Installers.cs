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
    public partial class V6Installers : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // update new location of checkscanner installer
            Sql( @"
    UPDATE [AttributeValue] 
    SET [Value] = 'http://storage.rockrms.com/externalapplications/sparkdevnetwork/checkscanner/1.6.0/checkscanner.exe' 
    WHERE [Guid] = '82960DBD-2EAA-47DF-B9AC-86F7A2FCA180'
" );

            // update new location of jobscheduler installer
            Sql( @"
    UPDATE [AttributeValue] 
    SET [Value] = 'http://storage.rockrms.com/externalapplications/sparkdevnetwork/jobscheduler/1.6.0/jobscheduler.exe'
    WHERE [Guid] = '7FBC4397-6BFD-451D-A6B9-83D7B7265641'
" );

            // update new location of statementgenerator installer
            Sql( @"
    UPDATE [AttributeValue] 
    SET [Value] = 'http://storage.rockrms.com/externalapplications/sparkdevnetwork/statementgenerator/1.6.0/statementgenerator.exe' 
    WHERE [Guid] = '10BE2E03-7827-41B5-8CB2-DEB473EA107A'
" );

            // update new location of checkinclient installer
            Sql( @"
    UPDATE [AttributeValue] 
    SET [Value] = 'http://storage.rockrms.com/externalapplications/sparkdevnetwork/windowscheckin/1.6.0/checkinclient.exe'
    WHERE [Guid] = '7ADC1B5B-D374-4B77-9DE1-4D788B572A10'
" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
