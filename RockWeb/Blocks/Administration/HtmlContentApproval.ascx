<%@ Control Language="C#" AutoEventWireup="true" CodeFile="HtmlContentApproval.ascx.cs" Inherits="RockWeb.Blocks.Administration.HtmlContentApproval" %>

<asp:UpdatePanel ID="upContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlContent" runat="server" Visible="true">
            <Rock:ModalAlert ID="mdGridWarning" runat="server" />
            <Rock:GridFilter ID="gContentListFilter" runat="server">
                <Rock:RockDropDownList ID="ddlSiteFilter" runat="server" Label="Site" />
                <Rock:RockDropDownList ID="ddlApprovedFilter" runat="server" Label="Approval Status">
                    <asp:ListItem Text="All" Value="All"></asp:ListItem>
                    <asp:ListItem Text="Approved" Value="Approved"></asp:ListItem>
                    <asp:ListItem Text="Unapproved" Value="Unapproved"></asp:ListItem>
                </Rock:RockDropDownList>
                <Rock:PersonPicker ID="ppApprovedByFilter" runat="server" Label="Approved By" />
            </Rock:GridFilter>
            <Rock:Grid ID="gContentList" runat="server" AllowSorting="true">
                <Columns>
                    <asp:BoundField DataField="SiteName" HeaderText="Site" SortExpression="SiteName" />
                    <asp:BoundField DataField="PageName" HeaderText="Page" SortExpression="PageName" />
                    <asp:BoundField DataField="Block" HeaderText="Block"  SortExpression="Block" />
                    <asp:BoundField DataField="BlockId" HeaderText="Block Id" SortExpression="BlockId" />
                    <asp:BoundField DataField="Content" HeaderText="Content" SortExpression="Content" />
                    <Rock:ToggleField DataField="IsApproved" HeaderText="Approved?" CssClass="switch-mini" Enabled="True" OnText="Yes" OffText="No" OnCheckedChanged="gContentList_CheckedChanged" SortExpression="IsApproved" />
                    <asp:BoundField DataField="ApprovedByPerson" HeaderText="Approved By" SortExpression="ApprovedByPerson" />
                    <Rock:DateField DataField="ApprovedDateTime" HeaderText="Approval Date" SortExpression="ApprovedDateTime" />
                </Columns>
            </Rock:Grid>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
