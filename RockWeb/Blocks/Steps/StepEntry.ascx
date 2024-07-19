<%@ Control Language="C#" AutoEventWireup="true" CodeFile="StepEntry.ascx.cs" Inherits="RockWeb.Blocks.Steps.StepEntry" %>

<asp:UpdatePanel ID="pnlGatewayListUpdatePanel" runat="server">
    <ContentTemplate>

        <asp:HiddenField ID="hfStepId" runat="server" />

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title">
                    <asp:Literal ID="lStepTypeTitle" runat="server" />
                </h1>
                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlCampus" runat="server" />
                    <asp:Literal ID="lStatus" runat="server" />
                </div>
            </div>

            <div class="panel-body">

                <Rock:NotificationBox ID="nbMessage" runat="server" NotificationBoxType="Warning" />

                <div id="pnlViewDetails" runat="server">
                    <div class="d-flex align-items-center">
                        <div class="mr-3">
                            <asp:Literal runat="server" ID="lPersonPhotoHtml" />
                        </div>
                        <div>
                            <h3 class="font-weight-semibold my-0">
                                <asp:Literal runat="server" ID="lPersonName" />
                            </h3>
                            <span class="text-muted">
                                <asp:Literal runat="server" ID="lPersonSubTitle" />
                            </span>
                        </div>
                    </div>
                    <div class="row">
                        <asp:Literal ID="lStartHtml" runat="server" />
                        <asp:Literal ID="lEndHtml" runat="server" />
                        <div runat="server" id="divTimespan" class="col-sm-4 mt-3">
                            <div class="kpi kpi-light text-blue-700 border-0 p-0">
                                <div class="kpi-icon">
                                    <img class="svg-placeholder" src="data:image/svg+xml;utf8,<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 1 1'></svg>">
                                    <div class="kpi-content">
                                        <asp:Literal runat="server" ID="lDays" />
                                    </div>
                                </div>
                                <div class="kpi-stat">
                                    <span class="kpi-label text-color">
                                        <asp:Literal runat="server" ID="lDaysLabel" /> to<br/>
                                        Complete
                                    </span>
                                </div>
                            </div>
                        </div>
                        <div class="col-sm-12">
                            <asp:Literal ID="lNotesHtml" runat="server" />
                        </div>
                        <div class="col-md-6">
                            <%-- Manual Workflow Triggers --%>
                            <Rock:ModalAlert ID="mdWorkflowResult" runat="server" />
                            <asp:Label ID="lblWorkflows" Text="Available Workflows" Font-Bold="true" runat="server" />
                            <div class="margin-b-md">
                                <asp:Repeater ID="rptWorkflows" runat="server">
                                    <ItemTemplate>
                                        <asp:LinkButton runat="server" CssClass="btn btn-default btn-xs" CommandArgument='<%# Eval("Id") %>' CommandName="LaunchWorkflow">
                                        <%# Eval("WorkflowType.Name") %>
                                        </asp:LinkButton>
                                    </ItemTemplate>

                                </asp:Repeater>
                            </div>
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-sm-12">
                            <Rock:AttributeValuesContainer ID="avcAttributesView" runat="server" NumberOfColumns="2" />
                        </div>
                    </div>


                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" data-shortcut-key="e" ToolTip="Alt+e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="btnDelete_Click" CausesValidation="false" />
                    </div>
                </div>

                <div id="pnlEditDetails" runat="server">

                    <asp:ValidationSummary ID="valGatewayDetail" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                    <div class="row">
                        <div class="col-sm-12 col-md-6">
                            <Rock:PersonPicker ID="ppPerson" runat="server" Label="Person" />
                        </div>
                        <div class="col-sm-12 col-md-6">
                            <Rock:CampusPicker ID="cpCampus" runat="server" Label="Campus" />
                        </div>
                        <div class="col-sm-6 col-md-3">
                            <Rock:DatePicker ID="rdpStartDate" runat="server" PropertyName="StartDate" Required="true" />
                        </div>
                        <div class="col-sm-6 col-md-3">
                            <Rock:DatePicker ID="rdpEndDate" runat="server" PropertyName="EndDate" />
                        </div>
                        <div class="col-sm-12 col-md-6">
                            <Rock:StepStatusPicker ID="rsspStatus" runat="server" Label="Status" Required="true" />
                        </div>
                        <div class="col-sm-12">
                            <Rock:RockTextBox ID="tbNote" runat="server" Label="Note" Required="false" Rows="4" TextMode="MultiLine" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-sm-12">
                            <Rock:AttributeValuesContainer ID="avcAttributes" runat="server" NumberOfColumns="2" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" data-shortcut-key="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" data-shortcut-key="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>

                </div>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
