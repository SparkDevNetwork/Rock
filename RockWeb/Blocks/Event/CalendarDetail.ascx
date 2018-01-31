<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CalendarDetail.ascx.cs" Inherits="RockWeb.Blocks.Event.CalendarDetail" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>

<asp:UpdatePanel ID="upEventCalendar" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server" Visible="false">
            <asp:HiddenField ID="hfEventCalendarId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><asp:Literal ID="lCalendarIcon" runat="server" />
                    <asp:Literal ID="lReadOnlyTitle" runat="server" /></h1>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <asp:ValidationSummary ID="valEventCalendarDetail" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                <div id="pnlViewDetails" runat="server">
                    <p class="description">
                        <asp:Literal ID="lEventCalendarDescription" runat="server"></asp:Literal>
                    </p>

                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" AccessKey="e" ToolTip="Alt+e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="btnDelete_Click" CausesValidation="false" />
                        <span class="pull-right">
                            <Rock:SecurityButton ID="btnSecurity" runat="server" class="btn btn-sm btn-security" />
                        </span>
                    </div>
                </div>

                <div id="pnlEditDetails" runat="server">

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.EventCalendar, Rock" PropertyName="Name" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbActive" runat="server" SourceTypeName="Rock.Model.EventCalendar, Rock" PropertyName="IsActive" Label="Active" />
                        </div>
                    </div>

                    <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.EventCalendar, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbIconCssClass" runat="server" SourceTypeName="Rock.Model.EventCalendar, Rock" PropertyName="IconCssClass" Label="Calendar CSS Class" />
                        </div>
                        <div class="col-md-6">
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <div class="attributes">
                                <Rock:DynamicPlaceHolder ID="phAttributes" runat="server" />
                            </div>
                        </div>
                    </div>

                    <Rock:PanelWidget ID="wpEventAttributes" runat="server" Title="Event Attributes">
                        <div class="grid">
                            <Rock:Grid ID="gEventAttributes" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Event Attribute" ShowConfirmDeleteDialog="false">
                                <Columns>
                                    <Rock:ReorderField />
                                    <Rock:RockBoundField DataField="Name" HeaderText="Attribute" />
                                    <Rock:RockBoundField DataField="Description" HeaderText="Description" />
                                    <Rock:RockBoundField DataField="FieldType" HeaderText="Field Type" />
                                    <Rock:BoolField DataField="IsRequired" HeaderText="Required" />
                                    <Rock:BoolField DataField="IsGridColumn" HeaderText="Show in Grid" />
                                    <Rock:BoolField DataField="AllowSearch" HeaderText="Allow Search" />
                                    <Rock:EditField OnClick="gEventAttributes_Edit" />
                                    <Rock:DeleteField OnClick="gEventAttributes_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="wpContentChannels" runat="server" Title="Content Channels">
                        <div class="grid">
                            <Rock:Grid ID="gContentChannels" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Content Channel" ShowConfirmDeleteDialog="false">
                                <Columns>
                                    <Rock:RockBoundField DataField="Name" HeaderText="Channel" />
                                    <Rock:DeleteField OnClick="gContentChannels_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </Rock:PanelWidget>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>
                </div>
            </div>
        </asp:Panel>

        <Rock:ModalAlert ID="modalAlert" runat="server" />

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="dlgEventAttribute" runat="server" Title="Calendar Event Attribute" OnSaveClick="dlgEventAttribute_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="Attributes">
            <Content>
                <Rock:AttributeEditor ID="edtEventAttributes" runat="server" ShowActions="false" ValidationGroup="Attributes" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgContentChannel" runat="server" Title="Content Channel" OnSaveClick="dlgContentChannel_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="Channels" SaveButtonText="Select">
            <Content>
                <Rock:RockDropDownList ID="ddlContentChannel" runat="server" Label="Channel" DataValueField="Guid" DataTextField="Name" />
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
