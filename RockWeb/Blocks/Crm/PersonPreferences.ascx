<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonPreferences.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonPreferences" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-user"></i> Preferences</h1>
            </div>
            <div class="panel-body">
                <div class="row">
                    <div class="col-md-4">
                        <Rock:DefinedValuePicker ID="dvpOriginateCallSource" runat="server" Label="Call Origination Source" Help="The number to call when you use click-to-call." />
                    </div>
                </div>

                <asp:LinkButton ID="btnSave" runat="server" CssClass="btn btn-primary" Text="Save" OnClick="btnSave_Click" />
            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>