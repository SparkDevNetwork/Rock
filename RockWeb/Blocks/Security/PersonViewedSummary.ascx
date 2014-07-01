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
                            <asp:BoundField DataField="FullName" HeaderText="Person" SortExpression="FullName" />
                            <asp:TemplateField HeaderText="Age" SortExpression="Age" ItemStyle-HorizontalAlign="Right"><ItemTemplate><%# ((DateTime?)Eval("BirthDate")).Age() %></ItemTemplate></asp:TemplateField>
                            <asp:BoundField DataField="Gender" HeaderText="Gender" SortExpression="Gender" />
                            <asp:BoundField DataField="FirstViewedDate" HeaderText="First Viewed" SortExpression="FirstViewedDate" />
                            <asp:BoundField DataField="LastViewedDate" HeaderText="Last Viewed" SortExpression="LastViewedDate" />
                            <asp:BoundField DataField="ViewedCount" HeaderText="Times Viewed" SortExpression="ViewedCount" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>

            <div id="pnlViewed" runat="server">
                <h4>Profiles Viewed</h4>
                
                <div class="grid">
                    <Rock:Grid ID="gViewed" runat="server" DisplayType="Full" AllowSorting="true" EmptyDataText="No Profiles Found" OnRowSelected="gViewed_RowSelected">
                        <Columns>
                            <asp:BoundField DataField="FullName" HeaderText="Person" SortExpression="FullName" />
                            <asp:TemplateField HeaderText="Age" SortExpression="Age" ItemStyle-HorizontalAlign="Right"><ItemTemplate><%# ((DateTime?)Eval("BirthDate")).Age() %></ItemTemplate></asp:TemplateField>
                            <asp:BoundField DataField="Gender" HeaderText="Gender" SortExpression="Gender" />
                            <asp:BoundField DataField="FirstViewedDate" HeaderText="First Viewed" SortExpression="FirstViewedDate" />
                            <asp:BoundField DataField="LastViewedDate" HeaderText="Last Viewed" SortExpression="LastViewedDate" />
                            <asp:BoundField DataField="ViewedCount" HeaderText="Times Viewed" SortExpression="ViewedCount" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
