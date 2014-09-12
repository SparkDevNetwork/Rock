<%@ control language="C#" autoeventwireup="true" inherits="RockWeb.Blocks.Communication.SystemEmailList, RockWeb" %>

<asp:UpdatePanel ID="upSettings" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Danger" Visible="false" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-envelope"></i> System Email List</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="rFilter" runat="server">
                        <Rock:CategoryPicker ID="cpCategory" runat="server" Label="Category" Required="false" EntityTypeName="Rock.Model.SystemEmail" />
                    </Rock:GridFilter>
                    <Rock:Grid ID="gEmailTemplates" runat="server" AllowSorting="true" OnRowSelected="gEmailTemplates_Edit">
                        <Columns>
                            <asp:BoundField DataField="Category.Name" HeaderText="Category" SortExpression="Category.Name" />
                            <asp:BoundField DataField="Title" HeaderText="Title" SortExpression="Title" />
                            <asp:BoundField DataField="FromName" HeaderText="From Name" SortExpression="FromName" />
                            <asp:BoundField DataField="From" HeaderText="From Address" SortExpression="From" />
                            <asp:BoundField DataField="Subject" HeaderText="Subject" SortExpression="Subject" />
                            <Rock:DeleteField OnClick="gEmailTemplates_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>

        

    </ContentTemplate>
</asp:UpdatePanel>
