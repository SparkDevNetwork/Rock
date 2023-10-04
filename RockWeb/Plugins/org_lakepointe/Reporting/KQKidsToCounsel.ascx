<%@ Control Language="C#" AutoEventWireup="true" CodeFile="KQKidsToCounsel.ascx.cs" Inherits="RockWeb.Plugins.org_lakepointe.Reporting.KQKidsToCounsel" %>
<asp:UpdatePanel ID="upContent" runat="server">
    <ContentTemplate>
        <div class="row">
            <div class="col-md-6" style="padding-left:30px; padding-right:30px;">
                <Rock:RockDropDownList ID="rddlRegistrationInstance" runat="server" Label="Registration Instance" DataTextField="Name" DataValueField="Id" />
            </div>
        </div>
        <div class="row">
            <div class="col-md-6" style="padding-left:30px; padding-right:30px;">
                <Rock:RockCheckBoxList ID="rcblColorGroup" runat="server" Label="Color Groups" DataTextField="Name" DataValueField="Name" />
            </div>
        </div>
        <div class="row">
            <div class="col-md-3" style="padding-left:30px; padding-right:30px;">
                <Rock:DatePicker ID="dpAttendanceStartDate" runat="server" Label="Attendance Start Date" Required="true" />
                <Rock:DatePicker ID="dpAttendanceEndDate" runat="server" Label="Attendance End Date" Required="true" />
            </div>
            <div class="col-md-3" style="padding-left:30px; padding-right:30px;">
                <Rock:NumberBox ID="nbMinimum" runat="server" Label="Minimum Grade" Required="true" />
                <Rock:NumberBox ID="nbMaximum" runat="server" Label="Maximum Grade" Required="true" />
            </div>
        </div>
        <div class="row">
            <div class="col-md-3" style="padding-left:30px; padding-right:30px;">
                <Rock:BootstrapButton ID="bbExecute" runat="server" Text="Execute" DataLoadingText="Running..." CssClass="btn btn-primary" OnClick="bbExecute_Click" />
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>