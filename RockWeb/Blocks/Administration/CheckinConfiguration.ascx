<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CheckinConfiguration.ascx.cs" Inherits="RockWeb.Blocks.Administration.CheckinConfiguration" %>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>
        <div class="row-fluid">
            <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
        </div>

        <asp:Panel ID="pnlDetails" runat="server">

            <asp:ValidationSummary ID="vsDetails" runat="server" CssClass="alert alert-error" />

            <fieldset>
                <legend>
                    <asp:Literal ID="lCheckinAreasTitle" runat="server" Text="Check-in Areas" />
                    <span class="pull-right">
                        <asp:LinkButton ID="lbAddCheckinArea" runat="server" CssClass="btn btn-mini" OnClick="lbAddCheckinArea_Click" CausesValidation="false"><i class="icon-plus"></i> Add Check-in Area</asp:LinkButton>
                    </span>
                </legend>
                <div class="row-fluid checkin-grouptype-list">
                    <asp:PlaceHolder ID="phCheckinGroupTypes" runat="server" />
                </div>
            </fieldset>

            <div class="actions">
                <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
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
                            __doPostBack('<%=upDetail.ClientID %>', 're-order-group:' + ui.item.attr('data-key') + ';' + ui.item.index());
                        }
                    }
                });
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
