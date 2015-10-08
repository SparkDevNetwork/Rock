<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Attributes.ascx.cs" Inherits="RockWeb.Blocks.Core.Attributes" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlList" runat="server">

            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-list-ul"></i> Attribute List</h1>
                </div>
                <div class="panel-body">

                    <div class="grid grid-panel">
                        <Rock:GridFilter ID="rFilter" runat="server" OnDisplayFilterValue="rFilter_DisplayFilterValue">
                            <Rock:EntityTypePicker ID="ddlEntityType" runat="server" Label="Entity Type" IncludeGlobalOption="true" AutoPostBack="true" OnSelectedIndexChanged="ddlEntityType_SelectedIndexChanged" />
                            <Rock:CategoryPicker ID="cpCategoriesFilter" runat="server" Label="Categories" AllowMultiSelect="true" />
                        </Rock:GridFilter>
                        <Rock:Grid ID="rGrid" runat="server" AllowSorting="true" RowItemText="setting" TooltipField="Description" OnRowSelected="rGrid_RowSelected">
                            <Columns>
                                <Rock:RockBoundField
                                    DataField="Id"
                                    HeaderText="Id"
                                    SortExpression="Id"
                                    ItemStyle-Wrap="false"
                                    ItemStyle-HorizontalAlign="Right"
                                    HeaderStyle-HorizontalAlign="Right" />
                                <Rock:RockTemplateField ItemStyle-Wrap="false">
                                    <HeaderTemplate>Qualifier</HeaderTemplate>
                                    <ItemTemplate>
                                        <asp:Literal ID="lEntityQualifier" runat="server"></asp:Literal>
                                    </ItemTemplate>
                                </Rock:RockTemplateField>
                                <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" ItemStyle-Wrap="false" />
                                <Rock:RockTemplateField ItemStyle-Wrap="false">
                                    <HeaderTemplate>Categories</HeaderTemplate>
                                    <ItemTemplate>
                                        <asp:Literal ID="lCategories" runat="server"></asp:Literal>
                                    </ItemTemplate>
                                </Rock:RockTemplateField>
                                <Rock:RockTemplateField>
                                    <HeaderTemplate>Default Value</HeaderTemplate>
                                    <ItemTemplate>
                                        <asp:Literal ID="lDefaultValue" runat="server"></asp:Literal>
                                    </ItemTemplate>
                                </Rock:RockTemplateField>
                                <Rock:RockTemplateField>
                                    <HeaderTemplate>Value</HeaderTemplate>
                                    <ItemTemplate>
                                        <asp:Literal ID="lValue" runat="server"></asp:Literal>
                                    </ItemTemplate>
                                </Rock:RockTemplateField>
                                <Rock:EditField OnClick="rGrid_Edit" />
                                <Rock:SecurityField TitleField="Name" />
                                <Rock:DeleteField OnClick="rGrid_Delete" />
                            </Columns>
                        </Rock:Grid>
                    </div>

                </div>
            </div>

            

        </asp:Panel>

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="mdAttribute" runat="server" Title="Attribute" OnCancelScript="clearActiveDialog();" ValidationGroup="Attribute">
            <Content>
                <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                <Rock:EntityTypePicker ID="ddlAttrEntityType" runat="server" Label="Entity Type" IncludeGlobalOption="true" Required="true" AutoPostBack="true" OnSelectedIndexChanged="ddlAttrEntityType_SelectedIndexChanged" />
                <Rock:RockTextBox ID="tbAttrQualifierField" runat="server" Label="Qualifier Field" />
                <Rock:RockTextBox ID="tbAttrQualifierValue" runat="server" Label="Qualifier Value" />
                <Rock:AttributeEditor ID="edtAttribute" runat="server" ShowActions="false" ValidationGroup="Attribute" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="mdAttributeValue" runat="server" Title="Attribute Value" OnCancelScript="clearActiveDialog();" ValidationGroup="AttributeValue">
            <Content>
                <asp:HiddenField ID="hfIdValues" runat="server" />
                <asp:ValidationSummary ID="ValidationSummaryValue" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="AttributeValue" />
                <asp:PlaceHolder ID="phEditControls" runat="server" EnableViewState="false" />
            </Content>
        </Rock:ModalDialog>

        <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Danger" Visible="false" />

    </ContentTemplate>
</asp:UpdatePanel>
