<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Upload.ascx.cs" Inherits="RockWeb.Blocks.Crm.PhotoRequest.Upload" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:PlaceHolder ID="phPhotos" runat="server"></asp:PlaceHolder>
        <asp:Repeater runat="server" ID="rptPhotos" OnItemDataBound="rptPhotos_ItemDataBound">
            <ItemTemplate>
                <Rock:ImageEditor ID="imgedPhoto" runat="server" Label="Name" ButtonText="<i class='fa fa-pencil'></i> Select Photo" 
                    ButtonCssClass="btn btn-primary margin-t-sm" CommandArgument='<%# Eval("Id") %>' 
                    OnFileSaved="imageEditor_FileSaved" ShowDeleteButton="false" />
            </ItemTemplate>
        </asp:Repeater>
    </ContentTemplate>
</asp:UpdatePanel>
