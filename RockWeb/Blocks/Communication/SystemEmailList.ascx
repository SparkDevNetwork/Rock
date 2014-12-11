<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SystemEmailList.ascx.cs" Inherits="RockWeb.Blocks.Communication.SystemEmailList" %>

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
                            <Rock:RockBoundField DataField="Category.Name" HeaderText="Category" SortExpression="Category.Name" />
                            <Rock:RockBoundField DataField="Title" HeaderText="Title" SortExpression="Title" />
                            <Rock:RockBoundField DataField="FromName" HeaderText="From Name" SortExpression="FromName" />
                            <Rock:RockBoundField DataField="From" HeaderText="From Address" SortExpression="From" />
                            <Rock:RockBoundField DataField="Subject" HeaderText="Subject" SortExpression="Subject" />
                            <Rock:DeleteField OnClick="gEmailTemplates_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>

        

    </ContentTemplate>
</asp:UpdatePanel>
