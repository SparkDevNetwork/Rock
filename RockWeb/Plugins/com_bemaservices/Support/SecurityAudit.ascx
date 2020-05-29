<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SecurityAudit.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.Support.SecurityAudit" %>
<style>
    .header-success {
        background-color: #16c98d !important;
    }

    .header-warning {
        background-color: #ffc870 !important;
    }

    .header-danger {
        background-color: #d4442e !important;
    }
</style>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <h3>
            <asp:Literal ID="lChecksPassed" runat="server" /></h3>
        <div id="divProgressBar" runat="server" class="progress" style="background-color: #dcdcdc;">
        </div>
        
        <div class="panel panel-widget rock-panel-widget">
            <div class="panel-heading clearfix clickable" id="divHeaderRockAdmin" runat="server" data-toggle="collapse" data-target="#divBodyRockAdmin">
            </div>

            <div id="divBodyRockAdmin" class="collapse panel-body">
                <div class="alert alert-info">
                    <p>
                        <asp:Literal ID="lDescriptionRockAdmin" runat="server" />
                    </p>
                </div>
                <div class="grid grid-panel">
                    <Rock:Grid ID="gRockAdmins" runat="server" AllowSorting="true">
                        <Columns>
                            <Rock:RockBoundField DataField="Person.FullName" HeaderText="Name" SortExpression="FullName" />
                            <Rock:RockBoundField DataField="DateTimeAdded" HeaderText="Date Added" SortExpression="AddedDateTime" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </div>
      
        <div class="panel panel-widget rock-panel-widget">
            <div class="panel-heading clearfix clickable" id="divHeaderSslEnabled" runat="server" data-toggle="collapse" data-target="#divBodySslEnabled">
            </div>

            <div id="divBodySslEnabled" class="collapse panel-body">
                <div class="alert alert-info">
                    <p>
                        <asp:Literal ID="lDescriptionSslEnabled" runat="server" />
                    </p>
                </div>
                <div class="grid grid-panel">
                    <Rock:Grid ID="gSslEnabled" runat="server" AllowSorting="true">
                        <Columns>
                            <Rock:RockBoundField DataField="Id" HeaderText="Id" SortExpression="Id" />
                            <Rock:RockBoundField DataField="Layout.Site.Name" HeaderText="Site" SortExpression="Site" />
                            <Rock:RockBoundField DataField="PageTitle" HeaderText="Name" SortExpression="Name" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </div>
      
        <div class="panel panel-widget rock-panel-widget">
            <div class="panel-heading clearfix clickable" id="divHeaderNonStaff" runat="server" data-toggle="collapse" data-target="#divBodyNonStaff">
            </div>

            <div id="divBodyNonStaff" class="collapse panel-body">
                <div class="alert alert-info">
                    <p>
                        <asp:Literal ID="lDescriptionNonStaff" runat="server" />
                    </p>
                </div>
                <div class="grid grid-panel">
                    <Rock:Grid ID="gNonStaff" runat="server" AllowSorting="true">
                        <Columns>
                            <Rock:RockBoundField DataField="Id" HeaderText="Id" SortExpression="Id" />
                            <Rock:RockBoundField DataField="Layout.Site.Name" HeaderText="Site" SortExpression="Site" />
                            <Rock:RockBoundField DataField="PageTitle" HeaderText="Name" SortExpression="Name" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
