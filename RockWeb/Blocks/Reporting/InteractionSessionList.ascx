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
                    <Rock:PersonPicker ID="ppPerson" runat="server" Label="Person" />
                    <Rock:DateRangePicker ID="drpDateFilter" runat="server" Label="Date Filter" />
                    <asp:Button ID="btnFilter" runat="server" Text="Filter" CssClass="btn btn-action btn-xs" OnClick="btnFilter_Click" />
                </div>
            </div>

            <div class="panel-body">
                <asp:Literal ID="lContent" runat="server"></asp:Literal>
                <div class="actions nav-paging">
                    <asp:HyperLink ID="hlPrev" CssClass="btn btn-primary btn-prev" Visible="false" runat="server" Text="<i class='fa fa-chevron-left'></i> Prev" />
                    <asp:HyperLink ID="hlNext" CssClass="btn btn-primary btn-next" Visible="false" runat="server" Text="Next <i class='fa fa-chevron-right'></i>" />
                </div>
            </div>

        </div>

    </ContentTemplate>
</asp:UpdatePanel>
