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
    public partial class BulkCommunication : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Communication", "IsBulkCommunication", c => c.Boolean(nullable: false));

            UpdateEntityType( "Rock.Communication.Channel.Email", "5A653EBE-6803-44B4-85D2-FB7B8146D55D", false, true );

            Sql( @"
    UPDATE [Communication] SET [IsBulkCommunication] = 1
    WHERE [ChannelDataJson] LIKE '%""BulkEmail"": ""True""%'

    DECLARE @EmailChannelId int = (SELECT [Id] FROM [EntityType] WHERE [Guid] = '5A653EBE-6803-44B4-85D2-FB7B8146D55D')
    DELETE [CommunicationTemplate] WHERE [Guid] = 'AFE2ADD1-5278-441E-8E84-1DC743D99824'
    INSERT INTO [CommunicationTemplate] ( [Name], [Description], [Subject], [ChannelEntityTypeId], [ChannelDataJson], [Guid] )
    VALUES ( 'Default Organization Template', 'Default template with organization header and footer.', '', @EmailChannelId, '{ ""HtmlMessage"": ""{{ GlobalAttribute.EmailHeader }}\n<p>Dear&nbsp;{{ Person.NickName }}</p>\n\n<p>--&gt; Insert Your Communication Text Here &lt;--</p>\n\n<p>{{ Communication.ChannelData.FromName }}<br />\nEmail: <a href=\""mailto:{{ Communication.ChannelData.FromAddress }}\"" style=\""color: #2ba6cb; text-decoration: none;\"">{{ Communication.ChannelData.FromAddress }}</a></p>\n{{ GlobalAttribute.EmailFooter }}"" }', 'AFE2ADD1-5278-441E-8E84-1DC743D99824' )

    DECLARE @FooterAttributeId int = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'ED326066-4A91-412A-805C-40DEDAE8F61A')
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
                                                                                <h5 style=""color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; text-align: left; line-height: 1.3; word-break: normal; font-size: 24px; margin: 0; padding: 0 0 10px;"" align=""left"">Contact Info:</h5>
                                                                                <p style=""color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px; margin: 0; padding: 0 0 10px;"" align=""left"">{{ GlobalAttribute.OrganizationAddress }}</p>
                                                                                <p style=""color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px; margin: 12px 0 0 0; padding: 0 0 10px;"" align=""left"">Phone: {{ GlobalAttribute.OrganizationPhone }}</p>
                                                                                <p style=""color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px; margin: 0; padding: 0 0 10px;"" align=""left"">Email: <a href=""mailto:{{ GlobalAttribute.OrganizationEmail }}"" style=""color: #2ba6cb; text-decoration: none;"">{{ GlobalAttribute.OrganizationEmail }}</a></p>
                                                                                <p style=""color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px; margin: 0; padding: 0 0 10px;"" align=""left"">Website: <a style=""color: #2ba6cb; text-decoration: none;"" href=""{{ GlobalAttribute.PublicApplicationRoot }}"">{{ GlobalAttribute.OrganizationWebsite }}</a></p>

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
    WHERE [Id] = @FooterAttributeId

    UPDATE [AttributeValue]
    SET [Value] = '
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
                                                                                <h5 style=""color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; text-align: left; line-height: 1.3; word-break: normal; font-size: 24px; margin: 0; padding: 0 0 10px;"" align=""left"">Contact Info:</h5>
                                                                                <p style=""color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px; margin: 0; padding: 0 0 10px;"" align=""left"">{{ GlobalAttribute.OrganizationAddress }}</p>
                                                                                <p style=""color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px; margin: 12px 0 0 0; padding: 0 0 10px;"" align=""left"">Phone: {{ GlobalAttribute.OrganizationPhone }}</p>
                                                                                <p style=""color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px; margin: 0; padding: 0 0 10px;"" align=""left"">Email: <a href=""mailto:{{ GlobalAttribute.OrganizationEmail }}"" style=""color: #2ba6cb; text-decoration: none;"">{{ GlobalAttribute.OrganizationEmail }}</a></p>
                                                                                <p style=""color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px; margin: 0; padding: 0 0 10px;"" align=""left"">Website: <a style=""color: #2ba6cb; text-decoration: none;"" href=""{{ GlobalAttribute.PublicApplicationRoot }}"">{{ GlobalAttribute.OrganizationWebsite }}</a></p>

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
    WHERE [AttributeId] = @FooterAttributeId
    AND ([ModifiedByPersonAliasId] IS NULL OR [ModifiedByPersonAliasId] = 1)

" );
            UpdateFieldType("Communication Template", "Communication Template", "Rock", "Rock.Field.Types.CommunicationTemplateFieldType", "C3B37465-DCAF-4C8C-930C-9A9B5D066CA9");
            UpdateBlockTypeAttribute( "D9834641-7F39-4CFA-8CB2-E64068127565", "C3B37465-DCAF-4C8C-930C-9A9B5D066CA9", "Default Template", "DefaultTemplate", "", "The default template to use for a new communication.  (Note: This will only be used if the template is for the same channel as the communication.)", 1, "", "73B61EB5-F6B7-495B-A781-8EE2D5717D14" );
            AddBlockAttributeValue( "BD9B2F32-AB18-4761-80C9-FDA4DBEEA9EC", "73B61EB5-F6B7-495B-A781-8EE2D5717D14", "AFE2ADD1-5278-441E-8E84-1DC743D99824" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteAttribute( "73B61EB5-F6B7-495B-A781-8EE2D5717D14" );

            DropColumn("dbo.Communication", "IsBulkCommunication");
        }
    }
}
