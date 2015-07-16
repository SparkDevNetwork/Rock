<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CalendarItemDetail.ascx.cs" Inherits="RockWeb.Blocks.Event.CalendarItemDetail" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>

<asp:UpdatePanel ID="upnlEventItemList" runat="server">
    <Triggers>
        <asp:AsyncPostBackTrigger ControlID="cblAdditionalCalendars" EventName="SelectedIndexChanged" />
    </Triggers>
    <ContentTemplate>

        <div class="wizard">

            <div class="wizard-item complete">
                <asp:LinkButton ID="lbCalendarDetail" runat="server" OnClick="lbCalendarDetail_Click" >
                    <div class="wizard-item-icon">
                        <i class="fa fa-fw fa-calendar"></i>
                    </div>
                    <div class="wizard-item-label">
                        Calendar
                    </div>
                </asp:LinkButton>
            </div>
    
            <div class="wizard-item active">
                <div class="wizard-item-icon">
                    <i class="fa fa-fw fa-calendar-o"></i>
                </div>
                <div class="wizard-item-label">
                     Calendar Item
                </div>
            </div>
    
            <div class="wizard-item">
                <div class="wizard-item-icon">
                    <i class="fa fa-fw fa-building-o"></i>
                </div>
                <div class="wizard-item-label">
                    Campus Detail
                </div>
            </div>
        </div>

        <asp:Panel ID="pnlDetails" runat="server" CssClass="panel panel-block" >

            <asp:HiddenField ID="hfEventItemId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title pull-left">
                    <i class="fa fa-calendar-o"></i>
                    <asp:Literal ID="lReadOnlyTitle" runat="server" />
                </h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlStatus" runat="server" />
                    <Rock:HighlightLabel ID="hlApproved" runat="server" />
                </div>
            </div>

            <div class="panel-body">

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <asp:ValidationSummary ID="vsSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                <div id="pnlViewDetails" runat="server">

                    <div class="row">
                        <div class="col-md-12">
                            <div id="divImage" runat="server" class="photo photoframe pull-right margin-l-sm margin-b-sm">
                                <asp:Literal ID="lImage" runat="server" />
                            </div>

                            <p>
                                <asp:Literal ID="lSummary" runat="server"></asp:Literal>
                            </p>

                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockLiteral ID="lCalendar" runat="server" Label="Calendars" />
                                    <Rock:RockLiteral ID="lAudiences" runat="server" Label="Audiences" />
                                </div>
                                <div class="col-sm-6">
                                    <asp:PlaceHolder id="phAttributesView" runat="server" /> 
                               </div>
                            </div>

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
                            <Rock:RockCheckBoxList ID="cblAdditionalCalendars" runat="server" Label="Additional Calendars" 
                                OnSelectedIndexChanged="cblAdditionalCalendars_SelectedIndexChanged" AutoPostBack="true"
                                RepeatDirection="Horizontal" />
                            <Rock:RockTextBox ID="tbDetailUrl" runat="server" Label="Details URL" />
                        </div>
                        <div class="col-md-6">
                            <Rock:ImageUploader ID="imgupPhoto" runat="server" Label="Photo" />
                        </div>
                    </div>

                    <Rock:PanelWidget ID="wpAttributes" runat="server" Title="Attribute Values">
                        <asp:PlaceHolder ID="phAttributes" runat="server" EnableViewState="false"></asp:PlaceHolder>
                    </Rock:PanelWidget>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>
                </div>
            </div>

        </asp:Panel>

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="dlgAudience" runat="server" ScrollbarEnabled="false" ValidationGroup="Audience" SaveButtonText="Add" OnCancelScript="clearActiveDialog();" OnSaveClick="btnSaveAudience_Click" Title="Select Audience">
            <Content>
                <asp:ValidationSummary ID="vsAudience" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="Audience" />
                <Rock:RockDropDownList ID="ddlAudience" runat="server" Label="Select Audience" ValidationGroup="Audience" Required="true" DataValueField="Id" DataTextField="Value" />
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
