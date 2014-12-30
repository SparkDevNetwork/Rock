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

        <div class="row gridresponsive-row gridresponsive-item">
            <div class="gridresponsive-item-view">
                <div class="col-xs-3 col-md-2 col-lg-1">
                    <div class="row">
                        <div class="col-xs-7">Mon</div>
                        <div class="col-xs-5">10/20</div>
                    </div>
                </div>
                <div class="col-lg-3 hidden-xs hidden-sm hidden-md">
                    <div class="row">
                        <div class="col-md-3">7:30am</div>
                        <div class="col-md-3">11:30am</div>
                        <div class="col-md-3">12:30pm</div>
                        <div class="col-md-3">5:30pm</div>
                    </div>
                </div>
                <div class="col-xs-4 col-md-2">
                    <div class="row">
                        <div class="col-xs-6">6</div>
                        <div class="col-xs-6">2</div>
                    </div>
                </div>
                <div class="col-md-2 hidden-xs hidden-sm">
                    <span class="js-hour-type badge badge-success" data-toggle="tooltip" data-placement="top" title="Vacation">1</span>
                    <span class="js-hour-type badge badge-info" data-toggle="tooltip" data-placement="top" title="Holiday">1</span>
                    <span class="js-hour-type badge badge-warning" data-toggle="tooltip" data-placement="top" title="Sick">1</span>
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
                                <input type="email" class="form-control input-width-md" id="exampleInputEmail1" placeholder="Enter hours">
                            </div>
                        </div>
                        <div class="col-md-3">
                            <div class="form-group">
                                <label for="exampleInputEmail1">Lunch Out</label>
                                <input type="email" class="form-control input-width-md" id="exampleInputEmail1" placeholder="Enter hours">
                            </div>
                        </div>
                        <div class="col-md-3">
                            <div class="form-group">
                                <label for="exampleInputEmail1">Lunch In</label>
                                <input type="email" class="form-control input-width-md" id="exampleInputEmail1" placeholder="Enter hours">
                            </div>
                        </div>
                        <div class="col-md-3">
                            <div class="form-group">
                                <label for="exampleInputEmail1">Time Out</label>
                                <input type="email" class="form-control input-width-md" id="exampleInputEmail1" placeholder="Enter hours">
                            </div>
                        </div>


                        <div class="col-md-3">
                            <div class="form-group">
                                <label for="exampleInputEmail1">Vacation Hrs</label>
                                <input type="email" class="form-control input-width-md" id="exampleInputEmail1" placeholder="Enter hours">
                            </div>
                        </div>
                        <div class="col-md-3">
                            <div class="form-group">
                                <label for="exampleInputEmail1">Holiday Hrs</label>
                                <input type="email" class="form-control input-width-md" id="exampleInputEmail1" placeholder="Enter hours">
                            </div>
                        </div>
                        <div class="col-md-3">
                            <div class="form-group">
                                <label for="exampleInputEmail1">Sick Hrs</label>
                                <input type="email" class="form-control input-width-md" id="exampleInputEmail1" placeholder="Enter hours">
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

        <div class="row gridresponsive-row gridresponsive-item">
            <div class="col-xs-3 col-md-2 col-lg-1">
                <div class="row">
                    <div class="col-xs-7">Tue</div>
                    <div class="col-xs-5">10/21</div>
                </div>
            </div>
            <div class="col-lg-3 hidden-xs hidden-sm hidden-md">
                <div class="row">
                    <div class="col-md-3">7:30am</div>
                    <div class="col-md-3">11:30am</div>
                    <div class="col-md-3">12:30pm</div>
                    <div class="col-md-3">5:30pm</div>
                </div>
            </div>
            <div class="col-xs-4 col-md-2">
                <div class="row">
                    <div class="col-xs-6">6</div>
                    <div class="col-xs-6">2</div>
                </div>
            </div>
            <div class="col-md-2 hidden-xs hidden-sm">
                <span class="js-hour-type badge badge-success" data-toggle="tooltip" data-placement="top" title="Vacation">1</span>
                <span class="js-hour-type badge badge-info" data-toggle="tooltip" data-placement="top" title="Holiday">1</span>
                <span class="js-hour-type badge badge-warning" data-toggle="tooltip" data-placement="top" title="Sick">1</span>
            </div>
            <div class="col-xs-3 hidden-md hidden-lg">3</div>
            <div class="col-xs-2 col-md-1 col-lg-1">11</div>
            <div class="col-md-4 col-lg-2 hidden-xs hidden-sm">This is a note.</div>
            <div class="col-md-1 hidden-xs hidden-sm gridresponsive-commandcolumn"><a class="btn btn-sm btn-default js-item-edit"><i class="fa fa-pencil"></i></a></div>
        </div>

        <div class="row gridresponsive-row gridresponsive-item">
            <div class="col-xs-3 col-md-2 col-lg-1">
                <div class="row">
                    <div class="col-xs-7">Wed</div>
                    <div class="col-xs-5">10/22</div>
                </div>
            </div>
            <div class="col-lg-3 hidden-xs hidden-sm hidden-md">
                <div class="row">
                    <div class="col-md-3">7:30am</div>
                    <div class="col-md-3">11:30am</div>
                    <div class="col-md-3">12:30pm</div>
                    <div class="col-md-3">5:30pm</div>
                </div>
            </div>
            <div class="col-xs-4 col-md-2">
                <div class="row">
                    <div class="col-xs-6">6</div>
                    <div class="col-xs-6">2</div>
                </div>
            </div>
            <div class="col-md-2 hidden-xs hidden-sm">
                <span class="js-hour-type badge badge-success" data-toggle="tooltip" data-placement="top" title="Vacation">1</span>
                <span class="js-hour-type badge badge-info" data-toggle="tooltip" data-placement="top" title="Holiday">1</span>
                <span class="js-hour-type badge badge-warning" data-toggle="tooltip" data-placement="top" title="Sick">1</span>
            </div>
            <div class="col-xs-3 hidden-md hidden-lg">3</div>
            <div class="col-xs-2 col-md-1 col-lg-1">11</div>
            <div class="col-md-4 col-lg-2 hidden-xs hidden-sm">This is a note.</div>
            <div class="col-md-1 hidden-xs hidden-sm gridresponsive-commandcolumn"><a class="btn btn-sm btn-default js-item-edit"><i class="fa fa-pencil"></i></a></div>
        </div>

        <div class="row gridresponsive-row gridresponsive-item">
            <div class="col-xs-3 col-md-2 col-lg-1">
                <div class="row">
                    <div class="col-xs-7">Thu</div>
                    <div class="col-xs-5">10/23</div>
                </div>
            </div>
            <div class="col-lg-3 hidden-xs hidden-sm hidden-md">
                <div class="row">
                    <div class="col-md-3">7:30am</div>
                    <div class="col-md-3">11:30am</div>
                    <div class="col-md-3">12:30pm</div>
                    <div class="col-md-3">5:30pm</div>
                </div>
            </div>
            <div class="col-xs-4 col-md-2">
                <div class="row">
                    <div class="col-xs-6">6</div>
                    <div class="col-xs-6">2</div>
                </div>
            </div>
            <div class="col-md-2 hidden-xs hidden-sm">
                <span class="js-hour-type badge badge-success" data-toggle="tooltip" data-placement="top" title="Vacation">1</span>
                <span class="js-hour-type badge badge-info" data-toggle="tooltip" data-placement="top" title="Holiday">1</span>
                <span class="js-hour-type badge badge-warning" data-toggle="tooltip" data-placement="top" title="Sick">1</span>
            </div>
            <div class="col-xs-3 hidden-md hidden-lg">3</div>
            <div class="col-xs-2 col-md-1 col-lg-1">11</div>
            <div class="col-md-4 col-lg-2 hidden-xs hidden-sm">This is a note.</div>
            <div class="col-md-1 hidden-xs hidden-sm gridresponsive-commandcolumn"><a class="btn btn-sm btn-default js-item-edit"><i class="fa fa-pencil"></i></a></div>
        </div>

        <div class="row gridresponsive-row gridresponsive-item">
            <div class="col-xs-3 col-md-2 col-lg-1">
                <div class="row">
                    <div class="col-xs-7">Fri</div>
                    <div class="col-xs-5">10/24</div>
                </div>
            </div>
            <div class="col-lg-3 hidden-xs hidden-sm hidden-md">
                <div class="row">
                    <div class="col-md-3">7:30am</div>
                    <div class="col-md-3">11:30am</div>
                    <div class="col-md-3">12:30pm</div>
                    <div class="col-md-3">5:30pm</div>
                </div>
            </div>
            <div class="col-xs-4 col-md-2">
                <div class="row">
                    <div class="col-xs-6">6</div>
                    <div class="col-xs-6">2</div>
                </div>
            </div>
            <div class="col-md-2 hidden-xs hidden-sm">
                <span class="js-hour-type badge badge-success" data-toggle="tooltip" data-placement="top" title="Vacation">1</span>
                <span class="js-hour-type badge badge-info" data-toggle="tooltip" data-placement="top" title="Holiday">1</span>
                <span class="js-hour-type badge badge-warning" data-toggle="tooltip" data-placement="top" title="Sick">1</span>
            </div>
            <div class="col-xs-3 hidden-md hidden-lg">3</div>
            <div class="col-xs-2 col-md-1 col-lg-1">11</div>
            <div class="col-md-4 col-lg-2 hidden-xs hidden-sm">This is a note.</div>
            <div class="col-md-1 hidden-xs hidden-sm gridresponsive-commandcolumn"><a class="btn btn-sm btn-default js-item-edit"><i class="fa fa-pencil"></i></a></div>
        </div>

        <div class="row gridresponsive-row gridresponsive-item">
            <div class="col-xs-3 col-md-2 col-lg-1">
                <div class="row">
                    <div class="col-xs-7">Sat</div>
                    <div class="col-xs-5">10/25</div>
                </div>
            </div>
            <div class="col-lg-3 hidden-xs hidden-sm hidden-md">
                <div class="row">
                    <div class="col-md-3">7:30am</div>
                    <div class="col-md-3">11:30am</div>
                    <div class="col-md-3">12:30pm</div>
                    <div class="col-md-3">5:30pm</div>
                </div>
            </div>
            <div class="col-xs-4 col-md-2">
                <div class="row">
                    <div class="col-xs-6">6</div>
                    <div class="col-xs-6">2</div>
                </div>
            </div>
            <div class="col-md-2 hidden-xs hidden-sm">
                <span class="js-hour-type badge badge-success" data-toggle="tooltip" data-placement="top" title="Vacation">1</span>
                <span class="js-hour-type badge badge-info" data-toggle="tooltip" data-placement="top" title="Holiday">1</span>
                <span class="js-hour-type badge badge-warning" data-toggle="tooltip" data-placement="top" title="Sick">1</span>
            </div>
            <div class="col-xs-3 hidden-md hidden-lg">3</div>
            <div class="col-xs-2 col-md-1 col-lg-1">11</div>
            <div class="col-md-4 col-lg-2 hidden-xs hidden-sm">This is a note.</div>
            <div class="col-md-1 hidden-xs hidden-sm gridresponsive-commandcolumn"><a class="btn btn-sm btn-default js-item-edit"><i class="fa fa-pencil"></i></a></div>
        </div>

        <div class="row gridresponsive-row gridresponsive-item">
            <div class="col-xs-3 col-md-2 col-lg-1">
                <div class="row">
                    <div class="col-xs-7">Sun</div>
                    <div class="col-xs-5">10/26</div>
                </div>
            </div>
            <div class="col-lg-3 hidden-xs hidden-sm hidden-md">
                <div class="row">
                    <div class="col-md-3">7:30am</div>
                    <div class="col-md-3">11:30am</div>
                    <div class="col-md-3">12:30pm</div>
                    <div class="col-md-3">5:30pm</div>
                </div>
            </div>
            <div class="col-xs-4 col-md-2">
                <div class="row">
                    <div class="col-xs-6">6</div>
                    <div class="col-xs-6">2</div>
                </div>
            </div>
            <div class="col-md-2 hidden-xs hidden-sm">
                <span class="js-hour-type badge badge-success" data-toggle="tooltip" data-placement="top" title="Vacation">1</span>
                <span class="js-hour-type badge badge-info" data-toggle="tooltip" data-placement="top" title="Holiday">1</span>
                <span class="js-hour-type badge badge-warning" data-toggle="tooltip" data-placement="top" title="Sick">1</span>
            </div>
            <div class="col-xs-3 hidden-md hidden-lg">3</div>
            <div class="col-xs-2 col-md-1 col-lg-1">11</div>
            <div class="col-md-4 col-lg-2 hidden-xs hidden-sm">This is a note.</div>
            <div class="col-md-1 hidden-xs hidden-sm gridresponsive-commandcolumn"><a class="btn btn-sm btn-default js-item-edit"><i class="fa fa-pencil"></i></a></div>
        </div>

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
