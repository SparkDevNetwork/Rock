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
    public partial class SecurityConfirmedLocked : Rock.Migrations.RockMigration3
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attrib for BlockType: Login:Locked Out Caption
            AddBlockTypeAttribute( "7B83D513-1178-429E-93FF-E76430E038E4", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Locked Out Caption", "LockedOutCaption", "", "", 5, @"Sorry, your account has temporarily been locked.  Please contact our office get it reactivated.", "BD6CB735-C86A-4D0D-BDA8-FBF1AAA261E9" );
            // Attrib for BlockType: Login:Confirmation Page
            AddBlockTypeAttribute( "7B83D513-1178-429E-93FF-E76430E038E4", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Confirmation Page", "ConfirmationPage", "", "Page for user to confirm their account (if blank will use 'ConfirmAccount' page route)", 3, @"", "EC728E86-78D1-42EC-8611-F6C7577A653D" );
            // Attrib for BlockType: Login:Confirm Account Template
            AddBlockTypeAttribute( "7B83D513-1178-429E-93FF-E76430E038E4", "CE7CA048-551C-4F68-8C0A-FCDCBEB5B956", "Confirm Account Template", "ConfirmAccountTemplate", "", "Confirm Account Email Template", 4, @"17aaceef-15ca-4c30-9a3a-11e6cf7e6411", "7D76CF1B-DFC7-47C0-AB55-C5CBDFAC811D" );
            // Attrib for BlockType: Login:Confirm Caption
            AddBlockTypeAttribute( "7B83D513-1178-429E-93FF-E76430E038E4", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Confirm Caption", "ConfirmCaption", "", "", 2, @"Thank-you for logging in, however, we need to confirm the email associated with this account belongs to you. We've sent you an email that contains a link for confirming.  Please click the link in your email to continue.", "9B22FDA2-A821-4CD6-9ED6-C95DD9E04107" );

            // **** Not sure why these attributes were missing, but they appear to never have had a migration to create them.
            // Attrib for BlockType: New Account:Forgot Username
            UpdateBlockTypeAttribute( "99362B60-71A5-44C6-BCFE-DDA9B00CC7F3", "CE7CA048-551C-4F68-8C0A-FCDCBEB5B956", "Forgot Username", "ForgotUsernameTemplate", "", "Forgot Username Email Template", 8, @"113593ff-620e-4870-86b1-7a0ec0409208", "949D3A21-C5F2-4FBD-8264-D9147B46D214" );
            // Attrib for BlockType: New Account:Confirm Account
            UpdateBlockTypeAttribute( "99362B60-71A5-44C6-BCFE-DDA9B00CC7F3", "CE7CA048-551C-4F68-8C0A-FCDCBEB5B956", "Confirm Account", "ConfirmAccountTemplate", "", "Confirm Account Email Template", 9, @"17aaceef-15ca-4c30-9a3a-11e6cf7e6411", "C2F61B3D-C565-4A1D-BF73-4B9CC3BAD2FD" );
            // Attrib for BlockType: New Account:Account Created
            UpdateBlockTypeAttribute( "99362B60-71A5-44C6-BCFE-DDA9B00CC7F3", "CE7CA048-551C-4F68-8C0A-FCDCBEB5B956", "Account Created", "AccountCreatedTemplate", "", "Account Created Email Template", 10, @"84e373e9-3aaf-4a31-b3fb-a8e3f0666710", "C402BF8C-3551-4843-A273-5B0D82831047" );
            // Attrib for BlockType: Forgot Username:Forgot Username Email Template
            UpdateBlockTypeAttribute( "02B3D7D1-23CE-4154-B602-F4A15B321757", "CE7CA048-551C-4F68-8C0A-FCDCBEB5B956", "Forgot Username Email Template", "EmailTemplate", "", "Email Template to send", 4, @"113593ff-620e-4870-86b1-7a0ec0409208", "D924EB46-061F-499E-AB00-B439748A1347" );
            // Attrib for BlockType: Confirm Account:New Account Page
            UpdateBlockTypeAttribute( "734DFF21-7465-4E02-BFC3-D40F7A65FB60", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "New Account Page", "NewAccountPage", "", "Page to navigate to when user selects 'Create New Account' option (if blank will use 'NewAccount' page route)", 0, @"", "A2BD1864-BCF1-4A7C-B21B-220A46D22290" );

            // Attrib Value for Block:Login, Attribute:Confirm Caption Page: Login, Site: Rock RMS
            AddBlockAttributeValue( "3D325BB3-E1C9-4194-8E9B-11BFFC347DC3", "9B22FDA2-A821-4CD6-9ED6-C95DD9E04107", @"Thank-you for logging in, however, we need to confirm the email associated with this account belongs to you. We've sent you an email that contains a link for confirming.  Please click the link in your email to continue." );
            // Attrib Value for Block:Login, Attribute:Confirm Account Template Page: Login, Site: Rock RMS
            AddBlockAttributeValue( "3D325BB3-E1C9-4194-8E9B-11BFFC347DC3", "7D76CF1B-DFC7-47C0-AB55-C5CBDFAC811D", @"17aaceef-15ca-4c30-9a3a-11e6cf7e6411" );
            // Attrib Value for Block:Login, Attribute:Locked Out Caption Page: Login, Site: Rock RMS
            AddBlockAttributeValue( "3D325BB3-E1C9-4194-8E9B-11BFFC347DC3", "BD6CB735-C86A-4D0D-BDA8-FBF1AAA261E9", @"Sorry, your account has temporarily been locked.  Please contact our office get it reactivated." );
            
            // Attrib Value for Block:Login, Attribute:Confirm Caption Page: Login, Site: Rock Solid Church
            AddBlockAttributeValue( "A8E221F0-DE4E-4B0F-B660-BC7AC2298EF8", "9B22FDA2-A821-4CD6-9ED6-C95DD9E04107", @"Thank-you for logging in, however, we need to confirm the email associated with this account belongs to you. We've sent you an email that contains a link for confirming.  Please click the link in your email to continue." );
            // Attrib Value for Block:Login, Attribute:Confirm Account Template Page: Login, Site: Rock Solid Church
            AddBlockAttributeValue( "A8E221F0-DE4E-4B0F-B660-BC7AC2298EF8", "7D76CF1B-DFC7-47C0-AB55-C5CBDFAC811D", @"17aaceef-15ca-4c30-9a3a-11e6cf7e6411" );
            // Attrib Value for Block:Login, Attribute:Locked Out Caption Page: Login, Site: Rock Solid Church
            AddBlockAttributeValue( "A8E221F0-DE4E-4B0F-B660-BC7AC2298EF8", "BD6CB735-C86A-4D0D-BDA8-FBF1AAA261E9", @"Sorry, your account has temporarily been locked.  Please contact our office get it reactivated." );

            Sql( @"
    -- Change the ConfirmAccount and ChangePassword routes to be for the public pages instead of internal
    DECLARE @PublicConfirmPageId int = (SELECT [Id] FROM [Page] WHERE [Guid] = '288DBEC5-8A43-4133-9313-AA2FE81FBA86')
    UPDATE [PageRoute] SET [PageId] = @PublicConfirmPageId WHERE [Guid] = '3C084922-DF00-40E6-971B-72FF4234A54F'
    DECLARE @PublicChangePasswordPageId int = (SELECT [Id] FROM [Page] WHERE [Guid] = 'FAD4F98A-2CBC-4C3E-B597-6C63E2177E7D')
    UPDATE [PageRoute] SET [PageId] = @PublicChangePasswordPageId WHERE [Guid] = '5230092F-126D-4169-A060-3B65211DCB71'

    -- Set email channel to use SMTP transport by default
    DECLARE @AttributeId int = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '8C25B7FE-12E1-4074-BA8A-45BEF08A6C44')
    DELETE [AttributeValue] WHERE [AttributeId] = @AttributeId
    INSERT INTO [AttributeValue] ([IsSystem],[AttributeId],[EntityId],[Order],[Value],[Guid])
    VALUES (0, @AttributeId, 0, 0, '1FEF44B2-8685-4001-BE5B-8A059BC65430', '6E936B5B-8F55-41DF-A2A6-F3BD727CA5FA')

    -- Set the page attributes correctly for the security blocks on the public and internal sites
    DECLARE @RockSiteId int = (SELECT [Id] FROM [Site] WHERE [Guid] = 'C2D29296-6A87-47A9-A753-EE4E9159C4C4')
    DECLARE @PublicSiteId int = (SELECT [Id] FROM [Site] WHERE [Guid] = 'F3F82256-2D66-432B-9D67-3552CD2F4C2B')

    DECLARE @LoginStatusBlockTypeId int = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '04712F3D-9667-4901-A49D-4507573EF7AD')
    DECLARE @LoginBlockTypeId int = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '7B83D513-1178-429E-93FF-E76430E038E4')
    DECLARE @ConfirmBlockTypeId int = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '734DFF21-7465-4E02-BFC3-D40F7A65FB60')
    DECLARE @NewAccountBlockTypeId int = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '99362B60-71A5-44C6-BCFE-DDA9B00CC7F3')
    DECLARE @ForgotBlockTypeId int = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '02B3D7D1-23CE-4154-B602-F4A15B321757')

    DECLARE @BlockEntityTypeId int = (SELECT [Id] FROM [EntityType] WHERE [Guid] = 'D89555CA-9AE4-4D62-8AF1-E5E463C1EF65')

    -- Delete all the security linked page attribute values for the rock and public sites
    DELETE [AV]
    FROM [AttributeValue] AV
    INNER JOIN [Attribute] A ON A.[Id] = AV.[AttributeId]
    INNER JOIN [Block] B ON B.[Id] = AV.[EntityId]
    LEFT OUTER JOIN [Page] P ON P.[Id] = B.[PageId]
    LEFT OUTER JOIN [Layout] PL ON PL.[Id] = P.[LayoutId]
    LEFT OUTER JOIN [Layout] BL ON BL.[Id] = B.[LayoutId]
    LEFT OUTER JOIN [Site] S ON (S.[Id] = PL.[SiteId] OR S.[Id] = BL.[SiteId])
    WHERE A.[EntityTypeId] = @BlockEntityTypeId 
    AND A.[EntityTypeQualifierColumn] = 'BlockTypeId' 
    AND A.[EntityTypeQualifierValue] IN (@LoginStatusBlockTypeId,@LoginBlockTypeId,@ConfirmBlockTypeId,@NewAccountBlockTypeId,@ForgotBlockTypeId) 
    AND A.[FieldTypeId] = 8
    AND S.[Id] IN ( @PublicSiteId, @RockSiteId )

    INSERT INTO [AttributeValue] ([IsSystem],[AttributeId],[EntityId],[Order],[Value],[Guid])
    SELECT  
	    0,
	    A.[Id],
	    B.[Id],
	    0,
	    CASE A.[Key] 
		    WHEN 'ConfirmationPage' THEN 'D73F83B4-E20E-4F95-9A2C-511FB669F44C'
		    WHEN 'HelpPage' THEN 'C6628FBD-F297-4C23-852E-40F1369C23A8'
		    WHEN 'LoginPage' THEN '03CB988A-138C-448B-A43D-8891844EEB18'
		    WHEN 'MyAccountPage' THEN '290C53DC-0960-484C-B314-8301882A454C'
		    WHEN 'NewAccountPage' THEN '7D4E2142-D24E-4DD2-84BC-B34C5C3D0D46'
	    END,
	    NEWID()
    FROM [Block] B 
    INNER JOIN [Attribute] A
    ON A.[EntityTypeId] = @BlockEntityTypeId 
    AND A.[EntityTypeQualifierColumn] = 'BlockTypeId' 
    AND A.[EntityTypeQualifierValue] = CAST(B.[BlockTypeId] AS varchar)
    AND A.[FieldTypeId] = 8
    LEFT OUTER JOIN [Page] P ON P.[Id] = B.[PageId]
    LEFT OUTER JOIN [Layout] PL ON PL.[Id] = P.[LayoutId]
    LEFT OUTER JOIN [Layout] BL ON BL.[Id] = B.[LayoutId]
    LEFT OUTER JOIN [Site] S ON (S.[Id] = PL.[SiteId] OR S.[Id] = BL.[SiteId])
    WHERE B.[BlockTypeId] IN (@LoginStatusBlockTypeId,@LoginBlockTypeId,@ConfirmBlockTypeId,@NewAccountBlockTypeId,@ForgotBlockTypeId) 
    AND S.[Id] = @RockSiteId

" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Login:Confirm Caption
            DeleteAttribute( "9B22FDA2-A821-4CD6-9ED6-C95DD9E04107" );
            // Attrib for BlockType: Login:Confirm Account Template
            DeleteAttribute( "7D76CF1B-DFC7-47C0-AB55-C5CBDFAC811D" );
            // Attrib for BlockType: Login:Confirmation Page
            DeleteAttribute( "EC728E86-78D1-42EC-8611-F6C7577A653D" );
            // Attrib for BlockType: Login:Locked Out Caption
            DeleteAttribute( "BD6CB735-C86A-4D0D-BDA8-FBF1AAA261E9" );
        }
    }
}
