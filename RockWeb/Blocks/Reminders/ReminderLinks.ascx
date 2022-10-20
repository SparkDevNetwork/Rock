<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ReminderLinks.ascx.cs" Inherits="RockWeb.Blocks.Reminders.ReminderLinks" %>
<%@ Import Namespace="Rock" %>

<style>
    /* This is a temporary workaround to hide the reminders button - REMOVE THIS when the feature is ready for public deployment. */
    .js-rock-reminders { display:none; }
</style>

<script type="text/javascript">
    function clearActiveReminderDialog() {
        $('#<%=hfActiveReminderDialog.ClientID %>').val('');
    }

    function unregisterReminderEvents() {
        $(document).off('.reminderLinks');
    }
    function registerReminderEvents() {
        var remindersButton = $('.js-rock-reminders');
        var remindersPanel = $('.js-reminders-popover');
        $(document).off('mouseup.reminderLinks').on('mouseup.reminderLinks', function (e) {
            var isRemindersButton = $('.js-rock-reminders').is(e.target) || $('.js-rock-reminders').has(e.target).length != 0;
            // 'js-rock-reminders' has it's own handler, so ignore if this is from js-rock-reminders
            if (isRemindersButton) {
                return;
            }

            // if the target of the click isn't the bookmarkPanel or a descendant of the bookmarkPanel
            var remindersPanel = $('.js-reminders-popover');
            if (!remindersPanel.is(e.target) && remindersPanel.has(e.target).length === 0) {
                hideReminderLinks();
            }
        });

        $(window).on('resize.reminderLinks', function (e) { positionReminderLinks(remindersButton, remindersPanel) });
    }

    function checkAddReminderVisibility() {
        var remindersPanel = $('.js-reminders-popover');
        var remindersPanelVisible = !remindersPanel.hasClass('d-none');
        if (!remindersPanelVisible) {
            return;
        }

        var contextEntityTypeId = $('#<%= hfContextEntityTypeId.ClientID %>').val();
        if (contextEntityTypeId != 0) {
            // use ajax to check if there are active reminder types for this entity type.
            var restUrl = Rock.settings.get('baseUrl') + 'api/ReminderTypes/ReminderTypesExistForEntityType';
            restUrl += '?entityTypeId=' + contextEntityTypeId;
            $.ajax({
                url: restUrl,
                dataType: 'json',
                success: function (data, status, xhr) {
                    if (data) {
                        $('.js-add-reminder').removeClass('d-none');
                    } else {
                        $('.js-add-reminder').addClass('d-none');
                    }
                },
                error: function (xhr, status, error) {
                    console.log('ReminderTypesExistForEntityType status: ' + status + ' [' + error + ']: ' + xhr.reponseText);
                }
            });
        }
    }

    function showReminderLinks($button, $panel) {
        registerReminderEvents();
        if (typeof $button !== 'undefined' && $panel !== null && $panel.hasClass('d-none')) {
            positionReminderLinks($button, $panel);
            $panel.removeClass('d-none');
            checkAddReminderVisibility();
        } else {
            this.hideReminderLinks();
        }
    }

    function hideReminderLinks() {
        $('.js-reminders-popover').addClass('d-none');
        unregisterReminderEvents();
    }

    function positionReminderLinks($button, $panel) {
        var bottom = ($button.position().top + $button.outerHeight(true));
        var buttonRight = ($button.offset().left + $button.outerWidth());
        var buttonCenter = ($button.offset().left + ($button.outerWidth() / 2));

        var left = (buttonCenter - ($panel.outerWidth() / 2));
        var leftMax = $(window).width() - $panel.outerWidth();
        left = Math.max(0, Math.min(left, leftMax));

        $panel.css('top', bottom).css('left', left);
    }

    Sys.Application.add_load(function () {
        var remindersButton = $('.js-rock-reminders');
        var remindersPanel = $('.js-reminders-popover');

        $(remindersButton).off('click').on('click', function (e) {
            e.preventDefault();
            showReminderLinks(remindersButton, remindersPanel);
        });

        Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(pageReloaded);
    });

    // Executed when all page content is refreshed, full page or async postback: https://msdn.microsoft.com/en-us/library/bb397523.aspx
    function pageReloaded(sender, args) {
        var isPostBack = sender.get_isInAsyncPostBack();
        if (isPostBack) {
            checkAddReminderVisibility();
        }
    }

    function remindersShowAdditionalOptions() {
        $('#reminders_show_additional_options').addClass('d-none');
        $('#reminders_additional_options').removeClass('d-none');
    }
</script>

<asp:HiddenField ID="hfContextEntityTypeId" runat="server" Value="0" />

<asp:LinkButton runat="server" ID="lbReminders" Visible="false" CssClass="rock-bookmark js-rock-reminders"
    href="#" ><i class="fa fa-bell"></i><asp:Literal ID="litReminderCount" runat="server"></asp:Literal></asp:LinkButton>

