<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LifeGroupList.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.LifeGroupFinder.LifeGroupList" %>

<asp:UpdatePanel ID="upnlGroupList" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i runat="server" id="iIcon"></i>
                    <asp:Literal ID="lTitle" runat="server" Text="Group List" />
                </h1>
            </div>

            <div class="panel-body">
                <div class="row">
                    <div class="pull-left">
                        <asp:LinkButton ID="lbReturn" runat="server" OnClick="lbReturn_Click" Text='<< Back' />
                    </div>
                </div>
                <asp:PlaceHolder ID="phGroups" runat="server" />
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
