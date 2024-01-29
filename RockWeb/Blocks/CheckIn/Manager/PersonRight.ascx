<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonRight.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Manager.PersonRight" %>

<script type="text/javascript">
    Sys.Application.add_load(function () {

        $('.js-cancel-checkin').on('click', function (event) {
            event.stopImmediatePropagation();
            var personName = $('h1.js-checkin-person-name').first().text().trim();
            return Rock.dialogs.confirmDelete(event, 'Checkin for ' + personName);
        });

        // ask ZebraPrinterPlugin if it running using the Windows CheckinClient or IPad app
        // If so, we can present the 'local printer' option for label reprints
        if (typeof (ZebraPrintPlugin) != 'undefined' && ZebraPrintPlugin.hasClientPrinter()) {
            $('.js-has-client-printer').val(1);
        }

    });
</script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <!-- Gender, Age, Badges & the Reprint Label action -->
        <div class="panel panel-block">
            <div class="row no-gutters mx-2 d-flex flex-wrap flex-column flex-sm-row">

                <div class="col d-flex flex-grow-1 justify-content-center justify-content-sm-start align-items-center">
                    <div class="profile-widget widget-gender p-2 text-center">
                        <asp:Literal ID="lGender" runat="server" />
                    </div>
                    <div class="profile-widget widget-age p-2 text-center">
                        <asp:Literal ID="lAge" runat="server" />
                    </div>
                    <div class="profile-widget widget-grade p-2 text-center">
                        <asp:Literal ID="lGrade" runat="server" />
                    </div>
                </div>

                <div class="col d-flex flex-column justify-content-center align-items-center">
                    <Rock:HiddenFieldWithClass ID="hfHasClientPrinter" runat="server" CssClass="js-has-client-printer" />
                    <Rock:NotificationBox ID="nbReprintMessage" runat="server" CssClass="js-reprintlabel-notification" Visible="false"></Rock:NotificationBox>
                    <Rock:ModalAlert ID="maNoLabelsFound" runat="server"></Rock:ModalAlert>
                    <asp:HiddenField ID="hfCurrentAttendanceIds" runat="server" />
                    <asp:HiddenField ID="hfPersonId" runat="server" />
                    <div>
                        <asp:LinkButton ID="btnPersonAttendanceHistory" runat="server" OnClick="btnPersonAttendanceHistory_Click" ToolTip="Attendance History" CssClass="btn btn-default btn-sm my-2"><i class="fa fa-history"></i></asp:LinkButton>
                        <asp:LinkButton ID="btnReprintLabels" runat="server" OnClick="btnReprintLabels_Click" ToolTip="Reprint Labels" CssClass="btn btn-default btn-sm my-2"><i class="fa fa-print"></i></asp:LinkButton>
                    </div>
                    <Rock:ModalDialog ID="mdReprintLabels" runat="server" ValidationGroup="vgReprintLabels" Title="Label Reprints" OnSaveClick="mdReprintLabels_PrintClick" SaveButtonText="Print" Visible="false">
                        <Content>
                            <Rock:NotificationBox ID="nbReprintLabelMessages" runat="server" NotificationBoxType="Validation"></Rock:NotificationBox>
                            <Rock:RockCheckBoxList ID="cblLabels" runat="server" Label="Labels" DataTextField="Name" DataValueField="FileGuid"></Rock:RockCheckBoxList>
                            <Rock:RockDropDownList ID="ddlPrinter" runat="server" Label="Printer" />
                        </Content>
                    </Rock:ModalDialog>
                </div>

            </div>
            <div class="row no-gutters d-flex flex-wrap">
                <div class="badge-zone d-flex flex-grow-1 justify-content-center justify-content-md-start align-items-center border-top border-gray-400">
                    <Rock:BadgeListControl ID="blBadgesLeft" runat="server" />
                </div>
                <div class="badge-zone d-flex flex-grow-1 justify-content-center justify-content-md-end align-items-center border-top border-gray-400">
                    <Rock:BadgeListControl ID="blBadgesRight" runat="server" />
                </div>
            </div>
        </div>

        <!-- Check-in History -->
        <asp:Panel ID="pnlCheckinHistory" runat="server" CssClass="panel panel-block">
            <Rock:Grid ID="gAttendanceHistory" runat="server" DisplayType="Light" UseFullStylesForLightGrid="true" AllowPaging="false" CssClass="table-condensed" OnRowDataBound="gAttendanceHistory_RowDataBound" ShowActionRow="false" OnRowSelected="gAttendanceHistory_RowSelected">
                <Columns>
                    <Rock:RockTemplateField HeaderText="When">
                        <ItemTemplate>
                            <span class="text-sm">
                                <asp:Literal ID="lCheckinDate" runat="server" /></span>
                            <span class="d-block text-sm text-muted">
                                <asp:Literal ID="lCheckinScheduleName" runat="server" /></span>
                            <asp:Literal ID="lWhoCheckedIn" runat="server" />
                        </ItemTemplate>
                    </Rock:RockTemplateField>
                    <Rock:RockTemplateField HeaderText="Location">
                        <ItemTemplate>
                            <span class="text-sm">
                                <asp:Literal ID="lLocationName" runat="server" /></span>
                            <span class="d-block text-sm text-muted">
                                <asp:Literal ID="lGroupName" runat="server" /></span>
                        </ItemTemplate>
                    </Rock:RockTemplateField>
                    <Rock:RockTemplateField HeaderText="Code" ItemStyle-CssClass="align-middle">
                        <ItemTemplate>
                            <asp:Literal ID="lCode" runat="server" />
                            <asp:Literal ID="lActiveLabel" runat="server" /><br />
                        </ItemTemplate>
                    </Rock:RockTemplateField>
                    <Rock:RockLiteralField ID="lChevronRight" Text="<i class='fa fa-chevron-right'></i>" ItemStyle-HorizontalAlign="Right" ItemStyle-CssClass="align-middle" />
                </Columns>
            </Rock:Grid>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
