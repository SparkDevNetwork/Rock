<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RockUpdate.ascx.cs" Inherits="RockWeb.Blocks.Administration.RockUpdate" %>

<asp:UpdateProgress id="updateProgress" runat="server">
		<ProgressTemplate>
		    <div id="updateProgress"> <i class="icon-spinner icon-spin"></i> Processing...
		    </div>
		</ProgressTemplate>
</asp:UpdateProgress>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <div class="row-fluid">
	        <div class="span6">
		        <asp:Literal ID="litRockVersion" runat="server"></asp:Literal>
	        </div>
            <div class="span2 offset4">
                <Rock:Badge ID="badge" runat="server" BadgeType="Important" Visible="false" ToolTip="There are one or more updates available.">updates available</Rock:Badge>
            </div>
        </div>

        <div class="row-fluid">
	        <div class="span12">
                <asp:Literal ID="litMessage" runat="server"></asp:Literal>
                <Rock:NotificationBox ID="nbSuccess" runat="server" NotificationBoxType="Success" Visible="false" Heading="<i class='icon-ok-circle'></i> Success" />
                <Rock:NotificationBox ID="nbErrors" runat="server" NotificationBoxType="Error" Visible="false" Heading="<i class='icon-frown'></i> Sorry..." />
            </div>
        </div>

        <div class="row-fluid">
            <div class="span12">
                <div class="panel" runat="server" id="divPackage" visible="false">
                    <div class="row-fluid">
                        <div class="span3">
                            <asp:LinkButton ID="btnInstall" runat="server" Text="<i class='icon-cloud-download'></i> Update" Visible="false" CssClass="btn btn-primary" OnClick="btnInstall_Click" />
                            <asp:LinkButton ID="btnUpdate" runat="server" Text="<i class='icon-cloud-download'></i> Update" Visible="false" CssClass="btn btn-primary" OnClick="btnUpdate_Click" />
                        </div>
                        <div class="span9">
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

