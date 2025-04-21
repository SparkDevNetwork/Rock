<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CaptivePortal.ascx.cs" Inherits="RockWeb.Blocks.Security.CaptivePortal" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfMacAddress" runat="server" />
        <Rock:NotificationBox ID="nbAlert" runat="server" NotificationBoxType="Warning" />
        <asp:ValidationSummary ID="valCaptivePortal" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="CaptivePortal" />
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-wifi"></i> Wifi Welcome</h1>
            </div>

            <div class="panel-body">

                <div class="row">
                    <div class="col-sm-6">
                        <Rock:RockTextBox ID="tbFirstName" runat="server" Required="false" Label="First Name" ValidationGroup="CaptivePortal" NoSpecialCharacters="true" NoEmojisOrSpecialFonts="true" />
                    </div>

                    <div class="col-sm-6">
                        <Rock:RockTextBox ID="tbLastName" runat="server" Required="false" Label="Last Name" ValidationGroup="CaptivePortal" NoSpecialCharacters="true" NoEmojisOrSpecialFonts="true" />
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

                <div>
                    <div class="iframe-container document-scroll">
                        <asp:Literal ID="litLegalNotice" runat="server"></asp:Literal>
                    </div>
                </div>

                <div>
                    <div style="display: inline-block;">
                        <Rock:RockCheckBoxList ID="cblAcceptTAC" runat="server" ValidationGroup="CaptivePortal"></Rock:RockCheckBoxList>
                    </div>
                </div>

                <div class="actions">
                    <div>
                        <asp:LinkButton ID="btnConnect" runat="server" Text="Connect To WiFi" CssClass="btn btn-primary" OnClick="btnConnect_Click" style="width:100%;" ValidationGroup="CaptivePortal" />
                    </div>
                </div>

            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>