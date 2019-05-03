<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CheckinAreas.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Config.CheckinAreas" %>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>
        <asp:HiddenField runat="server" ID="hfAreaGroupClicked" />
        <asp:Panel ID="pnlDetails" runat="server" CssClass="panel panel-block js-panel-details">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-list"></i>
                    Areas and Groups
                </h1>
                <div class="pull-right">
                    <asp:CheckBox Text="Show Inactive Groups" ID="cbShowInactive" AutoPostBack="true" OnCheckedChanged="cbShowInactive_CheckedChanged" runat="server" />
                </div>
            </div>
            <div class="panel-body">

                <Rock:NotificationBox ID="nbDeleteWarning" runat="server" NotificationBoxType="Warning" />

                <div class="row">
                    <div class="col-md-6">
                        <ul class="checkin-list checkin-list-first js-checkin-area-list">
                            <asp:PlaceHolder ID="phRows" runat="server" />
                        </ul>
                        <div class="pull-right checkin-item-actions">
                            <asp:LinkButton ID="lbAddArea" runat="server" ToolTip="Add New Area" CssClass="btn btn-xs btn-default" OnClick="lbAddArea_Click"><i class="fa fa-plus"></i> <i class="fa fa-folder-open"></i></asp:LinkButton>
                        </div>
                    </div>
                    <div class="col-md-6 js-area-group-details">

                        <asp:HiddenField ID="hfIsDirty" runat="server" Value="false" />

                        <asp:ValidationSummary ID="vsDetails" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                        <Rock:NotificationBox ID="nbInvalid" runat="server" NotificationBoxType="Danger" Visible="false" />
                        <Rock:NotificationBox ID="nbSaveSuccess" runat="server" NotificationBoxType="Success" Text="Changes have been saved." Visible="false" />

                        <Rock:CheckinArea ID="checkinArea" runat="server" Visible="false" OnAddCheckinLabelClick="checkinArea_AddCheckinLabelClick" OnDeleteCheckinLabelClick="checkinArea_DeleteCheckinLabelClick" />
                        <Rock:CheckinGroup ID="checkinGroup" runat="server" Visible="false" OnAddLocationClick="checkinGroup_AddLocationClick" OnDeleteLocationClick="checkinGroup_DeleteLocationClick" OnReorderLocationClick="checkinGroup_ReorderLocationClick" />

                        <div class="actions margin-t-md">
                            <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" Visible="false" />
                            <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="btnDelete_Click" Visible="false" />

                        </div>

                    </div>
                </div>

            </div>
        </asp:Panel>

        <Rock:ModalDialog ID="mdAddCheckinLabel" runat="server" ScrollbarEnabled="false" ValidationGroup="vgAddCheckinLabel" SaveButtonText="Add" OnSaveClick="mdAddCheckinLabel_SaveClick" Title="Select Check-in Label">
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
            /* This function is called after post back to animate scroll to the proper element
             * if the user just clicked an area/group.
            */
            var AfterPostBack = function () {
                // Detect if the two panels are side by side or in one column by finding the delta between the two.
                // If the offset is more than 58-80 then scroll to the js-area-group-details instead.
                if ($('#<%=hfAreaGroupClicked.ClientID %>').val() == "true" && $('.js-area-group-details').length && $('.js-panel-details').length) {
                    $('#<%=hfAreaGroupClicked.ClientID %>').val("false");
                    var panelDelta = $('.js-area-group-details').offset().top - $('.js-panel-details').offset().top;
                    var scrollToPanel = ".js-panel-details";
                    if (panelDelta > 80) {
                        scrollToPanel = ".js-area-group-details";
                    }

                    $('html, body').animate({
                        scrollTop: $(scrollToPanel).offset().top + 'px'
                    }, 400
                    );
                }
            }

            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(AfterPostBack);

            Sys.Application.add_load(function () {

                var fixHelper = function (e, ui) {
                    ui.children().each(function () {
                        $(this).width($(this).width());
                    });
                    return ui;
                };

                $('section.checkin-item').click(function () {
                    if (!isDirty()) {
                        var dataKeyValue = $(this).closest('li').attr('data-key');
                        var isCheckinArea = $(this).hasClass('checkin-area');
                        var postbackArg;
                        if (isCheckinArea) {
                            var postbackArg = 'select-area:' + dataKeyValue;
                        } else {
                            var postbackArg = 'select-group:' + dataKeyValue;
                        }

                        window.location = "javascript:__doPostBack('<%=upDetail.ClientID %>', '" + postbackArg + "')";
                    }
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
                                var dataKeyValue = ui.item.attr('data-key');
                                var postbackArg = 're-order-area:' + dataKeyValue + ';' + newGroupTypeIndex;
                                window.location = "javascript:__doPostBack('<%=upDetail.ClientID %>', '" + postbackArg + "')";
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
                                var dataKeyValue = ui.item.attr('data-key');
                                var postbackArg = 're-order-group:' + dataKeyValue + ';' + newGroupIndex;
                                window.location = "javascript:__doPostBack('<%=upDetail.ClientID %>', '" + postbackArg + "')";
                            }
                        }
                    }
                });
            });


        </script>

    </ContentTemplate>
</asp:UpdatePanel>

