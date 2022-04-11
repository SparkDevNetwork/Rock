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
    public partial class UpdateSignatureDocument2 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {

            AddColumn( "dbo.SignatureDocument", "SignatureVerificationHash", c => c.String( maxLength: 40 ) );
            Sql( "UPDATE [dbo].[SignatureDocument] SET [SignatureVerificationHash] = [SignatureVerficationHash]" );
            DropColumn( "dbo.SignatureDocument", "SignatureVerficationHash" );

            AddColumn( "dbo.SignatureDocument", "SignatureDataEncrypted", c => c.String() );
            Sql( "UPDATE [dbo].[SignatureDocument] SET [SignatureDataEncrypted] = [SignatureData]" );
            DropColumn( "dbo.SignatureDocument", "SignatureData" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn( "dbo.SignatureDocument", "SignatureVerficationHash", c => c.String( maxLength: 40 ) );
            Sql( "UPDATE [dbo].[SignatureDocument] SET [SignatureVerficationHash] = [SignatureVerificationHash]" );
            DropColumn( "dbo.SignatureDocument", "SignatureVerificationHash" );

            AddColumn( "dbo.SignatureDocument", "SignatureData", c => c.String() );
            Sql( "UPDATE [dbo].[SignatureDocument] SET [SignatureData] = [SignatureDataEncrypted]" );
            DropColumn( "dbo.SignatureDocument", "SignatureDataEncrypted" );
        }
    }
}
