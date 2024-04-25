<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SnippetList.ascx.cs" Inherits="RockWeb.Blocks.Communication.SnippetList" %>
<asp:UpdatePanel ID="upSettings" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Danger" Visible="false" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-sms"></i><asp:Literal ID="lTitle" Text="Snippets List" runat="server" /></h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="rFilter" runat="server">
                        <Rock:RockDropDownList ID="ddlTypeFilter" runat="server" Label="Ownership Type" />
                        <Rock:RockDropDownList ID="ddlActiveFilter" runat="server" Label="Active Status" />
                        <Rock:CategoryPicker ID="cpCategory" runat="server" Label="Category" EntityTypeName="Rock.Model.Snippet" />
                    </Rock:GridFilter>
                    <Rock:Grid ID="gSnippets" runat="server" AllowSorting="true" RowItemText="Snippets">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:RockBoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                            <Rock:BoolField DataField="Personal" HeaderText="Personal" SortExpression="Personal" />
                            <Rock:BoolField DataField="IsActive" HeaderText="Active" SortExpression="IsActive"/>
                            <Rock:SecurityField TitleField="Name" />
                            <Rock:DeleteField OnClick="gSnippets_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
