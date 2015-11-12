<%@ Control Language="C#" AutoEventWireup="false" CodeFile="HtmlPasscodeBlock.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Widgets.HtmlPasscodeBlock" %>

<asp:UpdatePanel runat="server" ID="upnlHtmlContent" ChildrenAsTriggers="false" UpdateMode="Conditional">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbConfiguration" runat="server" NotificationBoxType="Danger" Visible="false" />
        <div class="well">
            <div class="row">
                <div class="col-md-8">
                    <Rock:RockTextBox ID="tbPasscode" Placeholder="Enter the passcode" runat="server" TextMode="Password" />
                </div>
                <div class="col-md-4">
                    <Rock:BootstrapButton ID="btnEnterPasscode" runat="server" OnClick="btnEnterPasscode_Click" Text="Enter" CssClass="btn btn-default"/>
                </div>
            </div>
        </div>
        <asp:Literal ID="lHtmlContent" runat="server"/>

        <%-- Edit Panel --%>
        <asp:Panel ID="pnlEdit" runat="server" Visible="false">
            <Rock:ModalDialog ID="mdEdit" runat="server" OnSaveClick="lbOk_Click" SaveButtonText="Save" Title="Block Configuration">
                <Content>
                    <asp:UpdatePanel runat="server" ID="upnlEdit">
                        <ContentTemplate>
                            <Rock:RockTextBox ID="tbPasscodeAttribute" runat="server" Label="Passcode" />
                            <Rock:RockDropDownList ID="ddlBlock" runat="server" Label="Block to show" Help="The block that will be unhidden when the correct passcode is given." />
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </Content>
            </Rock:ModalDialog>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
