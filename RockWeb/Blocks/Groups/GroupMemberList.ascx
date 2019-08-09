<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupMemberList.ascx.cs" Inherits="RockWeb.Blocks.Groups.GroupMemberList" %>

<script type="text/javascript">
    Sys.Application.add_load(function () {
        $('.js-group-member-note').tooltip();

        // data view sync list popover
        $('.js-sync-popover').popover()
    });
</script>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlContent" runat="server">

            <div id="pnlGroupMembers" runat="server">

                <div class="panel panel-block">

                    <div class="panel-heading">
                        <h1 class="panel-title pull-left">
                            <i class="fa fa-users"></i>
                            <asp:Literal ID="lHeading" runat="server" Text="Group Members" />
                        </h1>

                        <div class="panel-labels">
                            <span runat="server" ID="spanSyncLink" Visible="false" data-toggle="popover" class="label label-info js-sync-popover" data-trigger="hover click focus" data-html="true" data-placement="left" data-delay="{&quot;hide&quot;: 1500}" ><i class='fa fa-exchange'></i></span> &nbsp;
                        </div>
                    </div>

                    <div class="panel-body">
                        <Rock:NotificationBox ID="nbRoleWarning" runat="server" CssClass="alert-grid" NotificationBoxType="Warning" Title="No roles!" Visible="false" />
                        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

                        <div class="grid grid-panel">
                            <Rock:GridFilter ID="rFilter" runat="server" OnDisplayFilterValue="rFilter_DisplayFilterValue" OnClearFilterClick="rFilter_ClearFilterClick">
                                <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" />
                                <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" />
                                <Rock:RockCheckBoxList ID="cblRole" runat="server" Label="Role" DataTextField="Name" DataValueField="Id" RepeatDirection="Horizontal" />
                                <Rock:RockCheckBoxList ID="cblGroupMemberStatus" runat="server" Label="Group Member Status" RepeatDirection="Horizontal" />
                                <Rock:CampusPicker ID="cpCampusFilter" runat="server" Label="Family Campus" />
                                <Rock:RockCheckBoxList ID="cblGenderFilter" runat="server" RepeatDirection="Horizontal" Label="Gender">
                                    <asp:ListItem Text="Male" Value="Male" />
                                    <asp:ListItem Text="Female" Value="Female" />
                                    <asp:ListItem Text="Unknown" Value="Unknown" />
                                </Rock:RockCheckBoxList>
                                <Rock:RockDropDownList ID="ddlRegistration" runat="server" Label="Registration" DataTextField="Name" DataValueField="Id" />
                                <Rock:RockDropDownList ID="ddlSignedDocument" runat="server" Label="Signed Document" >
                                    <asp:ListItem Text="" Value="" />
                                    <asp:ListItem Text="Yes" Value="Yes" />
                                    <asp:ListItem Text="No" Value="No" />
                                </Rock:RockDropDownList>
                                <Rock:DateRangePicker ID="drpDateAdded" runat="server" Label="Date Added" />
                                <asp:PlaceHolder ID="phAttributeFilters" runat="server" />
                            </Rock:GridFilter>
                            <Rock:Grid ID="gGroupMembers" runat="server" DisplayType="Full" AllowSorting="true" OnRowSelected="gGroupMembers_Edit" CssClass="js-grid-group-members" OnRowDataBound="gGroupMembers_RowDataBound" ExportSource="ColumnOutput" OnRowCreated="gGroupMembers_RowCreated" >
                                <Columns>
                                    <Rock:SelectField></Rock:SelectField>
                                    <Rock:RockLiteralField ID="lExportFullName" HeaderText="Name" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                    <Rock:RockLiteralField ID="lNameWithHtml" HeaderText="Name" SortExpression="Person.LastName,Person.NickName" ExcelExportBehavior="NeverInclude" />

                                    <%-- Fields that are shown based on GroupType settings --%>
                                    <Rock:RockLiteralField ID="lMaritalStatusValue" HeaderText="Marital Status" SortExpression="Person.MaritalStatusValue.Value" ExcelExportBehavior="AlwaysInclude"/>
                                    <Rock:RockLiteralField ID="lConnectionStatusValue" HeaderText="Connection Status" SortExpression="Person.ConnectionStatusValue.Value" ExcelExportBehavior="AlwaysInclude"/>

                                    <%-- Only shown if a registration is associated --%>
                                    <Rock:RockLiteralField ID="lRegistration" HeaderText="Registration" OnRowSelectedEnabled="false" ExcelExportBehavior="NeverInclude" />

                                    <Rock:RockBoundField DataField="GroupRole.Name" HeaderText="Role" SortExpression="GroupRole.Name" />
                                    <Rock:RockBoundField DataField="GroupMemberStatus" HeaderText="Member Status" SortExpression="GroupMemberStatus" />
                                    <Rock:DateField DataField="DateTimeAdded" HeaderText="Date Added" SortExpression="DateTimeAdded" />

                                    <%-- Fields that are only shown when ShowAttendance is enabled: NOTE: This used to support Sorting, but that can cause performance issues with large groups. --%>
                                    <Rock:RockLiteralField ID="lFirstAttended" HeaderText="First Attended" />
                                    <Rock:RockLiteralField ID="lLastAttended" HeaderText="Last Attended" />

                                    <Rock:RockBoundField DataField="Note" HeaderText="Note" SortExpression="Note" ItemStyle-CssClass="small" />

                                    <%-- Fields that are only shown when exporting --%>
                                    <Rock:RockBoundField DataField="Person.NickName" HeaderText="Nick Name" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                    <Rock:RockBoundField DataField="Person.LastName" HeaderText="Last Name" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                    <Rock:RockBoundField DataField="Person.BirthDate" HeaderText="Birth Date" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                    <Rock:RockBoundField DataField="Person.Age" HeaderText="Age" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                    <Rock:RockBoundField DataField="Person.Email" HeaderText="Email" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                    <Rock:RockBoundField DataField="Person.RecordStatusValueId" HeaderText="RecordStatusValueId" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                    <Rock:DefinedValueField DataField="Person.RecordStatusValueId" HeaderText="Record Status" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                    <Rock:RockBoundField DataField="Person.Gender" HeaderText="Gender" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                    <Rock:RockBoundField DataField="Person.IsDeceased" HeaderText="Is Deceased" Visible="false" ExcelExportBehavior="AlwaysInclude" />

                                    <Rock:RockLiteralField ID="lExportHomePhone" HeaderText="Home Phone" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                    <Rock:RockLiteralField ID="lExportCellPhone" HeaderText="Cell Phone" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                    <Rock:RockLiteralField ID="lExportHomeAddress" HeaderText="Home Address" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                    <Rock:RockLiteralField ID="lExportLatitude" HeaderText="Latitude" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                    <Rock:RockLiteralField ID="lExportLongitude" HeaderText="Longitude" Visible="false" ExcelExportBehavior="AlwaysInclude" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </div>
                </div>
            </div>

             <script>

                Sys.Application.add_load( function () {
                    $("div.photo-icon").lazyload({
                        effect: "fadeIn"
                    });

                    // person-link-popover
                    $('.js-person-popover').popover({
                        placement: 'right',
                        trigger: 'manual',
                        delay: 500,
                        html: true,
                        content: function() {
                            var dataUrl = Rock.settings.get('baseUrl') + 'api/People/PopupHtml/' +  $(this).attr('personid') + '/false';

                            var result = $.ajax({
                                                type: 'GET',
                                                url: dataUrl,
                                                dataType: 'json',
                                                contentType: 'application/json; charset=utf-8',
                                                async: false }).responseText;

                            var resultObject = jQuery.parseJSON(result);

                            return resultObject.PickerItemDetailsHtml;

                        }
                    }).on('mouseenter', function () {
                        var _this = this;
                        $(this).popover('show');
                        $(this).siblings('.popover').on('mouseleave', function () {
                            $(_this).popover('hide');
                        });
                    }).on('mouseleave', function () {
                        var _this = this;
                        setTimeout(function () {
                            if (!$('.popover:hover').length) {
                                $(_this).popover('hide')
                            }
                        }, 100);
                    });

                   // $('.js-person-popover').popover('show'); // uncomment for styling

                    // delete/archive prompt
                    $('table.js-grid-group-members a.grid-delete-button').click(function (e) {
                        var $btn = $(this);
                        var $row = $btn.closest('tr');
                        var actionName = 'delete';

                        if ( $row.hasClass('js-has-grouphistory') ) {
                            var actionName = 'archive';
                        }

                        var confirmMessage = 'Are you sure you want to ' + actionName + ' this group member?';

                        e.preventDefault();
                        Rock.dialogs.confirm(confirmMessage, function (result) {
                            if (result) {
                                if ( $row.hasClass('js-has-registration') )  {
                                    Rock.dialogs.confirm('This group member was added through a registration. Are you sure that you want to ' + actionName + ' this group member and remove the link from the registration? ', function (result) {
                                        if (result) {
                                            window.location = e.target.href ? e.target.href : e.target.parentElement.href;
                                        }
                                    });
                                } else {
                                    window.location = e.target.href ? e.target.href : e.target.parentElement.href;
                                }
                            }
                        });
                    });

                });
            </script>

            <Rock:ModalDialog ID="mdPlaceElsewhere" runat="server" Visible="false" ValidationGroup="vgPlaceElsewhere"
                Title="<i class='fa fa-share'></i> Place Elsewhere" OnSaveClick="mdPlaceElsewhere_SaveClick"
                SaveButtonText="Place">
                <Content>
                    <asp:ValidationSummary ID="vsPlaceElsewhere" runat="server" ValidationGroup="vgPlaceElsewhere" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                    <Rock:RockLiteral ID="lWorkflowTriggerName" runat="server" Label="Workflow Trigger" />
                    <Rock:RockControlWrapper ID="rcwSelectMemberTrigger" runat="server" Label="Select Workflow Trigger">
                        <Rock:HiddenFieldWithClass ID="hfPlaceElsewhereTriggerId" CssClass="js-hidden-selected" runat="server" />
                        <div class="controls">
                            <div class="btn-group-vertical">
                                <asp:Repeater ID="rptSelectMemberTrigger" runat="server" OnItemDataBound="rptSelectMemberTrigger_ItemDataBound">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="btnMemberTrigger" runat="server" CssClass="btn btn-default" CausesValidation="false" Text='<%# Eval("Name") %>' OnClick="btnMemberTrigger_Click" CommandArgument='<%# Eval("Id") %>' CommandName="TriggerId" />
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>
                        </div>
                    </Rock:RockControlWrapper>
                    <Rock:NotificationBox ID="nbPlaceElsewhereWarning" runat="server" NotificationBoxType="Warning" />
                    <asp:HiddenField ID="hfPlaceElsewhereGroupMemberId" runat="server" />
                    <Rock:RockLiteral ID="lPlaceElsewhereGroupMemberName" runat="server" Label="Group Member" />
                    <Rock:RockLiteral ID="lWorkflowName" runat="server" Label="Workflow" />
                    <Rock:RockTextBox ID="tbPlaceElsewhereNote" runat="server" Label="Note" Rows="4" TextMode="MultiLine" ValidationGroup="vgPlaceElsewhere" />
                </Content>
            </Rock:ModalDialog>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
