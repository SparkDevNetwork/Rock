<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonPreferences.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonPreferences" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="ti ti-user"></i> Preferences</h1>
            </div>
            <div class="panel-body">
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlDefaultSmsPhoneNumber"
                            runat="server"
                            Label="Default SMS Phone Number"
                            Help="The default phone number to use when sending SMS messages. This is currently only supported in specific places in Rock." />
                    </div>

                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbEmailClosingPhrase"
                            runat="server"
                            Label="E-mail Closing Phrase"
                            Help="A plain text string that will be appended to e-mails as your optional closing phrase. This is currently only supported in specific places in Rock." />

                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:DefinedValuePicker ID="dvpOriginateCallSource" runat="server" Label="Call Origination Source" Help="The number to call when you use click-to-call." />
                    </div>
                </div>

                <asp:LinkButton ID="btnSave" runat="server" CssClass="btn btn-primary" Text="Save" OnClick="btnSave_Click" />
            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>