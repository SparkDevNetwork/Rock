<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MediaAccountList.ascx.cs" Inherits="RockWeb.Blocks.Cms.MediaAccountList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-play-circle"></i> Media Accounts
                </h1>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                     <Rock:GridFilter ID="gfAccounts" runat="server">
                         <Rock:ComponentPicker ID="cpMediaAccountComponent" runat="server" Label="Account Type" Required="true" ContainerType="Rock.Media.MediaAccountContainer" />
                         <Rock:RockTextBox ID="txtAccountName" runat="server" Label="Name" />
                         <Rock:RockCheckBox ID="cbShowInactive" runat="server" Checked="false" Label="Include Inactive" />
                     </Rock:GridFilter>
                    <Rock:Grid ID="gAccountList" runat="server" AllowSorting="true" OnRowSelected="gAccountList_RowSelected"  CssClass="js-grid-accounts">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" ItemStyle-CssClass="js-name-account" />
                            <Rock:RockBoundField DataField="Type.FriendlyName" HeaderText="Type" />
                            <Rock:DateTimeField DataField="LastRefreshDateTime" HeaderText="Last Refresh Date/Time" SortExpression="LastRefreshDateTime" />
                             <Rock:RockBoundField DataField="Folders" HeaderText="Folders" SortExpression="Folders" />
                            <Rock:RockBoundField DataField="Videos" HeaderText="Videos" SortExpression="Videos" />
                            <Rock:DeleteField OnClick="gAccountList_DeleteClick" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
