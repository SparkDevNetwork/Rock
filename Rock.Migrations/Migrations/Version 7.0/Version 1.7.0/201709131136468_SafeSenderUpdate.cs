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
    public partial class SafeSenderUpdate : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add a Safe To Send To attribute to the Safe Sender domains.
            Sql( @"
    UPDATE [DefinedType] SET [Description] = 'Safe Sender Domains are the domains that can be used to safely send outgoing emails.  
If an email communication is created with a From Address that is not from one of these domains, Rock will change the From Address 
to the Organization Email global attribute value instead, and the original From Address will be used as the Reply To address.
Rock will not change the from address if the email is from one of these safe domains, OR if all the recipients of the email belong 
to one of these domains that has the ''Safe To Send To'' flag enabled. This is to help reduce the likelihood of communications 
being rejected by the receiving email servers due to the email having a different sending domain than the server sending the email.'
    WHERE [Guid] = 'DB91D0E9-DCA6-45A9-8276-AEF032BE8AED'
" );
            RockMigrationHelper.AddDefinedTypeAttribute( "DB91D0E9-DCA6-45A9-8276-AEF032BE8AED", Rock.SystemGuid.FieldType.BOOLEAN, "Safe To Send To", "SafeToSendTo", "Domains that are safe to send to without changing the From address.", 0, "false", "409486D1-33AF-4F4E-9721-626CF731CFAA" );
            Sql( "Update [Attribute] set [IsGridColumn] = 1 where [Guid] = '409486D1-33AF-4F4E-9721-626CF731CFAA' " );

            // MP: Add Index to InteractionComponent
            Sql( @"
    IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_EntityId_ChannelId' AND object_id = OBJECT_ID('InteractionComponent'))
    BEGIN
        CREATE INDEX [IX_EntityId_ChannelId] ON [dbo].[InteractionComponent] ([EntityId],[ChannelId])
    END
" );

            // MP: Tuneup ETL Stored Procs
            Sql( MigrationSQL._201709131136468_spAnalytics_ETL_Attendance );
            Sql( MigrationSQL._201709131136468_spAnalytics_ETL_FinancialTransaction );

            // MP: Add AnalyticsSourceAttendance Index
            Sql( @"
    IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_AttendanceType_CurrentPerson_StartDateTime' AND object_id = OBJECT_ID('AnalyticsSourceAttendance'))
    BEGIN
        CREATE INDEX [IX_AttendanceType_CurrentPerson_StartDateTime] ON [dbo].[AnalyticsSourceAttendance] ([AttendanceTypeId],[CurrentPersonKey],[StartDateTime])
    END
" );

            // MP: Update Communication InteractionChannel
            Sql( @"
    DECLARE @DefinedValueChannelTypeCommunication INT = (
        SELECT TOP 1 Id
        FROM DefinedValue
        WHERE [Guid] = '55004F5C-A8ED-7CB7-47EE-5988E9F8E0A8'
    )
    ,@InteractionChannel_COMMUNICATION UNIQUEIDENTIFIER = 'C88A187F-0343-4E7C-AF3F-79A8989DFA65'

    UPDATE InteractionChannel SET ChannelTypeMediumValueId = @DefinedValueChannelTypeCommunication
    WHERE ChannelTypeMediumValueId IS NULL
    AND [Guid] = @InteractionChannel_COMMUNICATION
" );

            // DT: Set default Communication Preference
            Sql( @"
    UPDATE [Person] SET [CommunicationPreference] = 1	-- Email
" );

            // JE: Update Universal Search Boost Global Attribute

            // update global attribute for boosting universal search models (this method deletes the existing value then adds)
            RockMigrationHelper.AddGlobalAttribute( SystemGuid.FieldType.KEY_VALUE_LIST, "", "", "Universal Search Index Boost", "Allows you to boost certain universal search indexes.", 1000, "", "757F912F-55E0-76A9-46D2-345BB61D7B02", "UniversalSearchIndexBoost" );

            // Attrib Value for Block:Universal Search, Attribute:Search Type Page: Universal Search, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue( "309A2477-9A5B-4FD4-A722-735F87861A05", "152FA041-3DA7-4BA4-A2D5-87BFA1618536", @"2" );

            // MP: Mask Account Number 
            Sql( @"
    DECLARE @CCType INT = ( SELECT TOP 1 [Id] FROM [DefinedValue] WHERE GUID = '928A2E04-C77B-4282-888F-EC549CEE026A' )

    DECLARE @Visa INT  = ( SELECT TOP 1 [Id] FROM [DefinedValue] WHERE GUID = 'FC66B5F8-634F-4800-A60D-436964D27B64' )
    DECLARE @MasterCard INT  = ( SELECT TOP 1 [Id] FROM [DefinedValue] WHERE GUID = '6373A4B6-4DCA-4EB6-9ADE-B30E8A7F8621' )
    DECLARE @Amex INT  = ( SELECT TOP 1 [Id] FROM [DefinedValue] WHERE GUID = '696A54E3-352C-49FB-88A1-BCDBD81AA9EC' )
    DECLARE @Discover INT  = ( SELECT TOP 1 [Id] FROM [DefinedValue] WHERE GUID = '4B746601-E9EB-4660-BA13-C0B66B24E248' )
    DECLARE @Diners INT  = ( SELECT TOP 1 [Id] FROM [DefinedValue] WHERE GUID = '1A9A4DB9-AFF3-4773-875C-C10346BD1CA7' )
    DECLARE @JCB INT  = ( SELECT TOP 1 [Id] FROM [DefinedValue] WHERE GUID = '4DD7F0C2-F6B7-4510-90E6-287ADC25FD05' )

    ;WITH CTE AS (
	    SELECT 
		    [Id],
		    CASE 
			    WHEN [AccountNumberMasked] LIKE '34%' AND LEN([AccountNumberMasked]) = 15 THEN @Amex
			    WHEN [AccountNumberMasked] LIKE '37%' AND LEN([AccountNumberMasked]) = 15 THEN @Amex

			    WHEN [AccountNumberMasked] LIKE '300%' AND LEN([AccountNumberMasked]) = 14 THEN @Diners
			    WHEN [AccountNumberMasked] LIKE '301%' AND LEN([AccountNumberMasked]) = 14 THEN @Diners
			    WHEN [AccountNumberMasked] LIKE '302%' AND LEN([AccountNumberMasked]) = 14 THEN @Diners
			    WHEN [AccountNumberMasked] LIKE '303%' AND LEN([AccountNumberMasked]) = 14 THEN @Diners
			    WHEN [AccountNumberMasked] LIKE '304%' AND LEN([AccountNumberMasked]) = 14 THEN @Diners
			    WHEN [AccountNumberMasked] LIKE '305%' AND LEN([AccountNumberMasked]) = 14 THEN @Diners
			    WHEN [AccountNumberMasked] LIKE '36%' AND LEN([AccountNumberMasked]) = 14 THEN @Diners

			    WHEN [AccountNumberMasked] LIKE '6011%' AND LEN([AccountNumberMasked]) = 16 THEN @Discover

			    WHEN [AccountNumberMasked] LIKE '3%' AND LEN([AccountNumberMasked]) = 16 THEN @JCB
			    WHEN [AccountNumberMasked] LIKE '1800%' AND LEN([AccountNumberMasked]) = 15 THEN @JCB
			    WHEN [AccountNumberMasked] LIKE '2131%' AND LEN([AccountNumberMasked]) = 15 THEN @JCB

			    WHEN [AccountNumberMasked] LIKE '51%' AND LEN([AccountNumberMasked]) = 16 THEN @MasterCard
			    WHEN [AccountNumberMasked] LIKE '52%' AND LEN([AccountNumberMasked]) = 16 THEN @MasterCard
			    WHEN [AccountNumberMasked] LIKE '53%' AND LEN([AccountNumberMasked]) = 16 THEN @MasterCard
			    WHEN [AccountNumberMasked] LIKE '54%' AND LEN([AccountNumberMasked]) = 16 THEN @MasterCard
			    WHEN [AccountNumberMasked] LIKE '55%' AND LEN([AccountNumberMasked]) = 16 THEN @MasterCard

			    WHEN [AccountNumberMasked] LIKE '4%' AND LEN([AccountNumberMasked]) = 13 THEN @Visa
			    WHEN [AccountNumberMasked] LIKE '4%' AND LEN([AccountNumberMasked]) = 16 THEN @Visa

		    END AS [CCType]	

	    FROM [FinancialPaymentDetail]
	    WHERE [CurrencyTypeValueId] = @CCType
	    AND [CreditCardTypeValueId] IS NULL
	    AND [AccountNumberMasked] IS NOT NULL
	    AND [AccountNumberMasked] NOT LIKE '*%'
    )

    UPDATE P SET [CreditCardTypeValueId] = DV.[Id]
    FROM CTE
    INNER JOIN [FinancialPaymentDetail] P ON P.[Id] = CTE.[Id]
    INNER JOIN [DefinedValue] DV ON DV.[Id] = CTE.[CCType]

    UPDATE FinancialPaymentDetail SET AccountNumberMasked = REPLICATE('*', len(AccountNumberMasked) - 4) + Right(AccountNumberMasked, 4)
    WHERE AccountNumberMasked IS NOT NULL
    AND REPLICATE('*', len(AccountNumberMasked) - 4) + Right(AccountNumberMasked, 4) != AccountNumberMasked

    UPDATE FinancialPersonBankAccount SET AccountNumberMasked = REPLICATE('*', len(AccountNumberMasked) - 4) + Right(AccountNumberMasked, 4)
    WHERE AccountNumberMasked IS NOT NULL
    AND REPLICATE('*', len(AccountNumberMasked) - 4) + Right(AccountNumberMasked, 4) != AccountNumberMasked
" );

            // MP: Add BinaryFileType "Communication Image"
            Sql( @"
    DECLARE @StorageEntityTypeFileSystemId INT = (
        SELECT Id
        FROM EntityType
        WHERE [Guid] = 'A97B6002-454E-4890-B529-B99F8F2F376A'
        )

    IF NOT EXISTS (
        SELECT Id
        FROM BinaryFileType
        WHERE [Guid] = '60B896C3-F00C-411C-A31C-2D5D4CCBB65F'
    )
    BEGIN
        INSERT INTO [dbo].[BinaryFileType] (
            [IsSystem]
            ,[Name]
            ,[Description]
            ,[IconCssClass]
            ,[StorageEntityTypeId]
            ,[AllowCaching]
            ,[Guid]
            ,[RequiresViewSecurity]
        )
        VALUES (
             1
            ,'Communication Image'
            ,'Image used for Communications'
            ,'fa fa-comment-o'
            ,@StorageEntityTypeFileSystemId
            ,1
            ,'60B896C3-F00C-411C-A31C-2D5D4CCBB65F'
            ,0
        );
    END
    ELSE
    BEGIN
        UPDATE [dbo].[BinaryFileType]
        SET 
             [Name] = 'Communication Image'
            ,[IsSystem] = 1
            ,[Description] = 'Image used for Communications'
            ,[IconCssClass] = 'fa fa-comment-o'
            ,[StorageEntityTypeId] = @StorageEntityTypeFileSystemId
            ,[AllowCaching] = 1
            ,[RequiresViewSecurity] = 0
        WHERE [Guid] = '60B896C3-F00C-411C-A31C-2D5D4CCBB65F'
    END
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
