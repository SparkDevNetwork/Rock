<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonGroupHistory.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.PersonGroupHistory" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfPersonId" runat="server" />
        <asp:HiddenField ID="hfStartDateTime" runat="server" />
        <asp:HiddenField ID="hfStopDateTime" runat="server" />

        <div class="person-group-history-swimlanes">
            <code>
                <pre>
                    <asp:Panel ID="divRawResponse" runat="server" />
                </pre>
            </code>
        </div>

        <script type="text/javascript">
            var restUrl = '<%=ResolveUrl( "~/api/GroupMemberHistoricals/GetGroupHistoricalSummary" ) %>'
            var personId = $('#<%=hfPersonId.ClientID%>').val();
            var startDateTime = $('#<%=hfStartDateTime.ClientID%>').val();
            var stopDateTime = $('#<%=hfStopDateTime.ClientID%>').val();
            debugger

            restUrl += '?PersonId=' + personId;
            if (startDateTime) {
                restUrl += '&startDateTime=' + startDateTime
            }

            if (stopDateTime) {
                restUrl += '&stopDateTime=' + stopDateTime
            }

            $.ajax({
                url: restUrl,
                dataType: 'json',
                contentType: 'application/json'
            }).done(function (data) {
                var jsonString = JSON.stringify(data, null, 2);
                $('#<%=divRawResponse.ClientID%>').html(jsonString);
                });


        </script>
    </ContentTemplate>
</asp:UpdatePanel>
