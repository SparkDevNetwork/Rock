<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SurveyDetail.ascx.cs" Inherits="RockWeb.Plugins.com_shepherdchurch.SurveySystem.SurveyDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbUnauthorized" runat="server" NotificationBoxType="Warning"></Rock:NotificationBox>

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server" Visible="true">
            <div class="panel-heading clearfix">
                <h1 class="panel-title pull-left">
                    <asp:Literal ID="lIconHtml" runat="server" />
                    <asp:Literal ID="lTitle" runat="server" />
                </h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlId" runat="server" LabelType="Info" />
                    <Rock:HighlightLabel ID="hlCategory" runat="server" LabelType="Default" />
                    <Rock:HighlightLabel ID="hlInactive" runat="server" LabelType="Warning" Text="Inactive" Visible="false" />
                </div>
            </div>

            <Rock:PanelDrawer ID="pdDetails" runat="server"></Rock:PanelDrawer>

            <div class="panel-body">
                <fieldset>
                    <p class="description">
                        <asp:Literal ID="lDescription" runat="server" />
                    </p>

                    <div class="actions">
                        <asp:LinkButton ID="lbEdit" runat="server" Text="Edit" CssClass="btn btn-primary" OnClick="lbEdit_Click" />
                        <asp:LinkButton ID="lbEditAnswers" runat="server" Text="Edit Answers" CssClass="btn btn-info margin-l-sm" OnClick="lbEditAnswers_Click" />
                        <asp:LinkButton ID="lbDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="lbDelete_Click" />

                        <span class="pull-right">
                            <asp:LinkButton ID="lbRun" runat="server" ToolTip="Take Survey" CssClass="btn btn-sm btn-default" OnClick="lbRun_Click"><i class="fa fa-play"></i></asp:LinkButton>
                            <asp:LinkButton ID="lbCopy" runat="server" ToolTip="Copy Survey" CssClass="btn btn-sm btn-default" OnClick="btnCopy_Click"><i class="fa fa-clone"></i></asp:LinkButton>
                            <asp:LinkButton ID="lbResults" runat="server" ToolTip="View Results" CssClass="btn btn-sm btn-default" OnClick="lbResults_Click"><i class="fa fa-list"></i></asp:LinkButton>
                            <Rock:SecurityButton ID="btnSecurity" runat="server" class="btn btn-sm btn-security" />
                        </span>
                    </div>
                </fieldset>
            </div>

            <Rock:ModalDialog ID="mdlConfirmDelete" runat="server" Title="Delete Survey?" ValidationGroup="ConfirmDelete" OnSaveClick="mdlConfirmDelete_SaveClick" SaveButtonText="Delete">
                <Content>
                    <Rock:NotificationBox ID="nbConfirmDelete" runat="server" NotificationBoxType="Warning" CssClass="clearfix">
                        <i class="fa fa-3x fa-exclamation-circle pull-left"></i>
                        This survey has results saved. Continuing will also delete those results
                        as well. Continue with delete?
                    </Rock:NotificationBox>
                </Content>
            </Rock:ModalDialog>
        </asp:Panel>

        <asp:Panel ID="pnlEdit" CssClass="panel panel-block" runat="server" Visible="true">
            <asp:HiddenField ID="hfId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-list-ol"></i>
                    <asp:Literal ID="lEditTitle" runat="server" />
                </h1>
            </div>

            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>

            <div class="panel-body">

                <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />
                <asp:ValidationSummary ID="vSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                <Rock:PanelWidget ID="pwDetails" runat="server" Title="Details">
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="com.shepherdchurch.SurveySystem.Model.Survey, com.shepherdchurch.SurveySystem" PropertyName="Name" />
                            <Rock:RockCheckBox ID="cbIsLoginRequired" runat="server" Label="Is Login Required" Help="Turn on to require a person to be logged in to take the survey." />
                        </div>

                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbIsActive" runat="server" Label="Is Active" Help="Turn off if you do not wish people to take this survey anymore." />
                            <Rock:RockCheckBox ID="cbRecordAnswers" runat="server" Label="Record Answers" Help="If set to true then answers provided by the user will be saved to the database." />
                        </div>
                    </div>

                    <Rock:RockTextBox ID="tbDescription" runat="server" TextMode="MultiLine" Rows="5" Label="Description" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:CategoryPicker ID="cpCategory" runat="server" Label="Category" Required="true" EntityTypeName="com.shepherdchurch.SurveySystem.Model.Survey" />
                            <Rock:WorkflowTypePicker ID="wtpWorkflow" runat="server" Label="Workflow Type" Help="Launches the specified workflow when a survey has been completed." />
                        </div>

                        <div class="col-md-6">
                            <Rock:RockDropDownList ID="ddlLastAttemptDateAttribute" runat="server" Label="Last Attempt Date Attribute" Help="If you wish to record the date the person last took this survey in a date attribute, select it here." />
                        </div>
                    </div>

                    <Rock:CodeEditor ID="ceInstructionTemplate" runat="server" EditorMode="Lava" Label="Survey Instructions" Help="This text will be displayed to the user at the start of the survey. <span class='tip tip-lava'></span>" Required="false" />

                    <Rock:CodeEditor ID="ceResultTemplate" runat="server" EditorMode="Lava" Label="Survey Complete Template" Help="This text will be displayed to the user upon completion of the survey. <span class='tip tip-lava'></span>" Required="true" />
                </Rock:PanelWidget>

                <Rock:PanelWidget ID="pwSurveyAttributes" runat="server" Title="Survey Questions">
                    <div class="grid">
                        <Rock:Grid ID="gSurveyAttributes" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Question" ShowConfirmDeleteDialog="false">
                            <Columns>
                                <Rock:ReorderField />
                                <Rock:RockBoundField DataField="Name" HeaderText="Attribute" />
                                <Rock:RockBoundField DataField="Description" HeaderText="Description" />
                                <Rock:BoolField DataField="IsRequired" HeaderText="Required" />
                                <Rock:EditField OnClick="gSurveyAttributes_Edit" />
                                <Rock:DeleteField OnClick="gSurveyAttributes_Delete" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </Rock:PanelWidget>

                <Rock:PanelWidget ID="pwPassFail" runat="server" Title="Pass Fail Mode">
                    <Rock:RockCheckBox ID="cbPassFail" runat="server" Label="Enable Pass Fail" Help="Enable the pass and fail scoring for this survey." OnCheckedChanged="cbPassFail_CheckedChanged" AutoPostBack="true" CausesValidation="false" />
                
                    <asp:Panel ID="pnlPassFail" runat="server" Visible="false">
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:NumberBox ID="nbPassingGrade" runat="server" MinimumValue="0" MaximumValue="100" Label="Passing Grade" Help="The percentage of questions the user must answer correctly to pass." Required="true" AppendText="%" />
                            </div>

                            <div class="col-md-6">
                                <Rock:RockDropDownList ID="ddlLastPassedDateAttribute" runat="server" Label="Last Passed Date Attribute" Help="If you wish to record the date the person last passed this test in a date attribute, select it here." />
                            </div>
                        </div>
                    </asp:Panel>
                </Rock:PanelWidget>

                <div class="actions">
                    <asp:LinkButton ID="lbSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="lbSave_Click" />
                    <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="lbCancel_Click" />
                </div>

            </div>

        </asp:Panel>

        <asp:Panel ID="pnlEditAnswers" runat="server" CssClass="panel panel-block" Visible="false">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-list-ol"></i> Edit Answers
                </h1>
            </div>

            <div class="panel-body">
                <asp:ValidationSummary ID="vAnswersSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                <asp:PlaceHolder ID="phAnswerAttributes" runat="server" />

                <asp:LinkButton ID="lbAnswersSave" runat="server" CssClass="btn btn-primary" OnClick="lbAnswersSave_Click" Text="Save" />
                <asp:LinkButton ID="lbAnswersCancel" runat="server" CssClass="btn btn-link" OnClick="lbAnswersCancel_Click" Text="Cancel" CausesValidation="false" />
            </div>
        </asp:Panel>

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="dlgSurveyAttributes" runat="server" Title="Survey Questions" OnSaveClick="dlgSurveyAttributes_SaveClick" ValidationGroup="SurveyAttributes">
            <Content>
                <Rock:AttributeEditor ID="edtSurveyAttributes" runat="server" ShowActions="false" ValidationGroup="SurveyAttributes" />
            </Content>
        </Rock:ModalDialog>
    </ContentTemplate>
</asp:UpdatePanel>
