<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Upload.ascx.cs" Inherits="RockWeb.Blocks.Crm.PhotoRequest.Upload" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox runat="server" ID="nbWarning" NotificationBoxType="Warning" Text="No, that's not right. Are you sure you copied that web address correctly?" Visible="false"></Rock:NotificationBox>
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
