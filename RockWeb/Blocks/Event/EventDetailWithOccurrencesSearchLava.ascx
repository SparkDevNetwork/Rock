<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EventDetailWithOccurrencesSearchLava.ascx.cs" Inherits="RockWeb.Blocks.Event.EventDetailWithOccurrencesSearchLava" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfEventItemId" runat="server" />
        <asp:Literal ID="lEventDetails" runat="server" />

        <div class="event-search-filters">
            <div class="row">
                <div class="col-md-6">
                    <Rock:CampusPicker ID="cpCampusPicker" runat="server" Label="" AutoPostBack="true" OnSelectedIndexChanged="btnSearch_Click" />
                </div>
                <div class="col-md-6">
                    <Rock:DateRangePicker ID="pDateRange" runat="server" Label="" />
                    <asp:LinkButton ID="btnSearch" runat="server" Style="display: none" OnClick="btnSearch_Click" />
                </div>
            </div>
        </div>

        <asp:Literal ID="lResults" runat="server" />

        <script type="text/javascript">
            Sys.Application.add_load(function () {
                $('#<%=pDateRange.ClientID%> .js-date-picker').on('changeDate', function () {
                    window.location = $('#<%=btnSearch.ClientID%>').prop('href');
                });
            });
        </script>

    </ContentTemplate>
</asp:UpdatePanel>
