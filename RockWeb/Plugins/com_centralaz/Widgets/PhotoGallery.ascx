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

    /* Styling overrides for dropzone*/
    .dropzone {
        border: 2px dashed rgba(0, 0, 0, 0.3);
    }
    .dropzone.dz-clickable .dz-message, .dropzone.dz-clickable .dz-message *
    {
        font-size: 1.2em;
    }
    .dropzone.dz-clickable {
        background-color: #eee;
    }
    .dropzone.dz-drag-hover {
        border: 2px solid green;
        background-color: #dff0d8;
        color: #468847;
    }

    .dropzone.dz-drag-hover .dz-message span {
        display: none;
    }
    .dropzone.dz-drag-hover .dz-message:after {
        content: "Bingo. You're free to drop.";
        font-size: 1.2em;
    }

</style>
<asp:UpdatePanel runat="server" ID="upnlHtmlContent" ChildrenAsTriggers="false" UpdateMode="Conditional">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbApprovalRequired" runat="server" NotificationBoxType="Info" Text="Your changes will not be visible until they are reviewed and approved." Visible="false" />
        <div id="myCarousel" class="carousel slide carousel-fade" data-ride="carousel" data-interval="<%= PauseMilliseconds %>">
            <!-- Wrapper for slides -->
            <div class="carousel-inner" role="listbox">
                <asp:Repeater ID="rptPhoto" runat="server" OnItemDataBound="rptPhoto_ItemDataBound">
                    <ItemTemplate>
                        <div class="<%# (Container.ItemIndex == 0) ? "item active" : "item" %>">
                            <img src="<%# Container.DataItem %>" id="imgPhoto" runat="server" class="img-responsive" />
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>
        <%-- Edit Panel --%>
        <asp:Panel ID="pnlEditModel" runat="server" Visible="false">
            <Rock:ModalDialog ID="mdEdit" runat="server" CancelLinkVisible="false" OnSaveClick="lbOk_Click" SaveButtonText="Ok" Title="Edit Html">
                <Content>
                    <asp:UpdatePanel runat="server" ID="upnlEdit">
                        <ContentTemplate>
                            <div id="divPhotoDropzone" class="dropzone margin-b-md" runat="server">
                                <div class="fallback">
                                    <input name="file" type="file" multiple />
                                </div>
                            </div>
                            <div class="row">
                                <asp:ListView runat="server" ID="lvImages" OnItemCommand="lvImages_ItemCommand" OnItemDataBound="lvImages_ItemDataBound">
                                    <ItemTemplate>
                                        <div class="col-xs-6 col-md-3">
                                            <div class="well thumbnail">
                                                <asp:Image ID="imbItem" runat="server" ImageUrl="<%# Container.DataItem %>" class="thumbnail"/>
                                                <asp:LinkButton ID="lbDelete" runat="server" CommandName="Remove" OnClientClick="if (!confirm('Are you sure you want delete?')) return false;"
                                                    CommandArgument="<%# Container.DataItem %>" Text="" CssClass="fa fa-times fa-2x text-danger margin-r-lg" ToolTip="Click to delete this photo." />
                                            </div>
                                         </div>
                                    </ItemTemplate>
                                </asp:ListView>
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </Content>
            </Rock:ModalDialog>
        </asp:Panel>
        <script>
            Sys.Application.add_load(function () {
                $("#<%=divPhotoDropzone.ClientID%>").dropzone({
                    url: "../FileUploader.ashx?rootFolder=" + encodeURIComponent('<%= Rock.Security.Encryption.EncryptString( ImageFolderPath ) %>'),
                    init: function () {
                        this.on("queuecomplete", function (file) { __doPostBack('<%=upnlEdit.ClientID%>', null); });
                    }
                });
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
