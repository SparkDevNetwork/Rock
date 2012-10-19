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
    public partial class DotLiquid : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
                UPDATE [crmEmailTemplate] SET [Body] = '{{ EmailHeader }}

Below are your current usernames at {{ OrganizationName }}<br/>

{% for Person in Persons %}
	<br/>
	For {{ Person.FirstName }} {{ Person.LastName }},<br/><br/>
	{% for User in Person.Users %}
		{{ User.UserName }} <a href=''{{ ConfirmAccountUrl }}?cc={{ User.ConfirmationCodeEncoded }}&action=reset''>Reset Password</a><br/>
	{% endfor %}
	<br/>
{% endfor %}

{{ EmailFooter }}' WHERE [Guid] = '113593FF-620E-4870-86B1-7A0EC0409208'
                UPDATE [crmEmailTemplate] SET [Body] = '{{ EmailHeader }}

{{ Person.FirstName }},<br/><br/>

Thank-you for creating an account at {{ OrganizationName }}. Please <a href=''{{ ConfirmAccountUrl }}?cc={{ User.ConfirmationCodeEncoded }}&action=confirm''>confirm</a> that you are the owner of this email address.<br/><br/>If you did not create this account, you can <a href=''{{ ConfirmAccountUrl }}?cc={{ User.ConfirmationCodeEncoded }}&action=delete''>Delete It</a>.<br/><br/>If the above links do not work, you can also go to {{ ConfirmAccountUrl }} and enter the following confirmation code:<br/>{{ User.ConfirmationCode }}<br/><br/>

Thank-you,<br/>
{{ OrganizationName }}  

{{ EmailFooter }}' WHERE [Guid] = '17AACEEF-15CA-4C30-9A3A-11E6CF7E6411'
                UPDATE [crmEmailTemplate] SET [Body] = '{{ EmailHeader }}

{{ Person.FirstName }},<br/><br/>

Thank-you for creating a new account at {{ OrganizationName }}.  Your ''{{ User.UserName }}'' username is now active and can be used to login to our site and access your information.<br/><br/>  If you did not create this account you can <a href=''{{ ConfirmAccountUrl }}?cc={{ User.ConfirmationCodeEncoded }}&action=delete''>Delete it here</a><br/><br/>  Thanks.
Thank-you,<br/>
{{ OrganizationName }}  

{{ EmailFooter }}' WHERE [Guid] = '84E373E9-3AAF-4A31-B3FB-A8E3F0666710'
                UPDATE [crmEmailTemplate] SET [Body] = '{{ EmailHeader }}

An exception has occurred in the Rock ChMS.  Details of this error can be found below: 

<p>{{ ExceptionDetails }}<p>  

{{ EmailFooter }}' WHERE [Guid] = '75CB0A4A-B1C5-4958-ADEB-8621BD231520'

                UPDATE [coreAttribute] SET [DefaultValue] = '<style>   body {   background-color: {{ EmailBodyBackgroundColor }};             font-family: Verdana, Arial, Helvetica, sans-serif;             font-size: 12px;             line-height: 1.3em;             margin: 0;   padding: 0;  }    a {   color: {{ EmailBodyTextLinkColor }};  }   </style>    <table style=''text-align: center; background-color: {{ EmailBackgroundColor }}; margin: 0pt'' border=0 cellSpacing=0 cellPadding=0 width=''100%'' align=center>           <tbody>                <tr>                     <td style=''background-color: {{ EmailBackgroundColor }}; margin: 0pt auto'' valign=top align=middle>    <!-- Begin Layout -->                       <table style=''text-align: left; background-color: {{ EmailBodyBackgroundColor }}; margin: 0px auto; width: 550px'' border=0 cellspacing=0 cellpadding=0 width=550>    <tbody>                               <tr>                                    <td valign=top align=left>       <!-- Header Start -->                                     <table style=''width: 100%'' border=0 cellSpacing=0 cellPadding=0 width=''100%''>                                         <tbody>                                              <tr>                                                   <td style=''height: 51px''>          <img style=''border-bottom: medium none; border-left: medium none; padding-bottom: 0pt; margin: 0px; padding-left: 0pt; padding-right: 0pt; border-top: medium none; border-right: medium none; padding-top: 0pt'' src=''{{ Config_BaseUrl }}{{ EmailHeaderLogo }}''>          </td>                                              </tr>                                         </tbody>                                     </table>                                     <!-- Header End -->                                                     <table style=''padding-bottom: 18px; width: 100%; background-color: {{ EmailBodyBackgroundColor }};'' cellspacing=0 cellpadding=20 >                                         <tbody>                                              <tr>                                                   <td>                                                  <!-- Main Text Start -->' WHERE [Guid] = 'EBC67F76-7305-4108-AD32-E2531EAB1637'
                UPDATE [coreAttribute] SET [DefaultValue] = '<!-- Main Text End -->         </td>                                  </tr>                                                          </tbody>                         </table>              <!-- Footer Start -->                                     <table cellpadding=20 align=center style=''background-color: {{ EmailBackgroundColor }}; width: 100%;''>                                         <tbody>                                              <tr>                                                   <td>                                                    <p style=''text-align: center; color: {{ EmailFooterTextColor }}''><span style=''font-size: 16px''>{{ OrganizationName }} | {{ OrganizationPhone }} <br>            <a href=''mailto:{{ OrganizationEmail }}''>{{ OrganizationEmail }}</A> | <span style=''color: {{ EmailFooterTextLink Color }};''><a style=''color: {{ EmailFooterTextLinkColor }}'' href=''{{ OrganizationWebsite }}''>{{ OrganizationWebsite }}</A></span></span></p>                                                   </td>                                              </tr>                                         </tbody>                                     </table>                                        <!-- Footer End -->     <!-- End Layout -->     </td>                   </tr>              </tbody>          </table>' WHERE [Guid] = 'ED326066-4A91-412A-805C-40DEDAE8F61A'

" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
                UPDATE [crmEmailTemplate] SET [Body] = '{EmailHeader}  Below are your current usernames at {OrganizationName} {Person:RepeatBegin}<br/><br/>For {Person:FirstName} {Person:LastName},<br/><br/>{User:RepeatBegin}{User:Username} <a href=''{ConfirmAccountUrl}?cc={User:ConfirmationCodeEncoded}&action=reset''>Reset Password</a><br/>{User:RepeatEnd}<br/>{Person:RepeatEnd}  {EmailFooter}' WHERE [Guid] = '113593FF-620E-4870-86B1-7A0EC0409208'
                UPDATE [crmEmailTemplate] SET [Body] = '{EmailHeader}  {Person:FirstName},<br/><br/>Thank-you for creating an account at {OrganizationName}. Please <a href=''{ConfirmAccountUrl}?cc={User:ConfirmationCodeEncoded}&action=confirm''>confirm</a> that you are the owner of this email address.<br/><br/>If you did not create this account, you can <a href=''{ConfirmAccountUrl}?cc={User:ConfirmationCodeEncoded}&action=delete''>Delete It</a>.<br/><br/>If the above links do not work, you can also go to {ConfirmAccountUrl} and enter the following confirmation code:<br/>{User:ConfirmationCode}<br/><br/>Thank-you,<br/>{OrganizationName}  {EmailFooter}' WHERE [Guid] = '17AACEEF-15CA-4C30-9A3A-11E6CF7E6411'
                UPDATE [crmEmailTemplate] SET [Body] = '{EmailHeader}  {Person:FirstName},<br/><br/>  Thank-you for creating a new account at {OrganizationName}.  Your ''{User:UserName}'' username is now active and can be used to login to our site and access your information.<br/><br/>  If you did not create this account you can <a href=''{ConfirmAccountUrl}?cc={User:ConfirmationCodeEncoded}&action=delete''>Delete it here</a><br/><br/>  Thanks.  {EmailFooter}' WHERE [Guid] = '84E373E9-3AAF-4A31-B3FB-A8E3F0666710'
                UPDATE [crmEmailTemplate] SET [Body] = '{EmailHeader}  An exception has occurred in the Rock ChMS.  Details of this error can be found below: <p> {ExceptionDetails} <p>  {EmailFooter}' WHERE [Guid] = '75CB0A4A-B1C5-4958-ADEB-8621BD231520'
                UPDATE [coreAttribute] SET [DefaultValue] = '<style>   body {   background-color: {EmailBodyBackgroundColor};             font-family: Verdana, Arial, Helvetica, sans-serif;             font-size: 12px;             line-height: 1.3em;             margin: 0;   padding: 0;  }    a {   color: {EmailBodyTextLinkColor};  }   </style>    <table style=''text-align: center; background-color: {EmailBackgroundColor}; margin: 0pt'' border=0 cellSpacing=0 cellPadding=0 width=''100%'' align=center>           <tbody>                <tr>                     <td style=''background-color: {EmailBackgroundColor}; margin: 0pt auto'' valign=top align=middle>    <!-- Begin Layout -->                       <table style=''text-align: left; background-color: {EmailBodyBackgroundColor}; margin: 0px auto; width: 550px'' border=0 cellspacing=0 cellpadding=0 width=550>    <tbody>                               <tr>                                    <td valign=top align=left>       <!-- Header Start -->                                     <table style=''width: 100%'' border=0 cellSpacing=0 cellPadding=0 width=''100%''>                                         <tbody>                                              <tr>                                                   <td style=''height: 51px''>          <img style=''border-bottom: medium none; border-left: medium none; padding-bottom: 0pt; margin: 0px; padding-left: 0pt; padding-right: 0pt; border-top: medium none; border-right: medium none; padding-top: 0pt'' src=''{Config:BaseUrl}{EmailHeaderLogo}''>          </td>                                              </tr>                                         </tbody>                                     </table>                                     <!-- Header End -->                                                     <table style=''padding-bottom: 18px; width: 100%; background-color: {EmailBodyBackgroundColor}; '' cellspacing=0 cellpadding=20 >                                         <tbody>                                              <tr>                                                   <td>                                                  <!-- Main Text Start -->' WHERE [Guid] = 'EBC67F76-7305-4108-AD32-E2531EAB1637'
                UPDATE [coreAttribute] SET [DefaultValue] = '<!-- Main Text End -->         </td>                                  </tr>                                                          </tbody>                         </table>              <!-- Footer Start -->                                     <table cellpadding=20 align=center style=''background-color: {EmailBackgroundColor}; width: 100%;''>                                         <tbody>                                              <tr>                                                   <td>                                                    <p style=''text-align: center; color: {EmailFooterTextColor}''><span style=''font-size: 16px''>{OrganizationName} | {OrganizationPhone} <br>            <a href=''mailto:{OrganizationEmail}''>{OrganizationEmail}</A> | <span style=''color: {EmailFooterTextLink Color};''><a style=''color: {EmailFooterTextLinkColor}'' href=''{OrganizationWebsite}''>{OrganizationWebsite}</A></span></span></p>                                                   </td>                                              </tr>                                         </tbody>                                     </table>                                        <!-- Footer End -->     <!-- End Layout -->     </td>                   </tr>              </tbody>          </table>' WHERE [Guid] = 'ED326066-4A91-412A-805C-40DEDAE8F61A'
" );
        }
    }
}
