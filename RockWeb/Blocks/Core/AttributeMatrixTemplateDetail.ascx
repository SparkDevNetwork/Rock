<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AttributeMatrixTemplateDetail.ascx.cs" Inherits="RockWeb.Blocks.Core.AttributeMatrixTemplateDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
            <asp:HiddenField ID="hfAttributeMatrixTemplateId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-list-alt"></i>&nbsp;<asp:Literal ID="lActionTitle" runat="server" /></h1>

            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">
                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

                <fieldset>

                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="tbName" runat="server" Label="Name" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbIsActive" runat="server" Label="Active" />
                        </div>
                    </div>

                    <Rock:RockTextBox ID="tbDescription" runat="server" TextMode="MultiLine" Label="Description" Rows="4" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:NumberBox ID="tbMinimumRows" runat="server" Label="Minimum Rows" />
                        </div>
                        <div class="col-md-6">
                            <Rock:NumberBox ID="tbMaximumRows" runat="server" Label="Maximum Rows" />
                        </div>
                    </div>

                    <Rock:CodeEditor ID="ceFormattedLava" runat="server" Label="Formatted Lava" Help="This will determine how the Matrix Attribute Field will display its formatted value" EditorMode="Lava" />

                    <div class="grid">
                        <Rock:Grid ID="gAttributes" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Matrix Attribute">
                            <Columns>
                                <Rock:ReorderField />
                                <Rock:RockBoundField DataField="Name" HeaderText="Attribute" />
                                <Rock:FieldTypeField DataField="FieldTypeId" HeaderText="Type" />
                                <Rock:SecurityField TitleField="Name" />
                                <Rock:EditField OnClick="gAttributes_Edit" />
                                <Rock:DeleteField OnClick="gAttributes_Delete" />
                            </Columns>
                        </Rock:Grid>
                    </div>

                </fieldset>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>

            </div>

        </asp:Panel>

        <Rock:ModalDialog ID="dlgAttribute" runat="server" Title="Matrix Template Attributes" OnSaveClick="dlgAttribute_SaveClick" OnCancelScript="" ValidationGroup="MatrixTemplateAttributes">
            <Content>
                <Rock:AttributeEditor ID="edtAttributes" runat="server" ShowActions="false" ValidationGroup="MatrixTemplateAttributes" />
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
