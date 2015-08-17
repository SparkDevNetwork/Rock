<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EventDetail.ascx.cs" Inherits="RockWeb.Blocks.Follow.EventDetail" %>

<asp:UpdatePanel ID="pnlEventListUpdatePanel" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">

            <asp:HiddenField ID="hfEventId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-flag"></i>
                    <asp:Literal ID="lActionTitle" runat="server" /></h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlInactive" runat="server" LabelType="Danger" Text="Inactive" />
                </div>
            </div>
            <div class="panel-body">

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <div id="pnlViewDetails" runat="server">
                    <p class="description">
                        <asp:Literal ID="lEventDescription" runat="server"></asp:Literal>
                    </p>

                    <div class="row">
                        <div class="col-md-12">
                            <asp:Literal ID="lblMainDetails" runat="server" />
                        </div>
                    </div>

                </div>

                <div id="pnlEditDetails" runat="server">

                    <asp:ValidationSummary ID="valEventDetail" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="tbName" runat="server" Label="Name" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbIsActive" runat="server" Label="Active" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:RockTextBox ID="tbDescription" runat="server" Label="Description" TextMode="MultiLine" Rows="3" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:ComponentPicker ID="cpEventType" runat="server" Label="Event Type" Required="true" ContainerType="Rock.Follow.EventContainer"
                                AutoPostBack="true" OnSelectedIndexChanged="cpEventType_SelectedIndexChanged" />
                            <Rock:RockCheckBox ID="cbRequireNotification" runat="server" Label="Notice Required" Text="Yes"
                                Help="Select this option to force user to get notified of this event for people they follow. Unselect this option if users should be able to decide whether to get notified or not." />
                            <Rock:RockCheckBox ID="cbSendOnFriday" runat="server" Label="Send Weekend Notices on Friday" Text="Yes"
                                Help="Should any notices that would normally be sent on a weekend be sent of preceding Friday instead?" />
                        </div>
                        <div class="col-md-6">
                            <asp:PlaceHolder ID="phAttributes" runat="server" EnableViewState="false" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:CodeEditor ID="ceNotificationFormat" runat="server" EditorHeight="200" EditorMode="Liquid" EditorTheme="Rock" Label="Notification Format"
                                Help="How the entity should be formatted in the following event notification email when this event occurs for the entity. The entity is available to Lava as an 'Entity' object." />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>

                </div>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
