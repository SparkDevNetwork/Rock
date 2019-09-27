<%@ Control Language="C#" AutoEventWireup="true" CodeFile="StreakDetail.ascx.cs" Inherits="RockWeb.Blocks.Streaks.StreakDetail" %>

<script type="text/javascript">
    Sys.Application.add_load(function () {
        var left = $('.chart-container').outerWidth() - $('.chart-scroll-container').width();
        console.log(left);
        $('.chart-container').scrollLeft(left);
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
                        <div class="col-lg-6">
                            <h5 id="h5Left" runat="server">This Enrollment</h5>                            
                            <asp:Literal ID="lEnrollmentDescription" runat="server" />
                        </div>
                        <div class="col-lg-6">
                            <h5 id="h5Right" runat="server">Aggregate Data</h5>
                            <asp:Literal ID="lStreakData" runat="server" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" AccessKey="e" ToolTip="Alt+e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" CausesValidation="false" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" CausesValidation="false" OnClick="btnDelete_Click" />
                        <span class="pull-right">
                            <asp:LinkButton runat="server" ID="btnRebuild" CausesValidation="false" OnClick="btnRebuild_Click" CssClass="btn btn-danger btn-sm margin-l-md" Text="Rebuild" />
                        </span>
                    </div>
                </div>

                <div id="pnlEditDetails" runat="server">
                    <div class="row">
                        <div class="col-sm-6 col-md-4">
                            <Rock:PersonPicker ID="rppPerson" runat="server" SourceTypeName="Rock.Model.Streak, Rock" PropertyName="PersonAliasId" Required="true" Label="Person" />
                        </div>
                        <div class="col-sm-6 col-md-4">
                            <Rock:LocationPicker ID="rlpLocation" runat="server" AllowedPickerModes="Named" SourceTypeName="Rock.Model.Streak, Rock" PropertyName="LocationId" Label="Location" />
                        </div>
                        <div class="col-sm-6 col-md-4">
                            <Rock:DatePicker ID="rdpEnrollmentDate" runat="server" SourceTypeName="Rock.Model.Streak, Rock" PropertyName="EnrollmentDate" Label="Enrollment Date" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
