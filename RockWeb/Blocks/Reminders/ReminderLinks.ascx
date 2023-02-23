<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ReminderLinks.ascx.cs" Inherits="RockWeb.Blocks.Reminders.ReminderLinks" %>
<%@ Import Namespace="Rock" %>

<script type="text/javascript">
    function clearActiveReminderDialog() {
        $('#<%=hfActiveReminderDialog.ClientID %>').val('');
        refreshReminderCount();
    }

    function checkAddReminderVisibility() {
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

    function refreshReminderCount() {
        // use ajax to pull the updated reminder count.
        var restUrl = Rock.settings.get('baseUrl') + 'api/Reminders/GetReminderCount';
        $.ajax({
            url: restUrl,
            dataType: 'json',
            success: function (data, status, xhr) {
                $('#<%= hfReminderCount.ClientID %>').val(data);
                checkReminders();
            },
            error: function (xhr, status, error) {
                console.log('GetReminderCount status: ' + status + ' [' + error + ']: ' + xhr.reponseText);
            }
        });
    }

    function checkReminders() {
        var reminderCount = $('#<%=hfReminderCount.ClientID %>').val();
        var remindersButton = $('#<%=btnReminders.ClientID %>');
        var buttonHtml = '<i class="fa fa-bell"></i>';

        if (reminderCount != '' && reminderCount != "0") {
            remindersButton.addClass('active has-reminders');
            buttonHtml = buttonHtml + '<span class="count-bottom">' + new Intl.NumberFormat().format(reminderCount) + "</span>";
        }

        remindersButton.html(buttonHtml);
    }

    Sys.Application.add_load(function () {
        checkReminders();
        var remindersButton = $('.js-rock-reminders');

        remindersButton.on('show.bs.dropdown', function () {
            checkAddReminderVisibility();
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
        $('#reminders-show-additional-options').addClass('d-none');
        $('#reminders-additional-options').removeClass('d-none');
    }
</script>

<asp:HiddenField ID="hfReminderCount" runat="server" Value="0" />
<asp:HiddenField ID="hfContextEntityTypeId" runat="server" Value="0" />

<div class="dropdown js-rock-reminders">
    <%-- LinkButton inner html is updated by checkReminders() function. --%>
    <asp:LinkButton runat="server" ID="btnReminders" Visible="false" CssClass="rock-bookmark" href="#" data-toggle="dropdown"><i class="fa fa-bell"></i></asp:LinkButton>
    <asp:Panel ID="pnlReminders" runat="server" CssClass="dropdown-menu js-reminders-container">
        <li class="js-add-reminder d-none">
            <asp:LinkButton runat="server" ID="btnAddReminder" CssClass="" OnClick="btnAddReminder_Click">Add Reminder</asp:LinkButton>
        </li>
        <li>
            <asp:LinkButton runat="server" ID="btnViewReminders" OnClick="btnViewReminders_Click">View Reminders</asp:LinkButton>
        </li>
    </asp:Panel>
</div>

<asp:UpdatePanel ID="upnlReminders" UpdateMode="Conditional" runat="server">
    <ContentTemplate>

        <asp:HiddenField ID="hfActiveReminderDialog" runat="server" />

        <%--
            The popover wrapper is on this div (rather than the UpdatePanel) because the modal dialog needs to be outside
            of the popover (otherwise clicking on certain controls within the modal will cause strange behaviors like
            hiding the modal from view).
        --%>
        <Rock:ModalDialog ID="mdAddReminder" runat="server" ValidationGroup="AddReminder"
            CancelLinkVisible="true" OnCancelScript="clearActiveReminderDialog();"
            SaveButtonText="Save" OnSaveClick="mdAddReminder_SaveClick">
            <Content>
                <asp:UpdatePanel ID="upnlInternalPanel" runat="server">
                    <ContentTemplate>

                        <asp:Panel ID="pnlExistingReminders" runat="server" Visible="false">
                            <h4 class="mt-0">Existing Reminders</h4>

                            <p>
                                <asp:Literal ID="lExistingReminderTextTemplate" runat="server" Visible="false">
                                    You currently have {REMINDER_QUANTITY_TEXT_1} for this {ENTITY_TYPE}. The most {REMINDER_QUANTITY_TEXT_2} listed below. 
                                </asp:Literal>
                                <asp:Literal ID="lExistingReminderText" runat="server" />
                                <asp:LinkButton runat="server" ID="btnViewReminders2" OnClick="btnViewReminders_Click" Visible="false">See your reminders settings for a complete list</asp:LinkButton>
                            </p>

                            <asp:Repeater ID="rptReminders" runat="server" OnItemCommand="rptReminders_ItemCommand">
                                <ItemTemplate>
                                    <div class="row d-flex flex-wrap flex-sm-nowrap margin-b-sm">
                                        <div class="col-xs-6 col-sm flex-grow-0">
                                            <span class="label label-default"><asp:Literal ID="lReminderDate" runat="server" Text='<%# Eval("ReminderDate") %>' /></span>
                                        </div>
                                        <div class="col-xs-12 col-sm mw-100 order-3 order-sm-2">
                                                <div class="note reminder-note">
                                                    <div class="meta">
                                                        <div class="meta-body">
                                                            <span class="note-details">
                                                                <span class="tag-flair">
                                                                    <asp:Literal ID="lIcon" runat="server" Text='<%# "<span class=\"tag-color\" style=\"background-color: " + Eval("HighlightColor") + "\"></span>" %>' />
                                                                    <asp:Literal ID="lReminderType" runat="server"  Text='<%# "<span class=\"tag-label\">" + Eval("ReminderTypeName") + "</span>" %>' />
                                                                </span>
                                                            </span>
                                                        </div>
                                                    </div>
                                                    <div class="note-content">
                                                        <asp:Literal ID="lNote" runat="server"  Text='<%# Eval("Note") %>' />
                                                    </div>
                                                </div>
                                        </div>

                                        <div class="col-xs-6 col-sm order-2 order-sm-3 flex-grow-0 text-right text-nowrap">
                                            <asp:Literal ID="lClock" runat="server" Visible='<%# Eval("IsRenewing") %>'><i class="fa fa-clock-o" title="Recurring Reminder"></i></asp:Literal>

                                            <div class="btn-group dropdown-right ml-1">
                                                <button type="button" class="btn btn-link btn-overflow dropdown-toggle" data-toggle="dropdown">
                                                    <i class="fa fa-ellipsis-v"></i>
                                                </button>
                                                <ul class="dropdown-menu">
                                                    <asp:HiddenField ID="hfReminderId" runat="server" Value='<%# Eval("Id") %>' />
                                                    <li>
                                                        <asp:LinkButton ID="btnMarkComplete" runat="server" Text="Mark Complete" CommandName="MarkComplete" />
                                                        <asp:LinkButton ID="btnCancelReoccurrence" runat="server" Visible='<%# Eval("IsRenewing") %>' Text="Cancel Reoccurrence" CommandName="CancelReoccurrence" />
                                                        <asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CommandName="EditReminder" />
                                                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="dropdown-item-danger" CommandName="DeleteReminder" />
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
                            <h4 class="mt-0">Reminder for <asp:Literal ID="lEntity" runat="server" /></h4>

                            <Rock:DatePicker ID="dpReminderDate" runat="server" Label="Reminder Date" Required="true" ValidationGroup="AddReminder" AllowPastDateSelection="false" />
                            <Rock:RockTextBox ID="tbNote" runat="server" Label="Note" TextMode="MultiLine" />
                            <Rock:RockDropDownList ID="ddlReminderType" runat="server" Label="Reminder Type" ValidationGroup="AddReminder" Required="true" />

                            <div id="reminders-show-additional-options">
                                <a href="javascript:remindersShowAdditionalOptions();">Additional Options</a>
                            </div>

                            <div id="reminders-additional-options" class="d-none">
                                <Rock:PersonPicker ID="ppPerson" runat="server" Label="Assign Reminder To" Required="true" ValidationGroup="AddReminder" EnableSelfSelection="true" />

                                <div class="row">
                                    <div class="col-md-6">
                                        <Rock:NumberBox ID="numbRepeatDays" runat="server" Label="Repeat Every" Help="Will repeat the reminder the provided number of days after the completion." AppendText="days" />
                                    </div>
                                    <div class="col-md-6">
                                        <Rock:NumberBox ID="numbRepeatTimes" runat="server" Label="Number of Times to Repeat" Help="The number of times to repeat.  Leave blank to repeat indefinitely." AppendText="times" />
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
