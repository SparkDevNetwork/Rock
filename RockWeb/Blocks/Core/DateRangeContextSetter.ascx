<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DateRangeContextSetter.ascx.cs" Inherits="RockWeb.Blocks.Core.DateRangeContextSetter" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <ul class="nav navbar-nav contextsetter contextsetter-date">
            <li class="dropdown">

                <a class="dropdown-toggle navbar-link" href="#" data-toggle="dropdown">
                    <asp:Literal ID="lCurrentSelection" runat="server" />
                    <b class="fa fa-caret-down"></b>
                </a>

                <ul class="dropdown-menu" style="min-width: 300px !important; padding: 10px">
                    <Rock:SlidingDateRangePicker ID="drpSlidingDateRange" runat="server" Label="Date Range" EnabledSlidingDateRangeTypes="Previous, Last, Current, DateRange" />

                    <div class="actions text-right">
                        <asp:LinkButton ID="btnSelect" runat="server" CssClass="btn btn-primary" ToolTip="Select" OnClick="btnSelect_Click" Text="Select" />
                    </div>
                </ul>
            </li>
        </ul>
    </ContentTemplate>
</asp:UpdatePanel>

<script>

    $('.dropdown-menu:not(a)').click(function (e) {
        e.stopPropagation();
    });
</script>