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
    public partial class RemoveMailgunSmtp_Mandrill : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"

                -- Get var
                DECLARE @emailCommunicationMediumEntityTypeId INT = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Communication.Medium.Email')

                DECLARE @mandrillEntityTypeId INT = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Communication.Transport.MandrillSmtp')
                DECLARE @mandrillEntityTypeGuid UNIQUEIDENTIFIER = (SELECT [Guid] FROM [EntityType] WHERE [Id] = @mandrillEntityTypeId)

                DECLARE @mailgunEntityTypeId INT = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Communication.Transport.MailgunSmtp')
                DECLARE @mailgunEntityTypeGuid UNIQUEIDENTIFIER = (SELECT [Guid] FROM [EntityType] WHERE [Id] = @mailgunEntityTypeId)

                DECLARE @checklistCompletedAttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'FBB2E564-29A3-4756-A255-38565B486000')
                DECLARE @AdminChecklistDefinedTypeId INT = (SELECT [Id] FROM [DefinedType] WHERE [Guid] = '4BF34677-37E9-4E71-BD03-252B66C9373D')

                -- First see if Mailgun or Mandrill are being used and create a checklist item if they are.
                DECLARE @SmtpConfiguredTransportContainer INT = (
                SELECT COUNT(av.[Value])
                FROM [Attribute] a
                JOIN [AttributeValue] av ON a.[Id] = av.[AttributeId]
                WHERE a.[Key] = 'TransportContainer'
	                AND a.[EntityTypeId] = @emailCommunicationMediumEntityTypeId
	                AND av.[Value] IS NOT NULL
	                AND (av.[Value] = @mandrillEntityTypeGuid OR av.[Value] = @mailgunEntityTypeGuid ))

                -- Create the admin checklist item
                IF (@SmtpConfiguredTransportContainer > 0)
                BEGIN
	                -- Make room at the top of the list.
                    UPDATE DefinedValue SET [Order] = [Order] + 1 WHERE DefinedTypeId = @AdminChecklistDefinedTypeId

	                -- Insert the row
	                INSERT INTO [dbo].[DefinedValue] (
		                  [IsSystem]
                        , [DefinedTypeId]
                        , [Order]
                        , [Value]
                        , [Description]
                        , [Guid]
                        , [CreatedDateTime]
                        , [ModifiedDateTime]
                        , [IsActive])
	                VALUES (
		                  1
                        , @AdminChecklistDefinedTypeId
                        , 0
                        , 'Update the Communication Transport'
                        , '<div class=""alert alert-warning"">Rock is still configured to use a Communication Transport that is no longer available.  Configure a new transport under <span class=""navigation-tip"">Admin Tools > Communications > Communication Transports.</div>'
                        , '97AAFEBA-C153-46AE-9BDA-7AC8E75EBBB1'
                        , GETDATE()
                        , GETDATE()
                        , 1)
                END

                -- Delete the attributes for MailgunSmtp and Mandrill
                IF(@mandrillEntityTypeId IS NOT NULL)
                BEGIN
	                DELETE FROM [Attribute] WHERE [EntityTypeId] = @mandrillEntityTypeId
                END

                IF(@mailgunEntityTypeId IS NOT NULL)
                BEGIN
	                DELETE FROM [Attribute] WHERE [EntityTypeId] = @mailgunEntityTypeId
                END

                -- Delete MailgunSmtp and Mandrill
                DELETE FROM [EntityType] WHERE [Name] = 'Rock.Communication.Transport.MandrillSmtp'
                DELETE FROM [EntityType] WHERE [Name] = 'Rock.Communication.Transport.MailgunSmtp'" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
