<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RegistrationInstanceLinkageDetail.ascx.cs" Inherits="RockWeb.Blocks.Event.RegistrationInstanceLinkageDetail" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" runat="server" CssClass="panel panel-block">

            <asp:HiddenField ID="hfLinkageId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><asp:Literal ID="lActionTitle" runat="server" /></h1>
            </div>

            <div class="panel-body">

                <asp:ValidationSummary ID="vsSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                <Rock:NotificationBox ID="NotificationBox1" runat="server" NotificationBoxType="Info" />

                <div class="row">
                    <div class="col-md-5">
                        <Rock:RockTextBox ID="tbLinkageUrlSlug" runat="server" Label="URL Slug" Help="This key, when present in the URL, tells the event registration to use the configured group and campus information." />
                        <asp:CustomValidator runat="server"
                                        ID="cvUrlSlug"
                                        ErrorMessage="URL Slug must be unique across all events."
                                        ControlToValidate="tbLinkageUrlSlug"
                                        OnServerValidate="cvUrlSlug_ServerValidate"
                                        Display="None" />
                        <asp:RegularExpressionValidator runat="server"
                                        ID="rvUrlSlug"
                                        ErrorMessage="URL Slug must be lowercase and cannot contain any special characters other than -"
                                        ControlToValidate="tbLinkageUrlSlug"
                                        ValidationExpression="^[a-z0-9]+(?:-[a-z0-9]+)*$"
                                        Display="None"/>
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:NotificationBox ID="nbGroupTypeWarning" runat="server" NotificationBoxType="Warning" Text="Warning: The group type of the selected group is different than the group type of the registration template. This could prevent registrants from getting added to the group." />
                        <Rock:GroupPicker ID="gpLinkageGroup" runat="server" Label="Group" OnSelectItem="gpLinkageGroup_SelectItem" Help="The optional group the the registrants will be added to when the registration is complete." />
                    </div>
                    <div class="col-md-5 col-md-offset-1">
                        <Rock:CampusPicker ID="cpLinkageCampus" runat="server" Label="Campus" Help="The campus that the registration linkage will be tied to." />
                    </div>
                </div>

                <div class="row mt-3">
                    <div class="col-md-6">
                        <asp:HiddenField ID="hfLinkageEventItemOccurrenceId" runat="server" />
                        <Rock:RockLiteral ID="lLinkageEventItemOccurrence" runat="server" Label="Calendar Item" CssClass="margin-b-none" />
                        <asp:LinkButton ID="lbLinkageEventItemOccurrenceAdd" runat="server" CssClass="btn btn-primary btn-xs margin-b-md" OnClick="lbLinkageEventItemOccurrenceAdd_Click"><i class="fa fa-plus"></i> Add Event Item</asp:LinkButton>
                        <asp:LinkButton ID="lbLinkageEventItemOccurrenceRemove" runat="server" CssClass="btn btn-danger btn-xs margin-b-md" Visible="false" OnClick="lbLinkageEventItemOccurrenceRemove_Click"><i class="fa fa-times"></i> Remove</asp:LinkButton>
                    </div>
                    <div class="col-md-6">

                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbLinkagePublicName" runat="server" Label="Public Name" Required="true" Help="The text to show when creating links to this specific configuration on the event pages." />
                    </div>
                </div>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" data-shortcut-key="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" data-shortcut-key="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>

            </div>

        </asp:Panel>

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="dlgAddCalendarItemPage1" runat="server" Title="New Calendar Item" SaveButtonText="Next" OnSaveClick="dlgAddCalendarItemPage1_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="DlgPage1">
            <Content>
                <asp:ValidationSummary ID="vsLinkageAdd" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="DlgPage1" />
                <Rock:NotificationBox ID="nbNoCalendar" runat="server" NotificationBoxType="Warning" Visible="false" Text="There are not any calendars available for you to add."/>
                <Rock:RockDropDownList ID="ddlCalendar" runat="server" Label="Select Calendar" Required="true" ValidationGroup="DlgPage1" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgAddCalendarItemPage2" runat="server" Title="New Calendar Item" SaveButtonText="Next" OnSaveClick="dlgAddCalendarItemPage2_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="DlgPage2">
            <Content>
                <Rock:DateRangePicker ID="drpLinkageDateRange" runat="server" Label="Date Range" Required="true" ValidationGroup="DlgPage2" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgAddCalendarItemPage3" runat="server" Title="New Calendar Item" SaveButtonText="OK" OnSaveClick="dlgAddCalendarItemPage3_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="DlgPage3">
            <Content>
                <Rock:NotificationBox ID="nbNoLinkage" runat="server" NotificationBoxType="Warning" Visible="false" Text="There are not any calendar items available for the selected calendar and date range."/>
                <div id="divNewCalendarItemPage3" runat="server" class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlCalendarItem" runat="server" Label="Calendar Item" Required="true" ValidationGroup="DlgPage3"
                            DataTextField="Name" DataValueField="Id" AutoPostBack="true" OnSelectedIndexChanged="ddlCalendarItem_SelectedIndexChanged" EnhanceForLongLists="true" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlCalendarItemOccurrence" runat="server" Label="Occurrence" Required="true" ValidationGroup="DlgPage3"
                            DataTextField="Name" DataValueField="Id"  />
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
