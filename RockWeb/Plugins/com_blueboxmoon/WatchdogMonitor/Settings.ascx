<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Settings.ascx.cs" Inherits="RockWeb.Plugins.com_blueboxmoon.WatchdogMonitor.Settings" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">System Settings</h1>
            </div>

            <div class="panel-body">
                <Rock:NotificationBox ID="nbSaved" runat="server" NotificationBoxType="Warning" Dismissable="true" Visible="false">
                    Your settings have been saved.
                </Rock:NotificationBox>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlStateChangeCommunication" runat="server" Label="Service State Change Communication" Help="The communication that is sent immediately when a service check's state changes." />
                    </div>

                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlStateCommunication" runat="server" Label="Service State Communication" Help="The communication that is sent by the notification job to inform of ongoing warnings and errors." />
                    </div>
                </div>

                <Rock:DefinedValuePicker ID="dvSMSFromNumber" runat="server" Label="SMS Number" Help="Which SMS number to use when sending out notifications." />

                <Rock:CodeEditor ID="ceStateChangeSMS" runat="server" Label="Service State Change SMS" Help="The SMS body that is sent immediately when a service check's state changes." EditorMode="Lava" />

                <Rock:CodeEditor ID="ceStateSMS" runat="server" Label="Service State SMS" Help="The SMS body that is sent by the notification job to inform of ongoing warnings and errors." EditorMode="Lava" />

                <Rock:RockTextBox ID="tbCollectorSharedSecret" runat="server" Label="Collector Shared Secret" Help="The shared secret used by the Windows Collector services." Required="true" />

                <Rock:RockTextBox ID="tbHistoryConnectionString" runat="server" Label="History Connection String" Help="The connection string to use when storing and retrieving history data. If blank then the default Rock database is used." />

                <div class="actions">
                    <asp:LinkButton ID="lbSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="lbSave_Click" />
                </div>
            </div>
        </div>

        <script>
            Sys.Application.add_load(function () {
                window.scrollTo(0, 0);
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
