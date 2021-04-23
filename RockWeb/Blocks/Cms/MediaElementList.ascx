<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MediaElementList.ascx.cs" Inherits="RockWeb.Blocks.Cms.MediaElementList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-play-circle"></i> <asp:Literal ID="lTitle" runat="server" />
                </h1>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:GridFilter ID="gfFilter" runat="server">
                        <Rock:RockTextBox ID="txtElementName" runat="server" Label="Name" />
                    </Rock:GridFilter>
                    <Rock:Grid ID="gElementList" runat="server" AllowSorting="true" OnRowSelected="gElementList_RowSelected"  CssClass="js-grid-elements">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" ItemStyle-CssClass="js-name-element" />
                            <Rock:RockBoundField DataField="Duration" HeaderText="Duration" SortExpression="Duration" />
                            <Rock:DeleteField OnClick="gElementList_DeleteClick" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
