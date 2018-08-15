<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PageSearch.ascx.cs" Inherits="RockWeb.Plugins.org_newpointe.ExtraSearch.PageSearch" %>


<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbNotice" runat="server" NotificationBoxType="Info" Visible="false"></Rock:NotificationBox>

        <div class="grid">
            <Rock:Grid ID="gPages" runat="server" EmptyDataText="No Pages Found" AllowSorting="true">
                <Columns>
                    <Rock:RockBoundField
                        DataField="PageTitle"
                        HeaderText="Page"
                        SortExpression="PageTitle desc" />
                    <Rock:RockBoundField
                        HeaderText="Location"
                        DataField="Structure"
                        SortExpression="Structure" HtmlEncode="false" />
                    <Rock:RockBoundField
                        DataField="Site"
                        HeaderText="Site"
                        SortExpression="Site desc"
                        ColumnPriority="Desktop" />
                </Columns>
            </Rock:Grid>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>


