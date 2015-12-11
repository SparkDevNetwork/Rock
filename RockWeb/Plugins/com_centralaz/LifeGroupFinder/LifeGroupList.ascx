<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LifeGroupList.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.LifeGroupFinder.LifeGroupList" %>

<asp:UpdatePanel ID="upnlGroupList" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbError" runat="server" NotificationBoxType="Danger" Visible="false"></Rock:NotificationBox>
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i runat="server" id="iIcon"></i>
                    <asp:Literal ID="lTitle" runat="server" Text="Group List" />
                </h1>
            </div>

            <div class="panel-body">
                <div class="row">
                    <div class="lifegrouplist-returnbutton pull-left">
                        <asp:LinkButton ID="lbReturn" runat="server" OnClick="lbReturn_Click" Text='<< Back' />
                    </div>
                </div>
                <asp:Literal ID="lOutput" runat="server"></asp:Literal>

                <asp:Literal ID="lDebug" Visible="false" runat="server"></asp:Literal>
                <div class="lifegrouplist-paginationbuttons">
                    <asp:LinkButton ID="btnBack" runat="server" CssClass="btn btn-default" Text="Back" Visible="false" OnClick="btnBack_Click"/>
                    <asp:LinkButton ID="btnNext" runat="server" CssClass="btn btn-primary" Text="Next" OnClick="btnNext_Click" />
                </div>
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
