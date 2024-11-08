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
    public partial class AssignSignatureDocumentToRegistrationRegistrant : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.SignatureDocumentTemplate", "IsValidInFuture", c => c.Boolean(nullable: false));
            AddColumn("dbo.SignatureDocumentTemplate", "ValidityDurationInDays", c => c.Int());

            AddColumn( "dbo.RegistrationRegistrant", "SignatureDocumentId", c => c.Int() );
            CreateIndex("dbo.RegistrationRegistrant", "SignatureDocumentId");
            AddForeignKey("dbo.RegistrationRegistrant", "SignatureDocumentId", "dbo.SignatureDocument", "Id");

            // Copy over the Signature Document Id from the Signature Document Table to the SignatureDocumentId column of the RegistrationRegistrant Table.
            // Once that is done, nullify the EntityId and the EntityTypeId columns of the Signature Documents which are used by the RegistrationRegistrant.
            Sql( @"DECLARE @RegistrationRegistrantEntityTypeId INT = (SELECT [Id] FROM [EntityType] WHERE [Guid] = '8A25E5CE-1B4F-4825-BCEA-216167836305');
                    UPDATE rr
	                    SET [SignatureDocumentId] = sd.[Id]
                    FROM [RegistrationRegistrant] rr 
                    JOIN [SignatureDocument] sd ON rr.[Id] = sd.[EntityId] AND sd.[EntityTypeId] = @RegistrationRegistrantEntityTypeId;
                    UPDATE [SignatureDocument]
                        SET [EntityId] = NULL, [EntityTypeId] = NULL
                    WHERE [EntityTypeId] = @RegistrationRegistrantEntityTypeId" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"DECLARE @RegistrationRegistrantEntityTypeId INT = (SELECT [Id] FROM [EntityType] WHERE [Guid] = '8A25E5CE-1B4F-4825-BCEA-216167836305');
                    UPDATE [SignatureDocument]
                        SET [EntityId] = rr.[Id], [EntityTypeId] = @RegistrationRegistrantEntityTypeId
					FROM [RegistrationRegistrant] rr 
                    JOIN [SignatureDocument] sd ON rr.[SignatureDocumentId] = sd.[Id]" );

            DropForeignKey("dbo.RegistrationRegistrant", "SignatureDocumentId", "dbo.SignatureDocument");
            DropIndex("dbo.RegistrationRegistrant", new[] { "SignatureDocumentId" });
            DropColumn("dbo.RegistrationRegistrant", "SignatureDocumentId");

            DropColumn( "dbo.SignatureDocumentTemplate", "ValidityDurationInDays" );
            DropColumn( "dbo.SignatureDocumentTemplate", "IsValidInFuture" );
        }
    }
}
