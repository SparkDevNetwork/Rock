<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CheckinConfiguration.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.CheckinConfiguration" %>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>
        
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-check-square-o"></i> <asp:Literal ID="lCheckinAreasTitle" runat="server" Text="Check-in Areas" /></h1>
            </div>
            <div class="panel-body">

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                <Rock:NotificationBox ID="nbDeleteWarning" runat="server" NotificationBoxType="Warning" />

                <asp:HiddenField ID="hfParentGroupTypeId" runat="server" />
                <asp:ValidationSummary ID="vsDetails" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                <div class="clearfix">
                    <asp:LinkButton ID="lbAddCheckinArea" runat="server" CssClass="btn btn-action btn-xs pull-right" OnClick="lbAddCheckinArea_Click" CausesValidation="false"><i class="fa fa-plus"></i> Add Check-in Area</asp:LinkButton>
                </div>

                <div class="checkin-grouptype-list">
                    <h4>Check-in Areas</h4>
                    <asp:PlaceHolder ID="phCheckinGroupTypes" runat="server" EnableViewState="false" />
                </div>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>

                <Rock:ConfirmPageUnload ID="confirmExit" runat="server" ConfirmationMessage="Changes have been made to this check-in configuration that have not yet been saved." Enabled="false" />

            </div>
        </asp:Panel>

        <Rock:ModalDialog ID="mdAddCheckinLabel" runat="server" ScrollbarEnabled="false" ValidationGroup="vgAddCheckinLabel" SaveButtonText="Add" OnSaveClick="btnAddCheckinLabel_Click" Title="Select Check-in Label">
            <Content>
                <asp:HiddenField ID="hfAddCheckinLabelGroupTypeGuid" runat="server" />
                <Rock:RockDropDownList ID="ddlCheckinLabel" runat="server" Label="Select Check-in Label" ValidationGroup="vgAddCheckinLabel" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="mdLocationPicker" runat="server" ScrollbarEnabled="false" SaveButtonText="Save" OnSaveClick="btnAddLocation_Click" Title="Select Check-in Location" ValidationGroup="Location">
            <Content ID="mdLocationPickerContent">
                <asp:HiddenField ID="hfAddLocationGroupGuid" runat="server" />
                <Rock:LocationItemPicker ID="locationPicker" runat="server" Label="Check-in Location" ValidationGroup="Location" Required="true" />
            </Content>
        </Rock:ModalDialog>

        <script>

            Sys.Application.add_load(function () {
                var fixHelper = function (e, ui) {
                    ui.children().each(function () {
                        $(this).width($(this).width());
                    });
                    return ui;
                };

                // javascript to make the Reorder buttons work on the CheckinGroupTypeEditor controls
                $('.checkin-grouptype-list').sortable({
                    helper: fixHelper,
                    handle: '.checkin-grouptype-reorder',
                    containment: 'parent',
                    tolerance: 'pointer',
                    start: function (event, ui) {
                        {
                            var start_pos = ui.item.index();
                            ui.item.data('start_pos', start_pos);
                        }
                    },
                    update: function (event, ui) {
                        {
                            $('#' + '<%=btnSave.ClientID %>').addClass('disabled');
                            var newGroupTypeIndex = $(ui.item).prevAll('.checkin-grouptype').length;
                            __doPostBack('<%=upDetail.ClientID %>', 're-order-grouptype:' + ui.item.attr('data-key') + ';' + newGroupTypeIndex);
                        }
                    }
                });

                // javascript to make the Reorder buttons work on the CheckinGroupEditor controls
                $('.checkin-group-list').sortable({
                    helper: fixHelper,
                    handle: '.checkin-group-reorder',
                    containment: 'parent',
                    tolerance: 'pointer',
                    start: function (event, ui) {
                        {
                            var start_pos = ui.item.index();
                            ui.item.data('start_pos', start_pos);
                        }
                    },
                    update: function (event, ui) {
                        {
                            $('#' + '<%=btnSave.ClientID %>').addClass('disabled');
                            var newGroupIndex = $(ui.item).prevAll('.checkin-group').length;
                            __doPostBack('<%=upDetail.ClientID %>', 're-order-group:' + ui.item.attr('data-key') + ';' + newGroupIndex);
                        }
                    }
                });
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
