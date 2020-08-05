<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AchievementAttemptDetail.ascx.cs" Inherits="RockWeb.Blocks.Streaks.AchievementAttemptDetail" %>

<asp:UpdatePanel ID="upAttemptDetail" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfIsEditMode" runat="server" />

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <asp:Literal ID="lIcon" runat="server" />
                    <asp:Literal ID="lTitle" runat="server" />
                </h1>
                <div class="panel-labels">
                    <asp:LinkButton ID="btnAchievement" runat="server" CssClass="label label-default" OnClick="btnAchievement_Click" CausesValidation="false">
                        <i class="fa fa-medal"></i>
                        Achievement Type
                    </asp:LinkButton>
                </div>
            </div>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                <asp:ValidationSummary ID="valValidationSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                <div id="pnlViewDetails" runat="server">

                    <div class="row">
                        <div class="col-sm-12">
                            <asp:Literal ID="lProgress" runat="server" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <h4 class="margin-b-lg"><asp:Literal ID="lAchiever" runat="server" /></h4>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <asp:Literal ID="lAttemptDescription" runat="server" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" AccessKey="e" ToolTip="Alt+e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" CausesValidation="false" OnClick="btnDelete_Click" />
                    </div>
                </div>

                <div id="pnlEditDetails" runat="server">
                    <div class="row">
                        <div class="col-md-6" id="divAchiever" runat="server">
                            <Rock:NumberBox ID="nbAchieverEntityId" runat="server" Required="true" Label="Achiever Id" />
                        </div>
                        <div class="col-md-6" id="divAchievement" runat="server">
                            <Rock:AchievementTypePicker ID="atpAchievementType" runat="server" Required="true" Label="Achievement" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DatePicker ID="dpStart" runat="server" SourceTypeName="Rock.Model.AchievementAttempt, Rock" PropertyName="AchievementAttemptStartDateTime" Required="true" Label="Start Date" Help="The date that progress toward this attempt began." />
                        </div>
                        <div class="col-md-6">
                            <Rock:DatePicker ID="dpEnd" runat="server" SourceTypeName="Rock.Model.AchievementAttempt, Rock" PropertyName="AchievementAttemptEndDateTime" Required="false" Label="End Date" Help="The date that progress toward this attempt ended." />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="tbProgress" runat="server" CssClass="input-width-md" SourceTypeName="Rock.Model.AchievementAttempt, Rock" PropertyName="Progress" Required="false" Label="Progress" Help="The percent towards completion of this attempt. 0.5 is 50%, 1 is 100%, etc." />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>