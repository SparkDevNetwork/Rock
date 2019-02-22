<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ReprintLabel.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.CheckIn.ReprintLabel" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-print"></i> Reprint Label</h1>
            </div>
            <div class="panel-body">

                <Rock:NotificationBox runat="server" ID="nbMessage" CssClass="margin-t-md margin-b-md"></Rock:NotificationBox>

              <%--  <Rock:RockDropDownList runat="server" ID="ddlDevice" Label="Printer"></Rock:RockDropDownList>

                <Rock:RockDropDownList runat="server" ID="ddlLabel" Label="Labels"></Rock:RockDropDownList>--%>
                
                <Rock:PersonPicker runat="server" ID="ppPerson" Label="Person Checking-in" />
                                                
                <Rock:BootstrapButton runat="server" ID="btnPrint" CssClass="btn btn-primary" OnClick="btnPrint_Click">Print</Rock:BootstrapButton>

            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
