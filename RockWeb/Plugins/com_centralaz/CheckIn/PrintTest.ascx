<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PrintTest.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.CheckIn.PrintTest" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-print"></i> Print Test</h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlblTest" runat="server" LabelType="Info" Text="Label" />
                </div>
            </div>
            <div class="panel-body">

                <div class="alert alert-info">
                    <h4>Print Test</h4>
                    <p>Utility to send sample or test labels to printers.</p>
                </div>

                <Rock:RockDropDownList runat="server" ID="ddlDevice" Label="Device"></Rock:RockDropDownList>

                <Rock:RockDropDownList runat="server" ID="ddlLabel" Label="Labels"></Rock:RockDropDownList>
                
                <Rock:PersonPicker runat="server" ID="ppPerson" Label="Person Checking-in" />
                                                
                <Rock:BootstrapButton runat="server" ID="bbtnPrint" CssClass="btn btn-primary" OnClick="bbtnPrint_Click">Print</Rock:BootstrapButton>

                <Rock:NotificationBox runat="server" ID="nbMessage" CssClass="alert"></Rock:NotificationBox>

            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
