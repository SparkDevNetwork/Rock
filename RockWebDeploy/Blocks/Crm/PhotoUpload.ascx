<%@ control language="C#" autoeventwireup="true" inherits="RockWeb.Blocks.Crm.PhotoUpload, RockWeb" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox runat="server" ID="nbWarning" NotificationBoxType="Warning" Text="The address you provided is not valid. Are you sure you copied that web address correctly?" Visible="false"></Rock:NotificationBox>
        <asp:PlaceHolder ID="phPhotos" runat="server"></asp:PlaceHolder>
        <asp:Repeater runat="server" ID="rptPhotos" OnItemDataBound="rptPhotos_ItemDataBound">
            <ItemTemplate>
                <div class="photoupload-photo">
                    <Rock:ImageEditor ID="imgedPhoto" runat="server" Label="Name" ButtonText="<i class='fa fa-pencil'></i> Select Photo" 
                        ButtonCssClass="btn btn-primary margin-t-sm" CommandArgument='<%# Eval("Id") %>' 
                        OnFileSaved="imageEditor_FileSaved" ShowDeleteButton="false" />
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </ContentTemplate>
</asp:UpdatePanel>
