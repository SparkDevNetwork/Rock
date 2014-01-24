<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonViewedSummary.ascx.cs" Inherits="RockWeb.Blocks.Security.PersonViewedSummary" %>

<asp:UpdatePanel ID="upPersonViewed" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlContent" runat="server">

            <Rock:ModalAlert ID="mdGridWarning" runat="server" />

            <div id="pnlViewedBy" runat="server">
                <h4>Profile Viewed By</h4>
                <Rock:Grid ID="gViewedBy" runat="server" DisplayType="Full" AllowSorting="true" EmptyDataText="No Profiles Found" OnRowSelected="gViewedBy_RowSelected">
                    <Columns>
                        <asp:BoundField DataField="FullName" HeaderText="Person" SortExpression="FullName" />
                        <asp:BoundField DataField="Age" HeaderText="Age" SortExpression="Age" />
                        <asp:BoundField DataField="Gender" HeaderText="Gender" SortExpression="Gender" />
                        <asp:BoundField DataField="FirstViewedDate" HeaderText="First Viewed" SortExpression="FirstViewedDate" />
                        <asp:BoundField DataField="LastViewedDate" HeaderText="Last Viewed" SortExpression="LastViewedDate" />
                        <asp:BoundField DataField="ViewedCount" HeaderText="Times Viewed" SortExpression="ViewedCount" />
                    </Columns>
                </Rock:Grid>
            </div>

            <div id="pnlViewed" runat="server">
                <h4>Profiles Viewed</h4>
                <Rock:Grid ID="gViewed" runat="server" DisplayType="Full" AllowSorting="true" EmptyDataText="No Profiles Found" OnRowSelected="gViewed_RowSelected">
                    <Columns>
                        <asp:BoundField DataField="FullName" HeaderText="Person" SortExpression="FullName" />
                        <asp:BoundField DataField="Age" HeaderText="Age" SortExpression="Age" />
                        <asp:BoundField DataField="Gender" HeaderText="Gender" SortExpression="Gender" />
                        <asp:BoundField DataField="FirstViewedDate" HeaderText="First Viewed" SortExpression="FirstViewedDate" />
                        <asp:BoundField DataField="LastViewedDate" HeaderText="Last Viewed" SortExpression="LastViewedDate" />
                        <asp:BoundField DataField="ViewedCount" HeaderText="Times Viewed" SortExpression="ViewedCount" />
                    </Columns>
                </Rock:Grid>
            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
