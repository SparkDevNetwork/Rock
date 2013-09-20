<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Attributes.ascx.cs" Inherits="RockWeb.Blocks.Administration.Attributes" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlList" runat="server">

            <Rock:GridFilter ID="rFilter" runat="server" OnDisplayFilterValue="rFilter_DisplayFilterValue">
                <Rock:LabeledDropDownList ID="ddlEntityType" runat="server" LabelText="Entity Type" AutoPostBack="true" OnSelectedIndexChanged="ddlEntityType_SelectedIndexChanged" />
                <Rock:CategoryPicker ID="cpCategoriesFilter" runat="server" LabelText="Categories" AllowMultiSelect="true" />
            </Rock:GridFilter>
            <Rock:Grid ID="rGrid" runat="server" AllowSorting="true" RowItemText="setting" OnRowSelected="rGrid_Edit">
                <Columns>
                    <asp:BoundField 
                        DataField="Id" 
                        HeaderText="Id" 
                        SortExpression="EntityType.FriendlyName" 
                        ItemStyle-Wrap="false" />
                    <asp:TemplateField ItemStyle-Wrap="false">
                        <HeaderTemplate>Qualifier</HeaderTemplate>
                        <ItemTemplate>
                            <asp:Literal ID="lEntityQualifier" runat="server"></asp:Literal>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" ItemStyle-Wrap="false" />
                    <asp:TemplateField>
                        <HeaderTemplate>Description</HeaderTemplate>
                        <ItemTemplate>
                            <asp:Literal ID="lDescription" runat="server"></asp:Literal>
                        </ItemTemplate>
                    </asp:TemplateField>                    
                    <asp:TemplateField ItemStyle-Wrap="false">
                        <HeaderTemplate>Categories</HeaderTemplate>
                        <ItemTemplate>
                            <asp:Literal ID="lCategories" runat="server"></asp:Literal>
                        </ItemTemplate>
                    </asp:TemplateField>
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
                    <Rock:SecurityField TitleField="Name" />
                    <Rock:DeleteField OnClick="rGrid_Delete" />
                </Columns>
            </Rock:Grid>

        </asp:Panel>

        <asp:Panel ID="pnlDetails" runat="server" Visible="false">

            <asp:ValidationSummary ID="valSummaryTop" runat="server"  
                HeaderText="Please Correct the Following" CssClass="alert alert-error block-message error" />

            <Rock:LabeledDropDownList ID="ddlAttrEntityType" runat="server" LabelText="Entity Type" />
            <Rock:LabeledTextBox ID="tbAttrQualifierField" runat="server" LabelText="Qualifier Field" />
            <Rock:LabeledTextBox ID="tbAttrQualifierValue" runat="server" LabelText="Qualifier Value" />

            <Rock:AttributeEditor ID="edtAttribute" runat="server" OnSaveClick="btnSave_Click" OnCancelClick="btnCancel_Click" />

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
