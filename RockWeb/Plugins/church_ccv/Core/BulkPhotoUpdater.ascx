<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BulkPhotoUpdater.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Core.BulkPhotoUpdater" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfPhotoIds" runat="server" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <asp:Literal ID="lProgressBar" runat="server"></asp:Literal>
            </div>
            <div class="panel-body">
                <div class="col-md-12">
                    <Rock:DataViewPicker ID="dvpDataView" runat="server" CssClass="margin-b-sm" AutoPostBack="true" OnSelectedIndexChanged="dvpDataView_SelectedIndexChanged" />
                </div>
                <div class="col-md-3">
                    <Rock:ImageEditor ID="imgPhoto" runat="server" Label="Photo" BinaryFileTypeGuid="03BD8476-8A9F-4078-B628-5B538F967AFC" />
                </div>
                <div class="col-md-9">
                    <h1 class="title name" style="margin-top: 0px;"><asp:Literal ID="lName" runat="server" /></h1>
                    <asp:Literal ID="lDimenions" runat="server" />
                    <asp:Literal ID="lSizeCheck" runat="server" />
                    <ul class="list-unstyled margin-t-sm">
                        <li><asp:Literal ID="lGender" runat="server" /></li>
                        <li<><asp:Literal ID="lAge" runat="server" /></li>
                        <li><asp:Literal ID="lConnectionStatus" runat="server" /></li>                            
                    </ul>
                </div>
                <div class="col-md-12">
                    <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnSkip" runat="server" AccessKey="k" Text="Skip" CssClass="btn btn-link" CausesValidation="false" OnClick="btnSkip_Click" />
                </div>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>