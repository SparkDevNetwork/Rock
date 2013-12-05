//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class UpdateEmailTemplates : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql(@"UPDATE Attribute 
SET DefaultValue = '

<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"">
<html xmlns=""http://www.w3.org/1999/xhtml"" xmlns=""http://www.w3.org/1999/xhtml"">
  <head>
    <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"" />
    <meta name=""viewport"" content=""width=device-width"" />
  </head>
  <body style=""width: 100% !important; -webkit-text-size-adjust: 100%; -ms-text-size-adjust: 100%; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px; margin: 0; padding: 0;""><style type=""text/css"">
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
background: #d0d0d0 !important; color: #555;
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
    width: auto !important; height: auto !important;
  }
  table[class=""body""] center {
    min-width: 0 !important;
  }
  table[class=""body""] .container {
    width: 95% !important;
  }
  table[class=""body""] .row {
    width: 100% !important; display: block !important;
  }
  table[class=""body""] .wrapper {
    display: block !important; padding-right: 0 !important;
  }
  table[class=""body""] .columns {
    table-layout: fixed !important; float: none !important; width: 100% !important; padding-right: 0px !important; padding-left: 0px !important; display: block !important;
  }
  table[class=""body""] .column {
    table-layout: fixed !important; float: none !important; width: 100% !important; padding-right: 0px !important; padding-left: 0px !important; display: block !important;
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
  <table class=""body"" style=""border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; height: 100%; width: 100%; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;""><tr style=""vertical-align: top; text-align: left; padding: 0;"" align=""left""><td class=""center"" align=""center"" valign=""top"" style=""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: center; padding: 0;"">
        <center style=""width: 100%; min-width: 580px;"">
         
          <table class=""row header"" style=""border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 100%; position: relative; background: #5e5e5e; padding: 0px;"" bgcolor=""#5e5e5e""><tr style=""vertical-align: top; text-align: left; padding: 0;"" align=""left""><td class=""center"" align=""center"" style=""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: center; padding: 0;"" valign=""top"">
                <center style=""width: 100%; min-width: 580px;"">
          
                  <table class=""container"" style=""border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: inherit; width: 580px; margin: 0 auto; padding: 0;""><tr style=""vertical-align: top; text-align: left; padding: 0;"" align=""left""><td class=""wrapper last"" style=""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; padding: 10px 0px 0px;"" align=""left"" valign=""top"">
            
                        <table class=""twelve columns"" style=""border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 580px; margin: 0 auto; padding: 0;""><tr style=""vertical-align: top; text-align: left; padding: 0;"" align=""left""><td class=""six sub-columns"" style=""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; min-width: 0px; width: 50% !important; padding: 0px 10px 10px 0px;"" align=""left"" valign=""top"">
                              <img src=""{{ GlobalAttribute.PublicApplicationRoot }}{{ GlobalAttribute.EmailHeaderLogo }}"" style=""outline: none; text-decoration: none; -ms-interpolation-mode: bicubic; width: auto; max-width: 100%; float: left; clear: both; display: block;"" align=""left"" /></td>
                            <td class=""six sub-columns last"" style=""text-align: right; vertical-align: middle; word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; min-width: 0px; width: 50% !important; padding: 0px 0px 10px;"" align=""right"" valign=""middle"">
                              <span class=""template-label"" style=""color: #ffffff; font-weight: bold; font-size: 11px;""></span>
                            </td>
                            <td class=""expander"" style=""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; padding: 0;"" align=""left"" valign=""top""></td>
                          </tr></table></td>
                    </tr></table></center>
              </td>
            </tr></table><table class=""container"" style=""border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: inherit; width: 580px; margin: 0 auto; padding: 0;""><tr style=""vertical-align: top; text-align: left; padding: 0;"" align=""left""><td style=""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; padding: 0;"" align=""left"" valign=""top"">
              
                <table class=""row"" style=""border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 100%; position: relative; display: block; padding: 0px;""><tr style=""vertical-align: top; text-align: left; padding: 0;"" align=""left""><td class=""wrapper last"" style=""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; padding: 10px 0px 0px;"" align=""left"" valign=""top"">
                  
                      <table class=""twelve columns"" style=""border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 580px; margin: 0 auto; padding: 0;""><tr style=""vertical-align: top; text-align: left; padding: 0;"" align=""left""><td style=""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; padding: 0px 0px 10px;"" align=""left"" valign=""top"">
                          <p class=""lead"" style=""color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px; margin: 0; padding: 0 0 10px;"" align=""left"">

'
WHERE [Guid] = 'EBC67F76-7305-4108-AD32-E2531EAB1637'");


            Sql(@"UPDATE Attribute 
SET DefaultValue = '
                    </p>
                    
                    <table class=""row footer"" style=""border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 100%; position: relative; display: block; padding: 0px;""><tr style=""vertical-align: top; text-align: left; padding: 0;"" align=""left""><td class=""wrapper"" style=""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; background: #ebebeb; padding: 10px 20px 0px 0px;"" align=""left"" bgcolor=""#ebebeb"" valign=""top"">
                            
                      <table class=""six columns"" style=""border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 280px; margin: 0 auto; padding: 0;""><tr style=""vertical-align: top; text-align: left; padding: 0;"" align=""left""><td class=""left-text-pad"" style=""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; padding: 0px 0px 10px 10px;"" align=""left"" valign=""top"">
                                                                                                                                                                                                                                            <!-- recommend social network links here -->
                          
                          </td>
                          <td class=""expander"" style=""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; padding: 0;"" align=""left"" valign=""top""></td>
                        </tr></table></td>
                    <td class=""wrapper last"" style=""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; background: #ebebeb; padding: 10px 0px 0px;"" align=""left"" bgcolor=""#ebebeb"" valign=""top"">

                      <table class=""six columns"" style=""border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 280px; margin: 0 auto; padding: 0;""><tr style=""vertical-align: top; text-align: left; padding: 0;"" align=""left""><td class=""last right-text-pad"" style=""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; padding: 0px 0px 10px;"" align=""left"" valign=""top"">
                            <h5 style=""color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; text-align: left; line-height: 1.3; word-break: normal; font-size: 24px; margin: 0; padding: 0 0 10px;"" align=""left"">Contact Info:</h5>
                            <p style=""color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px; margin: 0; padding: 0 0 10px;"" align=""left"">{{ GlobalAttribute.OrganizationAddress }}</p>
                            <p style=""color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px; margin: 12px 0 0 0; padding: 0 0 10px;"" align=""left"">Phone: {{ GlobalAttribute.OrganizationPhone }}</p>
                            <p style=""color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px; margin: 0; padding: 0 0 10px;"" align=""left"">Email: <a href=""mailto:{{ GlobalAttribute.OrganizationEmail }}"" style=""color: #2ba6cb; text-decoration: none;"">{{ GlobalAttribute.OrganizationEmail }}</a></p>
                            <p style=""color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px; margin: 0; padding: 0 0 10px;"" align=""left"">Website: <a style=""color: #2ba6cb; text-decoration: none;"" href=""{{ GlobalAttribute.PublicApplicationRoot }}"">{{ GlobalAttribute.OrganizationWebsite }}</a></p>
            
                          </td>          
                          <td class=""expander"" style=""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; padding: 0;"" align=""left"" valign=""top""></td>
                        </tr></table></td>
                  </tr></table><table class=""row"" style=""border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 100%; position: relative; display: block; padding: 0px;""><tr style=""vertical-align: top; text-align: left; padding: 0;"" align=""left""><td class=""wrapper last"" style=""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; padding: 10px 0px 0px;"" align=""left"" valign=""top"">
            
                      <table class=""twelve columns"" style=""border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 580px; margin: 0 auto; padding: 0;""><tr style=""vertical-align: top; text-align: left; padding: 0;"" align=""left""><td align=""center"" style=""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; padding: 0px 0px 10px;"" valign=""top"">
                            <center style=""width: 100%; min-width: 580px;"">
                              
                                <!-- recommend privacy - terms - unsubscribe here -->

                            </center>
                          </td>
                          <td class=""expander"" style=""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; padding: 0;"" align=""left"" valign=""top""></td>
                        </tr></table></td>
                  </tr></table><!-- container end below --></td>
            </tr></table></center>
      </td>
    </tr></table></body>
</html>
'
WHERE [Guid] = 'ED326066-4A91-412A-805C-40DEDAE8F61A'");


            Sql(@"UPDATE EmailTemplate
SET Body = '
{{ GlobalAttribute.EmailHeader }}

Below are your current usernames at {{ GlobalAttribute.OrganizationName }}<br/>

{% for Person in Persons %}
	<br/>
	For {{ Person.FirstName }} {{ Person.LastName }},<br/><br/>
	{% for User in Person.Users %}
		 <table style=""width: 100%""><tr><td style=""font-weight: normal; font-family: Helvetica, Arial, sans-serif; font-size: 14px;"">
                 <strong>Username:</strong> {{ User.UserName }}</td><td>
                     <table class=""tiny-button button reset-password"" style=""border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 100px; overflow: hidden; padding: 0;""><tr style=""vertical-align: top; text-align: left; padding: 0;"" align=""left""><td style=""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: center; display: block; width: auto !important; color: #ffffff; background: #2795b6; padding: 5px 0 4px; border: 1px solid #2284a1;"" align=""center"" bgcolor=""#00acee"" valign=""top"">
                                  <a href=""{{ ConfirmAccountUrl }}?cc={{ User.ConfirmationCodeEncoded }}&action=reset"" style=""color: #ffffff; text-decoration: none; font-weight: normal; font-family: Helvetica, Arial, sans-serif; font-size: 12px;"">Reset Password</a>
                                </td>
                              </tr>
		 </table></td></tr></table>
	{% endfor %}
	<br/>
{% endfor %}

{{ GlobalAttribute.EmailFooter }}'
WHERE [Guid] = '113593FF-620E-4870-86B1-7A0EC0409208'");

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
