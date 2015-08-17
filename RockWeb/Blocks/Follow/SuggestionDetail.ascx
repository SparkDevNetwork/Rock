<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SuggestionDetail.ascx.cs" Inherits="RockWeb.Blocks.Follow.SuggestionDetail" %>

<asp:UpdatePanel ID="pnlSuggestionListUpdatePanel" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">

            <asp:HiddenField ID="hfSuggestionId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-flag-o"></i>
                    <asp:Literal ID="lActionTitle" runat="server" /></h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlInactive" runat="server" LabelType="Danger" Text="Inactive" />
                </div>
            </div>
            <div class="panel-body">

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <div id="pnlViewDetails" runat="server">
                    <p class="description">
                        <asp:Literal ID="lSuggestionDescription" runat="server"></asp:Literal>
                    </p>

                    <div class="row">
                        <div class="col-md-12">
                            <asp:Literal ID="lblMainDetails" runat="server" />
                        </div>
                    </div>

                </div>

                <div id="pnlEditDetails" runat="server">

                    <asp:ValidationSummary ID="valSuggestionDetail" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

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
                            <Rock:ComponentPicker ID="cpSuggestionType" runat="server" Label="Suggestion Type" Required="true" ContainerType="Rock.Follow.SuggestionContainer"
                                AutoPostBack="true" OnSelectedIndexChanged="cpSuggestionType_SelectedIndexChanged" />
                            <Rock:RockTextBox ID="tbReasonNote" runat="server" Label="Reason Note" 
                                Help="Short note to describe to the recipient why an following was suggested." />
                            <Rock:NumberBox ID="nbReminderDays" runat="server" Label="Reminded Days" 
                                Help="The number of days to wait before notifying person of this suggestion again if they have not followed or ignored the suggestion. Leave blank if person should never be reminded." />
                        </div>
                        <div class="col-md-6">
                            <asp:PlaceHolder ID="phAttributes" runat="server" EnableViewState="false" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:CodeEditor ID="ceNotificationFormat" runat="server" EditorHeight="200" EditorMode="Liquid" EditorTheme="Rock" Label="Notification Format"
                                Help="How the entity should be formatted in the following suggestion notification email when this suggestion is made for the entity. The entity is available to Lava as an 'Entity' object." />
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
