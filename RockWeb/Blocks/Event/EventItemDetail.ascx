<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EventItemDetail.ascx.cs" Inherits="RockWeb.Blocks.Event.EventItemDetail" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }

    Sys.Application.add_load( function () {
        $('.js-follow-status').tooltip();
    });
</script>

<asp:UpdatePanel ID="upnlEventItemList" runat="server">
    <Triggers>
        <asp:AsyncPostBackTrigger ControlID="cblCalendars" EventName="SelectedIndexChanged" />
    </Triggers>
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" runat="server" CssClass="panel panel-block" >

            <asp:HiddenField ID="hfEventItemId" runat="server" />

            <div class="panel-heading panel-follow">
                <h1 class="panel-title pull-left">
                    <i class="fa fa-calendar-check-o"></i>
                    <asp:Literal ID="lReadOnlyTitle" runat="server" />
                </h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlStatus" runat="server" />
                    <Rock:HighlightLabel ID="hlApproved" runat="server" />
                </div>
                <asp:Panel runat="server" ID="pnlFollowing" CssClass="panel-follow-status js-follow-status" data-toggle="tooltip" data-placement="top" title="Click to Follow"></asp:Panel>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <asp:ValidationSummary ID="vsSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                <Rock:NotificationBox ID="nbValidation" runat="server" NotificationBoxType="Danger" Visible="false" />

                <div id="pnlViewDetails" runat="server">

                    <div class="row">
                            <div class="col-md-6">
                                <Rock:RockLiteral ID="lSummary" runat="server" Label="Summary" />
                                <Rock:RockLiteral ID="lCalendar" runat="server" Label="Calendars" />
                                <Rock:RockLiteral ID="lAudiences" runat="server" Label="Audiences" />
                            </div>
                            <div class="col-sm-6">
                                <div id="divImage" runat="server" class="margin-b-sm">
                                    <asp:Literal ID="lImage" runat="server" />
                                </div>

                                <Rock:DynamicPlaceHolder id="phAttributesView" runat="server" />
                            </div>
                        </div>


                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" AccessKey="e" ToolTip="Alt+e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link js-delete-event" OnClick="btnDelete_Click" CausesValidation="false" />
                    </div>
                </div>

                <div id="pnlEditDetails" runat="server">

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.EventItem, Rock" PropertyName="Name" />
                        </div>
                        <div class="col-md-6">
                            <div class="row">
                                <div class="col-sm-6">
                                    <Rock:RockCheckBox ID="cbIsActive" runat="server" Label="Active" />
                                </div>
                                <div class="col-sm-6">
                                    <Rock:RockCheckBox ID="cbIsApproved" runat="server" Label="Approved" />
                                    <span class="small"><asp:Literal ID="lApproval" runat="server"></asp:Literal></span>
                                </div>
                            </div>
                        </div>
                    </div>

                    <Rock:DataTextBox ID="tbSummary" runat="server" SourceTypeName="Rock.Model.EventItem, Rock" PropertyName="Summary" TextMode="MultiLine" Rows="4" />

                    <Rock:HtmlEditor ID="htmlDescription" runat="server" Label="Description" Toolbar="Light" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockControlWrapper ID="rcwAudiences" runat="server" Label="Audiences">
                                <div class="grid">
                                    <Rock:Grid ID="gAudiences" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Audience" ShowHeader="false">
                                        <Columns>
                                            <Rock:RockBoundField DataField="Value" />
                                            <Rock:DeleteField OnClick="gAudiences_Delete" />
                                        </Columns>
                                    </Rock:Grid>
                                </div>
                            </Rock:RockControlWrapper>
                            <Rock:RockCheckBoxList ID="cblCalendars" runat="server" Label="Calendars" Help="Calendars that this item should be added to (at least one is required)."
                                OnSelectedIndexChanged="cblCalendars_SelectedIndexChanged" AutoPostBack="true" RepeatDirection="Horizontal" Required="true" />
                            <Rock:RockTextBox ID="tbDetailUrl" runat="server" Label="Details URL" Help="A custom url to use for showing details of the calendar item (if the default item detail page should not be used)."/>
                        </div>
                        <div class="col-md-6">
                            <Rock:ImageUploader ID="imgupPhoto" runat="server" Label="Photo" />
                        </div>
                    </div>

                    <Rock:PanelWidget ID="wpAttributes" runat="server" Title="Event Attribute Values">
                        <Rock:DynamicPlaceHolder ID="phAttributes" runat="server" />
                    </Rock:PanelWidget>

                    <Rock:PanelWidget ID="wpEventOccurrenceAttributes" runat="server" Title="Event Occurrence Attributes">
                        <div class="grid">
                            <Rock:Grid ID="gEventOccurrenceAttributes" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Event Occurrence Attribute" ShowConfirmDeleteDialog="false">
                                <Columns>
                                    <Rock:ReorderField />
                                    <Rock:RockBoundField DataField="Name" HeaderText="Attribute" />
                                    <Rock:RockBoundField DataField="Description" HeaderText="Description" />
                                    <Rock:RockBoundField DataField="FieldType" HeaderText="Field Type" />
                                    <Rock:BoolField DataField="IsRequired" HeaderText="Required" />
                                    <Rock:BoolField DataField="IsGridColumn" HeaderText="Show in Grid" />
                                    <Rock:BoolField DataField="AllowSearch" HeaderText="Allow Search" />
                                    <Rock:EditField OnClick="gEventOccurrenceAttributes_Edit" />
                                    <Rock:DeleteField OnClick="gEventOccurrenceAttributes_Delete" />
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

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="dlgAudience" runat="server" ScrollbarEnabled="false" ValidationGroup="Audience" SaveButtonText="Add" OnCancelScript="clearActiveDialog();" OnSaveClick="btnSaveAudience_Click" Title="Select Audience">
            <Content>
                <asp:ValidationSummary ID="vsAudience" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="Audience" />
                <Rock:RockDropDownList ID="ddlAudience" runat="server" Label="Select Audience" ValidationGroup="Audience" Required="true" DataValueField="Id" DataTextField="Value" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgEventOccurrenceAttribute" runat="server" Title="Event Occurrence Attribute" OnSaveClick="dlgEventOccurrenceAttribute_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="OccurrenceAttributes">
            <Content>
                <Rock:AttributeEditor ID="edtEventOccurrenceAttributes" runat="server" ShowActions="false" ValidationGroup="OccurrenceAttributes" />
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
