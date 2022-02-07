<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SystemCommunicationList.ascx.cs" Inherits="RockWeb.Blocks.Communication.SystemCommunicationList" %>

<asp:UpdatePanel ID="upSettings" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Danger" Visible="false" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-envelope"></i> System Communication List</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="rFilter" runat="server">
                        <Rock:CategoryPicker ID="cpCategory" runat="server" Label="Category" Required="false" EntityTypeName="Rock.Model.SystemCommunication" />
                        <Rock:RockDropDownList ID="ddlSupports" runat="server" Label="Supports" />
                        <Rock:RockDropDownList ID="ddlActiveFilter" runat="server" Label="Active Status" />
                    </Rock:GridFilter>
                    <Rock:Grid ID="gEmailTemplates" runat="server" AllowSorting="true" OnRowSelected="gEmailTemplates_Edit" OnRowDataBound="gEmailTemplates_RowDataBound">
                        <Columns>
                            <Rock:RockBoundField DataField="Title" HeaderText="Title" SortExpression="Title" />
                            <Rock:RockBoundField DataField="Subject" HeaderText="Subject" SortExpression="Subject" />
                            <Rock:RockBoundField DataField="Category.Name" HeaderText="Category" SortExpression="Category.Name" />
                            <Rock:RockBoundField DataField="From" HeaderText="From Address" SortExpression="From" />
                            <Rock:RockLiteralField ID="lSupports" HeaderText="Supports" />
                            <Rock:BoolField DataField="IsActive" SortExpression="IsActive" HeaderText="Active" />
                            <Rock:SecurityField TitleField="Title" />
                            <Rock:DeleteField OnClick="gEmailTemplates_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
