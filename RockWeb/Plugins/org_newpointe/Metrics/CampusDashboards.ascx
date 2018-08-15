<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CampusDashboards.ascx.cs" Inherits="RockWeb.Plugins.org_newpointe.Metrics.CampusDashboards" %>

<div class="panel panel-block" style="background: #f9f9f9">
     <div class="panel-heading" style="display: block !important;"><h4 class="panel-title"><i class="fa fa-line-chart"></i> 10,000 by 2020 Dashboard - Fiscal Year 2016 - <%= SelectedCampus %> </h4></div>
    
    <div class="row center-block">
    <div class="col-md-12 center-block">
        <h4 style="text-align: center;"><strong>Every number has a name.  Every name has a story.  Every story matters to God.</strong></h4>
    </div>
   </div>

<div class="container">    
  <table class="table table-striped table-responsive table-condensed">
    <thead>
      <tr>
        <th>Item</th>
        <th>Current</th>
        <th>Progress</th>
        <th>2016 Goal</th>
        <th>2020 Goal</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td colspan="5" style="background-color: #8bc540; text-align: center; color: #fff; font-size: 120%;">Evangelism</td>
      </tr>
      <tr>
        <td>Weekend Attendance <small><a href="https://rock.newpointe.org/metrics/attendanceDashboard"><i class="fa fa-bar-chart"></i></a></small></td>
        <td><%= AttendanceLastWeekAll %></td>
        <td><%= AttendanceGoalProgress %></td>
        <td><%= AttendanceGoalCurrent %></td>
        <td><%= AttendanceGoal2020 %></td>
      </tr>
      <tr>
        <td style="padding-left:30px">Adults</td>
        <td><%= AttendanceLastWeekAud %></td>
        <td><%= AttendanceAudGoalProgress %></td>
        <td><%= AttendanceAudGoalCurrent %></td>
        <td><%= AttendanceAudGoal2020 %></td>
      </tr>
      <tr>
        <td style="padding-left:30px">Children</td>
        <td><%= AttendanceLastWeekChild %></td>
        <td><%= AttendanceChildGoalProgress %></td>
        <td><%= AttendanceChildGoalCurrent %></td>
        <td><%= AttendanceChildGoal2020 %></td>
      </tr>
      <tr>
        <td style="padding-left:30px">Students</td>
        <td><%= AttendanceLastWeekStudent %></td>
        <td><%= AttendanceStudentGoalProgress %></td>
        <td><%= AttendanceStudentGoalCurrent %></td>
        <td><%= AttendanceStudentGoal2020 %></td>
      </tr>
      <tr>
        <td>Commitments <small><a href="https://rock.newpointe.org/page/525"><i class="fa fa-bar-chart"></i></a></small></td>
        <td> <%= Commitments %></td>
          <td><%= CommitmentsGoalProgress %></td>
        <td><%= CommitmentsGoalCurrent %></td>
        <td><%= CommitmentsGoal2020 %></td>
      </tr>
      <tr>
        <td>Re-commitments <small><a href="https://rock.newpointe.org/page/525"><i class="fa fa-bar-chart"></i></a></small></td>
        <td><%= Recommitments %></td>
          <td><%= RecommitmentsGoalProgress %></td>
        <td><%= RecommitmentsGoalCurrent %></td>
        <td><%= RecommitmentsGoal2020 %></td>
      </tr>
      <tr>
        <td>Baptisms <small><a href="https://rock.newpointe.org/page/525"><i class="fa fa-bar-chart"></i></a></small></td>
        <td><%= Baptisms %></td>
        <td><%= BaptismsGoalProgress %></td>
        <td><%= BaptismsGoalCurrent %></td>
        <td><%= BaptismsGoal2020 %></td>
      </tr>
      <tr>
        <td>New Here Guests <small><a href="https://rock.newpointe.org/page/525"><i class="fa fa-bar-chart"></i></a></small></td>
        <td><%= NewHere %></td>
          <td><%= NewHereGoalProgress %></td>
        <td><%= NewHereGoalCurrent %></td>
        <td><%= NewHereGoal2020 %></td>
      </tr>
      <tr>
        <td colspan="5" style="background-color: #8bc540; text-align: center; color: #fff; font-size: 120%;">Assimilation</td>
      </tr>
      <tr>
        <td>New to NewPointe <small><a href="https://rock.newpointe.org/page/526"><i class="fa fa-bar-chart"></i></a></small></td>
        <td> <%= NewtoNewPointe %></td>
          <td><%= NewtoNewPointeGoalProgress %></td>
        <td><%= NewtoNewPointeGoalCurrent %></td>
        <td><%= NewtoNewPointeGoal2020 %></td>
      </tr>
      <tr>
        <td>Discover Groups <small><a href="https://rock.newpointe.org/page/526"><i class="fa fa-bar-chart"></i></a></small></td>
        <td><%= DiscoverGroups %></td>
          <td><%= DiscoverGroupsGoalProgress %></td>
        <td><%= DiscoverGroupsGoalCurrent %></td>
        <td><%= DiscoverGroupsGoal2020 %></td>
      </tr>
      <tr>
        <td>People in Small Groups</td>
        <td><%= SmallGroupParticipants %></td>
          <td><%= SmallGroupParticipantsGoalProgress %></td>
        <td><%= SmallGroupParticipantsGoalCurrent %></td>
        <td><%= SmallGroupParticipantsGoal2020 %></td>
      </tr>
        <!--
      <tr>
        <td>Campus Groups</td>
        <td>NYI</td>
          <td>95%</td>
        <td>NYI</td>
        <td>NYI</td>
      </tr>
            -->
      <tr>
      <td>Partners</td>
        <td><%= Partners %></td>
          <td><%= PartnersGoalProgress %></td>
        <td><%= PartnersGoalCurrent %></td>
        <td><%= PartnersGoal2020 %></td>
      </tr>
      <tr>
        <td colspan="5" style="background-color: #8bc540; text-align: center; color: #fff; font-size: 120%;">People</td>
      </tr>
      <tr>
        <td>Volunteers <small><a href="https://rock.newpointe.org/page/551"><i class="fa fa-bar-chart"></i></a></small></td>
        <td><%= Volunteers %></td>
          <td><%= VolunteersGoalProgress %></td>
        <td><%= VolunteersGoalCurrent %></td>
        <td><%= VolunteersGoal2020 %></td>
      </tr>
      <tr>
        <td>Small Group Leaders</td>
        <td><%= SmallGroupLeaders %></td>
          <td><%= SmallGroupLeadersGoalProgress %></td>
        <td><%= SmallGroupLeadersGoalCurrent %></td>
        <td><%= SmallGroupLeadersGoal2020 %></td>
      </tr>
      <!--<tr>
      <td>Ministry Leaders</td>
        <td>0</td>
        <td>95%</td>
        <td>0</td>
        <td>0</td>
      </tr> -->
        <tr>
        <td>Inactive Follow-up <small><a href="https://rock.newpointe.org/page/735?WorkflowTypeId=120"><i class="fa fa-file-text-o"></i></a></small></td> 
        <td><%= InactiveFollowupComplete %> <small>Completed</small></td>
          <td><%= InactiveFollowupGoalProgress %></td>
        <td><%= InactiveFollowupAll %></td>
        <td></td>
      </tr>
   <tfoot>
    <tr>
       <td colspan="5" style="background-color: #8bc540; text-align: center; color: #fff; font-size: 80%;"><em>He must become greater, and I must become less and less.  - John 3:30</em></td>
    </tr>
  </tfoot>
    </tbody>
  </table>
</div>
    

    <div class="panel-footer" style="text-align: right;">Choose a different campus: <Rock:ButtonDropDownList ID="cpCampus" runat="server" OnSelectionChanged="cpCampus_OnSelectionChanged" ToolTip="Choose a Campus"/></div>
    </div>