<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MembershipCovenantGlobalList.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.AdultMinistries.MembershipCovenantGlobalList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pList" runat="server" class="panel panel-block">
            <div class="panel-heading">
                <div class="row col-sm-4">
                    <h4 class="panel-title">Membership Covenants</h4>
                    <br />
                </div>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:GridFilter ID="rFilter" runat="server" OnDisplayFilterValue="rFilter_DisplayFilterValue">
                        <Rock:RockCheckBoxList ID="cblCampus" runat="server" Label="Campus" DataTextField="Name" DataValueField="Id" RepeatDirection="Horizontal" />
                        <Rock:RockTextBox ID="tbApplicantName" runat="server" Label="Applicant Name" />
                        <Rock:DateRangePicker ID="drpDates" runat="server" Label="Starting Point" />
                    </Rock:GridFilter>

                    <Rock:Grid ID="gGrid" runat="server" AllowSorting="true" OnRowSelected="gGrid_Edit">
                        <Columns>
                            <Rock:RockBoundField DataField="Applicant" HeaderText="Applicant" SortExpression="Applicant" />
                            <Rock:RockBoundField DataField="Campus" HeaderText="Campus" SortExpression="Campus" />
                            <Rock:RockBoundField DataField="StartingPoint" HeaderText="Starting Point" SortExpression="StartingPoint" DataFormatString="{0:M/dd/yy}" NullDisplayText="No Date Specified" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
