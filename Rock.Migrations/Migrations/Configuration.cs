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
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    public sealed class Configuration : DbMigrationsConfiguration<Rock.Data.RockContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            MigrationsNamespace = "Rock.Migrations";
            CodeGenerator = new RockCSharpMigrationCodeGenerator<Rock.Data.RockContext>();
            CommandTimeout = 300;
        }

        protected override void Seed(Rock.Data.RockContext context)
        {
            // In V7, the Communication and CommunicationTemplate models were updated to move data stored as JSON in a varchar(max) 
            // column (MediumDataJson) to specific columns. This method will update all of the communication templates, and the most 
            // recent 50 communications. A job will runto convert the remaining communications. This can be removed after every 
            // customer has migrated past v7
            Jobs.MigrateCommunicationMediumData.UpdateCommunicationRecords( true, 50, null );

            // MP: Populate AnalyticsSourceDate (if it isn't already)
            if ( !context.AnalyticsSourceDates.AsQueryable().Any() )
            {
                var analyticsStartDate = new DateTime( RockDateTime.Today.AddYears( -150 ).Year, 1, 1 );
                var analyticsEndDate = new DateTime( RockDateTime.Today.AddYears( 101 ).Year, 1, 1 ).AddDays( -1 );
                Rock.Model.AnalyticsSourceDate.GenerateAnalyticsSourceDateData( 1, false, analyticsStartDate, analyticsEndDate );
            }
        }
    }
}
