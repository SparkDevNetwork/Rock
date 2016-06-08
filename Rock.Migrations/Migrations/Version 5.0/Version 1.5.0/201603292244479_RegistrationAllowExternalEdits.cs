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
    public partial class RegistrationAllowExternalEdits : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // update the new AllowExternalRegistrationUpdates to reflect current behavior
            Sql( @" UPDATE [RegistrationTemplate] SET [AllowExternalRegistrationUpdates] = 1" );

            // update new location of jobscheduler installer
            Sql( "UPDATE [AttributeValue] set [Value] = 'http://storage.rockrms.com/externalapplications/sparkdevnetwork/jobscheduler/1.5.0/jobscheduler.exe' where [Guid] = '7FBC4397-6BFD-451D-A6B9-83D7B7265641'" );

            // add admin checklist item for Family Analytics
            RockMigrationHelper.AddDefinedValue( "4BF34677-37E9-4E71-BD03-252B66C9373D", "Enable Family Analytics", @"<a href='http://www.rockrms.com/Rock/BookContent/5#personfamilyanalytics'>Read up on the Family Analytics features</a> and enable the job that supports them.", "8CCE4657-409D-0998-445A-CA6DC2F43096" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
