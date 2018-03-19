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
                        <Rock:RockTextBox ID="tbMinistryLeader" runat="server" Label="Ministry Leader" />
                    </Rock:GridFilter>

                    <Rock:Grid ID="gGrid" runat="server" AllowSorting="true" OnRowSelected="gGrid_Edit">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:RockBoundField DataField="Campus" HeaderText="Campus" SortExpression="Campus" />
                            <Rock:RockBoundField DataField="MinistryLeader" HeaderText="Ministry Leader" SortExpression="MinistryLeader" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
