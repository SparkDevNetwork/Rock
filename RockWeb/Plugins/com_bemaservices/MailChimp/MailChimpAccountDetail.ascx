<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MailChimpAccountDetail.ascx.cs" Inherits="com.bemaservices.MailChimp.MailChimpAccountDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlContent" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-envelope"></i>
                    <asp:Literal ID="lTitle" runat="server" />
                </h1>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">
                <asp:HiddenField ID="hfDefinedValueId" runat="server" />

                <asp:Panel ID="pnlEdit" runat="server" Visible="false">
                    <asp:ValidationSummary ID="valSummaryValue" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="Account" />
                    <legend>
                        <asp:Literal ID="lActionTitleDefinedValue" runat="server" />
                    </legend>
                    <fieldset>
                        <div class="row-fluid">
                            <div class="span12">
                                <Rock:DataTextBox ID="tbValueName" runat="server" SourceTypeName="Rock.Model.DefinedValue, Rock" PropertyName="Value" ValidationGroup="Account" Label="Account" />
                                <Rock:DataTextBox ID="tbValueDescription" runat="server" SourceTypeName="Rock.Model.DefinedValue, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" ValidationGroup="Account" ValidateRequestMode="Disabled" />
                            </div>
                        </div>
                        <div class="attributes">
                            <Rock:AttributeValuesContainer ID="avcEditAttributes" runat="server" ValidationGroup="Account" />
                        </div>
                        <div class="actions">
                            <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        </div>
                    </fieldset>
                </asp:Panel>

                <asp:Panel ID="pnlView" runat="server">
                    <fieldset>
                        <div class="row">
                            <div class="col-md-6">
                                <asp:Literal ID="lblMainDetails" runat="server" />
                            </div>
                            <div class="col-md-6">
                                <Rock:DynamicPlaceholder ID="phDisplayAttributes" runat="server" />
                            </div>
                        </div>
                        <div class="actions">
                            <asp:LinkButton ID="btnEdit" runat="server" AccessKey="m" ToolTip="Alt+m" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" />
                        </div>
                    </fieldset>
                </asp:Panel>
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
