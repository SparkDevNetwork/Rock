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
                            <Rock:EntityTypePicker ID="ddlEntityType" runat="server" Label="Entity Type" IncludeGlobalOption="true" AutoPostBack="true" OnSelectedIndexChanged="ddlEntityType_SelectedIndexChanged" EnhanceForLongLists="true" />
                            <Rock:CategoryPicker ID="cpCategoriesFilter" runat="server" Label="Categories" AllowMultiSelect="true" />
                            <Rock:RockCheckBox ID="cbAnalyticsEnabled" runat="server" Label="Show only attributes with Analytics Enabled" />
                        </Rock:GridFilter>
                        <Rock:Grid ID="rGrid" runat="server" AllowSorting="true" RowItemText="setting" TooltipField="Description" OnRowSelected="rGrid_RowSelected">
                            <Columns>
                                <Rock:ReorderField Visible="false" />
                                <Rock:RockBoundField
                                    DataField="Id"
                                    HeaderText="Id"
                                    SortExpression="Id"
                                    ItemStyle-Wrap="false"
                                    ItemStyle-HorizontalAlign="Right"
                                    HeaderStyle-HorizontalAlign="Right" />
                                <Rock:RockLiteralField ItemStyle-Wrap="false" HeaderText="Qualifier" ID="lEntityQualifier" SortExpression="Qualifier"/>
                                <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                                <Rock:RockTemplateField>
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
                <asp:panel ID="pnlEntityTypeQualifier" runat="server" Visible="false" class="well">
                    <Rock:EntityTypePicker ID="ddlAttrEntityType" runat="server" Label="Entity Type" IncludeGlobalOption="true" Required="true" AutoPostBack="true" OnSelectedIndexChanged="ddlAttrEntityType_SelectedIndexChanged" EnhanceForLongLists="true" />
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="tbAttrQualifierField" runat="server" Label="Qualifier Field" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="tbAttrQualifierValue" runat="server" Label="Qualifier Value" />
                        </div>
                    </div>
                </asp:panel>
                <Rock:AttributeEditor ID="edtAttribute" runat="server" ShowActions="false" ValidationGroup="Attribute" IsShowInGridVisible="true" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="mdAttributeValue" runat="server" Title="Attribute Value" OnCancelScript="clearActiveDialog();" ValidationGroup="AttributeValue">
            <Content>
                <asp:HiddenField ID="hfIdValues" runat="server" />
                <asp:ValidationSummary ID="ValidationSummaryValue" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="AttributeValue" />
                <Rock:DynamicPlaceholder ID="phEditControls" runat="server" />
            </Content>
        </Rock:ModalDialog>

        <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Danger" Visible="false" />

    </ContentTemplate>
</asp:UpdatePanel>
