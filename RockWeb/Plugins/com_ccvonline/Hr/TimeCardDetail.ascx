<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TimeCardDetail.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.Hr.TimeCardDetail" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <style>
            .gridresponsive-header {
                background-color: #eaeaea;
            }

            .gridresponsive-item {
            }

            .gridresponsive-row {
                margin-bottom: 12px;
                border-bottom: 1px solid #eaeaea;
            }

            .gridresponsive-commandcolumn {
                text-align: right;
            }
        </style>

        <asp:HiddenField ID="hfTimeCardId" runat="server" />
        <asp:HiddenField ID="hfEditMode" runat="server" />

        <Rock:NotificationBox ID="nbMessage" runat="server" />

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1>
                    <asp:Literal ID="lTimeCardPersonName" runat="server" />
                </h1>

                <h1 class="panel-title">
                    <asp:Literal ID="lTitle" runat="server" Text="Pay Period" />
                </h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlblSubTitle" runat="server" LabelType="Info" Text="Status" />
                </div>
            </div>
            <div class="panel-body">

                <div class="row gridresponsive-row gridresponsive-header">
                    <div class="col-xs-3 col-md-2 col-lg-1">
                        <strong>Date</strong>
                    </div>
                    <div class="col-lg-3 hidden-xs hidden-sm hidden-md">
                        <div class="row">
                            <div class="col-md-3"><strong>Time In</strong></div>
                            <div class="col-md-3"><strong>Lunch Out</strong></div>
                            <div class="col-md-3"><strong>Lunch In</strong></div>
                            <div class="col-md-3"><strong>Time Out</strong></div>
                        </div>
                    </div>
                    <div class="col-xs-4 col-md-2">
                        <div class="row">
                            <div class="col-xs-6"><strong>Hrs Worked</strong></div>
                            <div class="col-xs-6"><strong>Overtime Hrs</strong></div>
                        </div>
                    </div>
                    <div class="col-md-2 hidden-xs hidden-sm">
                        <div class="row">
                            <strong>Other Hrs</strong>
                        </div>
                    </div>
                    <div class="col-xs-3 hidden-md hidden-lg"><strong>Other Hrs</strong></div>
                    <div class="col-xs-2 col-md-1 col-lg-1"><strong>Total Hrs</strong></div>
                    <div class="col-md-4 col-lg-2 hidden-xs hidden-sm"><strong>Note</strong></div>
                </div>

                <asp:Repeater runat="server" ID="rptTimeCardDay" OnItemDataBound="rptTimeCardDay_ItemDataBound">
                    <ItemTemplate>
                        <div class="row gridresponsive-row gridresponsive-item">
                            <div class="gridresponsive-item-view clickable">
                                <div class="col-xs-3 col-md-2 col-lg-1">
                                    <div class="row">
                                        <div class="col-xs-7">
                                            <asp:Literal runat="server" ID="lTimeCardDayName" />
                                        </div>
                                        <div class="col-xs-5">
                                            <asp:Literal runat="server" ID="lTimeCardDate" />
                                        </div>
                                    </div>
                                </div>
                                <div class="col-lg-3 hidden-xs hidden-sm hidden-md">
                                    <div class="row">
                                        <div class="col-md-3">
                                            <asp:Literal runat="server" ID="lStartDateTime" />
                                        </div>
                                        <div class="col-md-3">
                                            <asp:Literal runat="server" ID="lLunchStartDateTime" />
                                        </div>
                                        <div class="col-md-3">
                                            <asp:Literal runat="server" ID="lLunchEndDateTime" />
                                        </div>
                                        <div class="col-md-3">
                                            <asp:Literal runat="server" ID="lEndDateTime" />
                                        </div>
                                    </div>
                                </div>
                                <div class="col-xs-4 col-md-2">
                                    <div class="row">
                                        <div class="col-xs-6">
                                            <asp:Literal runat="server" ID="lWorkedRegularHours" />
                                        </div>
                                        <div class="col-xs-6">
                                            <Rock:Badge runat="server" ID="lWorkedOvertimeHours" BadgeType="Danger" ToolTip="Overtime" />
                                            <Rock:Badge runat="server" ID="lWorkedHolidayHours" BadgeType="Info" ToolTip="Worked Holiday" />
                                        </div>
                                    </div>
                                </div>
                                <div class="col-md-2 hidden-xs hidden-sm">
                                    <Rock:Badge runat="server" ID="lPaidVacationHours" BadgeType="Success" ToolTip="Vacation" />
                                    <Rock:Badge runat="server" ID="lPaidHolidayHours" BadgeType="Info" ToolTip="Paid Holiday" />
                                    <Rock:Badge runat="server" ID="lPaidSickHours" BadgeType="Warning" ToolTip="Sick" />
                                </div>
                                <div class="col-xs-3 hidden-md hidden-lg">
                                    <asp:Literal runat="server" ID="lOtherHours" />
                                </div>
                                <div class="col-xs-2 col-md-1 col-lg-1">
                                    <asp:Literal runat="server" ID="lTotalHours" />
                                </div>
                                <div class="col-md-4 col-lg-2 hidden-xs hidden-sm">
                                    <asp:Literal runat="server" ID="lNotes" />
                                </div>
                                <div class="col-md-1 hidden-xs hidden-sm gridresponsive-commandcolumn"><a class="btn btn-sm btn-default js-item-edit"><i class="fa fa-pencil"></i></a></div>
                            </div>
                            <div class="gridresponsive-item-edit padding-b-md" style="display: none;">

                                <div class="col-xs-4 col-md-2">
                                    <div class="row">
                                        <div class="col-xs-7">
                                            <asp:Literal runat="server" ID="lTimeCardDayNameEdit" />
                                        </div>
                                        <div class="col-xs-5">
                                            <asp:Literal runat="server" ID="lTimeCardDateEdit" />
                                        </div>
                                    </div>
                                </div>
                                <div class="col-xs-8 col-md-8">
                                    <asp:Panel ID="pnlEditRow" runat="server" CssClass="row" >
                                        <div class="col-md-3">
                                            <Rock:TimePicker runat="server" ID="tpTimeIn" Placeholder="Enter Time" Label="Time In" />
                                        </div>
                                        <div class="col-md-3">
                                            <Rock:TimePicker runat="server" ID="tpLunchOut" Placeholder="Enter Time" Label="Lunch Out" />
                                        </div>
                                        <div class="col-md-3">
                                            <Rock:TimePicker runat="server" ID="tpLunchIn" Placeholder="Enter Time" Label="Lunch In" />
                                        </div>
                                        <div class="col-md-3">
                                            <Rock:TimePicker runat="server" ID="tpTimeOut" Placeholder="Enter Time" Label="Time Out" />
                                        </div>

                                        <div class="col-md-3">
                                            <Rock:RockDropDownList runat="server" ID="ddlVacationHours" Label="Vacation Hrs" />
                                        </div>
                                        <div class="col-md-3">
                                            <Rock:RockDropDownList runat="server" ID="ddlHolidayHours" Label="Holiday Hrs" />
                                        </div>
                                        <div class="col-md-3">
                                            <Rock:RockDropDownList runat="server" ID="ddlSickHours" Label="Sick Hrs" />
                                        </div>

                                        <div class="col-md-3">
                                            <Rock:RockTextBox runat="server" ID="tbNotes" Label="Notes" MaxLength="200" />
                                        </div>
                                    </asp:Panel>
                                </div>

                                <div class="col-md-2 pull-right gridresponsive-commandcolumn">
                                    <asp:LinkButton runat="server" ID="lbSave" CssClass="btn btn-success btn-sm js-item-save margin-b-sm" OnClick="lbSave_Click"><i class="fa fa-check"></i></asp:LinkButton>
                                    <a runat="server" id="lbCancel" class="btn btn-warning btn-sm js-item-cancel margin-b-sm"><i class="fa fa-minus"></i></a>
                                </div>

                            </div>

                        </div>
                    </ItemTemplate>
                </asp:Repeater>

                <div class="margin-t-md pull-right hidden-sm hidden-xs">
                    <span class="label label-success">Vacation</span> <span class="label label-info">Holiday</span> <span class="label label-warning">Sick</span> <span class="label label-danger">Overtime</span>
                </div>

                <!-- Totals Panel -->
                <asp:Panel ID="pnlTotals" runat="server">
                    <h2>Totals:</h2>
                    <h4>Worked Hours</h4>
                    <div class="row">
                        <div class="col-md-3">Regular Hours</div>
                        <div class="col-md-1">
                            <asp:Literal ID="lTotalRegularWorked" runat="server" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-3">Overtime Hours</div>
                        <div class="col-md-1">
                            <asp:Literal ID="lTotalOvertimeWorked" runat="server" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-3">Holiday</div>
                        <div class="col-md-1">
                            <asp:Literal ID="lTotalHolidayWorked" runat="server" />
                        </div>
                    </div>
                    <h4>PTO Hours
                    </h4>
                    <div class="row">
                        <div class="col-md-3">Vacation Hours</div>
                        <div class="col-md-1">
                            <asp:Literal ID="lTotalVacationPaid" runat="server" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-3">Holiday Hours</div>
                        <div class="col-md-1">
                            <asp:Literal ID="lTotalHolidayPaid" runat="server" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-3">Sick Hours</div>
                        <div class="col-md-1">
                            <asp:Literal ID="lTotalSickPaid" runat="server" />
                        </div>
                    </div>
                    <div class="row">
                        <h4>
                            <div class="col-md-3">All Hours</div>
                            <div class="col-md-1">
                                <strong>
                                    <asp:Literal ID="lTotalHours" runat="server" />
                                </strong>
                            </div>
                        </h4>
                    </div>
                </asp:Panel>

                <!-- Actions/Submit Panel -->
                <asp:Panel ID="pnlPersonActions" runat="server">
                    <h2>Submit</h2>
                    <div class="row">
                        <div class="col-md-6">
                            <asp:ValidationSummary ID="valSummarySubmit" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="vgSubmit" />
                            <Rock:RockDropDownList ID="ddlSubmitTo" runat="server" Label="Submit to" Required="true" ValidationGroup="vgSubmit" />
                            <Rock:RockCheckBox ID="cbAgree" runat="server" CssClass="js-agree-checkbox" Text="I certify and agree that:" />
                            <asp:CustomValidator ID="validateAgree" runat="server" ValidationGroup="vgSubmit" ClientValidationFunction="validateCheckbox" ErrorMessage="Agree must be checked" />
                            <ol>
                                <li>all of the entries on this Time Card for this payroll period are both accurate and complete;</li>
                                <li>the entries include all hours worked plus paid time off hours, if any;</li>
                                <li>the entries do not include unpaid meal periods or other non-work time; and,</li>
                                <li>I have not worked either more or fewer hours than entered.</li>
                            </ol>
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton runat="server" ID="lbSubmit" CssClass="btn btn-action" ValidationGroup="vgSubmit" CausesValidation="true" OnClientClick="maintainScrollPosition(this)" OnClick="lbSubmit_Click" Text="Submit" />
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlApproverActions" runat="server">
                    <h2>Approve</h2>
                    <div class="actions">
                        <asp:LinkButton runat="server" ID="btnApprove" CssClass="btn btn-action" ValidationGroup="vgApprove" CausesValidation="true" OnClientClick="maintainScrollPosition(this)" OnClick="btnApprove_Click" Text="Approve" />
                    </div>
                </asp:Panel>


                <!-- History Panel -->
                <asp:Panel ID="pnlHistory" runat="server">
                    <div class="row">
                        <div class="col-md-6">
                            <h2>History</h2>
                            <Rock:Grid ID="gHistory" runat="server" AllowPaging="false" AllowSorting="false" DataKeyNames="Id" DisplayType="Light" OnRowDataBound="gHistory_RowDataBound">
                                <Columns>
                                    <Rock:DateTimeField HeaderText="Date/Time" DataField="HistoryDateTime" />
                                    <Rock:RockTemplateField HeaderText="Status">
                                        <ItemTemplate>
                                            <asp:Literal ID="lStatusText" runat="server" />
                                        </ItemTemplate>
                                    </Rock:RockTemplateField>
                                    <Rock:RockBoundField HeaderText="Notes" DataField="Notes" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                        <div class="col-md-6">
                        </div>
                    </div>
                </asp:Panel>

            </div>
        </asp:Panel>

        <script>
            Sys.Application.add_load(function () {
                $('.badge').tooltip();

                $(".js-item-edit, .js-item-cancel, .gridresponsive-item-view").on("click", function (a) {

                    // prevent the .js-item-edit from bubbling up to .gridresponsive-item-view
                    a.stopImmediatePropagation();

                    var $parent = $(this).closest(".gridresponsive-item");
                    $parent.find(".gridresponsive-item-view").slideToggle();
                    $parent.find(".gridresponsive-item-edit").slideToggle();
                });
            });

            // make the window scroll to the specified element.  use setTimeout to have it do it after any other scrolls that happen (like validation)
            function maintainScrollPosition(element) {
                setTimeout(function () {
                    $(window).scrollTop($(element).position().top);
                }, 0)
            }

            // custom validate function to check if Checkbox is Checked=True
            function validateCheckbox(a, b) {
                b.IsValid = $('.js-agree-checkbox').prop('checked');
            }

        </script>

    </ContentTemplate>
</asp:UpdatePanel>
