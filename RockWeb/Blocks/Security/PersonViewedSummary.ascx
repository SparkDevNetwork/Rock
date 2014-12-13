<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonViewedSummary.ascx.cs" Inherits="RockWeb.Blocks.Security.PersonViewedSummary" %>
<%@ Import Namespace="Rock" %>

<asp:UpdatePanel ID="upPersonViewed" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlContent" runat="server">

            <Rock:ModalAlert ID="mdGridWarning" runat="server" />

            <div id="pnlViewedBy" runat="server">
                <h4>Profile Viewed By</h4>
                
                <div class="grid">
                    <Rock:Grid ID="gViewedBy" runat="server" DisplayType="Full" AllowSorting="true" EmptyDataText="No Profiles Found" OnRowSelected="gViewedBy_RowSelected">
                        <Columns>
                            <Rock:RockBoundField DataField="FullName" HeaderText="Person" SortExpression="FullName" />
                            <Rock:RockTemplateField HeaderText="Age" SortExpression="Age" ItemStyle-HorizontalAlign="Right"><ItemTemplate><%# ((DateTime?)Eval("BirthDate")).Age() %></ItemTemplate></Rock:RockTemplateField>
                            <Rock:RockBoundField DataField="Gender" HeaderText="Gender" SortExpression="Gender" />
                            <Rock:RockBoundField DataField="FirstViewedDate" HeaderText="First Viewed" SortExpression="FirstViewedDate" />
                            <Rock:RockBoundField DataField="LastViewedDate" HeaderText="Last Viewed" SortExpression="LastViewedDate" />
                            <Rock:RockBoundField DataField="ViewedCount" HeaderText="Times Viewed" SortExpression="ViewedCount" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>

            <div id="pnlViewed" runat="server">
                <h4>Profiles Viewed</h4>
                
                <div class="grid">
                    <Rock:Grid ID="gViewed" runat="server" DisplayType="Full" AllowSorting="true" EmptyDataText="No Profiles Found" OnRowSelected="gViewed_RowSelected">
                        <Columns>
                            <Rock:RockBoundField DataField="FullName" HeaderText="Person" SortExpression="FullName" />
                            <Rock:RockTemplateField HeaderText="Age" SortExpression="Age" ItemStyle-HorizontalAlign="Right"><ItemTemplate><%# ((DateTime?)Eval("BirthDate")).Age() %></ItemTemplate></Rock:RockTemplateField>
                            <Rock:RockBoundField DataField="Gender" HeaderText="Gender" SortExpression="Gender" />
                            <Rock:RockBoundField DataField="FirstViewedDate" HeaderText="First Viewed" SortExpression="FirstViewedDate" />
                            <Rock:RockBoundField DataField="LastViewedDate" HeaderText="Last Viewed" SortExpression="LastViewedDate" />
                            <Rock:RockBoundField DataField="ViewedCount" HeaderText="Times Viewed" SortExpression="ViewedCount" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
