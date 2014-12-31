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
                    <strong>Other Hours</strong>
                </div>
            </div>
            <div class="col-xs-3 hidden-md hidden-lg"><strong>Other Hrs</strong></div>
            <div class="col-xs-2 col-md-1 col-lg-1"><strong>Total Hrs</strong></div>
            <div class="col-md-4 col-lg-2 hidden-xs hidden-sm"><strong>Note</strong></div>
        </div>

        <asp:Repeater runat="server" ID="rptTimeCardDay">
            <ItemTemplate>
                <div class="row gridresponsive-row gridresponsive-item">
                    <div class="gridresponsive-item-view">
                        <div class="col-xs-3 col-md-2 col-lg-1">
                            <div class="row">
                                <div class="col-xs-7"><%# ((DateTime)Eval("StartDateTime")).ToString("ddd") %></div>
                                <div class="col-xs-5"><%# ((DateTime)Eval("StartDateTime")).ToString("MM/dd") %></div>
                            </div>
                        </div>
                        <div class="col-lg-3 hidden-xs hidden-sm hidden-md">
                            <div class="row">
                                <div class="col-md-3"><%# FormatTimeCardTime(Eval("StartDateTime") as DateTime?) %></div>
                                <div class="col-md-3"><%# FormatTimeCardTime(Eval("LunchStartDateTime") as DateTime?) %></div>
                                <div class="col-md-3"><%# FormatTimeCardTime(Eval("LunchEndDateTime") as DateTime?) %></div>
                                <div class="col-md-3"><%# FormatTimeCardTime(Eval("EndDateTime") as DateTime?) %></div>
                            </div>
                        </div>
                        <div class="col-xs-4 col-md-2">
                            <div class="row">
                                <div class="col-xs-6"><%# FormatRegularHours(Container.DataItem as com.ccvonline.TimeCard.Model.TimeCardDay) %></div>
                                <div class="col-xs-6"><%# FormatOvertimeHours(Container.DataItem as com.ccvonline.TimeCard.Model.TimeCardDay ) %></div>
                            </div>
                        </div>
                        <div class="col-md-2 hidden-xs hidden-sm">
                            <span class="js-hour-type badge badge-success" data-toggle="tooltip" data-placement="top" title="Vacation"><%# FormatTimeCardHours((Container.DataItem as com.ccvonline.TimeCard.Model.TimeCardDay ).PaidVacationHours) %></span>
                            <span class="js-hour-type badge badge-info" data-toggle="tooltip" data-placement="top" title="Holiday"><%# FormatTimeCardHours((Container.DataItem as com.ccvonline.TimeCard.Model.TimeCardDay ).PaidHolidayHours) %></span>
                            <span class="js-hour-type badge badge-warning" data-toggle="tooltip" data-placement="top" title="Sick"><%# FormatTimeCardHours((Container.DataItem as com.ccvonline.TimeCard.Model.TimeCardDay ).PaidSickHours) %></span>
                        </div>
                        <div class="col-xs-3 hidden-md hidden-lg">3</div>
                        <div class="col-xs-2 col-md-1 col-lg-1">11</div>
                        <div class="col-md-4 col-lg-2 hidden-xs hidden-sm">This is a note.</div>
                        <div class="col-md-1 hidden-xs hidden-sm gridresponsive-commandcolumn"><a class="btn btn-sm btn-default js-item-edit"><i class="fa fa-pencil"></i></a></div>
                    </div>
                    <div class="gridresponsive-item-edit padding-b-md" style="display: none;">

                        <div class="col-xs-4 col-md-2">
                            <div class="row">
                                <div class="col-xs-7">Mon</div>
                                <div class="col-xs-5">10/20</div>
                            </div>
                        </div>
                        <div class="col-xs-8 col-md-8">
                            <div class="row">
                                <div class="col-md-3">
                                    <div class="form-group">
                                        <label for="exampleInputEmail1">Time In</label>
                                        <Rock:TimePicker runat="server" ID="tpTimeIn" />
                                    </div>
                                </div>
                                <div class="col-md-3">
                                    <div class="form-group">
                                        <label for="exampleInputEmail1">Lunch Out</label>
                                        <input type="email" class="form-control input-width-md" id="exampleInputEmail2" placeholder="Enter hours">
                                    </div>
                                </div>
                                <div class="col-md-3">
                                    <div class="form-group">
                                        <label for="exampleInputEmail1">Lunch In</label>
                                        <input type="email" class="form-control input-width-md" id="exampleInputEmail3" placeholder="Enter hours">
                                    </div>
                                </div>
                                <div class="col-md-3">
                                    <div class="form-group">
                                        <label for="exampleInputEmail1">Time Out</label>
                                        <input type="email" class="form-control input-width-md" id="exampleInputEmail4" placeholder="Enter hours">
                                    </div>
                                </div>


                                <div class="col-md-3">
                                    <div class="form-group">
                                        <label for="exampleInputEmail1">Vacation Hrs</label>
                                        <input type="email" class="form-control input-width-md" id="exampleInputEmail5" placeholder="Enter hours">
                                    </div>
                                </div>
                                <div class="col-md-3">
                                    <div class="form-group">
                                        <label for="exampleInputEmail1">Holiday Hrs</label>
                                        <input type="email" class="form-control input-width-md" id="exampleInputEmail6" placeholder="Enter hours">
                                    </div>
                                </div>
                                <div class="col-md-3">
                                    <div class="form-group">
                                        <label for="exampleInputEmail1">Sick Hrs</label>
                                        <input type="email" class="form-control input-width-md" id="exampleInputEmail7" placeholder="Enter hours">
                                    </div>
                                </div>

                            </div>


                        </div>

                        <div class="col-md-2 pull-right gridresponsive-commandcolumn">
                            <a href="" class="btn btn-success btn-sm js-item-save margin-b-sm"><i class="fa fa-check"></i></a>
                            <a href="" class="btn btn-warning btn-sm js-item-save margin-b-sm"><i class="fa fa-minus"></i></a>
                        </div>

                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>


        <div class="margin-t-md pull-right hidden-sm hidden-xs">
            <span class="label label-success">Vacation</span> <span class="label label-info">Holiday</span> <span class="label label-warning">Sick</span>
        </div>

        <script>
            $(document).ready(function () {
                $('.js-hour-type').tooltip();
            });

            $(".js-item-edit").on("click", function () {
                var $parent = $(this).closest(".gridresponsive-item");
                $parent.find(".gridresponsive-item-view").slideToggle(function () {
                    $parent.find(".gridresponsive-item-edit").slideToggle();
                });
            });

            $(".js-item-save").on("click", function () {
                var $parent = $(this).closest(".gridresponsive-item");
                $parent.find(".gridresponsive-item-edit").slideToggle(function () {
                    $parent.find(".gridresponsive-item-view").slideToggle();
                });
            });

        </script>

    </ContentTemplate>
</asp:UpdatePanel>
