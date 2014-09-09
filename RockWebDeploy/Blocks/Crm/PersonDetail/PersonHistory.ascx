<%@ control language="C#" autoeventwireup="true" inherits="RockWeb.Blocks.Crm.PersonDetail.PersonHistory, RockWeb" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlList" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa  fa-file-text-o"></i> Person History</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
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

            </div>

        </asp:Panel>

        <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Danger" Visible="false" />

    </ContentTemplate>
</asp:UpdatePanel>
