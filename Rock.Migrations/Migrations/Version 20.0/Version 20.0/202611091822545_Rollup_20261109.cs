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

    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_20261109 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // ----------------------------------------------------------------
            // HotFix data-migrations moved to this EF migration (v20):
            // Note: These are ordered by the Rock version they are expected to
            //       run in.
            // ----------------------------------------------------------------
            // v17.8; 330_EscapeMapInfoWindowAndSignatureReceiptTemplates.cs
            MSE_EscapeGroupMapInfoWindowTemplates_Up();
            MSE_EscapeElectronicSignatureReceipt_Up();

            // v17.8; 331_AddSystemPhoneNumberViewSecurity.cs
            JMH_AddSystemPhoneNumberViewAuth_Up();

            // v17.8; 332_AllowSmartSearchRequestsOnResultsPage.cs
            CH_AddAllowSmartSearchRequestsAttribute_Up();
            CH_EnableAllowSmartSearchRequestsOnResultsBlock_Up();

            // v17.8; 335_SecureDocumentTypeEntityTypeView.cs
            JMH_SecureDocumentTypeEntityTypeView_Up();

            // v17.8; 336_RestrictWorkflowActionTypeRestEdits.cs
            MSE_RestrictWorkflowActionTypeRestEdits_Up();

            // v17.8; 337_LinkDocumentBinaryFilesToDocument.cs
            CH_LinkDocumentBinaryFilesToDocument_Up();

            // v17.8; 338_RemoveDefaultPresenceRestKey.cs
            MSE_RemoveDefaultPresenceRestKey_Up();

            // v17.8; 341_RemoveTagRestControllerAllUsersAuth.cs
            JMH_RemoveTagRestControllerAllUsersAuth_Up();

            // v17.8; 344_EscapeStepFlowFooterLavaTemplate.cs
            JMH_EscapeStepFlowFooterLavaTemplate_Up();

            // v19.8; 333_LimitDynamicAuthClients.cs
            JPH_LimitDynamicAuthClients_Up();

            // JME: Update to Workflow Builder
            JME_UpdateToWorkflowBuilder_Up();

            CleanupMigrationHistory();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        #region 330_EscapeMapInfoWindowAndSignatureReceiptTemplates

        /*
            10/19/2026 - MSE

            Two stored templates still printed user supplied text as raw HTML after
            the code fixes, because changing a code default does not reach existing
            installs:

            1. The info window templates of the Group Map, Group Finder and Group Type
               Map blocks print group, person and location values that anonymous or
               low privileged people can set. Each block also keeps a saved copy of
               the template, so the saved values are updated too.
            2. The Electronic Signature Receipt email prints the typed signer name,
               the document name and person names. The anonymous signer of a public
               workflow controls the typed name.

            Only the exact original tokens are changed, so a template that already
            uses the Escape filter is left alone and running this again changes
            nothing. Customized templates that still contain an exact token are
            updated too; the Escape filter is safe in HTML text and attributes.

            Reason: Prevent stored XSS through map info windows and signature receipts.
        */

        /// <summary>
        /// MSE: Adds the Escape filter to the merge fields that the code fix changed in the
        /// default info window templates of the Group Map, Group Finder and Group Type
        /// Map blocks. Both the saved block setting values and the attribute default
        /// values are updated. Attributes are found by block type and key because the
        /// attribute guids are not the same on every install. Each token is matched
        /// with and without spaces inside the braces, using a binary collation so the
        /// match is exact (case and spacing) and an already escaped token never matches.
        /// </summary>
        private void MSE_EscapeGroupMapInfoWindowTemplates_Up()
        {
            Sql( @"
DECLARE @Tokens TABLE (
    [Id] int IDENTITY(1,1) NOT NULL,
    [BlockTypeGuid] uniqueidentifier NOT NULL,
    [AttributeKey] nvarchar(1000) NOT NULL,
    [OldText] nvarchar(200) NOT NULL,
    [NewText] nvarchar(200) NOT NULL )

/* Group Map: InfoWindowContents */
INSERT INTO @Tokens ( [BlockTypeGuid], [AttributeKey], [OldText], [NewText] ) VALUES
      ( '967F0D2B-DB76-486A-B034-D22B9D9240D3', N'InfoWindowContents', N'{{ GroupName }}', N'{{ GroupName | Escape }}' )
    , ( '967F0D2B-DB76-486A-B034-D22B9D9240D3', N'InfoWindowContents', N'{{GroupName}}', N'{{GroupName | Escape}}' )
    , ( '967F0D2B-DB76-486A-B034-D22B9D9240D3', N'InfoWindowContents', N'{{ GroupMember.NickName }}', N'{{ GroupMember.NickName | Escape }}' )
    , ( '967F0D2B-DB76-486A-B034-D22B9D9240D3', N'InfoWindowContents', N'{{GroupMember.NickName}}', N'{{GroupMember.NickName | Escape}}' )
    , ( '967F0D2B-DB76-486A-B034-D22B9D9240D3', N'InfoWindowContents', N'{{ GroupMember.LastName }}', N'{{ GroupMember.LastName | Escape }}' )
    , ( '967F0D2B-DB76-486A-B034-D22B9D9240D3', N'InfoWindowContents', N'{{GroupMember.LastName}}', N'{{GroupMember.LastName | Escape}}' )
    , ( '967F0D2B-DB76-486A-B034-D22B9D9240D3', N'InfoWindowContents', N'{{ GroupMember.Email }}', N'{{ GroupMember.Email | Escape }}' )
    , ( '967F0D2B-DB76-486A-B034-D22B9D9240D3', N'InfoWindowContents', N'{{GroupMember.Email}}', N'{{GroupMember.Email | Escape}}' )

/* Group Finder: MapInfo */
INSERT INTO @Tokens ( [BlockTypeGuid], [AttributeKey], [OldText], [NewText] ) VALUES
      ( '9F8F2D68-DEEA-4686-810F-AB32923F855E', N'MapInfo', N'{{ Group.Name }}', N'{{ Group.Name | Escape }}' )
    , ( '9F8F2D68-DEEA-4686-810F-AB32923F855E', N'MapInfo', N'{{Group.Name}}', N'{{Group.Name | Escape}}' )

/* Group Type Map: InfoWindowContents */
INSERT INTO @Tokens ( [BlockTypeGuid], [AttributeKey], [OldText], [NewText] ) VALUES
      ( '2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF', N'InfoWindowContents', N'{{GroupName}}', N'{{GroupName | Escape}}' )
    , ( '2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF', N'InfoWindowContents', N'{{ GroupName }}', N'{{ GroupName | Escape }}' )
    , ( '2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF', N'InfoWindowContents', N'{{GroupLocation.Street1}}', N'{{GroupLocation.Street1 | Escape}}' )
    , ( '2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF', N'InfoWindowContents', N'{{ GroupLocation.Street1 }}', N'{{ GroupLocation.Street1 | Escape }}' )
    , ( '2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF', N'InfoWindowContents', N'{{GroupLocation.City}}', N'{{GroupLocation.City | Escape}}' )
    , ( '2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF', N'InfoWindowContents', N'{{ GroupLocation.City }}', N'{{ GroupLocation.City | Escape }}' )
    , ( '2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF', N'InfoWindowContents', N'{{GroupLocation.State}}', N'{{GroupLocation.State | Escape}}' )
    , ( '2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF', N'InfoWindowContents', N'{{ GroupLocation.State }}', N'{{ GroupLocation.State | Escape }}' )
    , ( '2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF', N'InfoWindowContents', N'{{GroupLocation.PostalCode}}', N'{{GroupLocation.PostalCode | Escape}}' )
    , ( '2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF', N'InfoWindowContents', N'{{ GroupLocation.PostalCode }}', N'{{ GroupLocation.PostalCode | Escape }}' )
    , ( '2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF', N'InfoWindowContents', N'{{GroupMember.NickName}}', N'{{GroupMember.NickName | Escape}}' )
    , ( '2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF', N'InfoWindowContents', N'{{ GroupMember.NickName }}', N'{{ GroupMember.NickName | Escape }}' )
    , ( '2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF', N'InfoWindowContents', N'{{GroupMember.LastName}}', N'{{GroupMember.LastName | Escape}}' )
    , ( '2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF', N'InfoWindowContents', N'{{ GroupMember.LastName }}', N'{{ GroupMember.LastName | Escape }}' )
    , ( '2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF', N'InfoWindowContents', N'{{GroupMember.Email}}', N'{{GroupMember.Email | Escape}}' )
    , ( '2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF', N'InfoWindowContents', N'{{ GroupMember.Email }}', N'{{ GroupMember.Email | Escape }}' )

DECLARE @TokenId int = 1
DECLARE @MaxTokenId int = ( SELECT MAX( [Id] ) FROM @Tokens )
DECLARE @AttributeId int
DECLARE @OldText nvarchar(200)
DECLARE @NewText nvarchar(200)

WHILE @TokenId <= @MaxTokenId
BEGIN
    SELECT @AttributeId = A.[Id]
         , @OldText = T.[OldText]
         , @NewText = T.[NewText]
      FROM @Tokens T
     INNER JOIN [BlockType] BT
        ON BT.[Guid] = T.[BlockTypeGuid]
     INNER JOIN [Attribute] A
        ON A.[EntityTypeQualifierColumn] = 'BlockTypeId'
       AND A.[EntityTypeQualifierValue] = CAST( BT.[Id] AS nvarchar(50) )
       AND A.[Key] = T.[AttributeKey]
     WHERE T.[Id] = @TokenId

    IF @@ROWCOUNT = 1
    BEGIN
        UPDATE [Attribute]
           SET [DefaultValue] = REPLACE( [DefaultValue] COLLATE Latin1_General_BIN2, @OldText, @NewText )
             , [IsDefaultPersistedValueDirty] = 1
         WHERE [Id] = @AttributeId
           AND CHARINDEX( @OldText, [DefaultValue] COLLATE Latin1_General_BIN2 ) > 0

        UPDATE [AttributeValue]
           SET [Value] = REPLACE( [Value] COLLATE Latin1_General_BIN2, @OldText, @NewText )
             , [IsPersistedValueDirty] = 1
         WHERE [AttributeId] = @AttributeId
           AND CHARINDEX( @OldText, [Value] COLLATE Latin1_General_BIN2 ) > 0
    END

    SET @TokenId = @TokenId + 1
END" );
        }

        /// <summary>
        /// MSE: Adds the Escape filter to the signed name, the document name and the person
        /// names in the Electronic Signature Receipt system communication body. The
        /// stock body uses non-breaking spaces inside most of these tokens, so both
        /// the non-breaking space and the normal space forms are matched. A binary
        /// collation keeps the match exact, so an already escaped token never matches.
        /// The Default filter stays before Escape so the fallback text is unchanged.
        /// </summary>
        private void MSE_EscapeElectronicSignatureReceipt_Up()
        {
            Sql( @"
DECLARE @Nbsp nchar(1) = NCHAR(160)
DECLARE @Tokens TABLE (
    [Id] int IDENTITY(1,1) NOT NULL,
    [OldText] nvarchar(200) NOT NULL,
    [NewText] nvarchar(200) NOT NULL )

INSERT INTO @Tokens ( [OldText], [NewText] ) VALUES
      ( N'{{' + @Nbsp + N'SignatureDocument.SignedName' + @Nbsp + N'}}', N'{{ SignatureDocument.SignedName | Escape }}' )
    , ( N'{{ SignatureDocument.SignedName }}', N'{{ SignatureDocument.SignedName | Escape }}' )
    , ( N'{{' + @Nbsp + N'SignatureDocument.Name' + @Nbsp + N'}}', N'{{ SignatureDocument.Name | Escape }}' )
    , ( N'{{ SignatureDocument.Name }}', N'{{ SignatureDocument.Name | Escape }}' )
    , ( N'{{' + @Nbsp + N'SignatureDocument.SignedByPersonAlias.Person.FullName' + @Nbsp + N'|' + @Nbsp + N'Default:''Hello''' + @Nbsp + N'}}', N'{{ SignatureDocument.SignedByPersonAlias.Person.FullName | Default:''Hello'' | Escape }}' )
    , ( N'{{ SignatureDocument.SignedByPersonAlias.Person.FullName | Default:''Hello'' }}', N'{{ SignatureDocument.SignedByPersonAlias.Person.FullName | Default:''Hello'' | Escape }}' )
    , ( N'{{' + @Nbsp + N'SignatureDocument.SignedByPersonAlias.Person.FullName' + @Nbsp + N'}}', N'{{ SignatureDocument.SignedByPersonAlias.Person.FullName | Escape }}' )
    , ( N'{{ SignatureDocument.SignedByPersonAlias.Person.FullName }}', N'{{ SignatureDocument.SignedByPersonAlias.Person.FullName | Escape }}' )
    , ( N'{{' + @Nbsp + N'SignatureDocument.AppliesToPersonAlias.Person.FullName' + @Nbsp + N'|' + @Nbsp + N'Default:''-''' + @Nbsp + N'}}', N'{{ SignatureDocument.AppliesToPersonAlias.Person.FullName | Default:''-'' | Escape }}' )
    , ( N'{{ SignatureDocument.AppliesToPersonAlias.Person.FullName | Default:''-'' }}', N'{{ SignatureDocument.AppliesToPersonAlias.Person.FullName | Default:''-'' | Escape }}' )

DECLARE @TokenId int = 1
DECLARE @MaxTokenId int = ( SELECT MAX( [Id] ) FROM @Tokens )
DECLARE @OldText nvarchar(200)
DECLARE @NewText nvarchar(200)

WHILE @TokenId <= @MaxTokenId
BEGIN
    SELECT @OldText = [OldText]
         , @NewText = [NewText]
      FROM @Tokens
     WHERE [Id] = @TokenId

    UPDATE [SystemCommunication]
       SET [Body] = REPLACE( [Body] COLLATE Latin1_General_BIN2, @OldText, @NewText )
     WHERE [Guid] = '224A0E80-069B-463C-8187-E13682F8A550'
       AND CHARINDEX( @OldText, [Body] COLLATE Latin1_General_BIN2 ) > 0

    SET @TokenId = @TokenId + 1
END" );
        }

        #endregion 330_EscapeMapInfoWindowAndSignatureReceiptTemplates

        #region 331_AddSystemPhoneNumberViewSecurity

        private const string SystemPhoneNumberEntityTypeName = "Rock.Model.SystemPhoneNumber";

        /// <summary>
        /// JMH: Allows View on the System Phone Number entity type for the administration and staff
        /// security roles, then denies View for all other users.
        /// </summary>
        /// <remarks>
        /// Each rule is appended after any entity type View rules that already exist,
        /// so rules an administrator has added keep their precedence.
        /// </remarks>
        private void JMH_AddSystemPhoneNumberViewAuth_Up()
        {
            RockMigrationHelper.AddSecurityAuthForEntityType( SystemPhoneNumberEntityTypeName,
                int.MaxValue,
                Security.Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_ADMINISTRATORS,
                ( int ) Model.SpecialRole.None,
                "C64A249D-6210-46C4-B06C-0E3C6EF494D1" );

            RockMigrationHelper.AddSecurityAuthForEntityType( SystemPhoneNumberEntityTypeName,
                int.MaxValue,
                Security.Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_COMMUNICATION_ADMINISTRATORS,
                ( int ) Model.SpecialRole.None,
                "E82E4FC1-B45C-4315-8D93-D75C91877F6A" );

            RockMigrationHelper.AddSecurityAuthForEntityType( SystemPhoneNumberEntityTypeName,
                int.MaxValue,
                Security.Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_STAFF_MEMBERS,
                ( int ) Model.SpecialRole.None,
                "EB1F5BB8-96EA-4A4A-893C-65A5C2EC838D" );

            RockMigrationHelper.AddSecurityAuthForEntityType( SystemPhoneNumberEntityTypeName,
                int.MaxValue,
                Security.Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_STAFF_LIKE_MEMBERS,
                ( int ) Model.SpecialRole.None,
                "54087D1B-5808-4EDE-808D-3528279E2151" );

            // The helper's duplicate check compares GroupId with = and never matches a NULL group,
            // so check for an existing All Users rule here to avoid a duplicate or a Guid conflict.
            var allUsersRuleCount = SqlScalar( $@"
SELECT COUNT(*)
FROM [Auth] a
INNER JOIN [EntityType] et ON et.[Id] = a.[EntityTypeId]
WHERE et.[Name] = '{SystemPhoneNumberEntityTypeName}'
    AND a.[EntityId] = 0
    AND a.[Action] = '{Security.Authorization.VIEW}'
    AND a.[SpecialRole] = {( int ) Model.SpecialRole.AllUsers}
    AND a.[GroupId] IS NULL" ).ToStringSafe().AsInteger();

            if ( allUsersRuleCount == 0 )
            {
                RockMigrationHelper.AddSecurityAuthForEntityType( SystemPhoneNumberEntityTypeName,
                    int.MaxValue,
                    Security.Authorization.VIEW,
                    false,
                    null,
                    ( int ) Model.SpecialRole.AllUsers,
                    "30CC7B5E-7104-466E-A522-BD5AB9139C67" );
            }
        }

        #endregion 331_AddSystemPhoneNumberViewSecurity

        #region 332_AllowSmartSearchRequestsOnResultsPage

        private const string UniversalSearchBlockTypeGuid = "FDF1BBFF-7A7B-4F4E-BF34-831203B0FEAC";
        private const string UniversalSearchResultsBlockGuid = "309A2477-9A5B-4FD4-A722-735F87861A05";
        private const string AllowSmartSearchRequestsAttributeGuid = "2BAFBB7F-654B-42BC-AEC2-240ECD4BF96A";
        private const string AllowSmartSearchRequestsAttributeKey = "AllowSmartSearchRequests";

        /*
            10/19/2026 - CH

            The Universal Search block now ignores the SmartSearch page parameter
            unless its new Allow Smart Search Requests setting is on, and that
            setting defaults to off. Without this migration the header Smart Search
            (Universal Search component) would stop using the Smart Search settings
            on Rock's built-in results page.

            Only the block Rock seeds on the internal Universal Search page is turned
            on, and only if it still exists and is still a Universal Search block.
            Every other Universal Search block, including any on a public page, stays
            off until an administrator turns the setting on.

            Reason: Keep the built-in Smart Search results page working after the fix.
        */

        /// <summary>
        /// CH: Adds the Allow Smart Search Requests block type attribute so it exists
        /// before the block type is registered at startup.
        /// </summary>
        private void CH_AddAllowSmartSearchRequestsAttribute_Up()
        {
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute(
                UniversalSearchBlockTypeGuid,
                SystemGuid.FieldType.BOOLEAN,
                "Allow Smart Search Requests",
                AllowSmartSearchRequestsAttributeKey,
                "",
                "When enabled, a request with the SmartSearch page parameter uses the global Smart Search entities and field criteria instead of this block's Enabled Models and Base Field Filters. Only enable this on the internal Smart Search results page.",
                12,
                "False",
                AllowSmartSearchRequestsAttributeGuid );
        }

        /// <summary>
        /// CH: Turns on Allow Smart Search Requests for the Universal Search block Rock
        /// seeds on the internal Universal Search page. Does nothing if that block
        /// is missing or has been changed to another block type.
        /// </summary>
        private void CH_EnableAllowSmartSearchRequestsOnResultsBlock_Up()
        {
            Sql( $@"
DECLARE @BlockTypeId int = (
    SELECT [Id]
    FROM [BlockType]
    WHERE [Guid] = '{UniversalSearchBlockTypeGuid}' )

DECLARE @BlockId int = (
    SELECT [Id]
    FROM [Block]
    WHERE [Guid] = '{UniversalSearchResultsBlockGuid}'
        AND [BlockTypeId] = @BlockTypeId )

DECLARE @AttributeId int = (
    SELECT [Id]
    FROM [Attribute]
    WHERE [Guid] = '{AllowSmartSearchRequestsAttributeGuid}' )

IF @BlockId IS NOT NULL AND @AttributeId IS NOT NULL
BEGIN
    IF EXISTS ( SELECT 1 FROM [AttributeValue] WHERE [AttributeId] = @AttributeId AND [EntityId] = @BlockId )
    BEGIN
        UPDATE [AttributeValue]
        SET [Value] = 'True'
        WHERE [AttributeId] = @AttributeId
            AND [EntityId] = @BlockId
    END
    ELSE
    BEGIN
        INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
        VALUES ( 1, @AttributeId, @BlockId, 'True', NEWID() )
    END
END" );
        }

        #endregion 332_AllowSmartSearchRequestsOnResultsPage

        #region 335_SecureDocumentTypeEntityTypeView

        /// <summary>
        /// JMH: Gives the Document Type entity type staff-only View rules when it has none, so document types without their own rules are no longer viewable by everyone.
        /// </summary>
        private void JMH_SecureDocumentTypeEntityTypeView_Up()
        {
            /*
                10/21/26 - JMH

                Document files are authorized against the document, then its document type,
                then rules on the Document Type entity type (EntityId 0), then Global Default,
                which allows All Users to View. Entity type rules close that fall-through
                for every document type without View rules of its own, including ones added later,
                while rules on a document or document type still win.

                The guard skips the insert when the entity type already has any View rule,
                so View security an administrator configured is left alone.

                Reason: Files of custom document types could be downloaded anonymously.
            */
            Sql( @"
DECLARE @DocumentTypeEntityTypeId     INT = ( SELECT [Id] FROM [EntityType] WHERE [Guid] = '18CF366F-46B6-49CA-B557-BCABD6BBD175' ); -- Rock.Model.DocumentType
DECLARE @RSRRockAdministrationGroupId INT = ( SELECT [Id] FROM [Group] WHERE [Guid] = '628C51A8-4613-43ED-A18D-4A6FB999273E' ); -- RSR - Rock Administration
DECLARE @StaffWorkersGroupId          INT = ( SELECT [Id] FROM [Group] WHERE [Guid] = '2C112948-FF4C-46E7-981A-0257681EADF4' ); -- RSR - Staff Workers
DECLARE @StaffLikeWorkersGroupId      INT = ( SELECT [Id] FROM [Group] WHERE [Guid] = '300BA2C8-49A3-44BA-A82A-82E3FD8C3745' ); -- RSR - Staff Like Workers

IF  @DocumentTypeEntityTypeId IS NOT NULL
AND @RSRRockAdministrationGroupId IS NOT NULL
AND NOT EXISTS
(
    SELECT 1
    FROM   [Auth]
    WHERE  [EntityTypeId] = @DocumentTypeEntityTypeId
      AND  [EntityId]     = 0
      AND  [Action]       = 'View'
)
BEGIN
    DECLARE @Now DATETIME = GETDATE();

    INSERT INTO [Auth] (
        [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny],
        [SpecialRole], [GroupId], [PersonAliasId], [Guid],
        [CreatedDateTime], [ModifiedDateTime]
    )
    SELECT @DocumentTypeEntityTypeId, 0, [s].[Order], 'View', [s].[AllowOrDeny], [s].[SpecialRole], [s].[GroupId], NULL, [s].[Guid], @Now, @Now
    FROM
    (
        SELECT 0 AS [Order], 'A' AS [AllowOrDeny], 0 AS [SpecialRole], @RSRRockAdministrationGroupId AS [GroupId], CAST( '4C83631D-7569-4E35-B16A-083B3648347E' AS UNIQUEIDENTIFIER ) AS [Guid]
        UNION ALL SELECT 1, 'A', 0, @StaffWorkersGroupId, 'EA2B260F-5153-4B01-9A00-3E20E9B6BD82' WHERE @StaffWorkersGroupId IS NOT NULL
        UNION ALL SELECT 2, 'A', 0, @StaffLikeWorkersGroupId, '9FFDC852-828A-4C82-B24F-F18CC1FE7344' WHERE @StaffLikeWorkersGroupId IS NOT NULL
        UNION ALL SELECT 3, 'D', 1, NULL, '4F7EB6F9-2845-4E7E-AFBC-C685C52972A1'
    ) AS [s];
END
" );
        }

        #endregion 335_SecureDocumentTypeEntityTypeView

        #region 336_RestrictWorkflowActionTypeRestEdits

        /// <summary>
        /// MSE: Restricts inherited Edit access to the v1 workflow action type REST controller while preserving custom security.
        /// </summary>
        private void MSE_RestrictWorkflowActionTypeRestEdits_Up()
        {
            /*
                10/25/2026 - MSE

                Workflow action type writes inherit staff controller permissions by default.
                Append an administrator allowance and a terminal denial only when this controller
                has no terminal Edit rule. Existing specific rules and action rules continue to win.
                Register a missing controller by its generated identity so a fresh installation
                cannot miss this restriction before controller discovery runs.

                Reason: Creating workflow actions must not inherit general staff REST Edit access.
            */
            Sql( @"
DECLARE @ControllerClassName NVARCHAR(500) = 'Rock.Rest.Controllers.WorkflowActionTypesController';
DECLARE @ControllerGuid UNIQUEIDENTIFIER = 'E8C004E7-86F1-4AC0-9B43-C7F381ECD4BA';
DECLARE @RestControllerEntityTypeId INT = ( SELECT [Id] FROM [EntityType] WHERE [Guid] = '65CDFD5B-A9AA-48FA-8D22-669612D5EA7D' );
DECLARE @AdministrationGroupId INT = ( SELECT [Id] FROM [Group] WHERE [Guid] = '628C51A8-4613-43ED-A18D-4A6FB999273E' );

IF @RestControllerEntityTypeId IS NULL OR @AdministrationGroupId IS NULL
BEGIN
    THROW 51000, 'Workflow action REST security requires the RestController entity type and Rock Administration role.', 1;
END;

IF ( SELECT COUNT(*) FROM [RestController] WHERE [ClassName] = @ControllerClassName ) > 1
BEGIN
    THROW 51000, 'Workflow action REST security found ambiguous controller registrations.', 1;
END;

DECLARE @ControllerId INT = ( SELECT [Id] FROM [RestController] WHERE [ClassName] = @ControllerClassName );

IF EXISTS
(
    SELECT 1 FROM [RestController]
    WHERE [Guid] = @ControllerGuid
        AND ( @ControllerId IS NULL OR [Id] <> @ControllerId )
)
BEGIN
    THROW 51000, 'Workflow action REST security found a conflicting controller identifier.', 1;
END;

IF @ControllerId IS NULL
BEGIN
    INSERT INTO [RestController] ( [Name], [ClassName], [Guid] )
    VALUES ( 'WorkflowActionTypes', @ControllerClassName, @ControllerGuid );

    SET @ControllerId = CONVERT( INT, SCOPE_IDENTITY() );
END;

IF NOT EXISTS
(
    SELECT 1 FROM [Auth]
    WHERE [EntityTypeId] = @RestControllerEntityTypeId
        AND [EntityId] = @ControllerId
        AND [Action] = 'Edit'
        AND [SpecialRole] = 1
)
BEGIN
    DECLARE @AdminAllowGuid UNIQUEIDENTIFIER = 'A7FF7224-1764-4532-9497-9715FF8D1646';
    DECLARE @AllUsersDenyGuid UNIQUEIDENTIFIER = 'B11BBE93-A9C4-430F-A489-38463F51286C';
    DECLARE @NeedsAdminAllow BIT = CASE WHEN EXISTS
    (
        SELECT 1 FROM [Auth]
        WHERE [EntityTypeId] = @RestControllerEntityTypeId
            AND [EntityId] = @ControllerId
            AND [Action] = 'Edit'
            AND [AllowOrDeny] = 'A'
            AND [SpecialRole] = 0
            AND [GroupId] = @AdministrationGroupId
    ) THEN 0 ELSE 1 END;
    DECLARE @NextOrder BIGINT = ISNULL
    (
        ( SELECT MAX( CONVERT( BIGINT, [Order] ) ) FROM [Auth]
          WHERE [EntityTypeId] = @RestControllerEntityTypeId
            AND [EntityId] = @ControllerId
            AND [Action] = 'Edit' ), -1
    ) + 1;

    IF @NextOrder + CONVERT( INT, @NeedsAdminAllow ) > 2147483647
    BEGIN
        THROW 51000, 'Workflow action REST security cannot append rules beyond the maximum security order.', 1;
    END;

    DECLARE @Now DATETIME = GETDATE();

    IF @NeedsAdminAllow = 1
    BEGIN
        INSERT INTO [Auth]
        (
            [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny],
            [SpecialRole], [GroupId], [PersonAliasId], [Guid], [CreatedDateTime], [ModifiedDateTime]
        )
        VALUES
        (
            @RestControllerEntityTypeId, @ControllerId, @NextOrder, 'Edit', 'A',
            0, @AdministrationGroupId, NULL, @AdminAllowGuid, @Now, @Now
        );

        SET @NextOrder = @NextOrder + 1;
    END;

    INSERT INTO [Auth]
    (
        [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny],
        [SpecialRole], [GroupId], [PersonAliasId], [Guid], [CreatedDateTime], [ModifiedDateTime]
    )
    VALUES
    (
        @RestControllerEntityTypeId, @ControllerId, @NextOrder, 'Edit', 'D',
        1, NULL, NULL, @AllUsersDenyGuid, @Now, @Now
    );
END;
" );
        }

        #endregion 336_RestrictWorkflowActionTypeRestEdits

        #region 337_LinkDocumentBinaryFilesToDocument

        /// <summary>
        /// CH: Links existing document files to their Document so file delivery applies Document security.
        /// </summary>
        private void CH_LinkDocumentBinaryFilesToDocument_Up()
        {
            Sql( @"
DECLARE @DocumentEntityTypeId INT = ( SELECT [Id] FROM [EntityType] WHERE [Guid] = '7CEFA340-9D98-4B2C-B462-313C61944B6C' ); -- Rock.Model.Document

IF @DocumentEntityTypeId IS NOT NULL
BEGIN
    UPDATE [bf]
    SET [bf].[ParentEntityTypeId] = @DocumentEntityTypeId
        , [bf].[ParentEntityId] = [d].[DocumentId]
    FROM [BinaryFile] AS [bf]
    INNER JOIN
    (
        SELECT [BinaryFileId], MIN( [Id] ) AS [DocumentId]
        FROM [Document]
        GROUP BY [BinaryFileId]
    ) AS [d] ON [d].[BinaryFileId] = [bf].[Id]
    WHERE [bf].[ParentEntityTypeId] IS NULL
        AND [bf].[ParentEntityId] IS NULL;
END
" );
        }

        #endregion 337_LinkDocumentBinaryFilesToDocument

        #region 338_RemoveDefaultPresenceRestKey

        /// <summary>
        /// MSE: Removes the default presence login and its default credentials that were included with new installs.
        /// </summary>
        private void MSE_RemoveDefaultPresenceRestKey_Up()
        {
            /*
                10/29/2026 - MSE

                New installs included a built in presence login with a default REST key
                and a default password. Clear the default key, remove the login while it
                is still exactly as shipped, and clear the default password from any login
                that still has it. A login an administrator changed (new key or new password)
                is kept. The Presence person, its alias and its security rules are kept.
                Each statement matches only the shipped values, so it is safe to run again.

                Reason: Removes the default presence login and its default credentials that were included with new installs.
            */
            Sql( @"
UPDATE [UserLogin]
SET [ApiKey] = NULL
WHERE [ApiKey] = 'B421A0508102448AA50506E3452BB08D';

DELETE FROM [UserLogin]
WHERE [Guid] = '0CEB5F7F-AD1D-493E-8E7F-B6C12DCB5646'
    AND [UserName] = 'presence'
    AND [Password] = '$2a$11$bRBPqjG.Lzalcf6EN8msrebJEmzshVZdUIBgiVvqHgUzwRZDhfd46'
    AND ( [ApiKey] IS NULL OR [ApiKey] = 'B421A0508102448AA50506E3452BB08D' );

UPDATE [UserLogin]
SET [Password] = NULL
WHERE [Password] = '$2a$11$bRBPqjG.Lzalcf6EN8msrebJEmzshVZdUIBgiVvqHgUzwRZDhfd46';" );
        }

        #endregion 338_RemoveDefaultPresenceRestKey

        #region 341_RemoveTagRestControllerAllUsersAuth

        /// <summary>
        /// JMH: Updates the default security on the Tags and Tagged Items REST controllers.
        /// </summary>
        private void JMH_RemoveTagRestControllerAllUsersAuth_Up()
        {
            /*
                11/02/26 - JMH

                The rules are matched by their shipped Guids and shipped values,
                so a rule an administrator added or changed is kept and the statement is safe to run again.

                Reason: Only remove the unchanged default rules, never administrator-added or edited ones.
            */
            Sql( @"
DELETE FROM [Auth]
WHERE [Guid] IN (
        '9D921716-A946-4DEF-9EF9-40A5CD3FFAA4',
        'A9576D72-A98B-42AE-8A04-DEDCEEEF0671',
        '6212376F-B47B-46E1-B5DC-47B317BEC5F5' )
    AND [AllowOrDeny] = 'A'
    AND [SpecialRole] = 1
    AND [GroupId] IS NULL
    AND [PersonAliasId] IS NULL;" );
        }

        #endregion 341_RemoveTagRestControllerAllUsersAuth

        #region 344_EscapeStepFlowFooterLavaTemplate

        /// <summary>
        /// JMH: Adds the Escape filter to the step name and color in Step Flow footer templates that still match the shipped default.
        /// </summary>
        private void JMH_EscapeStepFlowFooterLavaTemplate_Up()
        {
            /*
                11/09/26 - JMH

                Each saved Step Flow block keeps its own copy of the footer template,
                so changing the code default does not reach blocks whose settings were saved.
                Only values that match the shipped default once carriage returns are removed are changed,
                so customized templates are left alone and running this again changes nothing.
                The tokens are replaced in place, so the stored line endings are kept.

                Reason: Escape step names in saved Step Flow footers without touching customized templates.
            */
            Sql( @"
DECLARE @OldTemplate nvarchar(max) = N'<div class=""flow-legend"">
{% for stepItem in Steps %}
    <div class=""flow-key"">
        <span class=""color"" style=""background-color:{{stepItem.Color}};""></span>
        <span>{{forloop.index}}. {{stepItem.StepName}}</span>
    </div>
{% endfor %}
</div>'
SET @OldTemplate = REPLACE( @OldTemplate, NCHAR(13), N'' )

DECLARE @AttributeId int = (
    SELECT A.[Id]
      FROM [Attribute] A
     INNER JOIN [BlockType] BT
        ON A.[EntityTypeQualifierColumn] = 'BlockTypeId'
       AND A.[EntityTypeQualifierValue] = CAST( BT.[Id] AS nvarchar(50) )
     WHERE BT.[Guid] = '2B4E0128-BCDF-48BF-AEC9-85001169DA3E'
       AND A.[Key] = N'FooterLavaTemplate' )

IF @AttributeId IS NOT NULL
BEGIN
    UPDATE [Attribute]
       SET [DefaultValue] = REPLACE( REPLACE( [DefaultValue] COLLATE Latin1_General_BIN2
               , N'{{stepItem.Color}}', N'{{stepItem.Color | Escape}}' )
               , N'{{stepItem.StepName}}', N'{{stepItem.StepName | Escape}}' )
         , [IsDefaultPersistedValueDirty] = 1
     WHERE [Id] = @AttributeId
       AND REPLACE( [DefaultValue], NCHAR(13), N'' ) COLLATE Latin1_General_BIN2 = @OldTemplate

    UPDATE [AttributeValue]
       SET [Value] = REPLACE( REPLACE( [Value] COLLATE Latin1_General_BIN2
               , N'{{stepItem.Color}}', N'{{stepItem.Color | Escape}}' )
               , N'{{stepItem.StepName}}', N'{{stepItem.StepName | Escape}}' )
         , [IsPersistedValueDirty] = 1
     WHERE [AttributeId] = @AttributeId
       AND REPLACE( [Value], NCHAR(13), N'' ) COLLATE Latin1_General_BIN2 = @OldTemplate
END" );
        }

        #endregion 344_EscapeStepFlowFooterLavaTemplate

        #region 333_LimitDynamicAuthClients

        /// <summary>
        /// JPH: Limits dynamically registered OAuth clients to the MCP scope so their
        /// tokens can't be used on the rest of the REST API.
        /// </summary>
        private void JPH_LimitDynamicAuthClients_Up()
        {
            Sql( @"
UPDATE [AuthClient]
SET [AllowUserApiAccess] = 0
WHERE [ClientId] LIKE 'dcr.%';

UPDATE [AuthClient]
SET [AllowedScopes] = '[""mcp:invoke""]'
WHERE [ClientId] LIKE 'dcr.%'
    AND ([AllowedScopes] IS NULL OR [AllowedScopes] = '[]');" );
        }

        #endregion 333_LimitDynamicAuthClients

        #region JME: Update to Workflow Builder

        /// <summary>
        /// JME: Fixes the Rock Coding Guide field types link in the Workflow Composer agents' instructions.
        /// </summary>
        private void JME_UpdateToWorkflowBuilder_Up()
        {
            var mcpGuid = "5EDECB28-22DD-4807-80D2-342862FCEED2";
            var chatGuid = "2585435A-01F8-4E43-8AB7-5135101A66DB";

            // SHA-256 of the instructions AddWorkflowBuilder seeded, before and after the link fix.
            var oldHash = "5e41cbb616c2ad27b8a55619af86ccc4042d5052475850f915d91541b0add13c";
            var newHash = "35d022763b86429d432490261c21b0ec653937ab7572b90607c972f20888f82b";

            Sql( $@"
UPDATE [AIAgent]
SET [AdditionalSettingsJson] = JSON_MODIFY([AdditionalSettingsJson], '$.SeededInstructionsHash', N'{newHash}')
WHERE [Guid] IN ('{mcpGuid}', '{chatGuid}')
    AND JSON_VALUE([AdditionalSettingsJson], '$.SeededInstructionsHash') = N'{oldHash}'

UPDATE [AIAgent]
SET [Instructions] = REPLACE([Instructions], N'coding-guide/field-types', N'coding-guide/reference/field-types')
WHERE [Guid] IN ('{mcpGuid}', '{chatGuid}')" );
        }

        #endregion JME: Update to Workflow Builder

        /// <summary>
        /// Cleanups the migration history records except the last one.
        /// </summary>
        private void CleanupMigrationHistory()
        {
            Sql( @"
UPDATE [dbo].[__MigrationHistory]
SET [Model] = 0x
WHERE MigrationId < (SELECT TOP 1 MigrationId FROM __MigrationHistory ORDER BY MigrationId DESC)" );
        }
    }
}
