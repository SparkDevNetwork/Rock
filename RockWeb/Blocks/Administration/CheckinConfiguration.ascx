<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CheckinConfiguration.ascx.cs" Inherits="RockWeb.Blocks.Administration.CheckinConfiguration" %>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>
        <div class="row-fluid">
            <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
            <Rock:NotificationBox ID="nbDeleteWarning" runat="server" NotificationBoxType="Warning" />
        </div>

        <asp:Panel ID="pnlDetails" runat="server">

            <asp:HiddenField ID="hfParentGroupTypeId" runat="server" />
            <asp:ValidationSummary ID="vsDetails" runat="server" CssClass="alert alert-error" />

            <fieldset>
                <legend>
                    <asp:Literal ID="lCheckinAreasTitle" runat="server" Text="Check-in Areas" />
                    <span class="pull-right">
                        <asp:LinkButton ID="lbAddCheckinArea" runat="server" CssClass="btn btn-mini" OnClick="lbAddCheckinArea_Click" CausesValidation="false"><i class="icon-plus"></i> Add Check-in Area</asp:LinkButton>
                    </span>
                </legend>
                <div class="row-fluid checkin-grouptype-list">
                    <asp:PlaceHolder ID="phCheckinGroupTypes" runat="server" EnableViewState="false" />
                </div>
            </fieldset>

            <div class="actions">
                <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>

            <Rock:ConfirmPageUnload ID="confirmExit" runat="server" ConfirmationMessage="Changes have been made to this check-in configuration that have not yet been saved." Enabled="false" />

        </asp:Panel>

        <asp:Panel ID="pnlCheckinLabelPicker" runat="server" Visible="false">
            <asp:HiddenField ID="hfAddCheckinLabelGroupTypeGuid" runat="server" />
            <Rock:LabeledDropDownList ID="ddlCheckinLabel" runat="server" LabelText="Select Check-in Label" />

            <div class="actions">
                <asp:LinkButton ID="btnAddCheckinLabel" runat="server" Text="Add" CssClass="btn btn-primary" OnClick="btnAddCheckinLabel_Click"></asp:LinkButton>
                <asp:LinkButton ID="btnCancelAddCheckinLabel" runat="server" Text="Cancel" CssClass="btn" OnClick="btnCancelAddCheckinLabel_Click"></asp:LinkButton>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlLocationPicker" runat="server" Visible="false">
            <asp:HiddenField ID="hfAddLocationGroupGuid" runat="server" />
            <Rock:LabeledDropDownList ID="ddlLocation" runat="server" LabelText="Select Check-in Location" />

            <div class="actions">
                <asp:LinkButton ID="btnAddLocation" runat="server" Text="Add" CssClass="btn btn-primary" OnClick="btnAddLocation_Click"></asp:LinkButton>
                <asp:LinkButton ID="btnCancelAddLocation" runat="server" Text="Cancel" CssClass="btn" OnClick="btnCancelAddLocation_Click"></asp:LinkButton>
            </div>
        </asp:Panel>

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
                            __doPostBack('<%=upDetail.ClientID %>', 're-order-grouptype:' + ui.item.attr('data-key') + ';' + ui.item.index());
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
                            __doPostBack('<%=upDetail.ClientID %>', 're-order-group:' + ui.item.attr('data-key') + ';' + ui.item.index());
                        }
                    }
                });
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
