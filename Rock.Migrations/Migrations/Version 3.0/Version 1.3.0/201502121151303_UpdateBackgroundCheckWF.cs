// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class UpdateBackgroundCheckWF : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.AssignActivityToSecurityRole", "08189B3F-B506-45E8-AA68-99EC51085CF3", false, true );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "08189B3F-B506-45E8-AA68-99EC51085CF3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "27BAC9C8-2BF7-405A-AA01-845A3D374295" ); // Rock.Workflow.Action.AssignActivityToSecurityRole:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "08189B3F-B506-45E8-AA68-99EC51085CF3", "7BD25DC9-F34A-478D-BEF9-0C787F5D39B8", "Security Role", "SecurityRole", "The security role to assign this activity to.", 0, @"", "D53823A1-28CB-4BA0-A24C-873ECF4079C5" ); // Rock.Workflow.Action.AssignActivityToSecurityRole:Security Role
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "08189B3F-B506-45E8-AA68-99EC51085CF3", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "120D39B5-8D2A-4B96-9419-C73BE0F2451A" ); // Rock.Workflow.Action.AssignActivityToSecurityRole:Order

            Sql( @"
    DECLARE @AssignToGroupEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = 'DB2D8C44-6E57-4B45-8973-5DE327D61554' )
    DECLARE @AssignToSecurityEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '08189B3F-B506-45E8-AA68-99EC51085CF3' )

    DECLARE @OldGroupIdAttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'BBFAD050-5968-4D11-8887-2FF877D8C8AB' )
    DECLARE @OldOrderAttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '041B7B51-A694-4AF5-B455-64D0DE7160A2' )
    DECLARE @OldActiveAttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'C0D75D1A-16C5-4786-A1E0-25669BEE8FE9' )

    DECLARE @NewGroupIdAttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'D53823A1-28CB-4BA0-A24C-873ECF4079C5' )
    DECLARE @NewOrderAttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '120D39B5-8D2A-4B96-9419-C73BE0F2451A' )
    DECLARE @NewActiveAttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '27BAC9C8-2BF7-405A-AA01-845A3D374295' )

    UPDATE T SET 
        [EntityTypeId] = @AssignToSecurityEntityTypeId
    FROM [AttributeValue] V
    INNER JOIN [WorkflowActionType] T
	    ON T.[Id] = V.[EntityId]
    WHERE V.[AttributeId] = @OldGroupIdAttributeId
    AND V.[Value] = '3981cf6d-7d15-4b57-aace-c0e25d28bd49|a6bcc49e-103f-46b0-8bac-84ea03ff04d5'

    UPDATE OV SET 
	    [AttributeId] = ( 
		    CASE OV.[AttributeId]
			    WHEN @OldGroupIdAttributeId THEN @NewGroupIdAttributeId
			    WHEN @OldOrderAttributeId THEN @NewOrderAttributeId
			    WHEN @OldActiveAttributeId THEN @NewActiveAttributeId
		    END ),
	    [Value] = ( 
		    CASE WHEN OV.[Value] = '3981cf6d-7d15-4b57-aace-c0e25d28bd49|a6bcc49e-103f-46b0-8bac-84ea03ff04d5'
			    THEN 'a6bcc49e-103f-46b0-8bac-84ea03ff04d5'
			    ELSE OV.[Value]
		    END )
    FROM [AttributeValue] V
    INNER JOIN [WorkflowActionType] T
	    ON T.[Id] = V.[EntityId]
    INNER JOIN [AttributeValue] OV
	    ON OV.[EntityId] = V.[EntityId]
	    AND ( 
		    OV.[AttributeId] = @OldGroupIdAttributeId 
		    OR OV.[AttributeId] = @OldOrderAttributeId
		    OR OV.[AttributeId] = @OldActiveAttributeId
	    )
    WHERE V.[AttributeId] = @OldGroupIdAttributeId
    AND V.[Value] = '3981cf6d-7d15-4b57-aace-c0e25d28bd49|a6bcc49e-103f-46b0-8bac-84ea03ff04d5'
    AND T.[EntityTypeId] = @AssignToSecurityEntityTypeId
