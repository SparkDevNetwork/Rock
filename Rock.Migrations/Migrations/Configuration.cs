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
    using System.Collections.Generic;
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

        protected override void Seed( Rock.Data.RockContext context )
        {
            // MP: Populate AnalyticsSourceDate (if it isn't already)
            if ( !context.AnalyticsSourceDates.AsQueryable().Any() )
            {
                var analyticsStartDate = new DateTime( RockDateTime.Today.AddYears( -150 ).Year, 1, 1 );
                var analyticsEndDate = new DateTime( RockDateTime.Today.AddYears( 101 ).Year, 1, 1 ).AddDays( -1 );
                Rock.Model.AnalyticsSourceDate.GenerateAnalyticsSourceDateData( 1, false, analyticsStartDate, analyticsEndDate );
            }

            // MP: Migrate RegistrationTemplateFee.CostValue to RegistrationTemplateFee.FeeItems
            using ( var rockContext = new Rock.Data.RockContext() )
            {
                var registrationTemplateFeeService = new Rock.Model.RegistrationTemplateFeeService( rockContext );
#pragma warning disable 612, 618
                registrationTemplateFeeService.MigrateFeeCostValueToFeeItems();
#pragma warning restore 612, 618
                rockContext.SaveChanges();
            }
        }
    }
}
