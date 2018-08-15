<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PublicMetricsDetail.ascx.cs" Inherits="RockWeb.Plugins.org_newpointe.Metrics.PublicMetricsDetail" %>

<div class="panel panel-block" style="background: #f9f9f9">
     <div class="panel-heading" style="display: block !important;"><h4 class="panel-title"><i class="fa fa-line-chart"></i> NewPointe Metrics - <%= SundayDate %> </h4></div>

<div class="row">
<div class='col-md-6'>
    <div class='col-md-12' style="background: #fff; border-radius: 4px; border: 1px solid #e7e7e7; border-top: 4px solid #8BC540; box-shadow: 2px 2px 0px 1px rgba(0,0,0,0.05)">
        <div class='col-md-12 text-center'>
            <h4>Weekend Attendance<br/><small style="color: #8bc540;">NewPointe Wide</small></h4>
        </div>

        <div class='row'>    
            <div class='col-md-6 text-right'>
                <span style='font-size:50px'> <%= AttendanceLastWeekAll %></span><br /><small style="display:block; margin-top: -15px;">Attendance This Week</small>
            </div>
            <div class='col-md-6 text-center' style="font-size: 88%; padding-top: 13px">
                <br />
                <strong><%= AttendanceLastWeekLastYearAll %></strong> <br/>
                This Week Last Year <br/><br/>
            </div>
        </div>
    </div>
</div>
    
    
    <div class='col-md-6'>
    <div class='col-md-12' style="background: #fff; border-radius: 4px; border: 1px solid #e7e7e7; border-top: 4px solid #8BC540; box-shadow: 2px 2px 0px 1px rgba(0,0,0,0.05)">
        <div class='col-md-12 text-center'>
            <h4>Weekend Attendance<br/><small style="color: #8bc540;"><%= SelectedCampus %> </small></h4>
        </div>

        <div class='row'>    
            <div class='col-md-6 text-right'>
                <span style='font-size:50px'> <%= AttendanceLastWeekCampus %></span><br /><small style="display:block; margin-top: -15px;">Attendance This Week</small>
            </div>
            <div class='col-md-6 text-center' style="font-size: 88%; padding-top: 13px">
                <br/>
                <strong><%= AttendanceLastWeekLastYearCampus %></strong> <br/>
                This Week Last Year <br/><br/>
            </div>
        </div>
    </div>
</div>
</div>

    <br/><br/>
<div class="row">
<div class='col-md-6'>
    <div class='col-md-12' style="background: #fff; border-radius: 4px; border: 1px solid #e7e7e7; border-top: 4px solid #4d4d4d; box-shadow: 2px 2px 0px 1px rgba(0,0,0,0.05)">
        <div class='col-md-12 text-center'>
            <h4>Giving<br/><small style="color: #4d4d4d;">NewPointe Wide</small></h4>
        </div>

        <div class='row'>    
            <div class='col-md-6 text-right'>
                <span style='font-size:50px'><%= GivingLastWeek %></span><br /><small style="display:block; margin-top: -15px;">Giving This Week</small>
            </div>
            <div class='col-md-6 text-center' style="font-size: 88%; padding-top: 13px">
                <strong><%= GivingTwoWeeksAgo %></strong> <br/>
                Giving Last Week<br/>
                <strong><%= GivingYtd %></strong> <br/>
                Giving Year-to-Date<br />
            </div>
        </div>
    </div>
</div>
    
    
<div class='col-md-6'>
    <div class='col-md-12' style="background: #fff; border-radius: 4px; border: 1px solid #e7e7e7; border-top: 4px solid #4d4d4d; box-shadow: 2px 2px 0px 1px rgba(0,0,0,0.05)">
        <div class='col-md-12 text-center'>
            <h4>Giving<br/><small style="color: #4d4d4d;"><%= SelectedCampus %> </small></h4>
        </div>

        <div class='row'>    
            <div class='col-md-6 text-right'>
                <span style='font-size:50px'><%= GivingLastWeekCampus %></span><br /><small style="display:block; margin-top: -15px;">Giving This Week</small>
            </div>
            <div class='col-md-6 text-center' style="font-size: 88%; padding-top: 13px">
                <strong><%= GivingTwoWeeksAgoCampus %></strong> <br/>
                Giving Last Week<br/>
                <strong><%= GivingYtdCampus %></strong> <br/>
                Giving Year-to-Date<br />
            </div>
        </div>
    </div>
</div>
    

</div>
    <br/>

    <div class="panel-footer" style="text-align: right;">Choose a different campus: <Rock:ButtonDropDownList ID="cpCampus" runat="server" OnSelectionChanged="cpCampus_OnSelectionChanged" ToolTip="Choose a Campus"/></div>
    </div>