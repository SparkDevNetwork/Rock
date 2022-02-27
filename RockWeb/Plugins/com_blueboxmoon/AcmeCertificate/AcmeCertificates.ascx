<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AcmeCertificates.ascx.cs" Inherits="RockWeb.Plugins.com_blueboxmoon.AcmeCertificate.AcmeCertificates" %>

<asp:UpdatePanel ID="upCertificates" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlCertificates" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h3 class="panel-title"><i class="fa fa-certificate"></i> Certificates</h3>
            </div>

            <Rock:Grid ID="gCertificates" runat="server" OnGridRebind="gCertificates_GridRebind" OnRowSelected="gCertificates_RowSelected" DataKeyNames="Id" RowItemText="Certificate">
                <Columns>
                    <Rock:RockBoundField HeaderText="Name" DataField="Name" SortExpression="Name" />
                    <Rock:DateTimeField HeaderText="Last Renewed" DataField="LastRenewed" SortExpression="LastRenewed" HeaderStyle-HorizontalAlign="Right" />
                    <Rock:DateTimeField HeaderText="Expires" DataField="Expires" SortExpression="Expires" HeaderStyle-HorizontalAlign="Right" />
                    <Rock:RockBoundField HeaderText="Domains" DataField="Domains" HtmlEncode="false" />
                    <Rock:DeleteField OnClick="gCertificates_Delete" />
                </Columns>
            </Rock:Grid>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>