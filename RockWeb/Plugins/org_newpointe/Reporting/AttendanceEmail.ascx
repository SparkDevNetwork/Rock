<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AttendanceEmail.ascx.cs" Inherits="RockWeb.Plugins.org_newpointe.Reporting.AttendanceEmail" %>
 

<table class="bodytbl" width="100%" cellspacing="0" cellpadding="0">

<tr>
	<td align="center">
	
		<table width="600" cellspacing="0" cellpadding="0" class="wrap">
		<tr height="20">
			<td align="left" valign="bottom">
				<div class="preheader">NewPointe Metrics</div> <div class="small"><span>NewPointe Metrics</span><a name="top"></a></div>
			</td>
		</tr>
		</table>
		
		<table width="600" cellspacing="0" cellpadding="0" class="wrap line"><tr><td height="19">&nbsp;</td></tr></table>
		<table width="600" cellspacing="0" cellpadding="0" class="wrap"><tr><td>
			<table cellspacing="0" cellpadding="0" align="right" class="m-w-auto">
			<tr>
				<td height="20" valign="middle" class="label">
				<span> <a href="#"><%= currentSundayDate %> </a> </span>
				</td>
			</tr>
			</table>
		</td></tr></table>	
		
		<table width="600" cellspacing="0" cellpadding="0" class="wrap">
		<tr height="80">
			<td align="left" valign="bottom">
				<table width="100%" cellspacing="0" cellpadding="0">
				<tr>
					<td rowspan="1" align="left" valign="bottom" class="h1">
					<span>Attendance</span>
					</td>
				</tr>
				<tr>
					<td align="left" valign="top" class="h2">
					<span>Weekend Service Attendance Report</span>
                                        <div class="small"><span>**This is an automated email based on reported attendance numbers**</span></div>
					</td>
				</tr>
				</table>
			</td>
		</tr>
		</table>
		
		<table width="600" cellspacing="0" cellpadding="0" class="wrap line"><tr><td height="19">&nbsp;</td></tr></table>
		<table width="600" cellspacing="0" cellpadding="0" class="wrap"><tr><td height="40">&nbsp;</td></tr></table>
		

