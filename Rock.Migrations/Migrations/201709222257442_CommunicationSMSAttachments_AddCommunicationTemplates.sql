/* MP: Create the System 'Blank' communication template */
UPDATE Communication
SET CommunicationTemplateId = NULL
WHERE CommunicationTemplateId IN (
		SELECT Id
		FROM CommunicationTemplate
		WHERE [Guid] = 'A3C7F623-7F6F-4C48-B66F-CBEE2DF30B6A'
		)

DELETE
FROM CommunicationTemplate
WHERE [Guid] = 'A3C7F623-7F6F-4C48-B66F-CBEE2DF30B6A'

INSERT INTO [dbo].[CommunicationTemplate] (
	[Name]
	,[Description]
	,[IsSystem]
	,[Subject]
	,[MediumDataJson]
	,[Guid]
	,[Message]
	)
VALUES (
	'Blank'
	,'A template without predefined content that can be used to create email and SMS communications'
	,1
	,''
	,'{}'
	,'A3C7F623-7F6F-4C48-B66F-CBEE2DF30B6A'
	,
	'<html>
<head>
<title>A Responsive Email Template</title>

<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<meta http-equiv="X-UA-Compatible" content="IE=edge" />
<style type="text/css">
    /* CLIENT-SPECIFIC STYLES */
    body, table, td, a{-webkit-text-size-adjust: 100%; -ms-text-size-adjust: 100%;} /* Prevent WebKit and Windows mobile changing default text sizes */
    table, td{mso-table-lspace: 0pt; mso-table-rspace: 0pt;} /* Remove spacing between tables in Outlook 2007 and up */
    img{-ms-interpolation-mode: bicubic;} /* Allow smoother rendering of resized image in Internet Explorer */

    /* RESET STYLES */
    img{border: 0; height: auto; line-height: 100%; outline: none; text-decoration: none;}
    table{border-collapse: collapse !important;}
    body{height: 100% !important; margin: 0 !important; padding: 0 !important; width: 100% !important;}

    /* iOS BLUE LINKS */
    a[x-apple-data-detectors] {
        color: inherit !important;
        text-decoration: none !important;
        font-size: inherit !important;
        font-family: inherit !important;
        font-weight: inherit !important;
        line-height: inherit !important;
    }

    /* MOBILE STYLES */
    @media screen and (max-width: 525px) {

        /* ALLOWS FOR FLUID TABLES */
        .wrapper {
          width: 100% !important;
        	max-width: 100% !important;
        }

        /* ADJUSTS LAYOUT OF LOGO IMAGE */
        .logo img {
          margin: 0 auto !important;
        }

        /* USE THESE CLASSES TO HIDE CONTENT ON MOBILE */
        .mobile-hide {
          display: none !important;
        }

        .img-max {
          max-width: 100% !important;
          width: 100% !important;
          height: auto !important;
        }

        /* FULL-WIDTH TABLES */
        .responsive-table {
          width: 100% !important;
        }

        /* UTILITY CLASSES FOR ADJUSTING PADDING ON MOBILE */
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

        /* ADJUST BUTTONS ON MOBILE */
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

    /* ANDROID CENTER FIX */
    div[style*="margin: 16px 0;"] { margin: 0 !important; }
</style>
<!--[if gte mso 12]>
<style type="text/css">
.mso-right {
	padding-left: 20px;
}
</style>
<![endif]-->
</head>
<body style="margin: 0 !important; padding: 0 !important;">

<!-- HEADER -->
<table border="0" cellpadding="0" cellspacing="0" width="100%">
    <tr>
        <td bgcolor="#FFFFFF" align="center" style="padding: 15px 15px 15px 15px;" class="section-padding">
            <!--[if (gte mso 9)|(IE)]>
            <table align="center" border="0" cellspacing="0" cellpadding="0" width="500">
            <tr>
            <td align="center" valign="top" width="500">
            <![endif]-->
            <table border="0" cellpadding="0" cellspacing="0" width="100%" class="responsive-table">
                <tr>
                    <td style="font-family: Helvetica, Arial, sans-serif;">
					    <div class="structure-dropzone">
					
                            <div class="dropzone">

						    </div>
                        </div>
					
					</td>
                </tr>
            </table>
            <!--[if (gte mso 9)|(IE)]>
            </td>
            </tr>
            </table>
            <![endif]-->
        </td>
    </tr>
</table>
</body>
</html>'
	);

-- JE: Add default communication template
INSERT INTO [CommunicationTemplate]
([Name], [Description], [Guid], [Message])
VALUES
('Default', 'The default email template for sending emails using Rock''s Communications Wizard.', '88B7DF18-9C30-4BAC-8CA2-5AD253D57E4D', '<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">


<head>


  <meta http-equiv="Content-Type" content="text/html; charset=utf-8">
  <meta name="viewport" content="width=device-width">
  <title>My Basic Email Template Subject</title>
  <!-- <style> -->
</head>


<body style="-moz-box-sizing: border-box; -ms-text-size-adjust: 100%; -webkit-box-sizing: border-box; -webkit-text-size-adjust: 100%; Margin: 0; background: #f3f3f3 !important; box-sizing: border-box; color: #0a0a0a; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.3; margin: 0; min-width: 100%; padding: 0; text-align: left; width: 100% !important;">
  <style>
    @media only screen {
      html {
        min-height: 100%;
        background: #f3f3f3;
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
      table.menu[align="center"] {
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
  <span class="preheader" style="color: #f3f3f3; display: none !important; font-size: 1px; line-height: 1px; max-height: 0px; max-width: 0px; mso-hide: all !important; opacity: 0; overflow: hidden; visibility: hidden;"></span>
  <table class="body" style="Margin: 0; background: #f3f3f3 !important; border-collapse: collapse; border-spacing: 0; color: #0a0a0a; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; height: 100%; line-height: 1.3; margin: 0; padding: 0; text-align: left; vertical-align: top; width: 100%;">
    <tbody>
      <tr style="padding: 0; text-align: left; vertical-align: top;">
        <td class="center" align="center" valign="top" style="-moz-hyphens: auto; -webkit-hyphens: auto; Margin: 0; border-collapse: collapse !important; color: #0a0a0a; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; hyphens: auto; line-height: 1.3; margin: 0; padding: 0; text-align: left; vertical-align: top; word-wrap: break-word;">
          <center data-parsed="" style="min-width: 580px; width: 100%;">


            <table align="center" class="wrapper header float-center" style="Margin: 0 auto; background: #5e5e5e; border-collapse: collapse; border-spacing: 0; float: none; margin: 0 auto; padding: 0; text-align: center; vertical-align: top; width: 100%;">
              <tbody>
                <tr style="padding: 0; text-align: left; vertical-align: top;">
                  <td class="wrapper-inner" style="-moz-hyphens: auto; -webkit-hyphens: auto; Margin: 0; border-collapse: collapse !important; color: #0a0a0a; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; hyphens: auto; line-height: 1.3; margin: 0; padding: 15px; text-align: left; vertical-align: top; word-wrap: break-word;">
                    <table align="center" class="container" style="Margin: 0 auto; background: transparent; border-collapse: collapse; border-spacing: 0; margin: 0 auto; padding: 0; text-align: inherit; vertical-align: top; width: 580px;">
                      <tbody>
                        <tr style="padding: 0; text-align: left; vertical-align: top;">
                          <td style="-moz-hyphens: auto; -webkit-hyphens: auto; Margin: 0; border-collapse: collapse !important; color: #0a0a0a; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; hyphens: auto; line-height: 1.3; margin: 0; padding: 0; text-align: left; vertical-align: top; word-wrap: break-word;">
                            <img src="/assets/images/email-header.jpg" style="margin: 0 auto" />
                          </td>
                        </tr>
                      </tbody>
                    </table>
                  </td>
                </tr>
              </tbody>
            </table>


            <table align="center" class="container float-center" style="Margin: 0 auto; background: #fefefe; border-collapse: collapse; border-spacing: 0; float: none; margin: 0 auto; padding: 0; text-align: center; vertical-align: top; width: 580px;">
              <tbody>
                <tr style="padding: 0; text-align: left; vertical-align: top;">
                  <td style="-moz-hyphens: auto; -webkit-hyphens: auto; Margin: 0; border-collapse: collapse !important; color: #0a0a0a; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; hyphens: auto; line-height: 1.3; margin: 0; padding: 0; text-align: left; vertical-align: top; word-wrap: break-word;">


                    <table class="spacer" style="border-collapse: collapse; border-spacing: 0; padding: 0; text-align: left; vertical-align: top; width: 100%;">
                      <tbody>
                        <tr style="padding: 0; text-align: left; vertical-align: top;">
                          <td height="16px" style="-moz-hyphens: auto; -webkit-hyphens: auto; Margin: 0; border-collapse: collapse !important; color: #0a0a0a; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; hyphens: auto; line-height: 16px; margin: 0; mso-line-height-rule: exactly; padding: 0; text-align: left; vertical-align: top; word-wrap: break-word;">&nbsp;</td>
                        </tr>
                      </tbody>
                    </table>


                    <table class="row" style="border-collapse: collapse; border-spacing: 0; display: table; padding: 0; position: relative; text-align: left; vertical-align: top; width: 100%;">
                      <tbody>
                        <tr style="padding: 0; text-align: left; vertical-align: top;">
                          <th class="small-12 large-12 columns first last" style="Margin: 0 auto; color: #0a0a0a; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.3; margin: 0 auto; padding: 0; padding-bottom: 16px; padding-left: 16px; padding-right: 16px; text-align: left; width: 564px;">
							<div class="structure-dropzone">
								<div class="dropzone">
									<div class="component component-text" data-content="<h1>Email Title</h1>" data-state="component">
										<h1>Email Title</h1>
										<p>
											Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aenean suscipit, arcu hendrerit mattis accumsan, quam sem tristique dolor, eget euismod enim tortor sit amet augue. Interdum et malesuada fames ac ante ipsum primis in faucibus. Donec finibus tortor lorem, at malesuada justo feugiat ut. Nam ac pharetra eros, nec feugiat nisl.
										</p>
									</div>
								</div>
							</div>
						  </th>
                        </tr>
                      </tbody>
                    </table>
                    <table class="wrapper secondary" align="center" style="background: #f3f3f3; border-collapse: collapse; border-spacing: 0; padding: 0; text-align: left; vertical-align: top; width: 100%;">
                      <tbody>
                        <tr style="padding: 0; text-align: left; vertical-align: top;">
                          <td class="wrapper-inner" style="-moz-hyphens: auto; -webkit-hyphens: auto; Margin: 0; border-collapse: collapse !important; color: #0a0a0a; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; hyphens: auto; line-height: 1.3; margin: 0; padding: 0; text-align: left; vertical-align: top; word-wrap: break-word;">


                            <table class="spacer" style="border-collapse: collapse; border-spacing: 0; padding: 0; text-align: left; vertical-align: top; width: 100%;">
                              <tbody>
                                <tr style="padding: 0; text-align: left; vertical-align: top;">
                                  <td height="16px" style="-moz-hyphens: auto; -webkit-hyphens: auto; Margin: 0; border-collapse: collapse !important; color: #0a0a0a; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; hyphens: auto; line-height: 16px; margin: 0; mso-line-height-rule: exactly; padding: 0; text-align: left; vertical-align: top; word-wrap: break-word;">&nbsp;</td>
                                </tr>
                              </tbody>
                            </table>


                            <table class="row" style="border-collapse: collapse; border-spacing: 0; display: table; padding: 0; position: relative; text-align: left; vertical-align: top; width: 100%;">
                              <tbody>
                                <tr style="padding: 0; text-align: left; vertical-align: top;">
                                  <th class="small-12 large-6 columns first" style="Margin: 0 auto; color: #0a0a0a; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.3; margin: 0 auto; padding: 0; padding-bottom: 16px; padding-left: 16px; padding-right: 8px; text-align: left; width: 274px;">
                                    <table style="border-collapse: collapse; border-spacing: 0; padding: 0; text-align: left; vertical-align: top; width: 100%;">
                                      <tbody>
                                        <tr style="padding: 0; text-align: left; vertical-align: top;">
                                          <th style="Margin: 0; color: #0a0a0a; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.3; margin: 0; padding: 0; text-align: left;">
                                            &nbsp;
                                          </th>
                                        </tr>
                                      </tbody>
                                    </table>
                                  </th>
                                  <th class="small-12 large-6 columns last" style="Margin: 0 auto; color: #0a0a0a; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.3; margin: 0 auto; padding: 0; padding-bottom: 16px; padding-left: 8px; padding-right: 16px; text-align: left; width: 274px;">
                                    <table style="border-collapse: collapse; border-spacing: 0; padding: 0; text-align: left; vertical-align: top; width: 100%;">
                                      <tbody>
                                        <tr style="padding: 0; text-align: left; vertical-align: top;">
                                          <th style="Margin: 0; color: #0a0a0a; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.3; margin: 0; padding: 0; text-align: left;">
                                            <h5 style="Margin: 0; Margin-bottom: 10px; color: inherit; font-family: Helvetica, Arial, sans-serif; font-size: 20px; font-weight: normal; line-height: 1.3; margin: 0; margin-bottom: 10px; padding: 0; text-align: left; word-wrap: normal;">Contact Info:</h5>
                                            <p style="Margin: 0; Margin-bottom: 10px; color: #0a0a0a; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.3; margin: 0; margin-bottom: 10px; padding: 0; text-align: left;">Website: <a href="{{ ''Global'' | Attribute:''OrganizationWebsite'' }}" style="Margin: 0; color: #2199e8; font-family: Helvetica, Arial, sans-serif; font-weight: normal; line-height: 1.3; margin: 0; padding: 0; text-align: left; text-decoration: none;">{{ ''Global'' | Attribute:''OrganizationWebsite'' }}</a></p>
                                            <p style="Margin: 0; Margin-bottom: 10px; color: #0a0a0a; font-family: Helvetica, Arial, sans-serif; font-size: 16px; font-weight: normal; line-height: 1.3; margin: 0; margin-bottom: 10px; padding: 0; text-align: left;">Email: <a href="mailto:{{ ''Global'' | Attribute:''OrganizationEmail'' }}" style="Margin: 0; color: #2199e8; font-family: Helvetica, Arial, sans-serif; font-weight: normal; line-height: 1.3; margin: 0; padding: 0; text-align: left; text-decoration: none;">{{ ''Global'' | Attribute:''OrganizationEmail'' }}</a></p>
                                          </th>
                                        </tr>
                                      </tbody>
                                    </table>
                                  </th>
                                </tr>
                              </tbody>
                            </table>
							<div style="width: 100%; text-align: center; font-size: 11px; font-color: #6f6f6f;">[[ UnsubscribeOption ]]</div>
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
  <div style="display:none; white-space:nowrap; font:15px courier; line-height:0;"> &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; &amp;nbsp; </div>






</body>


</html>');

