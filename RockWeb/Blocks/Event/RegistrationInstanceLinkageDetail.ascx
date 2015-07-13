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

                <asp:ValidationSummary ID="vsSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                <Rock:NotificationBox ID="NotificationBox1" runat="server" NotificationBoxType="Info" />

                <div class="row">
                    <div class="col-md-6">
                        <asp:HiddenField ID="hfLinkageEventItemCampusId" runat="server" />
                        <Rock:RockLiteral ID="lLinkageEventItemCampus" runat="server" Label="Calendar Item" CssClass="margin-b-none" />
                        <asp:LinkButton ID="lbLinkageEventItemCampusAdd" runat="server" CssClass="btn btn-primary btn-xs margin-b-md" OnClick="lbLinkageEventItemCampusAdd_Click"><i class="fa fa-plus"></i> Add Event Item</asp:LinkButton>
                        <asp:LinkButton ID="lbLinkageEventItemCampusRemove" runat="server" CssClass="btn btn-danger btn-xs margin-b-md" Visible="false" OnClick="lbLinkageEventItemCampusRemove_Click"><i class="fa fa-times"></i> Remove</asp:LinkButton>
                    </div>
                    <div class="col-md-6">
                        <Rock:GroupPicker ID="gpLinkageGroup" runat="server" Label="Group" ValidationGroup="Linkage" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbLinkagePublicName" runat="server" Label="Public Name" Required="true" ValidationGroup="Linkage" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbLinkageUrlSlug" runat="server" Label="URL Slug" ValidationGroup="Linkage" />
                    </div>
                </div>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>

            </div>

        </asp:Panel>

        <asp:HiddenField ID="hfActiveDialog" runat="server" />
    
        <Rock:ModalDialog ID="dlgAddCalendarItemPage1" runat="server" Title="New Calendar Item" SaveButtonText="Next" OnSaveClick="dlgAddCalendarItemPage1_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="DlgPage1">
            <Content>
                <asp:ValidationSummary ID="vsLinkageAdd" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="DlgPage1" />
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
                            DataTextField="Name" DataValueField="Id" AutoPostBack="true" OnSelectedIndexChanged="ddlCalendarItem_SelectedIndexChanged"/>
                    </div>
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlCalendarItemCampus" runat="server" Label="Campus" Required="true" ValidationGroup="DlgPage3"
                            DataTextField="Name" DataValueField="Id"  />
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