" );

            #region Migration Rollups

            // JE: Add new attribute to search block
            RockMigrationHelper.AddBlockTypeAttribute( "E3A99534-6FD9-49AD-AC52-32D53B2CEDD7", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Search Regex", "SearchRegex", "", "Regular Expression to run the search input through before sending it to the workflow. Useful for stripping off characters.", 0, @"", "5DCF5D08-2367-4CB9-9684-27631B054F97" );

            // JE: Add page for managing System Email Categories
            RockMigrationHelper.AddPage( "89B7A631-EA6F-4DA3-9380-04EE67B63E9E", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "System Email Categories", "", "66FAF7A6-7523-475C-A88D-51C75178A785", "fa fa-folder" ); // Site:Rock RMS

            // Add Block to Page: System Emails, Site: Rock RMS
            RockMigrationHelper.AddBlock( "89B7A631-EA6F-4DA3-9380-04EE67B63E9E", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "Admin Link", "Main", "", "", 0, "BF0C0BC3-7FD0-4753-9D8C-82AF63BE309F" );

            // Add Block to Page: System Email Categories, Site: Rock RMS
            RockMigrationHelper.AddBlock( "66FAF7A6-7523-475C-A88D-51C75178A785", "", "620FC4A2-6587-409F-8972-22065919D9AC", "Categories", "Main", "", "", 0, "624AE4BC-0A47-46EA-A078-E020BF3EF683" );

            // Add/Update HtmlContent for Block: Admin Link
            RockMigrationHelper.UpdateHtmlContentBlock( "BF0C0BC3-7FD0-4753-9D8C-82AF63BE309F", @"<div class='clearfix margin-b-sm'>
    <a href='~/SystemEmailCategories' class='btn btn-xs btn-default pull-right'><i class='fa fa-folder-o'></i> Manage Categories</a>
</div>", "630118FD-4AC6-4B7C-8045-9EBD24E2E301" );


            // Attrib Value for Block:Categories, Attribute:Entity Type Page: System Email Categories, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "624AE4BC-0A47-46EA-A078-E020BF3EF683", "C405A507-7889-4287-8342-105B89710044", @"b21fd119-893e-46c0-b42d-e4cdd5c8c49d" );

            RockMigrationHelper.AddPageRoute( "66FAF7A6-7523-475C-A88D-51C75178A785", "SystemEmailCategories" );

            // Update EmailHeader and EmailFooter default values to use new lava syntax for global attributes
            Sql( @"
    UPDATE [Attribute]
    SET [DefaultValue] = '
<!DOCTYPE HTML PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"">
<html xmlns=""http://www.w3.org/1999/xhtml"" xmlns=""http://www.w3.org/1999/xhtml"">
<head>
    <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"" />
    <meta name=""viewport"" content=""width=device-width"" />
</head>
<body style=""width: 100% !important; -webkit-text-size-adjust: 100%; -ms-text-size-adjust: 100%; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px; margin: 0; padding: 0;"">
    <style type=""text/css"">
        td, h1, h2, h3, h4, h5, h6, p, a {
            font-family: ''Helvetica'', ''Arial'', sans-serif;
            line-height: 1.4;
            color: #777777;
        }

        h1, h2, h3, h4 {
            color: #777;
        }

            h1.separator,
            h2.separator,
            h3.separator,
            h4.separator {
                padding-bottom: 4px;
                border-bottom: 1px solid #bababa;
            }

        p {
            color: #777;
        }

        em {
            color: #777;
        }

        a {
            color: #2ba6cb;
            text-decoration: none;
        }

        .btn a {
            line-height: 1;
            margin: 0;
            padding: 0;
        }

        a:hover {
            color: #2795b6 !important;
        }

        a:active {
            color: #2795b6 !important;
        }

        a:visited {
            color: #2ba6cb !important;
        }

        h1 a:active {
            color: #2ba6cb !important;
        }

        h2 a:active {
            color: #2ba6cb !important;
        }

        h3 a:active {
            color: #2ba6cb !important;
        }

        h4 a:active {
            color: #2ba6cb !important;
        }

        h5 a:active {
            color: #2ba6cb !important;
        }

        h6 a:active {
            color: #2ba6cb !important;
        }

        h1 a:visited {
            color: #2ba6cb !important;
        }

        h2 a:visited {
            color: #2ba6cb !important;
        }

        h3 a:visited {
            color: #2ba6cb !important;
        }

        h4 a:visited {
            color: #2ba6cb !important;
        }

        h5 a:visited {
            color: #2ba6cb !important;
        }

        h6 a:visited {
            color: #2ba6cb !important;
        }

        table.button:hover td {
            background: #2795b6 !important;
        }

        table.button:visited td {
            background: #2795b6 !important;
        }

        table.button:active td {
            background: #2795b6 !important;
        }

        table.button:hover td a {
            color: #fff !important;
        }

        table.button:visited td a {
            color: #fff !important;
        }

        table.button:active td a {
            color: #fff !important;
        }

        table.button:hover td {
            background: #2795b6 !important;
        }

        table.tiny-button:hover td {
            background: #2795b6 !important;
        }

        table.small-button:hover td {
            background: #2795b6 !important;
        }

        table.medium-button:hover td {
            background: #2795b6 !important;
        }

        table.large-button:hover td {
            background: #2795b6 !important;
        }

        table.button:hover td a {
            color: #ffffff !important;
        }

        table.button:active td a {
            color: #ffffff !important;
        }

        table.button td a:visited {
            color: #ffffff !important;
        }

        table.tiny-button:hover td a {
            color: #ffffff !important;
        }

        table.tiny-button:active td a {
            color: #ffffff !important;
        }

        table.tiny-button td a:visited {
            color: #ffffff !important;
        }

        table.small-button:hover td a {
            color: #ffffff !important;
        }

        table.small-button:active td a {
            color: #ffffff !important;
        }

        table.small-button td a:visited {
            color: #ffffff !important;
        }

        table.medium-button:hover td a {
            color: #ffffff !important;
        }

        table.medium-button:active td a {
            color: #ffffff !important;
        }

        table.medium-button td a:visited {
            color: #ffffff !important;
        }

        table.large-button:hover td a {
            color: #ffffff !important;
        }

        table.large-button:active td a {
            color: #ffffff !important;
        }

        table.large-button td a:visited {
            color: #ffffff !important;
        }

        table.secondary:hover td {
            background: #d0d0d0 !important;
            color: #555;
        }

            table.secondary:hover td a {
                color: #555 !important;
            }

        table.secondary td a:visited {
            color: #555 !important;
        }

        table.secondary:active td a {
            color: #555 !important;
        }

        table.success:hover td {
            background: #457a1a !important;
        }

        table.alert:hover td {
            background: #970b0e !important;
        }

        table.facebook:hover td {
            background: #2d4473 !important;
        }

        table.twitter:hover td {
            background: #0087bb !important;
        }

        table.google-plus:hover td {
            background: #CC0000 !important;
        }

        @media only screen and (max-width: 600px) {
            table[class=""body""] img {
                width: auto !important;
                height: auto !important;
            }

            table[class=""body""] center {
                min-width: 0 !important;
            }

            table[class=""body""] .container {
                width: 95% !important;
            }

            table[class=""body""] .row {
                width: 100% !important;
                display: block !important;
            }

            table[class=""body""] .wrapper {
                display: block !important;
                padding-right: 0 !important;
            }

            table[class=""body""] .columns {
                table-layout: fixed !important;
                float: none !important;
                width: 100% !important;
                padding-right: 0px !important;
                padding-left: 0px !important;
                display: block !important;
            }

            table[class=""body""] .column {
                table-layout: fixed !important;
                float: none !important;
                width: 100% !important;
                padding-right: 0px !important;
                padding-left: 0px !important;
                display: block !important;
            }

            table[class=""body""] .wrapper.first .columns {
                display: table !important;
            }

            table[class=""body""] .wrapper.first .column {
                display: table !important;
            }

            table[class=""body""] table.columns td {
                width: 100% !important;
            }

            table[class=""body""] table.column td {
                width: 100% !important;
            }

            table[class=""body""] td.offset-by-one {
                padding-left: 0 !important;
            }

            table[class=""body""] td.offset-by-two {
                padding-left: 0 !important;
            }

            table[class=""body""] td.offset-by-three {
                padding-left: 0 !important;
            }

            table[class=""body""] td.offset-by-four {
                padding-left: 0 !important;
            }

            table[class=""body""] td.offset-by-five {
                padding-left: 0 !important;
            }

            table[class=""body""] td.offset-by-six {
                padding-left: 0 !important;
            }

            table[class=""body""] td.offset-by-seven {
                padding-left: 0 !important;
            }

            table[class=""body""] td.offset-by-eight {
                padding-left: 0 !important;
            }

            table[class=""body""] td.offset-by-nine {
                padding-left: 0 !important;
            }

            table[class=""body""] td.offset-by-ten {
                padding-left: 0 !important;
            }

            table[class=""body""] td.offset-by-eleven {
                padding-left: 0 !important;
            }

            table[class=""body""] .expander {
                width: 9999px !important;
            }

            table[class=""body""] .right-text-pad {
                padding-left: 10px !important;
            }

            table[class=""body""] .text-pad-right {
                padding-left: 10px !important;
            }

            table[class=""body""] .left-text-pad {
                padding-right: 10px !important;
            }

            table[class=""body""] .text-pad-left {
                padding-right: 10px !important;
            }

            table[class=""body""] .hide-for-small {
                display: none !important;
            }

            table[class=""body""] .show-for-desktop {
                display: none !important;
            }

            table[class=""body""] .show-for-small {
                display: inherit !important;
            }

            table[class=""body""] .hide-for-desktop {
                display: inherit !important;
            }

            table[class=""body""] .right-text-pad {
                padding-left: 10px !important;
            }

            table[class=""body""] .left-text-pad {
                padding-right: 10px !important;
            }
        }
    </style>
    <table class=""body"" style=""border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; height: 100%; width: 100%; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;"">
        <tr style=""vertical-align: top; text-align: left; padding: 0;"" align=""left"">
            <td class=""center"" align=""center"" valign=""top"" style=""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: center; padding: 0;"">
                <center style=""width: 100%; min-width: 580px;"">

                    <table class=""row header"" style=""border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 100%; position: relative; background: #5e5e5e; padding: 0px;"" bgcolor=""#5e5e5e"">
                        <tr style=""vertical-align: top; text-align: left; padding: 0;"" align=""left"">
                            <td class=""center"" align=""center"" style=""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: center; padding: 0;"" valign=""top"">
                                <center style=""width: 100%; min-width: 580px;"">

                                    <table class=""container"" style=""border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: inherit; width: 580px; margin: 0 auto; padding: 0;"">
                                        <tr style=""vertical-align: top; text-align: left; padding: 0;"" align=""left"">
                                            <td class=""wrapper last"" style=""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; padding: 10px 0px 0px;"" align=""left"" valign=""top"">

                                                <table class=""twelve columns"" style=""border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 580px; margin: 0 auto; padding: 0;"">
                                                    <tr style=""vertical-align: top; text-align: left; padding: 0;"" align=""left"">
                                                        <td class=""six sub-columns"" style=""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; min-width: 0px; width: 50% !important; padding: 0px 10px 10px 0px;"" align=""left"" valign=""top"">
                                                            <img src=""{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}{{ ''Global'' | Attribute:''EmailHeaderLogo'' }}"" style=""outline: none; text-decoration: none; -ms-interpolation-mode: bicubic; width: auto; max-width: 100%; float: left; clear: both; display: block;"" align=""left"" />
                                                        </td>
                                                        <td class=""six sub-columns last"" style=""text-align: right; vertical-align: middle; word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; min-width: 0px; width: 50% !important; padding: 0px 0px 10px;"" align=""right"" valign=""middle"">
                                                            <span class=""template-label"" style=""color: #ffffff; font-weight: bold; font-size: 11px;""></span>
                                                        </td>
                                                        <td class=""expander"" style=""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; padding: 0;"" align=""left"" valign=""top""></td>
                                                    </tr>
                                                </table>
                                            </td>
                                        </tr>
                                    </table>
                                </center>
                            </td>
                        </tr>
                    </table><table class=""container"" style=""border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: inherit; width: 580px; margin: 0 auto; padding: 0;"">
                        <tr style=""vertical-align: top; text-align: left; padding: 0;"" align=""left"">
                            <td style=""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; padding: 0;"" align=""left"" valign=""top"">

                                <table class=""row"" style=""border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 100%; position: relative; display: block; padding: 0px;"">
                                    <tr style=""vertical-align: top; text-align: left; padding: 0;"" align=""left"">
                                        <td class=""wrapper last"" style=""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; padding: 10px 0px 0px;"" align=""left"" valign=""top"">

                                            <table class=""twelve columns"" style=""border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 580px; margin: 0 auto; padding: 0;"">
                                                <tr style=""vertical-align: top; text-align: left; padding: 0;"" align=""left"">
                                                    <td style=""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; padding: 0px 0px 10px;"" align=""left"" valign=""top"">
                                                        <p class=""lead"" style=""color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px; margin: 0; padding: 0 0 10px;"" align=""left"">
' 
    WHERE [Guid] = 'EBC67F76-7305-4108-AD32-E2531EAB1637'

    UPDATE [Attribute]
    SET [DefaultValue] = '
                                                        </p>
                                                        <table class=""row footer"" style=""border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 100%; position: relative; display: block; padding: 0px;"">
                                                            <tr style=""vertical-align: top; text-align: left; padding: 0;"" align=""left"">
                                                                <td class=""wrapper"" style=""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; background: #ebebeb; padding: 10px 20px 0px 0px;"" align=""left"" bgcolor=""#ebebeb"" valign=""top"">

                                                                    <table class=""six columns"" style=""border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 280px; margin: 0 auto; padding: 0;"">
                                                                        <tr style=""vertical-align: top; text-align: left; padding: 0;"" align=""left"">
                                                                            <td class=""left-text-pad"" style=""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; padding: 0px 0px 10px 10px;"" align=""left"" valign=""top"">
                                                                                <!-- recommend social network links here -->

                                                                            </td>
                                                                            <td class=""expander"" style=""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; padding: 0;"" align=""left"" valign=""top""></td>
                                                                        </tr>
                                                                    </table>
                                                                </td>
                                                                <td class=""wrapper last"" style=""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; background: #ebebeb; padding: 10px 0px 0px;"" align=""left"" bgcolor=""#ebebeb"" valign=""top"">
                                                                    <table class=""six columns"" style=""border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 280px; margin: 0 auto; padding: 0;"">
                                                                        <tr style=""vertical-align: top; text-align: left; padding: 0;"" align=""left"">
                                                                            <td class=""last right-text-pad"" style=""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; padding: 0px 0px 10px;"" align=""left"" valign=""top"">
                                                                                <h5 style=""color: #717171; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; text-align: left; line-height: 1.3; word-break: normal; font-size: 24px; margin: 0; padding: 0 0 10px;"" align=""left"">Contact Info:</h5>
                                                                                <p style=""color: #717171; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px; margin: 0; padding: 0 0 10px;"" align=""left"">{{ ''Global'' | Attribute:''OrganizationAddress'' }}</p>
                                                                                <p style=""color: #717171; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px; margin: 12px 0 0 0; padding: 0 0 10px;"" align=""left"">Phone: {{ ''Global'' | Attribute:''OrganizationPhone'' }}</p>
                                                                                <p style=""color: #717171; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px; margin: 0; padding: 0 0 10px;"" align=""left"">Email: <a href=""mailto:{{ ''Global'' | Attribute:''OrganizationEmail'' }}"" style=""color: #2ba6cb; text-decoration: none;"">{{ ''Global'' | Attribute:''OrganizationEmail'' }}</a></p>
                                                                                <p style=""color: #717171; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px; margin: 0; padding: 0 0 10px;"" align=""left"">Website: <a style=""color: #2ba6cb; text-decoration: none;"" href=""{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}"">{{ ''Global'' | Attribute:''OrganizationWebsite'' }}</a></p>

                                                                            </td>
                                                                            <td class=""expander"" style=""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; padding: 0;"" align=""left"" valign=""top""></td>
                                                                        </tr>
                                                                    </table>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                        <table class=""row"" style=""border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 100%; position: relative; display: block; padding: 0px;"">
                                                            <tr style=""vertical-align: top; text-align: left; padding: 0;"" align=""left"">
                                                                <td class=""wrapper last"" style=""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; padding: 10px 0px 0px;"" align=""left"" valign=""top"">

                                                                    <table class=""twelve columns"" style=""border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 580px; margin: 0 auto; padding: 0;"">
                                                                        <tr style=""vertical-align: top; text-align: left; padding: 0;"" align=""left"">
                                                                            <td align=""center"" style=""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; padding: 0px 0px 10px;"" valign=""top"">
                                                                                <center style=""width: 100%; min-width: 580px;"">

                                                                                    <!-- recommend privacy - terms - unsubscribe here -->
                                                                                    [[ UnsubscribeOption ]]
                                                                                </center>
                                                                            </td>
                                                                            <td class=""expander"" style=""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; padding: 0;"" align=""left"" valign=""top""></td>
                                                                        </tr>
                                                                    </table>
                                                                </td>
                                                            </tr>
                                                        </table><!-- container end below -->
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </center>
            </td>
        </tr>
    </table>
</body>
</html>
'
    WHERE [Guid] = 'ED326066-4A91-412A-805C-40DEDAE8F61A'
" );
            #endregion

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
