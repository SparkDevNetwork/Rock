using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Communication;
using System.Diagnostics;
using System.Globalization;



namespace RockWeb.Plugins.org_newpointe.Reporting
{

    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName("Attendance Email")]
    [Category("NewPointe Reporting")]
    [Description("Email attendance data.")]
    [EmailField("From Email","Address the email will be sent from",true)]


public partial class AttendanceEmail : Rock.Web.UI.RockBlock

{
    public Dictionary<string, decimal> dictionary = new Dictionary<string, decimal>();
    public string emailContent;
    public string currentDateString;
    public string currentSundayDate;
    public int weeknumber;
    public int currentYear;
    public int lastYear;
    public string dateThisYear;
    public string dateLastYear;
    public string date2YrsAgo;
    public string attendancePercentage;

    //Get the start day of the week in ISO8601, based on year and week number, then get the Sunday of the previous week.
    public static DateTime FirstDateOfWeekISO8601(int year, int weekOfYear)
    {
        DateTime jan1 = new DateTime(year, 1, 1);
        int daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;

        DateTime firstThursday = jan1.AddDays(daysOffset);
        var cal = CultureInfo.CurrentCulture.Calendar;
        int firstWeek = cal.GetWeekOfYear(firstThursday, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

        var weekNum = weekOfYear;
        if (firstWeek <= 1)
        {
            weekNum -= 1;
        }
        var result = firstThursday.AddDays(weekNum * 7);
        return result.AddDays(-4);
    }


    protected void Page_Load(object sender, EventArgs e)
    {
        
        //Get Date Information
        DateTime currentDate = DateTime.Now;
        var currentCulture = CultureInfo.CurrentCulture;
        currentYear = currentDate.Year;
        lastYear = currentDate.AddYears(-1).Year;
        weeknumber = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(currentDate.DayOfYear) / 7));
        currentDateString = DateTime.Now.ToShortDateString();
        dateThisYear = FirstDateOfWeekISO8601(currentYear, weeknumber+1).ToShortDateString(); ;
        dateLastYear = FirstDateOfWeekISO8601(currentYear - 1, weeknumber+1).ToShortDateString();
        date2YrsAgo = FirstDateOfWeekISO8601(currentYear - 2, weeknumber+1).ToShortDateString();

        currentSundayDate = dateThisYear;


        //Generate Attendance Data
        GetAttendance();
        

