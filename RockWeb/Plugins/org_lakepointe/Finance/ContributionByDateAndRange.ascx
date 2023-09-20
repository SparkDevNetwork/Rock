<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContributionByDateAndRange.ascx.cs" Inherits="RockWeb.Plugins.org_lakepointe.Finance.ContributionByDateAndRange" %>
<asp:UpdatePanel ID="upContent" runat="server">
    <ContentTemplate>
        <div class="row">
            <div class="col-md-12" style="padding-left:30px; padding-right:30px;">
                <Rock:CampusesPicker ID="cpCampuses" runat="server" />
            </div>
        </div>
        <div class="row">
            <div class="col-md-3" style="padding-left:30px; padding-right:30px;">
                <Rock:DatePicker ID="dpStartDate" runat="server" Label="Start Date" Required="true" />
            </div>
            <div class="col-md-3" style="padding-left:30px; padding-right:30px;">
                <Rock:DatePicker ID="dpEndDate" runat="server" Label="End Date" Required="true" />
            </div>
        </div>
        <div class="row">
            <div class="col-md-3" style="padding-left:30px; padding-right:30px;">
                <Rock:NumberBox ID="nbMinimum" runat="server" Label="Minimum Combined Contributions" Required="true" />
            </div>
            <div class="col-md-3" style="padding-left:30px; padding-right:30px;">
                <Rock:NumberBox ID="nbMaximum" runat="server" Label="Maximum Combined Contributions" Required="true" />
            </div>
        </div>
        <div class="row">
            <div class="col-md-3" style="padding-left:30px; padding-right:30px;">
                <Rock:BootstrapButton ID="bbExecute" runat="server" Text="Execute" DataLoadingText="Running..." CssClass="btn btn-primary" OnClick="bbExecute_Click" />
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>