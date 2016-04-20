<%@ Control Language="C#" AutoEventWireup="true" CodeFile="StepsTaken.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Steps.StepsTaken" %>

<script>

    Sys.Application.add_load( function () {
        $('.value-tip').tooltip();

        $(".js-settings-toggle").on("click", function () {
            $('.js-settings-panel').slideToggle();
        });
    });

</script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-road"></i> Steps Taken</h1>
                <div class="pull-right">
                    <Rock:HighlightLabel ID="hlDate" runat="server" CssClass="js-settings-toggle cursor-pointer" />
                </div>
            </div>
            <div class="panel-body">
                <div class="panel-settings js-settings-panel" style="display: none;">
                    <div class="row">
                        <div class="col-md-4">
                            <Rock:RockDropDownList ID="ddlSundayDates" Label="Recent Dates" runat="server" Help="The last 12 Sundays." />
                        </div>
                        <div class="col-md-4">
                            <Rock:DatePicker ID="dpSundayPicker" Label="Specific Date" runat="server" Help="Select a specific date you would like to report on. The system will select the Sunday Date for the date you select." />
                        </div>
                        <div class="col-md-4 text-right">
                            <asp:LinkButton ID="lbSetDate" runat="server" CssClass="btn btn-primary btn-sm margin-t-lg" Text="Set Date"  />
                        </div>
                    </div>
                </div>

            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
