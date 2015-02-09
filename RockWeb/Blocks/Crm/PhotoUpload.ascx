<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PhotoUpload.ascx.cs" Inherits="RockWeb.Blocks.Crm.PhotoUpload" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox runat="server" ID="nbWarning" NotificationBoxType="Warning" Text="The address you provided is not valid. Are you sure you copied that web address correctly?" Visible="false"></Rock:NotificationBox>
        <asp:PlaceHolder ID="phPhotos" runat="server"></asp:PlaceHolder>
        <asp:Repeater runat="server" ID="rptPhotos" OnItemDataBound="rptPhotos_ItemDataBound">
            <ItemTemplate>
                <div class="photoupload-photo">
                    <Rock:ImageEditor ID="imgedPhoto" runat="server" Label="Photo" ButtonText="<i class='fa fa-pencil'></i> Select Photo" 
                        ButtonCssClass="btn btn-primary btn-sm margin-t-sm" CommandArgument='<%# Eval("Id") %>' 
                        OnFileSaved="imageEditor_FileSaved" ShowDeleteButton="false"
                        BinaryFileTypeGuid="03BD8476-8A9F-4078-B628-5B538F967AFC" />
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </ContentTemplate>
</asp:UpdatePanel>
