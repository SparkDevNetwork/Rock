<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PassTester.ascx.cs" Inherits="RockWeb.Plugins.com_mineCartStudio.MinePass.PassTester" %>

<asp:UpdatePanel ID="upnlAccounts" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">

            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-id-card-alt"></i>
                        Pass Tester</h1>
                   
                </div>
                <div class="panel-body">
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="tbApiKey" runat="server" Label="API Key" Text="10B60F8D-0F23-4FAA-B35F-9A5F19F5F995" />
                            <Rock:RockTextBox ID="tbSerialNumber" runat="server" Label="Serial Number" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="tbTemplate" runat="server" Label="Template" Text="Camp-Template" />
                            <Rock:RockTextBox ID="tbPassPerson" runat="server" Label="Pass Person" Text="Ted Decker" />
                        </div>
                    </div>
                    
                    <Rock:CodeEditor ID="cePassJson" runat="server" Label="Pass JSON" EditorHeight="800" />

                    <asp:LinkButton ID="lbUploadPass" runat="server" CssClass="btn btn-primary" Text="Upload" OnClick="lbUploadPass_Click" />
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
