<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BulkPhotoUpdater.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Core.BulkPhotoUpdater" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbConfigurationWarning" runat="server" NotificationBoxType="Warning" Visible="false" />

        <asp:Panel ID="pnlView" runat="server">
            <asp:HiddenField ID="hfPersonIds" runat="server" />
            <asp:HiddenField ID="hfOrphanedPhotoIds" runat="server" />

            <div class="panel panel-block">
                <div class="panel-heading">
                    <h4 class="panel-title pull-left">Bulk Photo Updater</h4>
                    <div class="panel-labels">
                        <asp:Literal ID="lProgressBar" runat="server"></asp:Literal>
                    </div>
                </div>
                <div class="panel-body">
                    <div class="col-md-12">
                        <Rock:DataViewPicker ID="dvpDataView" runat="server" Label="Filter By Data View" CssClass="margin-b-sm" AutoPostBack="true" OnSelectedIndexChanged="dvpDataView_SelectedIndexChanged" />
                    </div>

                    <div class="col-md-12">
                        <Rock:NotificationBox ID="nbWarning" runat="server" NotificationBoxType="Warning" Text="Currently, there are no photos to process." Dismissable="false" />
                    </div>

                    <asp:Panel ID="pnlDetails" runat="server">
                        <div class="col-md-3">
                            
                            <Rock:ImageEditor ID="imgPhoto" runat="server" Label="Photo" BinaryFileTypeGuid="03BD8476-8A9F-4078-B628-5B538F967AFC" />
                            <div class="form-inline margin-t-md">
                                <asp:LinkButton ID="btnRotate" runat="server" CssClass="btn btn-action btn-sm" OnClick="btnRotate_Click" ><i class="fa fa-repeat" title="rotate"></i></asp:LinkButton>
                                <asp:LinkButton ID="btnShrink" runat="server" CssClass="btn btn-action btn-sm" Text="Shrink" OnClick="btnShrink_Click" />
                                <Rock:NumberBox ID="nbShrinkWidth" AppendText="px" runat="server" Value="1000" Maximum="1000" Minimum="250" CssClass="input-width-md" />
                            </div>
                            
                            <Rock:RockLiteral ID="lPhotoDate" runat="server" Label="Photo Date/Time" />
                        </div>
                        <div class="col-md-9">
                            <h1 class="title name" style="margin-top: 0px;"><asp:Literal ID="lName" runat="server" /></h1>
                            <asp:Literal ID="lDimenions" runat="server" />
                            <asp:Literal ID="lSizeCheck" runat="server" />
                            <asp:Literal ID="lByteSizeCheck" runat="server" />
                            
                            <ul class="list-unstyled margin-t-sm">
                                <li><asp:Literal ID="lGender" runat="server" /></li>
                                <li><asp:Literal ID="lAge" runat="server" /></li>
                                <li><asp:Literal ID="lConnectionStatus" runat="server" /></li>                            
                            </ul>
                        </div>
                        <div class="col-md-12">
                            <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                            <asp:LinkButton ID="btnSkip" runat="server" AccessKey="k" Text="Skip" CssClass="btn btn-link" CausesValidation="false" OnClick="btnSkip_Click" />
                        </div>
                    </asp:Panel>
                </div>
            </div>
        </asp:Panel>
        <script>
            $(document).ready(function () {
                Sys.Application.add_load(function () {
                    // hide the green saved status in the image editor
                    $("span[id$='_lSaveStatus']").hide();
                })
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>