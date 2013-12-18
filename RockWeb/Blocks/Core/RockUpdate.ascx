<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RockUpdate.ascx.cs" Inherits="RockWeb.Blocks.Core.RockUpdate" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
    <div class="container">
        <div class="row">
            <div class="col-md-12">
                <asp:Literal ID="litRockVersion" runat="server"></asp:Literal>
                <div class="pull-right">
                    <Rock:HighlightLabel ID="hlUpdates" runat="server" LabelType="Danger" Visible="false" ToolTipToolTip="There are one or more updates available." Text="updates available" />
                </div>
            </div>
        </div>

        <div class="row">
            <div class="col-md-12">
                <asp:Literal ID="litMessage" runat="server"></asp:Literal>
                <Rock:NotificationBox ID="nbSuccess" runat="server" NotificationBoxType="Success" Visible="false" Heading="<i class='fa fa-check-circle-o'></i> Success" />
                <Rock:NotificationBox ID="nbErrors" runat="server" NotificationBoxType="Danger" Visible="false" Heading="<i class='fa fa-frown-o'></i> Sorry..." />
            </div>
        </div>

        <div class="panel panel-default" runat="server" id="divPackage" visible="false">
            <div class="panel-body">
                <div class="row">
                    <div class="col-md-3">
                        <asp:LinkButton ID="btnInstall" runat="server" Text="<i class='fa fa-cloud-download'></i> Update" Visible="false" CssClass="btn btn-primary" OnClick="btnInstall_Click" />
                        <asp:LinkButton ID="btnUpdate" runat="server" Text="<i class='fa fa-cloud-download'></i> Update" Visible="false" CssClass="btn btn-primary" OnClick="btnUpdate_Click" />
                    </div>
                    <div class="col-md-9">
                        <h3><asp:Literal ID="litPackageTitle" runat="server"></asp:Literal></h3>
                        <asp:Literal ID="litPackageDescription" runat="server"></asp:Literal>
                        <h4>Release Notes</h4>
                        <asp:Literal ID="litReleaseNotes" runat="server"></asp:Literal>
                    </div>
                </div>
            </div>
        </div>
    </div>
    </ContentTemplate>
</asp:UpdatePanel>