<asp:UpdatePanel ID="upnlReminders" UpdateMode="Conditional" runat="server">
    <ContentTemplate>

        <asp:HiddenField ID="hfActiveReminderDialog" runat="server" />

        <%--
            The popover wrapper is on this div (rather than the UpdatePanel) because the modal dialog needs to be outside
            of the popover (otherwise clicking on certain controls within the modal will cause strange behaviors like
            hiding the modal from view).
            --%>
        <div class="popover rock-popover styled-scroll js-reminders-popover position-fixed d-none" role="tooltip">
            <asp:Panel ID="pnlReminders" runat="server" CssClass="rock-popover js-reminders-container">
                <div class="popover-panel">
                    <div class="popover-content">
                        <asp:LinkButton runat="server" ID="lbViewReminders" OnClick="lbViewReminders_Click">View Reminders</asp:LinkButton><br />
                        <asp:LinkButton runat="server" ID="lbAddReminder" CssClass="js-add-reminder d-none" OnClick="lbAddReminder_Click">Add Reminder</asp:LinkButton>
                    </div>
                </div>
            </asp:Panel>
        </div>

        <Rock:ModalDialog ID="mdAddReminder" runat="server" ValidationGroup="AddReminder"
            CancelLinkVisible="true" OnCancelScript="clearActiveReminderDialog();"
            SaveButtonText="Save" OnSaveClick="mdAddReminder_SaveClick">
            <Content>
                <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                    <ContentTemplate>

                        <asp:Panel ID="pnlExistingReminders" runat="server" Visible="false">
                            <h4>Existing Reminders</h4>

                            <p>
                                <asp:Literal ID="litExistingReminderTextTemplate" runat="server" Visible="false">
                                    You currently have {REMINDER_QUANTITY_TEXT_1} for this {ENTITY_TYPE}.  The most {REMINDER_QUANTITY_TEXT_2} listed below. 
                                </asp:Literal>
                                <asp:Literal ID="litExistingReminderText" runat="server" />
                                <asp:LinkButton runat="server" ID="lbViewReminders2" OnClick="lbViewReminders_Click" Visible="false">See your reminders settings for a complete list</asp:LinkButton>
                            </p>

                            <asp:Repeater ID="rptReminders" runat="server" OnItemCommand="rptReminders_ItemCommand">
                                <ItemTemplate>
                                    <div class="row margin-b-sm">
                                        <div class="col-md-2">
                                            <asp:Literal ID="lReminderDate" runat="server" Text='<%# Eval("ReminderDate") %>' />
                                        </div>
                                        <div class="col-md-8">
                                            <div>
                                                <asp:Literal ID="lIcon" runat="server" Text='<%# "<i class=\"fa fa-circle\" style=\"color: " + Eval("HighlightColor") + "\"></i>" %>' />
                                                <asp:Literal ID="lReminderType" runat="server"  Text='<%# Eval("ReminderType") %>' />
                                            </div>
                                            <div>
                                                <asp:Literal ID="lNote" runat="server"  Text='<%# Eval("Note") %>' />
                                            </div>
                                        </div>

                                        <div class="col-md-2">
                                            <asp:Literal ID="lClock" runat="server" Visible='<%# Eval("IsRenewing") %>'><i class="fa fa-clock-o"></i></asp:Literal>

                                            <div class="btn-group dropdown-right ml-1">
                                                <button type="button" class="btn btn-link btn-overflow dropdown-toggle" data-toggle="dropdown">
                                                    <i class="fa fa-ellipsis-v"></i>
                                                </button>
                                                <ul class="dropdown-menu">
                                                    <li>
                                                        <asp:HiddenField ID="hfReminderId" runat="server" Value='<%# Eval("Id") %>' />
                                                        <asp:LinkButton ID="lbMarkComplete" runat="server" Text="Mark Complete" CommandName="MarkComplete" />
                                                        <asp:LinkButton ID="lbCancelReoccurrence" runat="server" Text="Cancel Reoccurrence" CommandName="CancelReoccurrence" />
                                                        <asp:LinkButton ID="lbEdit" runat="server" Text="Edit" CommandName="EditReminder" />
                                                        <asp:LinkButton ID="lbDelete" runat="server" Text="Delete" CommandName="DeleteReminder" />
                                                    </li>
                                                </ul>
                                            </div>

                                        </div>
                                    </div>
                                    <hr />
                                </ItemTemplate>
                            </asp:Repeater>
                        </asp:Panel>

                        <asp:Panel ID="pnlAddReminder" runat="server">
                            <h4>Reminder for <asp:Literal ID="lEntity" runat="server" /></h4>

                            <div>
                                <Rock:DatePicker ID="rdpReminderDate" runat="server" Label="Reminder Date" Required="true" ValidationGroup="AddReminder" AllowPastDateSelection="false" />
                            </div>

                            <div>
                                <Rock:RockTextBox ID="rtbNote" runat="server" Label="Note" TextMode="MultiLine" />
                            </div>

                            <div>
                                <Rock:RockDropDownList ID="rddlReminderType" runat="server" Label="Reminder Type" ValidationGroup="AddReminder" Required="true" />
                            </div>

                            <div id="reminders_show_additional_options" >
                                <a href="javascript:remindersShowAdditionalOptions();">Additional Options</a>
                            </div>

                            <div id="reminders_additional_options" class="d-none">
                                <div>
                                    <Rock:PersonPicker ID="rppPerson" runat="server" Label="Send Reminder To" Required="true" ValidationGroup="AddReminder" EnableSelfSelection="true" />
                                </div>

                                <div class="row">
                                    <div class="col-md-6">
                                        <Rock:NumberBox ID="rnbRepeatDays" runat="server" Label="Repeat Every" Help="Will repeat the reminder the provided number of days after the completion." AppendText="days" />
                                    </div>
                                    <div class="col-md-6">
                                        <Rock:NumberBox ID="rnbRepeatTimes" runat="server" Label="Number of Times to Repeat" Help="The number of times to repeat.  Leave blank to repeat indefinately." AppendText="times" />
                                    </div>
                                </div>
                            </div>

                        </asp:Panel>

                    </ContentTemplate>
                </asp:UpdatePanel>
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
