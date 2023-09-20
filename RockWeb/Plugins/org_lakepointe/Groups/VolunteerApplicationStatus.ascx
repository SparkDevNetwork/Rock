<%@ Control Language="C#" AutoEventWireup="true" CodeFile="VolunteerApplicationStatus.ascx.cs" Inherits="RockWeb.Plugins.org_lakepointe.Groups.VolunteerApplicationStatus" %>
<asp:UpdatePanel ID="upContent" runat="server">
    <ContentTemplate>
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">Volunteer Application Status</h1>
            </div>
            <div class="panel-body">
                <div class="row">
                    <div class="col-xs-12">
                        <asp:Literal runat="server" ID="lContent" Text="" />
                    </div>
                </div>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
