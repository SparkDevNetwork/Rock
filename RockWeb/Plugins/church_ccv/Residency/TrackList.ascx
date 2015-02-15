<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TrackList.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.Residency.TrackList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlList" CssClass="panel panel-block" runat="server">
            
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-code-fork"></i> Tracks</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gList" runat="server" AllowSorting="false" OnRowSelected="gList_Edit" DataKeyNames="Id" TooltipField="Description">
                        <Columns>
                            <Rock:ReorderField />
                            <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:DeleteField OnClick="gList_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>

            <asp:HiddenField ID="hfPeriodId" runat="server" />
            <Rock:ModalAlert ID="mdGridWarning" runat="server" />
    

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
