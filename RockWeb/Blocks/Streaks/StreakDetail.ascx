<%@ Control Language="C#" AutoEventWireup="true" CodeFile="StreakDetail.ascx.cs" Inherits="RockWeb.Blocks.Streaks.StreakDetail" %>

<script type="text/javascript">
    Sys.Application.add_load(function () {
        $('.js-member-note').tooltip();

        // data view sync list popover
        $('.js-sync-popover').popover();
    });
</script>

<asp:UpdatePanel ID="upEnrollmentDetail" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfIsEditMode" runat="server" />

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-clipboard-check"></i>
                    <asp:Literal ID="lReadOnlyTitle" runat="server" />
                </h1>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                <asp:ValidationSummary ID="valEnrollmentDetail" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                <div id="pnlViewDetails" runat="server">

                    <div class="row">
                        <div class="col-md-12">
                            <ul class="streak-chart margin-b-md">
                                <asp:Literal ID="lStreakChart" runat="server" />
                            </ul>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <h4 class="margin-b-lg"><asp:Literal ID="lPersonHtml" runat="server" /></h4>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-lg-6">
                            <asp:Literal ID="lEnrollmentDescription" runat="server" />
                        </div>
                        <div class="col-lg-6">
                            <asp:Literal ID="lStreakData" runat="server" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" data-shortcut-key="e" ToolTip="Alt+e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" CausesValidation="false" OnClick="btnDelete_Click" />
                        <asp:LinkButton runat="server" ID="btnRebuild" CausesValidation="false" OnClick="btnRebuild_Click" CssClass="btn btn-danger pull-right" Text="Rebuild" />
                    </div>
                </div>

                <div id="pnlEditDetails" runat="server">
                    <div class="row">
                        <div class="col-sm-6 col-md-4">
                            <Rock:PersonPicker ID="rppPerson" runat="server" SourceTypeName="Rock.Model.Streak, Rock" PropertyName="PersonAliasId" Required="true" Label="Person" />
                            <div class="form-group" id="divPerson" runat="server">
                                <label class="control-label" for="ctl00_main_ctl25_ctl01_ctl06_rppPerson">Person</label>
                                <div class="control-wrapper">
                                    <asp:Literal ID="lPersonName" runat="server" />
                                </div>
                            </div>
                        </div>
                        <div class="col-sm-6 col-md-4">
                            <Rock:LocationPicker ID="rlpLocation" runat="server" AllowedPickerModes="Named" SourceTypeName="Rock.Model.Streak, Rock" PropertyName="LocationId" Label="Location" />
                        </div>
                        <div class="col-sm-6 col-md-4">
                            <Rock:DatePicker ID="rdpEnrollmentDate" runat="server" SourceTypeName="Rock.Model.Streak, Rock" PropertyName="EnrollmentDate" Label="Enrollment Date" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" data-shortcut-key="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" data-shortcut-key="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>

<script>
    Sys.Application.add_load(function () {
        $("div.photo-icon").lazyload({
            effect: "fadeIn"
        });

        // person-link-popover
        $('.js-person-popover').popover({
            placement: 'right',
            trigger: 'manual',
            delay: 500,
            html: true,
            content: function () {
                var dataUrl = Rock.settings.get('baseUrl') + 'api/People/PopupHtml/' + $(this).attr('personid') + '/false';

                var result = $.ajax({
                    type: 'GET',
                    url: dataUrl,
                    dataType: 'json',
                    contentType: 'application/json; charset=utf-8',
                    async: false
                }).responseText;

                var resultObject = JSON.parse(result);

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

    });
</script>