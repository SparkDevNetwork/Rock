<%@ Control Language="C#" AutoEventWireup="true" CodeFile="NamelessPersonList.ascx.cs" Inherits="RockWeb.Blocks.Communication.NamelessPersonList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-star"></i>
                    Blank List Block
                </h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gList" runat="server" AllowSorting="true">
                        <Columns>
                            <Rock:RockLiteralField ID="lUnmatchedPhoneNumber" HeaderText="Phone Number" SortExpression="Number" OnDataBound="lUnmatchedPhoneNumber_DataBound" />
                            <Rock:LinkButtonField ID="btnLinkToPerson" CssClass="fa fa-user" ToolTip="Link to Person" OnClick="btnLinkToPerson_Click" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
