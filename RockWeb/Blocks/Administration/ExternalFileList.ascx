<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ExternalFileList.ascx.cs" Inherits="RockWeb.Blocks.Administration.ExternalFileList" %>

<asp:UpdatePanel ID="upBinaryFile" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        
        <Rock:Grid ShowHeader="false" ID="gBinaryFile" CssClass="table-padded" runat="server" AllowSorting="true" DisplayType="Light" OnRowDataBound="gBinaryFile_RowDataBound" >
            <Columns>
                <Rock:AttributeField DataField="Icon" HeaderText="" />
                <asp:TemplateField SortExpression="Name" HeaderText="">
                    <ItemTemplate>
                       <h4><asp:Literal ID="lAppName" runat="server"></asp:Literal></h4>
                        <%# Eval( "Description" ) %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:HyperLinkField Text="Download" />
            </Columns>
        </Rock:Grid>

    </ContentTemplate>
</asp:UpdatePanel>
