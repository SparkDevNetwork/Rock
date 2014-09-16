<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContentTypeList.ascx.cs" Inherits="RockWeb.Blocks.Cms.ContentTypeList" %>

<asp:UpdatePanel ID="upContentType" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-lightbulb-o"></i> Content Type List</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gContentType" runat="server" AllowSorting="true" OnRowSelected="gContentType_Edit">
                        <Columns>
                            <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:BoolField DataField="RequiresApproval" HeaderText="Requires Approval" SortExpression="RequiresApproval" />
                            <Rock:BoolField DataField="IsSystem" HeaderText="System" SortExpression="IsSystem" />
                            <Rock:DeleteField OnClick="gContentType_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
