using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Tests.Shared;
using System.Collections.Generic;
using Rock.Communication;

namespace Rock.Tests.Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    [TestClass]
    public class CommunicationTemplateTests
    {
        /// <summary>
        /// Should correctly update the 'Default' communication template when custom values for lava fields are set
        /// </summary>
        [TestMethod]
        public void TemplateEditor_DefaultCommunicationTemplate()
        {
            string defaultCommunicationTemplate = @"<!DOCTYPE html>
<html>


<head>
  <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">
  <meta name=""viewport"" content=""width=device-width"">
  <title>My Basic Email Template Subject</title>
  <!-- <style> -->
</head>


<body style=""-moz-box-sizing: border-box; -ms-text-size-adjust: 100%; -webkit-box-sizing: border-box; -webkit-text-size-adjust: 100%; Margin: 0; background: {{ bodyBackgroundColor }} !important; box-sizing: border-box; color: {{ textColor }}; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.3; margin: 0; min-width: 100%; padding: 0; text-align: left; width: 100% !important;"">
  <div id=""lava-fields"" style=""display:none"">
    <!-- Lava Fields: Code-Generated from Template Editor -->
    {% assign headerBackgroundColor = '#5e5e5e' %}
    {% assign linkColor = '#2199e8' %}
    {% assign bodyBackgroundColor = '#f3f3f3' %}
    {% assign textColor = '#0a0a0a' %}
  </div>

  <style>
      a {
          color: {{ linkColor }};
      }
      
      .component-text td {
          color: {{ textColor }};
          font-family: Helvetica, Arial, sans-serif;
          font-size: 16px;
          font-weight: normal;
          line-height: 1.3;
      }
  </style>

  <style class=""ignore"">
    @media only screen {
      html {
        min-height: 100%;
        background: {{ bodyBackgroundColor }};
      }
    }
    
    @media only screen and (max-width: 596px) {
      .small-float-center {
        margin: 0 auto !important;
        float: none !important;
        text-align: center !important;
      }
      .small-text-center {
        text-align: center !important;
      }
      .small-text-left {
        text-align: left !important;
      }
      .small-text-right {
        text-align: right !important;
      }
    }
    
    @media only screen and (max-width: 596px) {
      .hide-for-large {
        display: block !important;
        width: auto !important;
        overflow: visible !important;
        max-height: none !important;
        font-size: inherit !important;
        line-height: inherit !important;
      }
    }
    
    @media only screen and (max-width: 596px) {
      table.body table.container .hide-for-large,
      table.body table.container .row.hide-for-large {
        display: table !important;
        width: 100% !important;
      }
    }
    
    @media only screen and (max-width: 596px) {
      table.body table.container .callout-inner.hide-for-large {
        display: table-cell !important;
        width: 100% !important;
      }
    }
    
    @media only screen and (max-width: 596px) {
      table.body table.container .show-for-large {
        display: none !important;
        width: 0;
        mso-hide: all;
        overflow: hidden;
      }
    }
    
    @media only screen and (max-width: 596px) {
      table.body img {
        width: auto;
        height: auto;
      }
      table.body center {
        min-width: 0 !important;
      }
      table.body .container {
        width: 95% !important;
      }
      table.body .columns,
      table.body .column {
        height: auto !important;
        -moz-box-sizing: border-box;
        -webkit-box-sizing: border-box;
        box-sizing: border-box;
        padding-left: 16px !important;
        padding-right: 16px !important;
      }
      table.body .columns .column,
      table.body .columns .columns,
      table.body .column .column,
      table.body .column .columns {
        padding-left: 0 !important;
        padding-right: 0 !important;
      }
      table.body .collapse .columns,
      table.body .collapse .column {
        padding-left: 0 !important;
        padding-right: 0 !important;
      }
      td.small-1,
      th.small-1 {
        display: inline-block !important;
        width: 8.33333% !important;
      }
      td.small-2,
      th.small-2 {
        display: inline-block !important;
        width: 16.66667% !important;
      }
      td.small-3,
      th.small-3 {
        display: inline-block !important;
        width: 25% !important;
      }
      td.small-4,
      th.small-4 {
        display: inline-block !important;
        width: 33.33333% !important;
      }
      td.small-5,
      th.small-5 {
        display: inline-block !important;
        width: 41.66667% !important;
      }
      td.small-6,
      th.small-6 {
        display: inline-block !important;
        width: 50% !important;
      }
      td.small-7,
      th.small-7 {
        display: inline-block !important;
        width: 58.33333% !important;
      }
      td.small-8,
      th.small-8 {
        display: inline-block !important;
        width: 66.66667% !important;
      }
      td.small-9,
      th.small-9 {
        display: inline-block !important;
        width: 75% !important;
      }
      td.small-10,
      th.small-10 {
        display: inline-block !important;
        width: 83.33333% !important;
      }
      td.small-11,
      th.small-11 {
        display: inline-block !important;
        width: 91.66667% !important;
      }
      td.small-12,
      th.small-12 {
        display: inline-block !important;
        width: 100% !important;
      }
      .columns td.small-12,
      .column td.small-12,
      .columns th.small-12,
      .column th.small-12 {
        display: block !important;
        width: 100% !important;
      }
      table.body td.small-offset-1,
      table.body th.small-offset-1 {
        margin-left: 8.33333% !important;
        Margin-left: 8.33333% !important;
      }
      table.body td.small-offset-2,
      table.body th.small-offset-2 {
        margin-left: 16.66667% !important;
        Margin-left: 16.66667% !important;
      }
      table.body td.small-offset-3,
      table.body th.small-offset-3 {
        margin-left: 25% !important;
        Margin-left: 25% !important;
      }
      table.body td.small-offset-4,
      table.body th.small-offset-4 {
        margin-left: 33.33333% !important;
        Margin-left: 33.33333% !important;
      }
      table.body td.small-offset-5,
      table.body th.small-offset-5 {
        margin-left: 41.66667% !important;
        Margin-left: 41.66667% !important;
      }
      table.body td.small-offset-6,
      table.body th.small-offset-6 {
        margin-left: 50% !important;
        Margin-left: 50% !important;
      }
      table.body td.small-offset-7,
      table.body th.small-offset-7 {
        margin-left: 58.33333% !important;
        Margin-left: 58.33333% !important;
      }
      table.body td.small-offset-8,
      table.body th.small-offset-8 {
        margin-left: 66.66667% !important;
        Margin-left: 66.66667% !important;
      }
      table.body td.small-offset-9,
      table.body th.small-offset-9 {
        margin-left: 75% !important;
        Margin-left: 75% !important;
      }
      table.body td.small-offset-10,
      table.body th.small-offset-10 {
        margin-left: 83.33333% !important;
        Margin-left: 83.33333% !important;
      }
      table.body td.small-offset-11,
      table.body th.small-offset-11 {
        margin-left: 91.66667% !important;
        Margin-left: 91.66667% !important;
      }
      table.body table.columns td.expander,
      table.body table.columns th.expander {
        display: none !important;
      }
      table.body .right-text-pad,
      table.body .text-pad-right {
        padding-left: 10px !important;
      }
      table.body .left-text-pad,
      table.body .text-pad-left {
        padding-right: 10px !important;
      }
      table.menu {
        width: 100% !important;
      }
      table.menu td,
      table.menu th {
        width: auto !important;
        display: inline-block !important;
      }
      table.menu.vertical td,
      table.menu.vertical th,
      table.menu.small-vertical td,
      table.menu.small-vertical th {
        display: block !important;
      }
      table.menu[align=""center""] {
        width: auto !important;
      }
      table.button.small-expand,
      table.button.small-expanded {
        width: 100% !important;
      }
      table.button.small-expand table,
      table.button.small-expanded table {
        width: 100%;
      }
      table.button.small-expand table a,
      table.button.small-expanded table a {
        text-align: center !important;
        width: 100% !important;
        padding-left: 0 !important;
        padding-right: 0 !important;
      }
      table.button.small-expand center,
      table.button.small-expanded center {
        min-width: 0;
      }
    }
  </style>
  <span class=""preheader"" style=""color: {{ bodyBackgroundColor }}; display: none !important; font-size: 1px; line-height: 1px; max-height: 0px; max-width: 0px; mso-hide: all !important; opacity: 0; overflow: hidden; visibility: hidden;""></span>
  <table class=""body"" style=""Margin: 0; background: {{ bodyBackgroundColor }} !important; border-collapse: collapse; border-spacing: 0; color: {{ textColor }}; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; height: 100%; line-height: 1.3; margin: 0; padding: 0; text-align: left; vertical-align: top; width: 100%;"">
    <tbody>
      <tr style=""padding: 0; text-align: left; vertical-align: top;"">
        <td class=""center"" align=""center"" valign=""top"" style=""-moz-hyphens: auto; -webkit-hyphens: auto; Margin: 0; border-collapse: collapse !important; color: {{ textColor }}; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; hyphens: auto; line-height: 1.3; margin: 0; padding: 0; text-align: left; vertical-align: top; word-wrap: break-word; background-color: {{ bodyBackgroundColor }}"">
          <center data-parsed="""" style=""min-width: 580px; width: 100%;"">


            <table align=""center"" class=""wrapper header float-center"" style=""Margin: 0 auto; background: {{ headerBackgroundColor }}; border-collapse: collapse; border-spacing: 0; float: none; margin: 0 auto; padding: 0; text-align: center; vertical-align: top; width: 100%;"">
              <tbody>
                <tr style=""padding: 0; text-align: left; vertical-align: top;"">
                  <td class=""wrapper-inner"" style=""-moz-hyphens: auto; -webkit-hyphens: auto; Margin: 0; border-collapse: collapse !important; color: {{ textColor }}; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; hyphens: auto; line-height: 1.3; margin: 0; padding: 15px; text-align: left; vertical-align: top; word-wrap: break-word;"">
                    <table align=""center"" class=""container"" style=""Margin: 0 auto; background: transparent; border-collapse: collapse; border-spacing: 0; margin: 0 auto; padding: 0; text-align: inherit; vertical-align: top; width: 580px;"">
                      <tbody>
                        <tr style=""padding: 0; text-align: left; vertical-align: top;"">
                          <td style=""-moz-hyphens: auto; -webkit-hyphens: auto; Margin: 0; border-collapse: collapse !important; color: {{ textColor }}; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; hyphens: auto; line-height: 1.3; margin: 0; padding: 0; text-align: left; vertical-align: top; word-wrap: break-word;"">
                            <!-- LOGO -->
                            <img id=""template-logo"" src=""/Content/EmailTemplates/placeholder-logo.png"" width=""200"" height=""50"" data-instructions=""Provide a PNG with a transparent background."" style=""display: block;"">  
                          </td>
                        </tr>
                      </tbody>
                    </table>
                  </td>
                </tr>
              </tbody>
            </table>


            <table align=""center"" class=""container float-center"" style=""Margin: 0 auto; background: #fefefe; border-collapse: collapse; border-spacing: 0; float: none; margin: 0 auto; padding: 0; text-align: center; vertical-align: top; width: 580px;"">
              <tbody>
                <tr style=""padding: 0; text-align: left; vertical-align: top;"">
                  <td style=""-moz-hyphens: auto; -webkit-hyphens: auto; Margin: 0; border-collapse: collapse !important; color: {{ textColor }}; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; hyphens: auto; line-height: 1.3; margin: 0; padding: 0; text-align: left; vertical-align: top; word-wrap: break-word;"">


                    <table class=""spacer"" style=""border-collapse: collapse; border-spacing: 0; padding: 0; text-align: left; vertical-align: top; width: 100%;"">
                      <tbody>
                        <tr style=""padding: 0; text-align: left; vertical-align: top;"">
                          <td height=""16px"" style=""-moz-hyphens: auto; -webkit-hyphens: auto; Margin: 0; border-collapse: collapse !important; color: {{ textColor }}; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; hyphens: auto; line-height: 16px; margin: 0; mso-line-height-rule: exactly; padding: 0; text-align: left; vertical-align: top; word-wrap: break-word;"">&nbsp;</td>
                        </tr>
                      </tbody>
                    </table>


                    <table class=""row"" style=""border-collapse: collapse; border-spacing: 0; display: table; padding: 0; position: relative; text-align: left; vertical-align: top; width: 100%;"">
                      <tbody>
                        <tr style=""padding: 0; text-align: left; vertical-align: top;"">
                          <th class=""small-12 large-12 columns first last"" style=""Margin: 0 auto; color: {{ textColor }}; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.3; margin: 0 auto; padding: 0; padding-bottom: 16px; padding-left: 16px; padding-right: 16px; text-align: left; width: 564px;"">
							<div class=""structure-dropzone"">
								<div class=""dropzone"">
								    <table class=""component component-text selected"" data-state=""component"" style=""border-collapse: collapse; border-spacing: 0px; display: table; padding: 0px; position: relative; text-align: left; vertical-align: top; width: 100%; background-color: transparent;"">
							            <tbody>
							                <tr>
							                    <td class=""js-component-text-wrapper"" style=""border-color: rgb(0, 0, 0);"">
							                        <h1>Title</h1><p> Can't wait to see what you have to say!</p>
						                        </td>
					                        </tr>
				                        </tbody>
			                        </table>

								</div>
							</div>
						  </th>
                        </tr>
                      </tbody>
                    </table>
                    <table class=""wrapper secondary"" align=""center"" style=""background: {{ bodyBackgroundColor }}; border-collapse: collapse; border-spacing: 0; padding: 0; text-align: left; vertical-align: top; width: 100%;"">
                      <tbody>
                        <tr style=""padding: 0; text-align: left; vertical-align: top;"">
                          <td class=""wrapper-inner"" style=""-moz-hyphens: auto; -webkit-hyphens: auto; Margin: 0; border-collapse: collapse !important; color: {{ textColor }}; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; hyphens: auto; line-height: 1.3; margin: 0; padding: 0; text-align: left; vertical-align: top; word-wrap: break-word;"">


                            <table class=""spacer"" style=""border-collapse: collapse; border-spacing: 0; padding: 0; text-align: left; vertical-align: top; width: 100%;"">
                              <tbody>
                                <tr style=""padding: 0; text-align: left; vertical-align: top;"">
                                  <td height=""16px"" style=""-moz-hyphens: auto; -webkit-hyphens: auto; Margin: 0; border-collapse: collapse !important; color: {{ textColor }}; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; hyphens: auto; line-height: 16px; margin: 0; mso-line-height-rule: exactly; padding: 0; text-align: left; vertical-align: top; word-wrap: break-word;"">&nbsp;</td>
                                </tr>
                              </tbody>
                            </table>


                            <table class=""row"" style=""border-collapse: collapse; border-spacing: 0; display: table; padding: 0; position: relative; text-align: left; vertical-align: top; width: 100%;"">
                              <tbody>
                                <tr style=""padding: 0; text-align: left; vertical-align: top;"">
                                  <th class=""small-12 large-6 columns first"" style=""Margin: 0 auto; color: {{ textColor }}; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.3; margin: 0 auto; padding: 0; padding-bottom: 16px; padding-left: 16px; padding-right: 8px; text-align: left; width: 274px;"">
                                    <table style=""border-collapse: collapse; border-spacing: 0; padding: 0; text-align: left; vertical-align: top; width: 100%;"">
                                      <tbody>
                                        <tr style=""padding: 0; text-align: left; vertical-align: top;"">
                                          <th style=""Margin: 0; color: {{ textColor }}; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.3; margin: 0; padding: 0; text-align: left;"">
                                            &nbsp;
                                          </th>
                                        </tr>
                                      </tbody>
                                    </table>
                                  </th>
                                  <th class=""small-12 large-6 columns last"" style=""Margin: 0 auto; color: {{ textColor }}; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.3; margin: 0 auto; padding: 0; padding-bottom: 16px; padding-left: 8px; padding-right: 16px; text-align: left; width: 274px;"">
                                    <table style=""border-collapse: collapse; border-spacing: 0; padding: 0; text-align: left; vertical-align: top; width: 100%;"">
                                      <tbody>
                                        <tr style=""padding: 0; text-align: left; vertical-align: top;"">
                                          <th style=""Margin: 0; color: {{ textColor }}; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.3; margin: 0; padding: 0; text-align: left;"">
                                            <h5 style=""Margin: 0; Margin-bottom: 10px; color: inherit; font-family: Helvetica, Arial, sans-serif; font-size: 20px; font-weight: normal; line-height: 1.3; margin: 0; margin-bottom: 10px; padding: 0; text-align: left; word-wrap: normal;"">Contact Info:</h5>
                                            <p style=""Margin: 0; Margin-bottom: 10px; color: {{ textColor }}; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.3; margin: 0; margin-bottom: 10px; padding: 0; text-align: left;"">Website: <a href=""{{ 'Global' | Attribute:'OrganizationWebsite' }}"" style=""Margin: 0; color: {{ linkColor }}; font-family: Helvetica, Arial, sans-serif; font-weight: normal; line-height: 1.3; margin: 0; padding: 0; text-align: left; text-decoration: none;"">{{ 'Global' | Attribute:'OrganizationWebsite' }}</a></p>
                                            <p style=""Margin: 0; Margin-bottom: 10px; color: {{ textColor }}; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.3; margin: 0; margin-bottom: 10px; padding: 0; text-align: left;"">Email: <a href=""mailto:{{ 'Global' | Attribute:'OrganizationEmail' }}"" style=""Margin: 0; color: {{ linkColor }}; font-family: Helvetica, Arial, sans-serif; font-weight: normal; line-height: 1.3; margin: 0; padding: 0; text-align: left; text-decoration: none;"">{{ 'Global' | Attribute:'OrganizationEmail' }}</a></p>
                                          </th>
                                        </tr>
                                      </tbody>
                                    </table>
                                  </th>
                                </tr>
                              </tbody>
                            </table>
							<div style=""width: 100%; text-align: center; font-size: 11px; font-color: #6f6f6f; margin-bottom: 24px;"">[[ UnsubscribeOption ]]</div>
                          </td>
                        </tr>
                      </tbody>
                    </table>
                  </td>
                </tr>
              </tbody>
            </table>


          </center>
        </td>
      </tr>
    </tbody>
  </table>
  <!-- prevent Gmail on iOS font size manipulation -->
  <div style=""display:none; white-space:nowrap; font:15px courier; line-height:0;""> &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; </div>
</body>


</html>";
            var defaultCommunicationTemplateLavaFieldsDefaultDictionary = @"{ ""headerBackgroundColor"":""#5e5e5e"",""linkColor"":""#2199e8"",""bodyBackgroundColor"":""#f3f3f3"",""textColor"":""#0a0a0a""}".FromJsonOrNull<Dictionary<string, string>>();
            Dictionary<string, string> lavaFieldsTemplateDictionaryFromControls = new Dictionary<string, string>()
            {
                { "headerBackgroundColor", "blue" }
            };

            var expectedDefaultCommunicationTemplateOutput = @"<!DOCTYPE html>
<html>


<head><noscript id=""lava-fields"">
  {% comment %}  Lava Fields: Code-Generated from Template Editor {% endcomment %}
  {% assign headerBackgroundColor = 'blue' %}
  {% assign linkColor = '#2199e8' %}
  {% assign bodyBackgroundColor = '#f3f3f3' %}
  {% assign textColor = '#0a0a0a' %}
</noscript>
  <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">
  <meta name=""viewport"" content=""width=device-width"">
  <title>My Basic Email Template Subject</title>
  <!-- <style> -->
</head>


<body style=""-moz-box-sizing: border-box; -ms-text-size-adjust: 100%; -webkit-box-sizing: border-box; -webkit-text-size-adjust: 100%; Margin: 0; background: {{ bodyBackgroundColor }} !important; box-sizing: border-box; color: {{ textColor }}; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.3; margin: 0; min-width: 100%; padding: 0; text-align: left; width: 100% !important;"">
  

  <style>
      a {
          color: {{ linkColor }};
      }
      
      .component-text td {
          color: {{ textColor }};
          font-family: Helvetica, Arial, sans-serif;
          font-size: 16px;
          font-weight: normal;
          line-height: 1.3;
      }
  </style>

  <style class=""ignore"">
    @media only screen {
      html {
        min-height: 100%;
        background: {{ bodyBackgroundColor }};
      }
    }
    
    @media only screen and (max-width: 596px) {
      .small-float-center {
        margin: 0 auto !important;
        float: none !important;
        text-align: center !important;
      }
      .small-text-center {
        text-align: center !important;
      }
      .small-text-left {
        text-align: left !important;
      }
      .small-text-right {
        text-align: right !important;
      }
    }
    
    @media only screen and (max-width: 596px) {
      .hide-for-large {
        display: block !important;
        width: auto !important;
        overflow: visible !important;
        max-height: none !important;
        font-size: inherit !important;
        line-height: inherit !important;
      }
    }
    
    @media only screen and (max-width: 596px) {
      table.body table.container .hide-for-large,
      table.body table.container .row.hide-for-large {
        display: table !important;
        width: 100% !important;
      }
    }
    
    @media only screen and (max-width: 596px) {
      table.body table.container .callout-inner.hide-for-large {
        display: table-cell !important;
        width: 100% !important;
      }
    }
    
    @media only screen and (max-width: 596px) {
      table.body table.container .show-for-large {
        display: none !important;
        width: 0;
        mso-hide: all;
        overflow: hidden;
      }
    }
    
    @media only screen and (max-width: 596px) {
      table.body img {
        width: auto;
        height: auto;
      }
      table.body center {
        min-width: 0 !important;
      }
      table.body .container {
        width: 95% !important;
      }
      table.body .columns,
      table.body .column {
        height: auto !important;
        -moz-box-sizing: border-box;
        -webkit-box-sizing: border-box;
        box-sizing: border-box;
        padding-left: 16px !important;
        padding-right: 16px !important;
      }
      table.body .columns .column,
      table.body .columns .columns,
      table.body .column .column,
      table.body .column .columns {
        padding-left: 0 !important;
        padding-right: 0 !important;
      }
      table.body .collapse .columns,
      table.body .collapse .column {
        padding-left: 0 !important;
        padding-right: 0 !important;
      }
      td.small-1,
      th.small-1 {
        display: inline-block !important;
        width: 8.33333% !important;
      }
      td.small-2,
      th.small-2 {
        display: inline-block !important;
        width: 16.66667% !important;
      }
      td.small-3,
      th.small-3 {
        display: inline-block !important;
        width: 25% !important;
      }
      td.small-4,
      th.small-4 {
        display: inline-block !important;
        width: 33.33333% !important;
      }
      td.small-5,
      th.small-5 {
        display: inline-block !important;
        width: 41.66667% !important;
      }
      td.small-6,
      th.small-6 {
        display: inline-block !important;
        width: 50% !important;
      }
      td.small-7,
      th.small-7 {
        display: inline-block !important;
        width: 58.33333% !important;
      }
      td.small-8,
      th.small-8 {
        display: inline-block !important;
        width: 66.66667% !important;
      }
      td.small-9,
      th.small-9 {
        display: inline-block !important;
        width: 75% !important;
      }
      td.small-10,
      th.small-10 {
        display: inline-block !important;
        width: 83.33333% !important;
      }
      td.small-11,
      th.small-11 {
        display: inline-block !important;
        width: 91.66667% !important;
      }
      td.small-12,
      th.small-12 {
        display: inline-block !important;
        width: 100% !important;
      }
      .columns td.small-12,
      .column td.small-12,
      .columns th.small-12,
      .column th.small-12 {
        display: block !important;
        width: 100% !important;
      }
      table.body td.small-offset-1,
      table.body th.small-offset-1 {
        margin-left: 8.33333% !important;
        Margin-left: 8.33333% !important;
      }
      table.body td.small-offset-2,
      table.body th.small-offset-2 {
        margin-left: 16.66667% !important;
        Margin-left: 16.66667% !important;
      }
      table.body td.small-offset-3,
      table.body th.small-offset-3 {
        margin-left: 25% !important;
        Margin-left: 25% !important;
      }
      table.body td.small-offset-4,
      table.body th.small-offset-4 {
        margin-left: 33.33333% !important;
        Margin-left: 33.33333% !important;
      }
      table.body td.small-offset-5,
      table.body th.small-offset-5 {
        margin-left: 41.66667% !important;
        Margin-left: 41.66667% !important;
      }
      table.body td.small-offset-6,
      table.body th.small-offset-6 {
        margin-left: 50% !important;
        Margin-left: 50% !important;
      }
      table.body td.small-offset-7,
      table.body th.small-offset-7 {
        margin-left: 58.33333% !important;
        Margin-left: 58.33333% !important;
      }
      table.body td.small-offset-8,
      table.body th.small-offset-8 {
        margin-left: 66.66667% !important;
        Margin-left: 66.66667% !important;
      }
      table.body td.small-offset-9,
      table.body th.small-offset-9 {
        margin-left: 75% !important;
        Margin-left: 75% !important;
      }
      table.body td.small-offset-10,
      table.body th.small-offset-10 {
        margin-left: 83.33333% !important;
        Margin-left: 83.33333% !important;
      }
      table.body td.small-offset-11,
      table.body th.small-offset-11 {
        margin-left: 91.66667% !important;
        Margin-left: 91.66667% !important;
      }
      table.body table.columns td.expander,
      table.body table.columns th.expander {
        display: none !important;
      }
      table.body .right-text-pad,
      table.body .text-pad-right {
        padding-left: 10px !important;
      }
      table.body .left-text-pad,
      table.body .text-pad-left {
        padding-right: 10px !important;
      }
      table.menu {
        width: 100% !important;
      }
      table.menu td,
      table.menu th {
        width: auto !important;
        display: inline-block !important;
      }
      table.menu.vertical td,
      table.menu.vertical th,
      table.menu.small-vertical td,
      table.menu.small-vertical th {
        display: block !important;
      }
      table.menu[align=""center""] {
        width: auto !important;
      }
      table.button.small-expand,
      table.button.small-expanded {
        width: 100% !important;
      }
      table.button.small-expand table,
      table.button.small-expanded table {
        width: 100%;
      }
      table.button.small-expand table a,
      table.button.small-expanded table a {
        text-align: center !important;
        width: 100% !important;
        padding-left: 0 !important;
        padding-right: 0 !important;
      }
      table.button.small-expand center,
      table.button.small-expanded center {
        min-width: 0;
      }
    }
  </style>
  <span class=""preheader"" style=""color: {{ bodyBackgroundColor }}; display: none !important; font-size: 1px; line-height: 1px; max-height: 0px; max-width: 0px; mso-hide: all !important; opacity: 0; overflow: hidden; visibility: hidden;""></span>
  <table class=""body"" style=""Margin: 0; background: {{ bodyBackgroundColor }} !important; border-collapse: collapse; border-spacing: 0; color: {{ textColor }}; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; height: 100%; line-height: 1.3; margin: 0; padding: 0; text-align: left; vertical-align: top; width: 100%;"">
    <tbody>
      <tr style=""padding: 0; text-align: left; vertical-align: top;"">
        <td class=""center"" align=""center"" valign=""top"" style=""-moz-hyphens: auto; -webkit-hyphens: auto; Margin: 0; border-collapse: collapse !important; color: {{ textColor }}; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; hyphens: auto; line-height: 1.3; margin: 0; padding: 0; text-align: left; vertical-align: top; word-wrap: break-word; background-color: {{ bodyBackgroundColor }}"">
          <center data-parsed="""" style=""min-width: 580px; width: 100%;"">


            <table align=""center"" class=""wrapper header float-center"" style=""Margin: 0 auto; background: {{ headerBackgroundColor }}; border-collapse: collapse; border-spacing: 0; float: none; margin: 0 auto; padding: 0; text-align: center; vertical-align: top; width: 100%;"">
              <tbody>
                <tr style=""padding: 0; text-align: left; vertical-align: top;"">
                  <td class=""wrapper-inner"" style=""-moz-hyphens: auto; -webkit-hyphens: auto; Margin: 0; border-collapse: collapse !important; color: {{ textColor }}; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; hyphens: auto; line-height: 1.3; margin: 0; padding: 15px; text-align: left; vertical-align: top; word-wrap: break-word;"">
                    <table align=""center"" class=""container"" style=""Margin: 0 auto; background: transparent; border-collapse: collapse; border-spacing: 0; margin: 0 auto; padding: 0; text-align: inherit; vertical-align: top; width: 580px;"">
                      <tbody>
                        <tr style=""padding: 0; text-align: left; vertical-align: top;"">
                          <td style=""-moz-hyphens: auto; -webkit-hyphens: auto; Margin: 0; border-collapse: collapse !important; color: {{ textColor }}; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; hyphens: auto; line-height: 1.3; margin: 0; padding: 0; text-align: left; vertical-align: top; word-wrap: break-word;"">
                            <!-- LOGO -->
                            <img id=""template-logo"" src=""/GetImage.ashx?Id=1"" width=""200"" height=""50"" data-instructions=""Provide a PNG with a transparent background."" style=""display: block;"">  
                          </td>
                        </tr>
                      </tbody>
                    </table>
                  </td>
                </tr>
              </tbody>
            </table>


            <table align=""center"" class=""container float-center"" style=""Margin: 0 auto; background: #fefefe; border-collapse: collapse; border-spacing: 0; float: none; margin: 0 auto; padding: 0; text-align: center; vertical-align: top; width: 580px;"">
              <tbody>
                <tr style=""padding: 0; text-align: left; vertical-align: top;"">
                  <td style=""-moz-hyphens: auto; -webkit-hyphens: auto; Margin: 0; border-collapse: collapse !important; color: {{ textColor }}; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; hyphens: auto; line-height: 1.3; margin: 0; padding: 0; text-align: left; vertical-align: top; word-wrap: break-word;"">


                    <table class=""spacer"" style=""border-collapse: collapse; border-spacing: 0; padding: 0; text-align: left; vertical-align: top; width: 100%;"">
                      <tbody>
                        <tr style=""padding: 0; text-align: left; vertical-align: top;"">
                          <td height=""16px"" style=""-moz-hyphens: auto; -webkit-hyphens: auto; Margin: 0; border-collapse: collapse !important; color: {{ textColor }}; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; hyphens: auto; line-height: 16px; margin: 0; mso-line-height-rule: exactly; padding: 0; text-align: left; vertical-align: top; word-wrap: break-word;"">&nbsp;</td>
                        </tr>
                      </tbody>
                    </table>


                    <table class=""row"" style=""border-collapse: collapse; border-spacing: 0; display: table; padding: 0; position: relative; text-align: left; vertical-align: top; width: 100%;"">
                      <tbody>
                        <tr style=""padding: 0; text-align: left; vertical-align: top;"">
                          <th class=""small-12 large-12 columns first last"" style=""Margin: 0 auto; color: {{ textColor }}; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.3; margin: 0 auto; padding: 0; padding-bottom: 16px; padding-left: 16px; padding-right: 16px; text-align: left; width: 564px;"">
							<div class=""structure-dropzone"">
								<div class=""dropzone"">
								    <table class=""component component-text selected"" data-state=""component"" style=""border-collapse: collapse; border-spacing: 0px; display: table; padding: 0px; position: relative; text-align: left; vertical-align: top; width: 100%; background-color: transparent;"">
							            <tbody>
							                <tr>
							                    <td class=""js-component-text-wrapper"" style=""border-color: rgb(0, 0, 0);"">
							                        <h1>Title</h1><p> Can't wait to see what you have to say!</p>
						                        </td>
					                        </tr>
				                        </tbody>
			                        </table>

								</div>
							</div>
						  </th>
                        </tr>
                      </tbody>
                    </table>
                    <table class=""wrapper secondary"" align=""center"" style=""background: {{ bodyBackgroundColor }}; border-collapse: collapse; border-spacing: 0; padding: 0; text-align: left; vertical-align: top; width: 100%;"">
                      <tbody>
                        <tr style=""padding: 0; text-align: left; vertical-align: top;"">
                          <td class=""wrapper-inner"" style=""-moz-hyphens: auto; -webkit-hyphens: auto; Margin: 0; border-collapse: collapse !important; color: {{ textColor }}; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; hyphens: auto; line-height: 1.3; margin: 0; padding: 0; text-align: left; vertical-align: top; word-wrap: break-word;"">


                            <table class=""spacer"" style=""border-collapse: collapse; border-spacing: 0; padding: 0; text-align: left; vertical-align: top; width: 100%;"">
                              <tbody>
                                <tr style=""padding: 0; text-align: left; vertical-align: top;"">
                                  <td height=""16px"" style=""-moz-hyphens: auto; -webkit-hyphens: auto; Margin: 0; border-collapse: collapse !important; color: {{ textColor }}; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; hyphens: auto; line-height: 16px; margin: 0; mso-line-height-rule: exactly; padding: 0; text-align: left; vertical-align: top; word-wrap: break-word;"">&nbsp;</td>
                                </tr>
                              </tbody>
                            </table>


                            <table class=""row"" style=""border-collapse: collapse; border-spacing: 0; display: table; padding: 0; position: relative; text-align: left; vertical-align: top; width: 100%;"">
                              <tbody>
                                <tr style=""padding: 0; text-align: left; vertical-align: top;"">
                                  <th class=""small-12 large-6 columns first"" style=""Margin: 0 auto; color: {{ textColor }}; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.3; margin: 0 auto; padding: 0; padding-bottom: 16px; padding-left: 16px; padding-right: 8px; text-align: left; width: 274px;"">
                                    <table style=""border-collapse: collapse; border-spacing: 0; padding: 0; text-align: left; vertical-align: top; width: 100%;"">
                                      <tbody>
                                        <tr style=""padding: 0; text-align: left; vertical-align: top;"">
                                          <th style=""Margin: 0; color: {{ textColor }}; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.3; margin: 0; padding: 0; text-align: left;"">
                                            &nbsp;
                                          </th>
                                        </tr>
                                      </tbody>
                                    </table>
                                  </th>
                                  <th class=""small-12 large-6 columns last"" style=""Margin: 0 auto; color: {{ textColor }}; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.3; margin: 0 auto; padding: 0; padding-bottom: 16px; padding-left: 8px; padding-right: 16px; text-align: left; width: 274px;"">
                                    <table style=""border-collapse: collapse; border-spacing: 0; padding: 0; text-align: left; vertical-align: top; width: 100%;"">
                                      <tbody>
                                        <tr style=""padding: 0; text-align: left; vertical-align: top;"">
                                          <th style=""Margin: 0; color: {{ textColor }}; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.3; margin: 0; padding: 0; text-align: left;"">
                                            <h5 style=""Margin: 0; Margin-bottom: 10px; color: inherit; font-family: Helvetica, Arial, sans-serif; font-size: 20px; font-weight: normal; line-height: 1.3; margin: 0; margin-bottom: 10px; padding: 0; text-align: left; word-wrap: normal;"">Contact Info:</h5>
                                            <p style=""Margin: 0; Margin-bottom: 10px; color: {{ textColor }}; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.3; margin: 0; margin-bottom: 10px; padding: 0; text-align: left;"">Website: <a href=""{{ 'Global' | Attribute:'OrganizationWebsite' }}"" style=""Margin: 0; color: {{ linkColor }}; font-family: Helvetica, Arial, sans-serif; font-weight: normal; line-height: 1.3; margin: 0; padding: 0; text-align: left; text-decoration: none;"">{{ 'Global' | Attribute:'OrganizationWebsite' }}</a></p>
                                            <p style=""Margin: 0; Margin-bottom: 10px; color: {{ textColor }}; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.3; margin: 0; margin-bottom: 10px; padding: 0; text-align: left;"">Email: <a href=""mailto:{{ 'Global' | Attribute:'OrganizationEmail' }}"" style=""Margin: 0; color: {{ linkColor }}; font-family: Helvetica, Arial, sans-serif; font-weight: normal; line-height: 1.3; margin: 0; padding: 0; text-align: left; text-decoration: none;"">{{ 'Global' | Attribute:'OrganizationEmail' }}</a></p>
                                          </th>
                                        </tr>
                                      </tbody>
                                    </table>
                                  </th>
                                </tr>
                              </tbody>
                            </table>
							<div style=""width: 100%; text-align: center; font-size: 11px; font-color: #6f6f6f; margin-bottom: 24px;"">[[ UnsubscribeOption ]]</div>
                          </td>
                        </tr>
                      </tbody>
                    </table>
                  </td>
                </tr>
              </tbody>
            </table>


          </center>
        </td>
      </tr>
    </tbody>
  </table>
  <!-- prevent Gmail on iOS font size manipulation -->
  <div style=""display:none; white-space:nowrap; font:15px courier; line-height:0;""> &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; </div>
</body>


</html>";
            var defaultCommunicationTemplateOutput = CommunicationTemplateHelper.GetUpdatedTemplateHtml( defaultCommunicationTemplate, 1, lavaFieldsTemplateDictionaryFromControls, defaultCommunicationTemplateLavaFieldsDefaultDictionary );
            Assert.That.AreEqual( expectedDefaultCommunicationTemplateOutput.Replace( "\r\n", "\n" ), defaultCommunicationTemplateOutput.Replace( "\r\n", "\n" ) );

            // see if it still ok after running it again
            defaultCommunicationTemplateOutput = CommunicationTemplateHelper.GetUpdatedTemplateHtml( defaultCommunicationTemplateOutput, 1, lavaFieldsTemplateDictionaryFromControls, defaultCommunicationTemplateLavaFieldsDefaultDictionary );
            Assert.That.AreEqual( expectedDefaultCommunicationTemplateOutput.Replace( "\r\n", "\n" ), defaultCommunicationTemplateOutput.Replace( "\r\n", "\n" ) );
        }

        /// <summary>
        /// Should correctly update the a very simple html doc when lava fields are defined in the UI
        /// </summary>
        [TestMethod]
        public void TemplateEditor_SimpleHtml()
        {
            /* simple html*/
            string originalTemplate = @"<html><head></head><body><body></html>";
            Dictionary<string, string> lavaFieldsDefaultDictionary = new Dictionary<string, string>() { { "headerBackgroundColor", "#5e5e5e" } };
            Dictionary<string, string> lavaFieldsFromControls = new Dictionary<string, string>() { { "headerBackgroundColor", "blue" } };
            string expectedOutput = @"<html><head><noscript id=""lava-fields"">
  {% comment %}  Lava Fields: Code-Generated from Template Editor {% endcomment %}
  {% assign headerBackgroundColor = 'blue' %}
</noscript></head><body><body></html>";

            var actualOutput = CommunicationTemplateHelper.GetUpdatedTemplateHtml( originalTemplate, 1, lavaFieldsFromControls, lavaFieldsDefaultDictionary );

            Assert.That.AreEqual( expectedOutput.Replace( "\r\n", "\n" ), actualOutput.Replace( "\r\n", "\n" ) );

            // see if it still ok after running it again
            actualOutput = CommunicationTemplateHelper.GetUpdatedTemplateHtml( actualOutput, 1, lavaFieldsFromControls, lavaFieldsDefaultDictionary );
            Assert.That.AreEqual( expectedOutput.Replace( "\r\n", "\n" ), actualOutput.Replace( "\r\n", "\n" ) );
        }

        /// <summary>
        /// Should correctly update the a formatted simple html doc when lava fields are defined in the UI
        /// </summary>
        [TestMethod]
        public void TemplateEditor_FormattedSimpleHtml()
        {
            /* formatted simple html*/
            string originalTemplate =
            @"<html>
  <head>
  </head>
  <body>
  <body>
</html>";
            Dictionary<string, string> lavaFieldsDefaultDictionary = new Dictionary<string, string>() { { "headerBackgroundColor", "#5e5e5e" } };
            Dictionary<string, string> lavaFieldsFromControls = new Dictionary<string, string>() { { "headerBackgroundColor", "blue" } };
            string expectedOutput =
            @"<html>
  <head><noscript id=""lava-fields"">
  {% comment %}  Lava Fields: Code-Generated from Template Editor {% endcomment %}
  {% assign headerBackgroundColor = 'blue' %}
</noscript>
  </head>
  <body>
  <body>
</html>";
            string actualOutput = CommunicationTemplateHelper.GetUpdatedTemplateHtml( originalTemplate, 1, lavaFieldsFromControls, lavaFieldsDefaultDictionary );
            Assert.That.AreEqual( expectedOutput.Replace( "\r\n", "\n" ), actualOutput.Replace( "\r\n", "\n" ) );

            // see if it still ok after running it again
            actualOutput = CommunicationTemplateHelper.GetUpdatedTemplateHtml( actualOutput, 1, lavaFieldsFromControls, lavaFieldsDefaultDictionary );
            Assert.That.AreEqual( expectedOutput.Replace( "\r\n", "\n" ), actualOutput.Replace( "\r\n", "\n" ) );
        }

        /// <summary>
        /// Should correctly update the a formatted simple html doc when lava fields are defined in the UI
        /// </summary>
        [TestMethod]
        public void TemplateEditor_FormattedSimpleHtmlWithMeta()
        {
            /* formatted simple html*/
            string originalTemplate =
            @"<html>
  <head>
    <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">
    <meta name=""viewport"" content=""width=device-width"">
    <title>My Test Template</title> 
  </head>
  <body>
  <body>
</html>";
            Dictionary<string, string> lavaFieldsDefaultDictionary = new Dictionary<string, string>() { { "headerBackgroundColor", "#5e5e5e" } };
            Dictionary<string, string> lavaFieldsFromControls = new Dictionary<string, string>() { { "headerBackgroundColor", "blue" } };
            string expectedOutput =
            @"<html>
  <head><noscript id=""lava-fields"">
  {% comment %}  Lava Fields: Code-Generated from Template Editor {% endcomment %}
  {% assign headerBackgroundColor = 'blue' %}
</noscript>
    <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">
    <meta name=""viewport"" content=""width=device-width"">
    <title>My Test Template</title> 
  </head>
  <body>
  <body>
</html>";
            string actualOutput = CommunicationTemplateHelper.GetUpdatedTemplateHtml( originalTemplate, 1, lavaFieldsFromControls, lavaFieldsDefaultDictionary );
            Assert.That.AreEqual( expectedOutput.Replace( "\r\n", "\n" ), actualOutput.Replace( "\r\n", "\n" ) );

            // see if it still ok after running it again
            actualOutput = CommunicationTemplateHelper.GetUpdatedTemplateHtml( actualOutput, 1, lavaFieldsFromControls, lavaFieldsDefaultDictionary );
            Assert.That.AreEqual( expectedOutput.Replace( "\r\n", "\n" ), actualOutput.Replace( "\r\n", "\n" ) );
        }

        /// <summary>
        /// Should correctly update an empty html doc when lava fields are defined in the UI
        /// </summary>
        [TestMethod]
        public void TemplateEditor_EmptyHtml()
        {
            /* empty html*/
            string originalTemplate = string.Empty;
            Dictionary<string, string> lavaFieldsDefaultDictionary = new Dictionary<string, string>() { { "headerBackgroundColor", "#5e5e5e" } };
            Dictionary<string, string> lavaFieldsFromControls = new Dictionary<string, string>() { { "headerBackgroundColor", "blue" } };
            string expectedOutput =
            @"<noscript id=""lava-fields"">
  {% comment %}  Lava Fields: Code-Generated from Template Editor {% endcomment %}
  {% assign headerBackgroundColor = 'blue' %}
</noscript>
";
            string actualOutput = CommunicationTemplateHelper.GetUpdatedTemplateHtml( originalTemplate, 1, lavaFieldsFromControls, lavaFieldsDefaultDictionary );
            Assert.That.AreEqual( expectedOutput.Replace( "\r\n", "\n" ), actualOutput.Replace( "\r\n", "\n" ) );

            // see if it still ok after running it again
            actualOutput = CommunicationTemplateHelper.GetUpdatedTemplateHtml( actualOutput, 1, lavaFieldsFromControls, lavaFieldsDefaultDictionary );
            Assert.That.AreEqual( expectedOutput.Replace( "\r\n", "\n" ), actualOutput.Replace( "\r\n", "\n" ) );
        }

        /// <summary>
        /// Should correctly update an html doc that has lava logic in it when lava fields are defined in the UI
        /// </summary>
        /// [TestMethod]
        [TestMethod]
        public void TemplateEditor_HasLavaLogic()
        {
            /* already has lava html*/
            string originalTemplate =
@"{% if Person.NickName == 'Ted' %}
    Great Guy!
{% elseif Person.NickName == 'Alisha' %}
    Great Gal!
{% else %}
    Who are you?
{% endif %}";
            Dictionary<string, string> lavaFieldsDefaultDictionary = new Dictionary<string, string>() { { "headerBackgroundColor", "#5e5e5e" } };
            Dictionary<string, string> lavaFieldsFromControls = new Dictionary<string, string>() { { "headerBackgroundColor", "blue" } };

            string expectedOutput =
@"<noscript id=""lava-fields"">
  {% comment %}  Lava Fields: Code-Generated from Template Editor {% endcomment %}
  {% assign headerBackgroundColor = 'blue' %}
</noscript>
{% if Person.NickName == 'Ted' %}
    Great Guy!
{% elseif Person.NickName == 'Alisha' %}
    Great Gal!
{% else %}
    Who are you?
{% endif %}";

            string actualOutput = CommunicationTemplateHelper.GetUpdatedTemplateHtml( originalTemplate, 1, lavaFieldsFromControls, lavaFieldsDefaultDictionary );
            Assert.That.AreEqual( expectedOutput.Replace( "\r\n", "\n" ), actualOutput.Replace( "\r\n", "\n" ) );

            // see if it still ok after running it again
            actualOutput = CommunicationTemplateHelper.GetUpdatedTemplateHtml( actualOutput, 1, lavaFieldsFromControls, lavaFieldsDefaultDictionary );
            Assert.That.AreEqual( expectedOutput.Replace( "\r\n", "\n" ), actualOutput.Replace( "\r\n", "\n" ) );
        }
    }
}
