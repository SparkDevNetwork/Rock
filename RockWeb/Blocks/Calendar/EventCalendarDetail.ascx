<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EventCalendarDetail.ascx.cs" Inherits="RockWeb.Blocks.Calendar.EventCalendarDetail" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>

<asp:UpdatePanel ID="upEventCalendar" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server" Visible="false">
            <asp:HiddenField ID="hfCalendarTypeId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-calendar"></i>
                    <asp:Literal ID="lReadOnlyTitle" runat="server" /></h1>
            </div>
            <div class="panel-body">

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                <Rock:NotificationBox ID="nbNotAllowedToEdit" runat="server" NotificationBoxType="Danger" Visible="false"
                    Text="You are not authorized to save group with the selected group type and/or parent group." />
                <asp:ValidationSummary ID="valEventCalendarDetail" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                <div id="pnlViewDetails" runat="server">
                    <p class="description">
                        <asp:Literal ID="lEventCalendarDescription" runat="server"></asp:Literal>
                    </p>

                    <div class="row">
                        <div class="cold-md-12">
                            <asp:Literal ID="lblMainDetails" runat="server" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" AccessKey="e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="btnDelete_Click" CausesValidation="false" />
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

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.EventCalendar, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbIconCssClass" runat="server" SourceTypeName="Rock.Model.EventCalendar, Rock" PropertyName="IconCssClass" />
                        </div>
                        <div class="col-md-6">
                        </div>
                    </div>

                    <Rock:PanelWidget ID="wpEventCalendarAttributes" runat="server" Title="Calendar Item Attributes">
                        <div class="grid">
                            <Rock:Grid ID="gEventCalendarAttributes" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Event Calendar Attribute">
                                <Columns>
                                    <Rock:ReorderField />
                                    <Rock:RockBoundField DataField="Name" HeaderText="Attribute" />
                                    <Rock:RockBoundField DataField="FieldType" HeaderText="Field Type" />
                                    <Rock:BoolField DataField="AllowSearch" HeaderText="Allow Search" />
                                    <Rock:EditField OnClick="gEventCalendarAttributes_Edit" />
                                    <Rock:DeleteField OnClick="gEventCalendarAttributes_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </Rock:PanelWidget>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>
                </div>
            </div>
        </asp:Panel>

        <Rock:ModalAlert ID="modalAlert" runat="server" />

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="dlgEventCalendarAttribute" runat="server" Title="Event Calendar Attributes" OnSaveClick="dlgEventCalendarAttribute_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="EventCalendarAttributes">
            <Content>
                <Rock:AttributeEditor ID="edtEventCalendarAttributes" runat="server" ShowActions="false" ValidationGroup="EventCalendarAttributes" />
            </Content>
        </Rock:ModalDialog>
    </ContentTemplate>
</asp:UpdatePanel>