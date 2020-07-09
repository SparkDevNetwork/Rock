<%@ Control Language="C#" AutoEventWireup="true" CodeFile="StepEntry.ascx.cs" Inherits="RockWeb.Blocks.Steps.StepEntry" %>

<asp:UpdatePanel ID="pnlGatewayListUpdatePanel" runat="server">
    <ContentTemplate>

        <asp:HiddenField ID="hfStepId" runat="server" />

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title">
                    <asp:Literal ID="lStepTypeTitle" runat="server" />
                </h1>
            </div>

            <div class="panel-body">

                <Rock:NotificationBox ID="nbMessage" runat="server" NotificationBoxType="Warning" />

                <div id="pnlViewDetails" runat="server">
                    <div class="row">
                        <div class="col-md-6">
                            <asp:Literal ID="lStepDescription" runat="server"></asp:Literal>
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
                        <asp:LinkButton ID="btnEdit" runat="server" AccessKey="e" ToolTip="Alt+e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
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
                    </div>

                    <div class="row">
                        <div class="col-sm-12">
                            <Rock:AttributeValuesContainer ID="avcAttributes" runat="server" NumberOfColumns="2" />
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