<!-- ☺ header block ends here -->
<!-- Plans start  -->
	
		<table width="600" cellpadding="0" cellspacing="0" class="wrap">
		<tr>
			<td valign="top">
				<table width="100%" cellpadding="0" cellspacing="0">
				<tr>
		<!-- CONTENT start -->
					<td valign="top" align="left">
						<h1><span>Attendance for Week <%= weeknumber %> </span></h1>
						<table width="600" cellpadding="0" cellspacing="0" class="plan">
						
						<tr valign="middle"><th height="40">&nbsp;</th><th><span><strong>2016</strong><br>(<%= currentSundayDate %> )</span></th><th><span>2015<br>(<%= dateLastYear %> )</span></th><th><span>2014<br>(<%= date2YrsAgo %> )</span></th><th><span>Trend</span></th></tr>
						
						<tr valign="middle"><td height="40" align="left"><span><strong>All Org</strong></span></td><td><span><strong><%= Convert.ToInt32(dictionary["ORG2016"]) %> </strong></span></td><td><span><strong><%= Convert.ToInt32(dictionary["ORG2015"]) %> </strong></span></td><td><span><strong><%= Convert.ToInt32(dictionary["ORG2014"]) %> </strong></span></td><td class="last" align="center" style="text-align: center; width:20%;"><span><strong><img src="http://metrics.newpointe.org/email/chart-ORG?week=$$$$$week" /></strong></span></td></tr>
			
						<tr valign="middle"><td height="40" align="left"><span>Canton</span></td><td><span><%= Convert.ToInt32(dictionary["22016"]) %></span></td><td><span><%= Convert.ToInt32(dictionary["22015"]) %></span></td><td><span><%= Convert.ToInt32(dictionary["22014"]) %></span></td><td class="last" align="center" style="text-align: center;"><span><strong><img src="http://metrics.newpointe.org/email/chart-CAN?week=$$$$$week" /></strong></span></td></tr>
						
						<tr valign="middle"><td height="40" align="left"><span>Coshocton</span></td><td><span><%= Convert.ToInt32(dictionary["32016"]) %></span></td><td><span><%= Convert.ToInt32(dictionary["32015"]) %></span></td><td><span><%= Convert.ToInt32(dictionary["32014"]) %></span></td><td class="last" align="center" style="text-align: center;"><span><strong><img src="http://metrics.newpointe.org/email/chart-COS?week=$$$$$week" /></strong></span></td></tr>
						
                        <tr valign="middle"><td height="40" align="left"><span>Dover</span></td><td><span><%= Convert.ToInt32(dictionary["12016"]) %></span></td><td><span><%= Convert.ToInt32(dictionary["12015"]) %></span></td><td><span><%= Convert.ToInt32(dictionary["12014"]) %></span></td><td class="last" align="center" style="text-align: center;"><span><strong><img src="http://metrics.newpointe.org/email/chart-DOV?week=$$$$$week" /></strong></span></td></tr>
                                                
                         <tr valign="middle"><td height="40" align="left"><span>Millersburg</span></td><td><span><%= Convert.ToInt32(dictionary["42016"]) %></span></td><td><span><%= Convert.ToInt32(dictionary["42015"]) %></span></td><td><span><%= Convert.ToInt32(dictionary["42014"]) %></span></td><td class="last" align="center" style="text-align: center;"><span><strong><img src="http://metrics.newpointe.org/email/chart-MIL?week=$$$$$week" /></strong></span></td></tr>
                                                    
                         <tr valign="middle"><td height="40" align="left"><span>Wooster</span></td><td><span><%= Convert.ToInt32(dictionary["52016"]) %></span></td><td><span><%= Convert.ToInt32(dictionary["52015"]) %></span></td><td><span><%= Convert.ToInt32(dictionary["52014"]) %></span></td><td class="last" align="center" style="text-align: center;"><span><strong><img src="http://metrics.newpointe.org/email/chart-MIL?week=$$$$$week" /></strong></span></td></tr>

                         <tr valign="middle"><td height="40" align="left"><span>Online</span></td><td><span><%= Convert.ToInt32(dictionary["82016"]) %></span></td><td><span><%= Convert.ToInt32(dictionary["82015"]) %></span></td><td><span><%= Convert.ToInt32(dictionary["82014"]) %></span></td><td class="last" align="center" style="text-align: center;"><span><strong><img src="http://metrics.newpointe.org/email/chart-WEB?week=$$$$$week" /></strong></span></td></tr>
                                                    
						</table>
					</td>
				</tr>
				<tr>
					<td valign="top" align="right">
						<div class="label">
							<a href="https://rock.newpointe.org/metrics/attendanceDashboard">See More Info on the Dashboard</a>
                            <br /><small>(Online numbers not included in total)</small>
						</div>
					</td>
				</tr>
				</table>
                            <br><br>
                            <h3><span>Overall, there was a <%= attendancePercentage %>  in attendance from last year.</span></h3>
                                
                            
			</td>
		</tr>
		</table>
	
<!-- Plans end   -->


		<!-- Footer start -->


		<table width="600" cellspacing="0" cellpadding="0" class="wrap line"><tr><td height="39">&nbsp;</td></tr></table>
		<table width="600" cellpadding="0" cellspacing="0" class="wrap">
		<tr>
			<td valign="top" align="center">
				<table width="100%" cellpadding="0" cellspacing="0" class="o-fix">
				<tr>
		<!-- CONTENT start -->
					<td valign="top" align="left">
						<table width="390" cellpadding="0" cellspacing="0" align="left">
						<tr>
							<td class="small" align="left" valign="top">
								<div><span>Questions?  Please contact Bronson at <a href="mailto:bwitting@newpointe.org">bwitting@newpointe.org</a>.</span></div>
								<div><span>&copy; 2015 NewPointe Community Church</span></div>
							</td>
						</tr>
						</table>
						<table width="190" cellpadding="0" cellspacing="0" align="right">
						<tr>
							<td class="small social" align="right" valign="top">
								<div>
									<img src="http://metrics.newpointe.org/images/emailfooter.png" />
								</div>
							</td>
						</tr>
						</table>
					</td>
		<!-- CONTENT end -->
				</tr>
				<tr class="m-0"><td height="24">&nbsp;</td></tr>
				</table>
			</td>
		</tr>
		</table>
<!-- Footer end -->


	</td>
</tr>
</table>



    <fieldset> 
        <div class="actions">
            <Rock:BootstrapButton ID="btnSend" runat="server" Text="Send Attendance Email" OnClick="btnSend_Click" CssClass="btn btn-primary" DataLoadingText="&lt;i class='fa fa-refresh fa-spin'&gt;&lt;/i&gt; Sending Emails..." />
        </div>
    </fieldset>   
