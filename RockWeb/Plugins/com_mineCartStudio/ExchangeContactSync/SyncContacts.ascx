<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SyncContacts.ascx.cs" Inherits="RockWeb.Plugins.com_mineCartStudio.ExchangeContactSync.SyncContacts" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-exchange"></i> Exchange Contacts</h1>
            </div>
            <div class="panel-body">

                <Rock:NotificationBox ID="nbNotice" runat="server" Visible="false" />

                <asp:Panel ID="pnlEntry" runat="server">

                    <div class="row">
                        <div class="col-sm-6">
                            <Rock:RockCheckBox ID="cbFollowing" runat="server" Label="Include People you Follow" Text="Yes"
                                Help="If selected, anyone who you have followed will be added to your Exchange contacts list."/>
                        </div>
                        <div class="col-sm-6">
                            <Rock:RockCheckBox ID="cbStaff" runat="server" Label="Include Staff Members" Text="Yes"
                                Help="If selected, all staff members will be added to your Exchange contacts lit."/>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-sm-6">
                            <Rock:GroupPicker ID="gpGroup" runat="server" Label="Include Group Members" 
                                Help="The active members of each groups selected here will be added to your Exchange contacts list." AllowMultiSelect="true" />
                        </div>
                        <div class="col-sm-6">
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="lbSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="lbSave_Click" />
                        <asp:LinkButton ID="lbRemove" runat="server" Text="Clear and Remove" ToolTip="Clear all the selected options above and remove all contacts from exchange that were previously added." CssClass="btn btn-link" CausesValidation="false" OnClick="lbRemove_Click" />
                        <asp:LinkButton ID="lbTest" runat="server" Text="Test Connection" ToolTip="Tests to see if Rock can connect to your Exchange server and has the neccessary permissions to update your contacts." CssClass="btn btn-link pull-right" CausesValidation="false" OnClick="lbTest_Click" />
                    </div>

                </asp:Panel>
            </div>

        </div>

    </ContentTemplate>
</asp:UpdatePanel>