        if (!IsPostBack)
        {
            string email = PageParameter("email");

            if (!String.IsNullOrWhiteSpace(email))
            {
                var rockContext = new RockContext();
                string emailSubject = string.Format("[Metrics] Attendance for Week {0} ({1})", weeknumber, currentSundayDate);
                SendEmail(email, GetAttributeValue("FromEmail"), emailSubject, emailContent, rockContext);
            }

        }

         }

    protected void btnSend_Click(object sender, EventArgs e)
    {
       //Send the email
        var rockContext = new RockContext();
        string emailSubject = string.Format("[Metrics] Attendance for Week {0} ({1})", weeknumber, currentSundayDate);
        SendEmail("bwitting@newpointe.org", GetAttributeValue("FromEmail"), emailSubject, emailContent, rockContext);
        
    }


    private void SendEmail( string recipient, string from, string subject, string body, RockContext rockContext )
        {
            var recipients = new List<string>();
            recipients.Add( recipient );
             
            var mediumData = new Dictionary<string, string>();
            mediumData.Add( "From", from );
            mediumData.Add( "Subject", subject );
            mediumData.Add( "Body", body );

            var mediumEntity = EntityTypeCache.Read( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid(), rockContext );
            if ( mediumEntity != null )
            {
                var medium = MediumContainer.GetComponent( mediumEntity.Name );
                if ( medium != null && medium.IsActive )
                {
                    var transport = medium.Transport;
                    if ( transport != null && transport.IsActive )
                    {
                        var appRoot = GlobalAttributesCache.Read( rockContext ).GetValue( "InternalApplicationRoot" );
                        transport.Send( mediumData, recipients, appRoot, string.Empty );
                    }
                }
            }
        }


      //get attendance metrics from database
      protected void GetAttendance()
      {

          var rockContext = new RockContext();

          var metricTest = rockContext.MetricValues.SqlQuery("SET DATEFIRST 1; SELECT * FROM dbo.MetricValue WHERE DATEPART(week, MetricValueDateTime) = DATEPART(week, GETDATE()) - 1 AND (MetricId = 2 OR MetricId = 3 OR MetricId = 4 OR MetricId = 5) ORDER BY MetricValueDateTime DESC;").ToList();

            //Add key/values to dictionary in case they aren't in SQL lookup
           dictionary.Add("ORG2016", 0);
           dictionary.Add("ORG2015",0);
          dictionary.Add("ORG2014", 0);
          dictionary.Add("ORG2013", 0);
          dictionary.Add("ORG2012", 0);
          dictionary.Add("ORG2011", 0);
            dictionary.Add("12016", 0);
            dictionary.Add("12015", 0);
          dictionary.Add("12014", 0);
          dictionary.Add("12013", 0);
            dictionary.Add("22016", 0);
            dictionary.Add("22015", 0);
          dictionary.Add("22014", 0);
          dictionary.Add("22013", 0);
            dictionary.Add("32016", 0);
            dictionary.Add("32015", 0);
          dictionary.Add("32014", 0);
          dictionary.Add("32013", 0);
            dictionary.Add("42016", 0);
            dictionary.Add("42015", 0);
          dictionary.Add("42014", 0);
          dictionary.Add("42013", 0);
            dictionary.Add("52016", 0);
            dictionary.Add("52015", 0);
          dictionary.Add("52014", 0);
          dictionary.Add("52013", 0);
            dictionary.Add("62016", 0);
            dictionary.Add("62015", 0);
          dictionary.Add("62014", 0);
          dictionary.Add("62013", 0);
            dictionary.Add("72016", 0);
            dictionary.Add("72015", 0);
          dictionary.Add("72014", 0);
          dictionary.Add("72013", 0);
            dictionary.Add("82016", 0);
            dictionary.Add("82015", 0);
          dictionary.Add("82014", 0);
          dictionary.Add("82013", 0);


          //loop through results and populate dictionary
          foreach (var metricsvar in metricTest)
          {
              DateTime theDate = metricsvar.MetricValueDateTime ?? DateTime.MinValue;
              int year = theDate.Year;
                      
                      //add data to dictionary
/*                      if (!dictionary.ContainsKey(metricsvar.EntityId.ToString() + year.ToString()))
                      {
                          dictionary.Add(metricsvar.EntityId.ToString() + year.ToString(), metricsvar.YValue ?? 0);
                      }
                      else
                      {
                          dictionary[metricsvar.EntityId.ToString() + year.ToString()] = dictionary[metricsvar.EntityId.ToString() + year.ToString()] + metricsvar.YValue ?? 0;
                      }
                      
                      //figure out yearly totals
                      if (metricsvar.EntityId != 8) {
                      dictionary["ORG" + year.ToString()] += metricsvar.YValue ?? 0 ;
                      }
                      */
          }

          //find the percent increase/decrease

          decimal difference = dictionary["ORG2016"] / dictionary["ORG2015"];
          decimal percentDifference = (difference - 1) * 100;

          if (percentDifference >= 0)
          {
              attendancePercentage = Convert.ToInt32(percentDifference) + "% increase";
          }
          else
          {
              attendancePercentage = Convert.ToInt32(percentDifference) * -1 + "% decrease";
          }



          emailContent = GenerateHTML(dictionary);

         
      }


      protected string GenerateHTML(Dictionary<string, decimal> attendanceDictionary)
      {
          string emailHeader = @"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"">
                <html xmlns=""http://www.w3.org/1999/xhtml"">
                <head>
                <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"" />
                <meta name=""viewport"" content=""width=device-width"" />
                <title>NewPointe Metrics</title>
                </head>
                <body>

                <style type=""text/css"">

	                #outlook a{padding:0;}
	                body{width:100% !important;-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%;margin:0;padding:0;}
	                .ExternalClass{width:100%;}
	                .ExternalClass,.ExternalClass p,.ExternalClass span,.ExternalClass font,.ExternalClass td,.ExternalClass div{line-height:100%;}
	                .bodytbl{margin:0;padding:0;width:100% !important;}
	                img{outline:none;text-decoration:none;-ms-interpolation-mode:bicubic;image-rendering:optimizeQuality;display:block;max-width:100%;}
	                a img{border:none;}
	                p{margin:1em 0;}
	
	                table{border-collapse:collapse;mso-table-lspace:0pt;mso-table-rspace:0pt;}
	                table td{border-collapse:collapse;}
	                .o-fix table,.o-fix td{mso-table-lspace:0pt;mso-table-rspace:0pt;}
	
	                body,.bodytbl{background-color:#FAFAFA/*Background Color*/;}
	
	                table{color:#787878/*Text Color*/;}
	                td,p{color:#787878;}
	                .h1{color:#353535/*Headings*/;}
	                .h2{color:#353535;}
	                .quote{color:#AAAAAA;}
	                .invert,.invert h1,.invert td,.invert p{background-color:#353535;color:#FAFAFA !important;}
	
	                .wrap.header{}
	                .wrap.body{}
	                .wrap.body-i{background-color:#353535;}
	                .wrap.footer{}
	                .padd{width:20px;}
	
	                a{color:#00E18E;}
	                a:link,a:visited,a:hover{color:#80BA42/*Contrast*/;}
	                .btn,.btn div,.btn a,td.label a{color:#FFFFFF/*Contrast Link Color*/;}
	                .btn a,.btn a img,td.label{background:#80BA42/*Button Color*/;}
	                .invert .btn a,.invert .btn a img{background:none;}
	
	                h1,h2,h3,h4,h5,h6{color:#353535;font-family:Helvetica,Arial,sans-serif;font-weight:bold;}
	                h1{font-size:24px;letter-spacing:-2px;margin-bottom:6px;margin-top:6px;line-height:24px;}
	                h2{font-size:20px;margin-bottom:12px;margin-top:2px;line-height:24px;}
	                h3{font-size:18px;margin-bottom:12px;margin-top:2px;line-height:24px;}
	                h4{font-size:16px;}
	                h5{font-size:14px;}
	                h6{font-size:12px;}
	                h1 a,h2 a,h3 a,h4 a,h5 a,h6 a{color:#00E18E;}
	                h1 a:active,h2 a:active,h3 a:active,h4 a:active,h5 a:active,h6 a:active{color:#80BA42 !important;}
	                h1 a:visited,h2 a:visited,h3 a:visited,h4 a:visited,h5 a:visited,h6 a:visited{color:#80BA42 !important;}
	                .h1{font-family:Helvetica,Arial,sans-serif;font-size:55px;font-weight:bold;line-height:40px !important;letter-spacing:-2px;}
	                .h2{font-family:Helvetica,Arial,sans-serif;font-size:19px;font-weight:bold;letter-spacing:-1px;line-height:24px;}
	                .h3{font-family:Helvetica,Arial,sans-serif;font-size:17px;letter-spacing:-1px;line-height:24px;}

	                body{margin:0;padding:0;}
	                .bodytbl{margin:0;padding:0;-webkit-text-size-adjust:none;}
	                .line{border-bottom:1px solid #AAAAAA/*Separator*/;}
	                table{font-family:Helvetica,Arial,sans-serif;font-size:12px;}
	                td,p{line-height:20px;}
	                ul,ol{margin-top:20px;margin-bottom:20px;}
	                li{line-height:20px;} 
	                td,tr{padding:0;}
	                .quote{font-family:Helvetica,Arial,sans-serif;font-size:24px;letter-spacing:0;margin-bottom:6px;margin-top:6px;line-height:24px;font-style:italic;}
	                .small{font-size:10px;color:#787878;line-height:15px;text-transform:uppercase;word-spacing:-1px;margin-bottom:4px;margin-top:6px;}
	                .plan td{border-right:1px solid #EBEBEB/*Lines*/;border-bottom:1px solid #EBEBEB;text-align:center;}
	                .plan td.last{border-right:0;}
	                .plan th{text-align:center;border-bottom:1px solid #EBEBEB;}
	                a{text-decoration:none;padding:2px 0px;}
	                td.label a{margin:2px 4px;line-height:1em;text-decoration:none;display:block;mso-padding-alt:4pt 4pt 4pt 4pt;}
	                td.label{mso-padding-alt:0 4px 4px 4px;}
	                .btn{margin-top:10px;display:block;}
	                .btn a{display:inline-block;padding:0;line-height:0.5em;}
	                .btn img,.social img{display:inline;margin:0;}
	                .social .btn a,.social .btn a img{background:none;}
	                .right{text-align:right;}
	
	                table.textbutton td{background:#80BA42;padding:0px 14px;color:#FFF;display:block;height:24px;vertical-align:top;}
	                table.textbutton a{color:#FFF;font-size:13px;font-weight:100;line-height:24px;width:100%;display:inline-block;}
	
	                div.preheader{line-height:0px;font-size:0px;height:0px;display:none !important;visibility:hidden;text-indent:-9999px;}
	
	                @media only screen and (max-device-width: 480px) {
		                body{-webkit-text-size-adjust:120% !important;-ms-text-size-adjust:120% !important;}
		                table[class=bodytbl] .wrap{width:460px !important;}
		                table[class=bodytbl] .wrap .padd{width:10px !important;}
		                table[class=bodytbl] .wrap table{width:100% !important;}
		                table[class=bodytbl] .wrap img {max-width:100% !important;height:auto !important;}
		                table[class=bodytbl] .wrap .m-padd {padding:0 20px !important;}
		                table[class=bodytbl] .wrap .m-w-auto{;width:auto !important;}
		                table[class=bodytbl] .wrap .m-100{width:100% !important;max-width:460px !important;}
		                table[class=bodytbl] .wrap .m-l{text-align:left !important;}
		                table[class=bodytbl] .wrap .h div{letter-spacing:-1px !important;font-size:18px !important;}
		                table[class=bodytbl] .m-0{width:0;display:none;}
		                table[class=bodytbl] .m-b{display:block;width:100% !important;margin-bottom:20px !important;}
		                table[class=bodytbl] .m-1-2{max-width:264px !important;}
		                table[class=bodytbl] .m-1-3{max-width:168px !important;}
		                table[class=bodytbl] .m-1-4{max-width:120px !important;}
		                table[class=bodytbl] .wrap .m-mac{width:340px !important;max-width:340px !important;margin-left:38px;}
		                table[class=bodytbl] .wrap .m-mac-t img{width:339px !important;}
		                table[class=bodytbl] .wrap .m-mac-h img{height:192px !important;width:15px !important;}
		                table[class=bodytbl] .wrap .m-mac-i img{width:309px !important;}
		                table[class=bodytbl] .wrap .m-mac-b img{max-width:406px !important;margin-left:4px;}
	                }
	
	                @media only screen and (max-device-width: 320px) {
		                table[class=bodytbl] .wrap{width:310px !important;}
		                table[class=bodytbl] .wrap .m-100{max-width:310px !important;}
		                table[class=bodytbl] .wrap .m-mac{width:239px !important;max-width:239px !important;margin-left:23px;}
		                table[class=bodytbl] .wrap .m-mac-t img{width:238px !important;}
		                table[class=bodytbl] .wrap .m-mac-h img{height:134px !important;width:11px !important;}
		                table[class=bodytbl] .wrap .m-mac-i img{width:216px !important;}
		                table[class=bodytbl] .wrap .m-mac-b img{max-width:285px !important;margin-left:-1px;}
	                }
	
                </style>";


          string emailBody = @"<table class=""bodytbl"" width=""100%"" cellspacing=""0"" cellpadding=""0"">
                    <tr>
	                    <td align=""center"">
	
		                    <table width=""600"" cellspacing=""0"" cellpadding=""0"" class=""wrap"">
		                    <tr height=""20"">
			                    <td align=""left"" valign=""bottom"">
				                    <div class=""preheader"">NewPointe Metrics</div> <div class=""small""><span>NewPointe Metrics </span><a name=""top""></a></div>
			                    </td>
		                    </tr>
		                    </table>
		
		                    <table width=""600"" cellspacing=""0"" cellpadding=""0"" class=""wrap line""><tr><td height=""19"">&nbsp;</td></tr></table>
		                    <table width=""600"" cellspacing=""0"" cellpadding=""0"" class=""wrap""><tr><td>
			                    <table cellspacing=""0"" cellpadding=""0"" align=""right"" class=""m-w-auto"">
			                    <tr>
				                    <td height=""20"" valign=""middle"" class=""label"">
				                    <span> <a href=""#"">{0}</a> </span>
				                    </td>
			                    </tr>
			                    </table>
		                    </td></tr></table>	
		
		                    <table width=""600"" cellspacing=""0"" cellpadding=""0"" class=""wrap"">
		                    <tr height=""80"">
			                    <td align=""left"" valign=""bottom"">
				                    <table width=""100%"" cellspacing=""0"" cellpadding=""0"">
				                    <tr>
					                    <td rowspan=""1"" align=""left"" valign=""bottom"" class=""h1"">
					                    <span>Attendance</span>
					                    </td>
				                    </tr>
				                    <tr>
					                    <td align=""left"" valign=""top"" class=""h2"">
					                    <span>Weekend Service Attendance Report</span>
                                                            <div class=""small""><span>**This is an automated email based on reported attendance numbers**</span></div>
					                    </td>
				                    </tr>
				                    </table>
			                    </td>
		                    </tr>
		                    </table>
		
		                    <table width=""600"" cellspacing=""0"" cellpadding=""0"" class=""wrap line""><tr><td height=""19"">&nbsp;</td></tr></table>
		                    <table width=""600"" cellspacing=""0"" cellpadding=""0"" class=""wrap""><tr><td height=""40"">&nbsp;</td></tr></table>";


          string emailAttendance = @"<!-- Plans start  -->
	
		<table width=""600"" cellpadding=""0"" cellspacing=""0"" class=""wrap"">
		<tr>
			<td valign=""top"">
				<table width=""100%"" cellpadding=""0"" cellspacing=""0"">
				<tr>
		<!-- CONTENT start -->
					<td valign=""top"" align=""left"">
						<h1><span>Attendance for Week {0}</span></h1>
						<table width=""600"" cellpadding=""0"" cellspacing=""0"" class=""plan"">
						
						<tr valign=""middle""><th height=""40"">&nbsp;</th><th><span><strong>2016</strong><br>({1})</span></th><th><span>2015<br>({2})</span></th><th><span>2014<br>({3})</span></th><th><span>Trend</span></th></tr>
						
						<tr valign=""middle""><td height=""40"" align=""left""><span><strong>All Org</strong></span></td><td><span><strong>{4}</strong></span></td><td><span><strong>{5}</strong></span></td><td><span><strong>{6}</strong></span></td><td class=""last"" align=""center"" style=""text-align: center; width:20%;""><span><strong><img src=""https://metrics.newpointe.org/email/chart-ORG?week=??"" /></strong></span></td></tr>
						
						<tr valign=""middle""><td height=""40"" align=""left""><span>Canton</span></td><td><span>{7}</span></td><td><span>{8}</span></td><td><span>{9}<span></td><td class=""last"" align=""center"" style=""text-align: center;""><span><strong><img src=""https://metrics.newpointe.org/email/chart-CAN?week=??"" /></strong></span></td></tr>
						
						<tr valign=""middle""><td height=""40"" align=""left""><span>Coshocton</span></td><td><span>{10}</span></td><td><span>{11}</span></td><td><span>{12}</span></td><td class=""last"" align=""center"" style=""text-align: center;""><span><strong><img src=""https://metrics.newpointe.org/email/chart-COS?week=??"" /></strong></span></td></tr>
						
                        <tr valign=""middle""><td height=""40"" align=""left""><span>Dover</span></td><td><span>{13}</span></td><td><span>{14}</span></td><td><span>{15}</span></td><td class=""last"" align=""center"" style=""text-align: center;""><span><strong><img src=""https://metrics.newpointe.org/email/chart-DOV?week=??"" /></strong></span></td></tr>
                                                
                        <tr valign=""middle""><td height=""40"" align=""left""><span>Millersburg</span></td><td><span>{16}</span></td><td><span>{17}</span></td><td><span>{18}</span></td><td class=""last"" align=""center"" style=""text-align: center;""><span><strong><img src=""https://metrics.newpointe.org/email/chart-MIL?week=??"" /></strong></span></td></tr>

                        <tr valign=""middle""><td height=""40"" align=""left""><span>Wooster</span></td><td><span>{19}</span></td><td><span>{20}</span></td><td><span>{21}<span></td><td class=""last"" align=""center"" style=""text-align: center;""><span><strong><img src=""https://metrics.newpointe.org/email/chart-CAN?week=??"" /></strong></span></td></tr>
                                                    
                        <tr valign=""middle""><td height=""40"" align=""left""><span>Online</span></td><td><span>{22}</span></td><td><span>{23}</span></td><td><span>{24}</span></td><td class=""last"" align=""center"" style=""text-align: center;""><span><strong><img src=""https://metrics.newpointe.org/email/chart-WEB?week=??"" /></strong></span></td></tr>
                                                    
						</table>
					</td>
				</tr>
				<tr>
					<td valign=""top"" align=""right"">
						<div class=""label"">
							<a href=""https://rock.newpointe.org/metrics/attendanceDashboard"">See More Info on the Dashboard</a>
                            <br /><small>(Online numbers not included in total)</small>
						</div>
					</td>
				</tr>
				</table>
                            <br><br>
                            <h3><span>Overall, there was a {25} in attendance from last year.</span></h3>       
			</td>
		</tr>
		</table>

";


          string emailFooter = @"<!-- Footer start -->
		        <table width=""600"" cellspacing=""0"" cellpadding=""0"" class=""wrap line""><tr><td height=""39"">&nbsp;</td></tr></table>
		        <table width=""600"" cellpadding=""0"" cellspacing=""0"" class=""wrap"">
		        <tr>
			        <td valign=""top"" align=""center"">
				        <table width=""100%"" cellpadding=""0"" cellspacing=""0"" class=""o-fix"">
				        <tr>
		        <!-- CONTENT start -->
					        <td valign=""top"" align=""left"">
						        <table width=""390"" cellpadding=""0"" cellspacing=""0"" align=""left"">
						        <tr>
							        <td class=""small"" align=""left"" valign=""top"">
								        <div><span>Questions?  Please contact Bronson at <a href=""mailto:bwitting@newpointe.org"">bwitting@newpointe.org</a>.</span></div>
								        <div><span>&copy; 2016 NewPointe Community Church</span></div>
							        </td>
						        </tr>
						        </table>
						        <table width=""190"" cellpadding=""0"" cellspacing=""0"" align=""right"">
						        <tr>
							        <td class=""small social"" align=""right"" valign=""top"">
								        <div>
									        <img src=""http://metrics.newpointe.org/images/emailfooter.png"" />
								        </div>
							        </td>
						        </tr>
						        </table>
					        </td>
		        <!-- CONTENT end -->
				        </tr>
				        <tr class=""m-0""><td height=""24"">&nbsp;</td></tr>
				        </table>
			        </td>
		        </tr>
		        </table>
        <!-- Footer end -->


	        </td>
        </tr>
        </table>
        </body>
        </html>";


          string emailBodyMerged = string.Format(emailBody, currentDateString);
          string emailAttendanceMerged = string.Format(emailAttendance, weeknumber, currentSundayDate, dateLastYear, date2YrsAgo, Convert.ToInt32(dictionary["ORG2016"]), Convert.ToInt32(dictionary["ORG2015"]), Convert.ToInt32(dictionary["ORG2014"]), Convert.ToInt32(dictionary["22016"]), Convert.ToInt32(dictionary["22015"]), Convert.ToInt32(dictionary["22014"]), Convert.ToInt32(dictionary["32016"]), Convert.ToInt32(dictionary["32015"]), Convert.ToInt32(dictionary["32014"]), Convert.ToInt32(dictionary["12016"]), Convert.ToInt32(dictionary["12015"]), Convert.ToInt32(dictionary["12014"]), Convert.ToInt32(dictionary["42016"]), Convert.ToInt32(dictionary["42015"]), Convert.ToInt32(dictionary["42014"]), Convert.ToInt32(dictionary["52016"]), Convert.ToInt32(dictionary["52015"]), Convert.ToInt32(dictionary["52014"]), Convert.ToInt32(dictionary["82016"]), Convert.ToInt32(dictionary["82015"]), Convert.ToInt32(dictionary["82014"]), attendancePercentage);

          return emailHeader + emailBodyMerged + emailAttendanceMerged + emailFooter;
      }


    }
    
}
