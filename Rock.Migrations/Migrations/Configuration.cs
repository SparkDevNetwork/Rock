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
            // Previous to Rock v4.0 the saved routing|account numbers in the PersonBankAccount table may have included
            // leading or trailing spaces for the routing number and/or account number. v4.0 trims these leading/trailing
            // spaces before looking for and saving the routing|account numbers. Because of this, any existing saved 
            // values need to be updated so they don't include any spaces. This can probably be removed in a future update 
            // after v4.0 -DT
            var txnService = new Rock.Model.FinancialTransactionService( context );
            var personBankAcctService = new Rock.Model.FinancialPersonBankAccountService( context );

            foreach( string encryptedMicr in txnService
                .Queryable()
                .Where( t => 
                    t.CheckMicrParts != null &&
                    t.CheckMicrParts != "" )
                .Select( t => t.CheckMicrParts )
                .Distinct() )
            {
                if ( !string.IsNullOrWhiteSpace( encryptedMicr ) )
                {
                    string clearMicr = Rock.Security.Encryption.DecryptString( encryptedMicr ) ?? string.Empty;
                    var parts = clearMicr.Split( '_' );
                    if ( parts.Length >= 2 && ( parts[0] != parts[0].Trim() || parts[1] != parts[1].Trim() ) )
                    {
                        string oldHash = Rock.Security.Encryption.GetSHA1Hash( string.Format( "{0}|{1}", parts[0], parts[1] ) );
                        string newHash = Rock.Security.Encryption.GetSHA1Hash( string.Format( "{0}|{1}", parts[0].Trim(), parts[1].Trim() ) );

                        foreach( var match in personBankAcctService.Queryable().Where( a => a.AccountNumberSecured == oldHash ) )
                        {
                            match.AccountNumberSecured = newHash;
                        }

                        context.SaveChanges();
                    }
                }
            }

            // In V7, the Communication and CommunicationTemplate models were updated to move data stored as JSON in a varchar(max) 
            // column (MediumDataJson) to specific columns. This method will update all of the communication templates, and the most 
            // recent 5000 communications. A job will runto convert the remaining communications. This can be removed after every 
            // customer has migrated past v7
            Jobs.MigrateCommunicationMediumData.UpdateCommunicationRecords( true, 5000 );

        }
    }
}
