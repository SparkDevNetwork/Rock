<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CacheManager.ascx.cs" Inherits="CacheManager" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-tachometer"></i>Cache Manager</h1>
            </div>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbMessage" runat="server" Visible="false" />
                <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-validation" />

                <div class="row">
                    <div class="col-md-6"> put the cache tags here </div>
                    <div class="col-md-3"> put the cache stats here </div>
                    <div class="col-md-3"> put the clear cache button and ddl here</div>
                </div>



            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>