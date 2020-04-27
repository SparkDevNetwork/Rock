<%@ Control Language="C#" AutoEventWireup="true" CodeFile="UserPreferenceList.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.ReportingTools.UserPreferenceList" %>

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
                    <h1 class="panel-title"><i class="fa fa-list-ul"></i>User Preference List</h1>
                </div>
                <div class="panel-body">
                    <asp:Panel ID="pnlGrid" runat="server" CssClass="grid grid-panel">
                        <Rock:GridFilter ID="rFilter" runat="server" OnDisplayFilterValue="rFilter_DisplayFilterValue" OnClearFilterClick="rFilter_ClearFilterClick">
                            <Rock:PersonPicker ID="ppPerson" runat="server" Label="Person" />
                            <Rock:RockTextBox ID="rtbAttributeKey" runat="server" Label="Attribute Key" />
                        </Rock:GridFilter>
                        <Rock:Grid ID="rGrid" runat="server" RowItemText="setting" TooltipField="Description" OnRowSelected="rGrid_RowSelected">
                            <Columns>
                                <Rock:RockBoundField DataField="Person.FullName" HeaderText="Name" SortExpression="Name" />
                                <Rock:RockBoundField DataField="AttributeValue.Attribute.Key" HeaderText="Attribute Key" SortExpression="AttributeKey" />
                                <Rock:RockBoundField DataField="AttributeValue.Value" HeaderText="Value" SortExpression="Value" />
                                <Rock:DeleteField OnClick="rGrid_Delete" />
                            </Columns>
                        </Rock:Grid>
                    </asp:Panel>
                </div>
            </div>
        </asp:Panel>

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="mdAttributeValue" runat="server" Title="Attribute Value" OnCancelScript="clearActiveDialog();" ValidationGroup="AttributeValue">
            <Content>
                <asp:HiddenField ID="hfAttributeValueId" runat="server" />
                <asp:ValidationSummary ID="ValidationSummaryValue" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="AttributeValue" />
                <Rock:DynamicPlaceholder ID="phEditControls" runat="server" />
            </Content>
        </Rock:ModalDialog>

        <Rock:NotificationBox ID="nbMessage" runat="server" NotificationBoxType="Warning" Visible="false" />

    </ContentTemplate>
</asp:UpdatePanel>
