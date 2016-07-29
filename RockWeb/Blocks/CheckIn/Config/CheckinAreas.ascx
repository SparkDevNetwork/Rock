<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CheckinAreas.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Config.CheckinAreas" %>

<style>
    .checkin-item {
        padding: 12px;
        border: 1px solid #d8d1c8;
        cursor: pointer;
        margin-bottom: 6px;
        border-top-width: 3px;
    }

    .checkin-item-selected {
        background-color: #d8d1c8;
    }

    .checkin-list {
        list-style-type: none;
        padding-left: 40px;
    }
    .checkin-list-first {
        padding-left: 0;
    }
    .checkin-item .fa-bars {
        opacity: .5;
        margin-right: 6px;
    }
    
    .checkin-group {
        border-top-color: #afd074;
    }
    
    .checkin-area {
        border-top-color: #5593a4;
    }
</style>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" runat="server" CssClass="panel panel-block js-panel-details">
            <div class="panel-heading"><h3 class="panel-title"><i class="fa fa-list"></i> Areas and Groups</h3></div>
            <div class="panel-body">
    
                <Rock:NotificationBox ID="nbDeleteWarning" runat="server" NotificationBoxType="Warning" />

                <div class="row">
                    <div class="col-md-6">
                        <ul class="checkin-list checkin-list-first js-checkin-area-list">
                            <asp:PlaceHolder ID="phRows" runat="server" />
                        </ul>
                        <div class="pull-right checkin-item-actions"><asp:LinkButton ID="lbAddArea" runat="server" CssClass="btn btn-xs btn-default" OnClick="lbAddArea_Click"><i class="fa fa-plus"></i> <i class="fa fa-folder-open"></i></asp:LinkButton></div>
                    </div>
                    <div class="col-md-6 js-area-group-details">

                        <asp:HiddenField ID="hfIsDirty" runat="server" Value="false" />

                        <asp:ValidationSummary ID="vsDetails" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                        <Rock:NotificationBox ID="nbSaveSuccess" runat="server" NotificationBoxType="Success" Text="Changes have been saved." Visible="false" />

                        <Rock:CheckinArea ID="checkinArea" runat="server" Visible="false" OnAddCheckinLabelClick="checkinArea_AddCheckinLabelClick" OnDeleteCheckinLabelClick="checkinArea_DeleteCheckinLabelClick" />
                        <Rock:CheckinGroup ID="checkinGroup" runat="server" Visible="false" OnAddLocationClick="checkinGroup_AddLocationClick" OnDeleteLocationClick="checkinGroup_DeleteLocationClick" />

                        <div class="actions margin-t-md">
                            <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" Visible="false" />
                        </div>

                    </div>
                </div>
    
            </div>
        </asp:Panel>

        <Rock:ModalDialog ID="mdAddCheckinLabel" runat="server" ScrollbarEnabled="false" ValidationGroup="vgAddCheckinLabel" SaveButtonText="Add" OnSaveClick="mdAddCheckinLabel_SaveClick"  Title="Select Check-in Label">
            <Content>
                <Rock:RockDropDownList ID="ddlCheckinLabel" runat="server" Label="Select Check-in Label" ValidationGroup="vgAddCheckinLabel" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="mdLocationPicker" runat="server" ScrollbarEnabled="false" SaveButtonText="Save" OnSaveClick="mdLocationPicker_SaveClick" Title="Select Check-in Location" ValidationGroup="Location">
            <Content ID="mdLocationPickerContent">
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

                $('section.checkin-item').click(function () {
                    if (!isDirty()) {
                        var $li = $(this).closest('li');
                        if ($(this).hasClass('checkin-area')) {
                            __doPostBack('<%=upDetail.ClientID %>', 'select-area:' + $li.attr('data-key'));
                        } else {
                            __doPostBack('<%=upDetail.ClientID %>', 'select-group:' + $li.attr('data-key'));
                        }
                    }

                    // Used to scroll to the top of the 'details' section when it's currently off-screen.
                    // We tried using the "if (!$('.js-panel-details').visible(true))" approach but it wasn't as awesome.
                    $('html, body').animate({
                        scrollTop: $('.js-panel-details').offset().top + 'px'
                    }, 400);
                });

                // javascript to make the Reorder buttons work on the CheckinGroupTypeEditor controls
                $('.js-checkin-area-list').sortable({
                    helper: fixHelper,
                    handle: '.checkin-area-reorder',
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
                            if (!isDirty()) {
                                var newGroupTypeIndex = $(ui.item).prevAll('li').length;
                                __doPostBack('<%=upDetail.ClientID %>', 're-order-area:' + ui.item.attr('data-key') + ';' + newGroupTypeIndex);
                            }
                        }
                    }
                });

                // javascript to make the Reorder buttons work on the CheckinGroupEditor controls
                $('.js-checkin-group-list').sortable({
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
                            if (!isDirty()) {
                                var newGroupIndex = $(ui.item).prevAll('li').length;
                                __doPostBack('<%=upDetail.ClientID %>', 're-order-group:' + ui.item.attr('data-key') + ';' + newGroupIndex);
                            }
                        }
                    }
                });

            });
        </script>

    </ContentTemplate>
</asp:UpdatePanel>

