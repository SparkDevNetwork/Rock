<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CaptivePortal.ascx.cs" Inherits="RockWeb.Blocks.Security.CaptivePortal" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfMacAddress" runat="server" />
        <asp:HiddenField ID="hfPersonAliasId" runat="server" />
        <Rock:NotificationBox ID="nbAlert" runat="server" NotificationBoxType="Warning" />
        <asp:ValidationSummary ID="valCaptivePortal" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-validation" ValidationGroup="CaptivePortal" />
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">
            
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-wifi"></i> Wifi Welcome</h1>
            </div>
            <div class="panel-body">
                
                <div class="row">
                    <div class="col-sm-6">
                        <Rock:RockTextBox ID="tbFirstName" runat="server" Required="false" Label="First Name" ValidationGroup="CaptivePortal" />
                    </div>
               
                    <div class="col-sm-6">
                        <Rock:RockTextBox ID="tbLastName" runat="server" Required="false" Label="Last Name" ValidationGroup="CaptivePortal" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-sm-6">
                        <Rock:PhoneNumberBox ID="tbMobilePhone" runat="server" Required="false" Label="Mobile Number" ValidationGroup="CaptivePortal" />
                    </div>
                
                    <div class="col-sm-6">
                        <Rock:RockTextBox ID="tbEmail" runat="server" Required="false" Label="Email Address" ValidationGroup="CaptivePortal" />
                    </div>
                </div>

                <%-- box here to display T&C --%>
                <div>
                    <div>
                        <iframe id="iframeLegalNotice" runat="server" style="width: 100%; height: 400px; background-color: #fff;"></iframe>
                    </div>
                </div>
                
                <%-- Checkbox here to indicate agreement with T&C --%>
                <div>
                    <div style="display: inline-block;">
                        <Rock:RockCheckBox ID="cbAcceptTAC" runat="server" ValidationGroup="CaptivePortal" />
                    </div>
                </div>

                <%-- Button here to connect --%>
                <div class="actions">
                    <div>
                        <asp:LinkButton ID="btnConnect" runat="server" Text="Connect To WiFi" CssClass="btn btn-primary" OnClick="btnConnect_Click" style="width:100%;" ValidationGroup="CaptivePortal" />
                    </div>
                </div>
            </div>
    
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>