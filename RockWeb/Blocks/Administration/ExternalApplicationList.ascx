<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ExternalApplicationList.ascx.cs" Inherits="RockWeb.Blocks.Administration.ExternalApplicationList" %>

<asp:UpdatePanel ID="upBinaryFile" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        <div class="grid">
            <Rock:Grid ShowHeader="false" ID="gExternalApplication" CssClass="table-padded" runat="server" AllowSorting="true" DisplayType="Light" OnRowDataBound="gExternalApplication_RowDataBound">
                <Columns>
                    <Rock:AttributeField DataField="Icon" HeaderText="" ItemStyle-CssClass="grid-icon" />
                    <asp:TemplateField SortExpression="Name" HeaderText="">
                        <ItemTemplate>
                            <h4>
                                <asp:Literal ID="lAppName" runat="server" />
                                <small>
                                    <asp:Literal ID="lVendorName" runat="server" />
                                </small>
                            </h4>
                            <%# Eval( "Description" ) %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:HyperLinkField Text="Download" />
                </Columns>
            </Rock:Grid>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
