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
    public partial class AddCommunicationTopicDefinedValues : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            this.RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.COMMUNICATION_TOPIC, "General Announcements", "Church-wide updates, news, and important information.", "1BD309A1-6AE8-4896-A45D-ED9E116074CC", isSystem: false );
            this.RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.COMMUNICATION_TOPIC, "Events & Registrations", "Promotions and reminders for upcoming events, conferences, and sign-ups.", "C22D71AA-C78A-46B4-94C6-DC48CA6ECAB5", isSystem: false );
            this.RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.COMMUNICATION_TOPIC, "Serving Opportunities", "Volunteer recruitment and service-related communications.", "DF6CF12E-7183-464B-B6B2-B67D76300F8A", isSystem: false );
            this.RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.COMMUNICATION_TOPIC, "Small Groups & Discipleship", "Messages related to small groups, Bible studies, and spiritual growth.", "C6504FFB-E7E8-4EC2-AED9-FE0BC1A86B37", isSystem: false );
            this.RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.COMMUNICATION_TOPIC, "Giving & Stewardship", "Tithing reminders, financial updates, and generosity campaigns.", "3B8A938D-3E42-4504-87AB-729A2A55FEFB", isSystem: false );
            this.RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.COMMUNICATION_TOPIC, "Prayer & Care", "Requests for prayer, pastoral care updates, and encouragement.", "46B017B1-2232-481D-81C5-A48D9B7CABEE", isSystem: false );
            this.RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.COMMUNICATION_TOPIC, "Kids Ministry", "Communications for children’s ministry, family engagement, and parenting resources.", "96E49813-322C-4465-9800-BAECBB8958F4", isSystem: false );
            this.RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.COMMUNICATION_TOPIC, "Student Ministry", "Updates for youth groups, student events, and discipleship opportunities.", "E9054E68-9BF0-4AB9-8216-1B8D29D6C51D", isSystem: false );
            
            Sql( @"UPDATE [CommunicationTemplate]
SET [Message] = N'
<!DOCTYPE html>
<html lang=""en""><head>
        <title>A Responsive Email Template</title>
    
        <meta charset=""utf-8"">
        <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
        <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
        <style>
            /* ========================================================
               CLIENT-SPECIFIC STYLES
               - Handles quirks across various email clients
            ======================================================== */
    
            /* Prevents mobile devices from adjusting text size */
            body, table, td, a {
                -webkit-text-size-adjust: 100%;
                -ms-text-size-adjust: 100%;
            }
    
            /* Removes extra spacing between tables in Outlook */
            table, td {
                mso-table-lspace: 0pt;
                mso-table-rspace: 0pt;
            }
    
            /* Improves image scaling in older versions of Outlook (Word engine) */
            img {
                -ms-interpolation-mode: bicubic;
            }
    
            /* ========================================================
               GENERAL RESET STYLES
               - Normalize elements across clients
            ======================================================== */
    
            img {
                border: 0;
                height: auto;
                line-height: 100%;
                outline: none;
                text-decoration: none;
            }
    
            table {
                border-collapse: collapse !important;
            }
    
            table.border-background-wrapper {
                border-collapse: separate !important;
                border-spacing: 0;
            }
    
            body {
                height: 100% !important;
                margin: 0 !important;
                padding: 0 !important;
                width: 100% !important;
            }
    
            /* ========================================================
               iOS BLUE LINKS
               - Resets auto-linked phone numbers, emails, etc.
            ======================================================== */
    
            a[x-apple-data-detectors] {
                color: inherit !important;
                text-decoration: none !important;
                font-size: inherit !important;
                font-family: inherit !important;
                font-weight: inherit !important;
                line-height: inherit !important;
            }
    
            /* ========================================================
               RESPONSIVE MOBILE STYLES
               - Adjusts layout for screens <= 525px wide
            ======================================================== */
    
            @media screen and (max-width: 525px) {
                /* Makes tables fluid */
                .wrapper {
                    width: 100% !important;
                    max-width: 100% !important;
                }
    
                /* Centers logo images */
                .logo img {
                    margin: 0 auto !important;
                }
    
                /* Hides elements on mobile */
                .mobile-hide {
                    display: none !important;
                }
    
                /* Ensures images scale fluidly */
                .img-max {
                    max-width: 100% !important;
                    width: 100% !important;
                    height: auto !important;
                }
    
                /* Makes tables take full width */
                .responsive-table {
                    width: 100% !important;
                }
    
                /* Padding utility classes for mobile adjustments */
                .padding {
                    padding: 10px 5% 15px 5% !important;
                }
    
                .padding-meta {
                    padding: 30px 5% 0px 5% !important;
                    text-align: center;
                }
    
                .padding-copy {
                    padding: 10px 5% 10px 5% !important;
                    text-align: center;
                }
    
                .no-padding {
                    padding: 0 !important;
                }
    
                .section-padding {
                    padding: 50px 15px 50px 15px !important;
                }
    
                /* Styles buttons for better touch targets */
                .mobile-button-container {
                    margin: 0 auto;
                    width: 100% !important;
                }
    
                .mobile-button {
                    padding: 15px !important;
                    border: 0 !important;
                    font-size: 16px !important;
                    display: block !important;
                }
            }
    
            /* ========================================================
               ANDROID GMAIL FIX
               - Removes margin added by some Android email clients
            ======================================================== */
            div[style*=""margin: 16px 0;""] {
                margin: 0 !important;
            }
        </style>
        <!-- ========================================================
             OUTLOOK (MSO) CONDITIONAL STYLES
             - Targets Outlook 2007+ (mso 12 and later)
             - Used for padding adjustments or MSO-specific tweaks
        ========================================================= -->
        <!--[if gte mso 12]>
        <style>
            .mso-right {
                padding-left: 20px;
            }
        </style>
        <![endif]--></head>

    <body class="""">
        <div id=""preheader-text"" style=""display: none; font-size: 1px; color: #ffffff; line-height: 1px; font-family: Helvetica, Arial, sans-serif; max-height: 0px; max-width: 0px; opacity: 0; overflow: hidden; mso-hide: all; height: 0px; visibility: hidden;""> </div>
        <table class=""email-wrapper"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" role=""presentation"" bgcolor=""#e7e7e7"" style=""min-width: 100%; height: 100%; background-size: 100% auto;"">
            <tbody>
                <tr>
                    <td align=""center"" valign=""top"" style=""height: 100%;"">
                        <div class=""structure-dropzone""><table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" role=""presentation"" class=""margin-wrapper margin-wrapper-for-row component component-row"" data-state=""component"" data-version=""v2-alpha""><tbody><tr><td align=""center""><table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""600"" role=""presentation"" class=""border-wrapper border-wrapper-for-row"" style=""border-collapse: separate !important;""><tbody><tr><td style=""overflow: hidden;""><table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" role=""presentation"" class=""padding-wrapper padding-wrapper-for-row"" bgcolor=""#ffffff""><tbody><tr><td><div class=""dropzone""></div></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table></div>
                    </td>
                </tr>
            </tbody>
        </table>
    </body>
</html>'
WHERE [Guid] = '6280214C-404E-4F4E-BC33-7A5D4CDF8DBC'" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            this.RockMigrationHelper.DeleteDefinedValue( "1BD309A1-6AE8-4896-A45D-ED9E116074CC" );
            this.RockMigrationHelper.DeleteDefinedValue( "C22D71AA-C78A-46B4-94C6-DC48CA6ECAB5" );
            this.RockMigrationHelper.DeleteDefinedValue( "DF6CF12E-7183-464B-B6B2-B67D76300F8A" );
            this.RockMigrationHelper.DeleteDefinedValue( "C6504FFB-E7E8-4EC2-AED9-FE0BC1A86B37" );
            this.RockMigrationHelper.DeleteDefinedValue( "3B8A938D-3E42-4504-87AB-729A2A55FEFB" );
            this.RockMigrationHelper.DeleteDefinedValue( "46B017B1-2232-481D-81C5-A48D9B7CABEE" );
            this.RockMigrationHelper.DeleteDefinedValue( "96E49813-322C-4465-9800-BAECBB8958F4" );
            this.RockMigrationHelper.DeleteDefinedValue( "E9054E68-9BF0-4AB9-8216-1B8D29D6C51D" );
        }
    }
}
