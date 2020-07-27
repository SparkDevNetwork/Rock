<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RockModalGallery.ascx.cs" Inherits="RockWeb.Blocks.Examples.RockModalGallery" %>

<asp:UpdatePanel ID="upnlExample" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title" data-toc-skip="1">
                    <i class="fa fa-magic"></i>
                    Modal Gallery
                </h1>
            </div>
            <div class="panel-body">

                <asp:Panel ID="pnlDetails" runat="server">

                    <a id="SimpleConfirmationModal"></a>
                    <h2>Simple Confirmation</h2>
                    <div runat="server" class="r-example">
                        <button class="btn btn-danger" onclick="javascript: Rock.dialogs.confirmDelete(event, 'foo'); return false;">Confirm Delete</button>
                    </div>

                    <a id="RockModalAlert"></a>
                    <h2>Rock Modal Alert</h2>
                    <div runat="server" class="r-example">
                        <button class="btn btn-default" runat="server" id="btnModalAlert" onserverclick="btnModalAlert_ServerClick">Show</button>
                        <Rock:ModalAlert ID="maModalAlert" runat="server" />
                    </div>

                    <a id="RockModalDialog"></a>
                    <h2>Rock Modal Dialog</h2>
                    <div runat="server" class="r-example">
                        <button class="btn btn-default" runat="server" id="btnModalDialog" onserverclick="btnModalDialog_ServerClick">Show</button>
                        <Rock:ModalDialog ID="mdModalDialog" runat="server" Title="A Dialog">
                            <Content>
                                <p>This is some HTML</p>
                                <p>And some other HTML</p>
                                <p>And even more here</p>
                                <button class="btn btn-danger" onclick="javascript: Rock.dialogs.confirmDelete(event, 'foo'); return false;">Confirm Delete</button>
                                <button class="btn btn-default" runat="server" id="btnModalDialogNested" onserverclick="btnModalDialogNested_ServerClick">Show Nested Dialog</button>

                                <Rock:ModalDialog ID="mdModalDialogNested" runat="server" Title="A Nested Dialog">
                                    <Content>
                                        <p>This is some HTML in a nested modal</p>
                                        <button class="btn btn-danger" onclick="javascript: Rock.dialogs.confirmDelete(event, 'foo'); return false;">Confirm Delete</button>
                                        <button class="btn btn-default" runat="server" id="btnModalAlertNested" onserverclick="btnModalAlertNested_ServerClick">Show Alert</button>
                                        <Rock:ModalAlert ID="maModalAlertNested" runat="server" />
                                    </Content>
                                </Rock:ModalDialog>
                            </Content>
                        </Rock:ModalDialog>
                    </div>

                </asp:Panel>

            </div>
    </ContentTemplate>
</asp:UpdatePanel>

