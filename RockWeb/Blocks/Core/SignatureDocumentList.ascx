<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SignatureDocumentList.ascx.cs" Inherits="RockWeb.Blocks.Core.SignatureDocumentList" %>

<asp:UpdatePanel ID="upnlSettings" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlContent" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-file-o"></i> Documents</h1>
            </div>
            <div class="panel-body">

                    <Rock:ModalAlert ID="mdGridWarningValues" runat="server" />
                        
                    <div class="grid grid-panel">
                        <Rock:Grid ID="gSignatureDocuments" runat="server" AllowPaging="true" OnRowSelected="gSignatureDocuments_Edit"  RowItemText="Document">
                            <Columns>
                                <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                                <Rock:RockBoundField DataField="SignatureDocumentTemplate.Name" HeaderText="Document Type" SortExpression="SignatureDocumentTemplate.Name" />
                                <Rock:DateTimeField DataField="LastInviteDate" HeaderText="Last Invite Date" SortExpression="LastInviteDate" />
                                <Rock:PersonField DataField="AppliesToPersonAlias.Person" HeaderText="Applies To" />
                                <Rock:PersonField DataField="AssignedToPersonAlias.Person" HeaderText="Assigned To" />
                                <Rock:PersonField DataField="SignedByPersonAlias.Person" HeaderText="Signed By" />
                                <Rock:EnumField DataField="Status" HeaderText="Status" SortExpression="Status" />
                                <asp:HyperLinkField HeaderText="Document" DataNavigateUrlFields="FileId" DataNavigateUrlFormatString="~/GetFile.ashx?id={0}" DataTextField="FileText" Target="_blank" ItemStyle-HorizontalAlign="Center"  />
                                <Rock:DeleteField OnClick="gSignatureDocuments_Delete" HeaderText="Delete" />
                            </Columns>
                        </Rock:Grid>
                    </div>

            </div>
             
        </asp:Panel>
        
    </ContentTemplate>
</asp:UpdatePanel>
