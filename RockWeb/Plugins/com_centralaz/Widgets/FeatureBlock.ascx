<%@ Control Language="C#" AutoEventWireup="false" CodeFile="FeatureBlock.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Widgets.FeatureBlock" %>

<style>
    /* Styling overrides for dropzone*/
    .dropzone {
        border: 2px dashed rgba(0, 0, 0, 0.3);
    }

        .dropzone.dz-clickable .dz-message, .dropzone.dz-clickable .dz-message * {
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

    .dz-progress {
        visibility: hidden;
    }
</style>
<asp:UpdatePanel runat="server" ID="upnlHtmlContent" ChildrenAsTriggers="false" UpdateMode="Conditional">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbApprovalRequired" runat="server" NotificationBoxType="Info" Text="Your changes will not be visible until they are reviewed and approved." Visible="false" />
        <asp:Literal ID="lOutput" runat="server" />
        <asp:Literal ID="lDebug" runat="server" />

        <%-- Edit Panel --%>
        <asp:Panel ID="pnlEditModel" runat="server" Visible="false">
            <Rock:ModalDialog ID="mdEdit" runat="server" OnSaveClick="lbOk_Click" SaveButtonText="Save" Title="Edit Html">
                <Content>
                    <asp:UpdatePanel runat="server" ID="upnlEdit">
                        <ContentTemplate>
                            <Rock:RockTextBox ID="tbTitle" runat="server" Label="Title" />
                            <Rock:NumberBox ID="nbXAxis" runat="server" Label="Background Position X Axis" MinimumValue="0" MaximumValue="100" NumberType="Integer" />
                            <Rock:NumberBox ID="nbYAxis" runat="server" Label="Background Position Y Axis" MinimumValue="0" MaximumValue="100" NumberType="Integer" />
                            <div id="divPhotoDropzone" class="dropzone margin-b-md" runat="server">
                                <div class="fallback">
                                    <input name="file" type="file" multiple />
                                </div>
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
                    autoProcessQueue: false,
                    init: function () {

                        var submitButton = document.querySelector("[id$='serverSaveLink']")
                        myDropzone = this;
                        submitButton.addEventListener("click", function () {
                            myDropzone.processQueue();
                        });

                    }
                });
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
