<%@ Control Language="C#" AutoEventWireup="true" CodeFile="InteractionSessionList.ascx.cs" Inherits="RockWeb.Blocks.Reporting.InteractionSessionList" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-desktop"></i>
                    Interactions Session List
                </h1>
                <div class="form-inline pull-right clearfix hidden-xs">
                    <Rock:DateRangePicker ID="drpDateFilter" runat="server" Label="Date Filter" />
                    <asp:Button ID="btnFilter" runat="server" Text="Filter" CssClass="btn btn-action btn-xs" OnClick="btnFilter_Click" />
                </div>
            </div>

            <div class="panel-body">
                <asp:Literal ID="lContent" runat="server"></asp:Literal>
            </div>

        </div>

    </ContentTemplate>
</asp:UpdatePanel>
