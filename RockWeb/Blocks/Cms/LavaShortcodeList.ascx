<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LavaShortcodeList.ascx.cs" Inherits="RockWeb.Blocks.Cms.LavaShortcodeList" %>

<asp:UpdatePanel ID="upLavaShortcode" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Danger" Visible="true" />

        <asp:Panel ID="pnlLavaShortcodeList" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-cube"></i> Lava Shortcode List</h1>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                    <Rock:Grid ID="gLavaShortcode" runat="server" AllowSorting="true" OnRowSelected="gLavaShortCode_Edit">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:RockBoundField DataField="TagName" HeaderText="Tag Name" SortExpression="TagName" />
                            <Rock:RockBoundField DataField="TagType" HeaderText="Tag Type" SortExpression="TagType" />
                            <Rock:BoolField DataField="IsSystem" HeaderText="System" />
                            <Rock:BoolField DataField="IsActive" HeaderText="Active" />
                            <Rock:DeleteField OnClick="gLavaShortCode_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
