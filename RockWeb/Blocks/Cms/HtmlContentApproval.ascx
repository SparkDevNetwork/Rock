<%@ Control Language="C#" AutoEventWireup="true" CodeFile="HtmlContentApproval.ascx.cs" Inherits="RockWeb.Blocks.Cms.HtmlContentApproval" %>

<asp:UpdatePanel ID="upContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlContent" runat="server" Visible="true">
            <Rock:ModalAlert ID="mdGridWarning" runat="server" />
            
            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-check"></i> HTML Content Approval List</h1>
                </div>
                <div class="panel-body">

                    <div class="grid grid-panel">
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
                                <asp:BoundField DataField="SiteName" HeaderText="Site" SortExpression="Block.Page.Layout.Site.Name,Block.Layout.Site.Name" />
                                <asp:BoundField DataField="PageName" HeaderText="Page" SortExpression="Block.Page.InternalName" />
                                <Rock:DateTimeField DataField="ModifiedDateTime" HeaderText="Modified Date/Time" SortExpression="ModifiedDateTime" />
                                <Rock:ToggleField DataField="IsApproved" HeaderText="Approved" ButtonSizeCssClass="btn-xs" Enabled="True" OnText="Yes" OffText="No" OnCheckedChanged="gContentList_CheckedChanged" SortExpression="IsApproved" />
                                <asp:BoundField DataField="ApprovedByPerson" HeaderText="Approved By" SortExpression="ApprovedByPersonAlias.Person.NickName,ApprovedByPersonAlias.Person.LastName" />
                                <Rock:DateField DataField="ApprovedDateTime" HeaderText="Approval Date" SortExpression="ApprovedDateTime" />
                            </Columns>
                        </Rock:Grid>
                    </div>

                </div>
            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
