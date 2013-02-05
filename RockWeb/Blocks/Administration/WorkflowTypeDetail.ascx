<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WorkflowTypeDetail.ascx.cs" Inherits="RockWeb.Blocks.Administration.WorkflowTypeDetail" %>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server" Visible="false">
            <asp:HiddenField ID="hfWorkflowTypeId" runat="server" />

            <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="alert alert-error" />

            <div id="pnlEditDetails" runat="server" class="well">

                <fieldset>
                    <legend>
                        <asp:Literal ID="lActionTitle" runat="server" />
                    </legend>

                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                    <div class="row-fluid">
                        <div class="span6">
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.WorkflowType, Rock" PropertyName="Name" />
                            <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.WorkflowType, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
                            <Rock:LabeledCheckBox ID="cbIsActive" runat="server" LabelText="Active" />
                            <Rock:DataDropDownList ID="ddlCategory" runat="server" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Model.Category, Rock" PropertyName="Name" LabelText="Category" />
                            <Rock:DataTextBox ID="tbWorkTerm" runat="server" SourceTypeName="Rock.Model.WorkflowType, Rock" PropertyName="WorkTerm" LabelText="Work Term" />
                        </div>
                        <div class="span6">
                            <Rock:DataTextBox ID="tbOrder" runat="server" SourceTypeName="Rock.Model.WorkflowType, Rock" PropertyName="Order" LabelText="Order" Required="true" />
                            <Rock:DataTextBox ID="tbProcessingInterval" runat="server" SourceTypeName="Rock.Model.WorkflowType, Rock" PropertyName="ProcessingIntervalSeconds" LabelText="Processing Interval (seconds)" />
                            <Rock:LabeledCheckBox ID="cbIsPersisted" runat="server" LabelText="Persisted" />
                            <Rock:LabeledDropDownList ID="ddlLoggingLevel" runat="server" LabelText="Logging Level" />
                        </div>
                    </div>
                </fieldset>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>

            </div>

            <fieldset id="fieldsetViewDetails" runat="server">
                <legend>
                    <asp:Literal ID="lReadOnlyTitle" runat="server" />
                </legend>
                <asp:Literal ID="lblActiveHtml" runat="server" />
                <div class="well">
                    <div class="row-fluid">
                        <Rock:NotificationBox ID="NotificationBox1" runat="server" NotificationBoxType="Info" />
                    </div>
                    <div class="row-fluid">
                        <asp:Literal ID="lblMainDetails" runat="server" />
                    </div>
                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-primary btn-mini" OnClick="btnEdit_Click" />
                    </div>
                </div>
            </fieldset>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
