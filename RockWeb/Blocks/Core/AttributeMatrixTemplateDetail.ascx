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
                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

                <fieldset>

                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="tbName" runat="server" Label="Name" Required="true"/>
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbIsActive" runat="server" Label="Active" />
                        </div>
                    </div>

                    <Rock:RockTextBox ID="tbDescription" runat="server" TextMode="MultiLine" Label="Description" Rows="4" />

                    <label>Item Attributes</label>
                    <Rock:NotificationBox ID="nbAttributeCountWarning" runat="server" NotificationBoxType="Warning" Text="At least one item attribute needs to be defined" Visible="false" />
                    <Rock:HelpBlock ID="hAttributes" runat="server" Text="Item Attributes define the columns that each item row has" />
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

                    <Rock:PanelWidget ID="pwAdvanced" runat="server" Title="Advanced" CssClass="mb-0" Expanded="false">
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:NumberBox ID="tbMinimumRows" runat="server" MinimumValue="0" Label="Minimum Rows" />
                            </div>
                            <div class="col-md-6">
                                <Rock:NumberBox ID="tbMaximumRows" runat="server" MinimumValue="0" Label="Maximum Rows" />
                            </div>
                        </div>

                        <Rock:CodeEditor ID="ceFormattedLava" runat="server" Label="Formatted Lava" EditorHeight="400" Help="This will determine how the Matrix Attribute Field will display its formatted value." EditorMode="Lava" />
                    </Rock:PanelWidget>

                </fieldset>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" data-shortcut-key="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" data-shortcut-key="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>

            </div>

        </asp:Panel>

        <Rock:ModalDialog ID="dlgAttribute" runat="server" Title="Matrix Template Attributes" OnSaveClick="dlgAttribute_SaveClick" OnCancelScript="" ValidationGroup="MatrixTemplateAttributes">
            <Content>
                <Rock:AttributeEditor ID="edtAttributes" runat="server" ShowActions="false" ValidationGroup="MatrixTemplateAttributes" IsShowInGridVisible="false" />
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
