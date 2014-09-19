<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContentItemDetail.ascx.cs" Inherits="RockWeb.Blocks.Cms.ContentItemDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server" >

            <asp:HiddenField ID="hfContentItemId" runat="server" />
            <asp:HiddenField ID="hfContentChannelId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title">
                    <asp:Literal ID="lIcon" runat="server" />
                    <asp:Literal ID="lTitle" runat="server" />
                </h1>
                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlContentChannel" runat="server" LabelType="Type" />
                </div>
            </div>

            <div class="panel-body">

                <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

                <div id="pnlEditDetails" runat="server">

                    <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                    <div class="row">
                        <div class="col-md-6">
                        <Rock:DataTextBox ID="tbTitle" runat="server" SourceTypeName="Rock.Model.ContentItem, Rock" PropertyName="Title" />
                        </div>
                        <div class="col-md-6">
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:CodeEditor ID="ceContent" runat="server" Label="Content" Height="400" EditorMode="Html" EditorTheme="Rock"  />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DateTimePicker ID="dtpStart" runat="server" Label="Start" Required="true" />
                            <Rock:DateTimePicker ID="dtpExpire" runat="server" Label="Expire" />
                            <Rock:DataTextBox ID="tbPermalink" runat="server" Label="Permalink" SourceTypeName="Rock.Model.ContentItem, Rock" PropertyName="Permalink" />
                        </div>
                        <div class="col-md-6">
                            <asp:PlaceHolder ID="phAttributes" runat="server" EnableViewState="false" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="lbSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="lbSave_Click" />
                        <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="lbCancel_Click" />
                    </div>

                </div>

                <fieldset id="fieldsetViewSummary" runat="server" >
                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                    <p class="description">
                        <asp:Literal ID="lContent" runat="server"></asp:Literal>
                    </p>

                    <div class="row">
                        <div class="col-md-6">
                            <asp:Literal ID="lDetails" runat="server" />
                        </div>
                        <div class="col-md-6">
                            <asp:Literal ID="lAttributes" runat="server" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="lbEdit" runat="server" Text="Edit" CssClass="btn btn-primary" CausesValidation="false" OnClick="lbEdit_Click" />
                    </div>

                </fieldset>

            </div>

        </asp:Panel>
        
    </ContentTemplate>
</asp:UpdatePanel>
