<%@ Control Language="C#" AutoEventWireup="true" CodeFile="VolunteerScreeningGlobalList.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.SafetySecurity.VolunteerScreeningGlobalList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pVolunteerScreeningList" runat="server" class="panel panel-block">
            <div class="panel-heading">
                <div class="row col-sm-4">
                    <h4 class="panel-title">Volunteer Screening</h4>
                    <br />
                </div>
            </div>
            <div class="panel-body">
            

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="rFilter" runat="server" OnDisplayFilterValue="rFilter_DisplayFilterValue">
                        <Rock:RockDropDownList ID="ddlStatus" runat="server" Label="Status" />
                        <Rock:RockDropDownList ID="ddlApplicationType" runat="server" Label="Application Type" />
                        <Rock:RockCheckBoxList ID="cblCampus" runat="server" Label="Campus" DataTextField="Name" DataValueField="Id" RepeatDirection="Horizontal" />
                        <Rock:RockTextBox ID="tbApplicantName" runat="server" Label="Applicant Name" />
                        <Rock:RockTextBox ID="tbMinistryLeader" runat="server" Label="Ministry Leader" />
                        <Rock:RockCheckBoxList ID="cblApplicationCompleted" runat="server" Label="Application Completed" RepeatDirection="Horizontal">
                            <asp:ListItem>Completed</asp:ListItem>
                            <asp:ListItem>Not Completed</asp:ListItem>
                        </Rock:RockCheckBoxList>
                    </Rock:GridFilter>

                    <Rock:Grid ID="gGrid" runat="server" AllowSorting="true" OnRowSelected="gGrid_Edit">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:RockBoundField DataField="State" HeaderText="Status" SortExpression="State" />
                            <Rock:RockBoundField DataField="SentDate" HeaderText="Application Sent" SortExpression="SentDate" DataFormatString="{0:M/dd/yy}" />
                            <Rock:RockBoundField DataField="CompletedDate" HeaderText="Application Completed" SortExpression="CompletedDate" DataFormatString="{0:M/dd/yy}" NullDisplayText="Not Completed" />
                            <Rock:RockBoundField DataField="Campus" HeaderText="Campus" SortExpression="Campus" />
                            <Rock:RockBoundField DataField="ApplicationType" HeaderText="Application Type" SortExpression="ApplicationType" />
                            <Rock:RockBoundField DataField="MinistryLeader" HeaderText="Ministry Leader" SortExpression="MinistryLeader" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
