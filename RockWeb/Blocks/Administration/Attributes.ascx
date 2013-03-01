<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Attributes.ascx.cs" Inherits="RockWeb.Blocks.Administration.Attributes" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlList" runat="server">

            <Rock:GridFilter ID="rFilter" runat="server">
                <Rock:LabeledDropDownList ID="ddlCategoryFilter" runat="server" LabelText="Category" />
            </Rock:GridFilter>
            <Rock:Grid ID="rGrid" runat="server" AllowSorting="true" RowItemText="setting" OnRowSelected="rGrid_Edit">
                <Columns>
                    <asp:BoundField DataField="Id" HeaderText="Id" SortExpression="Id" />
                    <asp:BoundField DataField="Category" HeaderText="Category" SortExpression="Category" />
                    <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                    <asp:BoundField DataField="Description" HeaderText="Description" />
                    <asp:BoundField DataField="FieldType" HeaderText="Type" />
                    <Rock:BoolField DataField="IsMultiValue" HeaderText="Multi-Value" SortExpression="IsMultiValue" />
                    <Rock:BoolField DataField="IsRequired" HeaderText="Required" SortExpression="IsRequired" />
                    <asp:TemplateField>
                        <HeaderTemplate>Default Value</HeaderTemplate>
                        <ItemTemplate>
                            <asp:Literal ID="lDefaultValue" runat="server"></asp:Literal>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField>
                        <HeaderTemplate>Value</HeaderTemplate>
                        <ItemTemplate>
                            <asp:Literal ID="lValue" runat="server"></asp:Literal>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <Rock:EditValueField OnClick="rGrid_EditValue" />
                    <Rock:DeleteField OnClick="rGrid_Delete" />
                </Columns>
            </Rock:Grid>

        </asp:Panel>

        <asp:Panel ID="pnlDetails" runat="server" Visible="false">

            <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-error block-message error" />
            <Rock:RockAttributeEditor ID="edtAttribute" runat="server" OnSaveClick="btnSave_Click" OnCancelClick="btnCancel_Click" />

        </asp:Panel>

        <Rock:ModalDialog ID="modalDetails" runat="server" Title="Attribute">
            <Content>
                <asp:HiddenField ID="hfIdValues" runat="server" />
                <asp:ValidationSummary ID="ValidationSummaryValue" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-error block-message error" />
                <fieldset>
                    <div class="control-group">
                        <label class="control-label">
                            <asp:Literal ID="lCaption" runat="server"></asp:Literal></label>
                        <div class="controls">
                            <asp:PlaceHolder ID="phEditControl" runat="server"></asp:PlaceHolder>
                        </div>
                    </div>
                </fieldset>
            </Content>
        </Rock:ModalDialog>

        <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Error" Visible="false" />

    </ContentTemplate>
</asp:UpdatePanel>
