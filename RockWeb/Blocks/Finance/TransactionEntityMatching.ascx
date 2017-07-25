<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TransactionEntityMatching.ascx.cs" Inherits="RockWeb.Blocks.Finance.TransactionEntityMatching" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
            <asp:HiddenField ID="hfBatchId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-check-square-o"></i>&nbsp;<asp:Literal ID="lPanelTitle" runat="server" /></h1>
            </div>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbBlockConfigurationWarning" runat="server" NotificationBoxType="Warning" Text="Please set the Entity Type in block settings" Visible="false" />
                <div class="row">
                    <div class="col-md-4">
                        <Rock:RockDropDownList ID="ddlBatch" runat="server" Label="Open Batches" AutoPostBack="true" OnSelectedIndexChanged="ddlBatch_SelectedIndexChanged" />
                    </div>
                </div>
                <div class="grid grid-panel">
                    <asp:Panel ID="pnlTransactions" runat="server">
                        <table class="grid-table table table-striped">
                            <thead>
                                <th>Person</th>
                                <th>Amount</th>
                                <th>Account</th>
                                <th>Transaction Type</th>
                                <th>
                                    <asp:Literal ID="lEntityHeaderText" runat="server" /></th>
                            </thead>
                            <tbody>
                                <asp:PlaceHolder ID="phTableRows" runat="server" />
                            </tbody>
                        </table>

                        <br />
                        <div class="margin-all-md">
                            <Rock:NotificationBox ID="nbSaveSuccess" runat="server" NotificationBoxType="Success" Text="Changes Saved" Visible="false" />
                            <div class="actions">
                                <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                            </div>

                        </div>
                    </asp:Panel>
                </div>

            </div>

        </asp:Panel>

        <asp:Panel ID="pnlSettings" runat="server">
            <Rock:ModalDialog ID="mdSettings" runat="server" OnSaveClick="mdSettings_SaveClick">
                <Content>
                    <asp:UpdatePanel ID="upnlSettings" runat="server">
                        <ContentTemplate>
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:DefinedValuePicker ID="ddlTransactionType" runat="server" Label="Transaction Type" Help="The Transaction Type that the transaction should be set to. Leave blank to get the original Transaction Type." />
                                    <Rock:EntityTypePicker ID="etpEntityType" runat="server" AutoPostBack="true" OnSelectedIndexChanged="etpEntityType_SelectedIndexChanged" />
                                    <Rock:RockTextBox ID="tbEntityTypeQualifierColumn" runat="server" Label="Entity Type Qualifier Column" />
                                    <Rock:RockTextBox ID="tbEntityTypeQualifierValue" runat="server" Label="Entity Type Qualifier Value" />
                                    <Rock:RockDropDownList ID="ddlDefinedTypePicker" runat="server" Visible="false" Label="Defined Type" />
                                    <Rock:GroupTypePicker ID="gtpGroupType" runat="server" Visible="false" Label="Group Type" />
                                </div>
                                <div class="col-md-6">
                                </div>
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </Content>

            </Rock:ModalDialog>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
