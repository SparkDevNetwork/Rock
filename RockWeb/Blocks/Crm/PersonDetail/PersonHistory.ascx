<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonHistory.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.PersonHistory" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlList" runat="server">

            <div class="grid">
                <Rock:GridFilter ID="gfSettings" runat="server">
                    <Rock:PersonPicker ID="ppWhoFilter" runat="server" Label="Who" />
                    <Rock:RockTextBox ID="tbSummary" runat="server" Label="Summary Contains" />
                    <Rock:RockDropDownList ID="ddlCategory" runat="server" Label="Category" />
                    <Rock:DateRangePicker ID="drpDates" runat="server" Label="Date Range" />
                </Rock:GridFilter>
                <Rock:Grid ID="gHistory" runat="server" AllowSorting="true" RowItemText="Change">
                    <Columns>
                        <asp:HyperLinkField DataTextField="PersonName" DataNavigateUrlFields="CreatedByPersonId" SortExpression="PersonName" DataNavigateUrlFormatString="~/Person/{0}" HeaderText="Who" />
                        <asp:TemplateField HeaderText="Changed" SortExpression="Summary">
                            <ItemTemplate><%# FormatSummary( (int)Eval("EntityTypeId"), (int)Eval( "EntityId" ), Eval( "Summary" ).ToString() ) %></ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="What">
                            <ItemTemplate><%# FormatCaption( (int)Eval("CategoryId"), Eval( "Caption" ).ToString(), (int)Eval( "RelatedEntityTypeId" ), (int)Eval( "RelatedEntityId" ) ) %></ItemTemplate>
                        </asp:TemplateField>
                        <Rock:DateTimeField DataField="CreatedDateTime" SortExpression="CreatedDateTime" HeaderText="When" FormatAsElapsedTime="true" />
                        <asp:BoundField DataField="Category" SortExpression="Category" HeaderText="Category" />
                    </Columns>
                </Rock:Grid>
            </div>

        </asp:Panel>

        <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Danger" Visible="false" />

    </ContentTemplate>
</asp:UpdatePanel>
