<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TimeCardDetail.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.TimeCard.TimeCardDetail" %>
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

        <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

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
                                    <asp:Literal runat="server" ID="lRegularHours" />
                                </div>
                                <div class="col-xs-6">
                                    <asp:Literal runat="server" ID="lOvertimeHours" />
                                </div>
                            </div>
                        </div>
                        <div class="col-md-2 hidden-xs hidden-sm">
                            <span class="js-hour-type badge badge-success" data-toggle="tooltip" data-placement="top" title="Vacation">
                                <asp:Literal runat="server" ID="lPaidVacationHours" /></span>
                            <span class="js-hour-type badge badge-info" data-toggle="tooltip" data-placement="top" title="Holiday">
                                <asp:Literal runat="server" ID="lPaidHolidayHours" /></span>
                            <span class="js-hour-type badge badge-warning" data-toggle="tooltip" data-placement="top" title="Sick">
                                <asp:Literal runat="server" ID="lPaidSickHours" /></span>
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
                            <div class="row">
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
                            </div>
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
            <span class="label label-success">Vacation</span> <span class="label label-info">Holiday</span> <span class="label label-warning">Sick</span>
        </div>

        <!-- Actions/Submit Panel -->
        <asp:Panel ID="pnlPersonActions" runat="server">
            <Rock:RockDropDownList ID="ddlSubmitTo" runat="server" Label="Submit to" />
            <Rock:RockCheckBox ID="cbAgree" runat="server" Text="I certify and agree that:" />
            <ol>
                <li>all of the entries on this Time Card for this payroll period are both accurate and complete;</li>
                <li>the entries include all hours worked plus paid time off hours, if any;</li>
                <li>the entries do not include unpaid meal periods or other non-work time; and,</li>
                <li>I have not worked either more or fewer hours than entered.</li>
            </ol>
            <asp:LinkButton runat="server" ID="lbSubmit" CssClass="btn btn-action" OnClick="lbSubmit_Click" Text="Submit" />
        </asp:Panel>

        <!-- Totals Panel -->
        <asp:Panel ID="pnlTotals" runat="server">
            <div class="row">
                <div class="col-md-6">
                    <Rock:RockLiteral ID="lTotalsWorkedHtml" runat="server" />
                </div>
                <div class="col-md-6">
                    <Rock:RockLiteral ID="lTotalsPaidHtml" runat="server" />
                </div>
            </div>
            <div class="row">
                <h4>Total</h4>
                <Rock:RockLiteral ID="lTotalsTotalHtml" runat="server" />
            </div>



        </asp:Panel>

        <!-- History Panel -->
        <asp:Panel ID="pnlHistory" runat="server">
            <Rock:RockLiteral ID="lHistoryHtml" runat="server" />
        </asp:Panel>

        <script>
            Sys.Application.add_load(function () {
                $('.js-hour-type').tooltip();

                $(".js-item-edit, .js-item-cancel, .gridresponsive-item-view").on("click", function (a) {

                    // prevent the .js-item-edit from bubbling up to .gridresponsive-item-view
                    a.stopImmediatePropagation();

                    var $parent = $(this).closest(".gridresponsive-item");
                    $parent.find(".gridresponsive-item-edit").slideToggle(function () {
                        $parent.find(".gridresponsive-item-view").slideToggle();
                    });
                });
            });

        </script>

    </ContentTemplate>
</asp:UpdatePanel>
