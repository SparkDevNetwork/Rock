<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContentChannelTypeDetail.ascx.cs" Inherits="RockWeb.Blocks.Cms.ContentChannelTypeDetail" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server" >
            <asp:HiddenField ID="hfId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-lightbulb-o"></i>
                    <asp:Literal ID="lTitle" runat="server" />
                </h1>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">

                <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />
                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.ContentChannelType, Rock" PropertyName="Name" />
                    </div>
                    <div class="col-md-6">
                        <div class="row">
                            <div class="col-xs-6">
                                <Rock:RockDropDownList ID="ddlDateRangeType" runat="server" Label="Date Range Type" AutoPostBack="true" OnSelectedIndexChanged="ddlDateRangeType_SelectedIndexChanged" />
                            </div>
                            <div class="col-xs-6">
                                <Rock:RockCheckBox ID="cbIncludeTime" runat="server" Label="Include Time"
                                    Help="Should the Date Range include the time of day along with the date?" />
                            </div>
                        </div>
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-6">
                        <div class="row">
                            <div class="col-sm-6">
                                <Rock:RockCheckBox ID="cbDisablePriority" runat="server" Label="Disable Priority" 
                                    Help="Should channels of this type disable the use of priorities?" />
                            </div>
                            <div class="col-sm-6">
                                <Rock:RockCheckBox ID="cbDisableContentField" runat="server" Label="Disable Content Field" 
                                    Help="Should channels of this type disable the use of the content field?" />
                            </div>
                        </div>
                    </div>
                    <div class="col-md-6">
                        <div class="row">
                            <div class="col-sm-6">
                                <Rock:RockCheckBox ID="cbDisableStatus" runat="server" Label="Disable Status"  
                                    Help="Should channels of this type disable the use of the status and all be treated as 'Approved'?" />
                            </div>
                            <div class="col-sm-6">
                                <Rock:RockCheckBox ID="cbShowInChannelList" runat="server" Label="Show in Channel Lists"
                                    Help="Unchecking this option means any channel block's 'Channel Types Include' settings MUST specifically include the type in order to show it." />
                            </div>
                        </div>
                    </div>
                </div>

                <Rock:PanelWidget ID="wpChannelAttributes" runat="server" Title="Channel Attributes">
                    <div class="grid">
                        <Rock:Grid ID="gChannelAttributes" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Channel Attribute" ShowConfirmDeleteDialog="false">
                            <Columns>
                                <Rock:ReorderField />
                                <Rock:RockBoundField DataField="Name" HeaderText="Attribute" />
                                <Rock:RockBoundField DataField="Description" HeaderText="Description" />
                                <Rock:BoolField DataField="IsRequired" HeaderText="Required" />
                                <Rock:EditField OnClick="gChannelAttributes_Edit" />
                                <Rock:DeleteField OnClick="gChannelAttributes_Delete" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </Rock:PanelWidget>

                <Rock:PanelWidget ID="wpItemAttributes" runat="server" Title="Item Attributes">
                    <div class="grid">
                        <Rock:Grid ID="gItemAttributes" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Item Attribute" ShowConfirmDeleteDialog="false">
                            <Columns>
                                <Rock:ReorderField />
                                <Rock:RockBoundField DataField="Name" HeaderText="Attribute" />
                                <Rock:RockBoundField DataField="Description" HeaderText="Description" />
                                <Rock:BoolField DataField="IsRequired" HeaderText="Required" />
                                <Rock:EditField OnClick="gItemAttributes_Edit" />
                                <Rock:DeleteField OnClick="gItemAttributes_Delete" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </Rock:PanelWidget>

                <div class="actions">
                    <asp:LinkButton ID="lbSave" runat="server" data-shortcut-key="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="lbSave_Click" />
                    <asp:LinkButton ID="lbCancel" runat="server" data-shortcut-key="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="lbCancel_Click" />
                </div>

            </div>

        </asp:Panel>

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="dlgChannelAttributes" runat="server" Title="Content Channel Attributes" OnSaveClick="dlgChannelAttributes_SaveClick"  OnCancelScript="clearActiveDialog();" ValidationGroup="ChannelAttributes">
            <Content>
                <Rock:AttributeEditor ID="edtChannelAttributes" runat="server" ShowActions="false" ValidationGroup="ChannelAttributes" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgItemAttributes" runat="server" Title="Content Item Attributes" OnSaveClick="dlgItemAttributes_SaveClick"  OnCancelScript="clearActiveDialog();" ValidationGroup="ItemAttributes">
            <Content>
                <Rock:AttributeEditor ID="edtItemAttributes" runat="server" ShowActions="false" ValidationGroup="ItemAttributes" />
            </Content>
        </Rock:ModalDialog>


    </ContentTemplate>
</asp:UpdatePanel>
