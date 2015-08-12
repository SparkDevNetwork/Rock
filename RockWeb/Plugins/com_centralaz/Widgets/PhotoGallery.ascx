<%@ Control Language="C#" AutoEventWireup="false" CodeFile="PhotoGallery.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Widgets.PhotoGallery" %>
<style>
    .carousel-fade .carousel-inner .item {
        opacity: 0;
        -webkit-transition-property: opacity;
        -moz-transition-property: opacity;
        -o-transition-property: opacity;
        transition-property: opacity;
    }

    .carousel-fade .carousel-inner .active {
        opacity: 1;
    }

        .carousel-fade .carousel-inner .active.left,
        .carousel-fade .carousel-inner .active.right {
            left: 0;
            opacity: 0;
            z-index: 1;
        }

    .carousel-fade .carousel-inner .next.left,
    .carousel-fade .carousel-inner .prev.right {
        opacity: 1;
    }

    .carousel-fade .carousel-control {
        z-index: 2;
    }
</style>
<asp:UpdatePanel runat="server" ID="upnlHtmlContent" ChildrenAsTriggers="false" UpdateMode="Conditional">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbApprovalRequired" runat="server" NotificationBoxType="Info" Text="Your changes will not be visible until they are reviewed and approved." Visible="false" />
        <div id="myCarousel" class="carousel slide carousel-fade" data-ride="carousel" data-interval="4000">
            <!-- Wrapper for slides -->
            <div class="carousel-inner" role="listbox">
                <asp:Repeater ID="rptPhoto" runat="server" OnItemDataBound="rptPhoto_ItemDataBound">
                    <ItemTemplate>
                        <div class="<%# (Container.ItemIndex == 0) ? "item active" : "item" %>">
                            <img src="<%# Container.DataItem %>" runat="server" />
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>
        <%-- Edit Panel --%>
        <asp:Panel ID="pnlEditModel" runat="server" Visible="false">
            <Rock:ModalDialog ID="mdEdit" runat="server" OnSaveClick="lbSave_Click" CancelLinkVisible="false" SaveButtonText="Close" Title="Edit Html">
                <Content>

                    <asp:UpdatePanel runat="server" ID="upnlEdit">
                        <ContentTemplate>
                            <div id="divPhotoDropzone" class="dropzone" runat="server">
                                <div class="fallback">
                                    <input name="file" type="file" multiple />
                                </div>
                            </div>
                            <asp:ListView runat="server" ID="lvImages" OnItemCommand="lvImages_ItemCommand" OnItemDataBound="lvImages_ItemDataBound">
                                <ItemTemplate>
                                    <asp:ImageButton ID="imbItem" runat="server"
                                        CommandArgument="<%# Container.DataItem %>"
                                        ImageUrl="<%# Container.DataItem %>" Width="320" Height="240"
                                        OnCommand="imbItem_Command" />

                                    <asp:LinkButton ID="lbDelete" runat="server" CommandName="Remove"
                                        CommandArgument="<%# Container.DataItem %>" Text="Delete" CssClass="fa fa-minus" />
                                </ItemTemplate>
                            </asp:ListView>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </Content>
            </Rock:ModalDialog>
        </asp:Panel>
        <script>
            Sys.Application.add_load(function () {
                $("#<%=divPhotoDropzone.ClientID%>").dropzone({
                    url: "../FileUploader.ashx?rootFolder=" + encodeURIComponent('<%= Rock.Security.Encryption.EncryptString(GetAttributeValue( "ImageRootFolder" )) %>'),
                    init: function () {
                        this.on("queuecomplete", function (file) { __doPostBack('<%=upnlEdit.ClientID%>', null); });
                    }
                });
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
