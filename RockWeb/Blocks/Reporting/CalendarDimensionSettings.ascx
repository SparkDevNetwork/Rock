<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CalendarDimensionSettings.ascx.cs" Inherits="RockWeb.Blocks.Reporting.CalendarDimensionSettings" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-calendar"></i>&nbsp;Calendar Dimension Settings</h1>
            </div>
            <div class="panel-body">

                <div class="row">
                    <div class="col-md-6">
                        <Rock:DatePicker ID="dpStartDate" runat="server" Label="Start Date" Required="true" />
                        <Rock:RockDropDownList ID="monthDropDownList" CssClass="input-width-md" runat="server" Label="Fiscal Start Month" Required="true" />
                        <Rock:RockCheckBox ID="cbGivingMonthUseSundayDate" runat="server" Help="This and Fiscal Start Month are used to determine the GivingMonth of a particular date. If 'Use Sunday Date for Giving Month' is enabled, it will be whatever the SundayDate of the Date is, but not if it crosses the Fiscal Year. For example, if their Fiscal year starts on April 1st, it won't use the SundayDate for any of the last days of March if it ends up being in April." Label="Use Sunday Date for Giving Month" />
                    </div>
                    <div class="col-md-6">
                        <Rock:DatePicker ID="dpEndDate" runat="server" Label="End Date" Required="true" />
                    </div>
                </div>

                <Rock:HelpBlock ID="hbGenerate" runat="server" Text="This will populate the AnalyticsDimDate table (and associated Views). It will first empty the AnalyticDimDate table if there is already data in it." />
                <div class="actions">
                    <asp:LinkButton ID="btnGenerate" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Generate Dimension" CssClass="btn btn-primary" OnClick="btnGenerate_Click" />
                </div>

                <br />
                <Rock:NotificationBox ID="nbGenerateSuccess" runat="server" NotificationBoxType="Success" Visible="true" Dismissable="true" />

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
