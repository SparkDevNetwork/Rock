<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContentChannelDetail.ascx.cs" Inherits="RockWeb.Blocks.Cms.ContentChannelDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server" >

            <asp:HiddenField ID="hfId" runat="server" />
            <asp:HiddenField ID="hfTypeId" runat="server" />

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

                    <Rock:ModalAlert ID="maContentChannelWarning" runat="server" />
                    <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                    <div class="row">
                        <div class="col-md-6">
                        <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.ContentChannel, Rock" PropertyName="Name" />
                        </div>
                        <div class="col-md-6">
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.ContentChannel, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockDropDownList ID="ddlChannelType" runat="server" Label="Type" Required="true" AutoPostBack="true" OnSelectedIndexChanged="ddlChannelType_SelectedIndexChanged" />
                            <Rock:RockDropDownList ID="ddlContentControlType" runat="server" Label="Content Control" Required="true" AutoPostBack="true" OnSelectedIndexChanged="ddlContentControlType_SelectedIndexChanged" />
                            <Rock:RockTextBox ID="tbRootImageDirectory" runat="server" Label="Root Image Directory" Help="The path to use for the HTML Editor's Image Folder Root (e.g. '~/content/my_channel_images' ) " />
                            <Rock:RockCheckBox ID="cbRequireApproval" runat="server" Label="Items Require Approval" Text="Yes" />
                        </div>
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbIconCssClass" runat="server" Label="Icon CSS Class" SourceTypeName="Rock.Model.ContentChannel, Rock" PropertyName="IconCssClass" />
                            <asp:PlaceHolder ID="phAttributes" runat="server" EnableViewState="false" />
                            <Rock:RockCheckBox ID="cbEnableRss" runat="server" Label="Enable RSS" Text="Yes" CssClass="js-content-channel-enable-rss" />
                            <div id="divRss" runat="server" class="js-content-channel-rss"> 
                                <Rock:DataTextBox ID="tbChannelUrl" runat="server" Label="Channel Url" SourceTypeName="Rock.Model.ContentChannel, Rock" PropertyName="ChannelUrl" />
                                <Rock:DataTextBox ID="tbItemUrl" runat="server" Label="Item Url" SourceTypeName="Rock.Model.ContentChannel, Rock" PropertyName="ItemUrl" />
                                <Rock:NumberBox ID="nbTimetoLive" runat="server" Label="Time to Live (TTL)" NumberType="Integer" MinimumValue="0" 
                                    Help="The number of minutes a feed can stay cached before it is refreshed from the source."/>
                            </div>
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="lbSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="lbSave_Click" />
                        <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="lbCancel_Click" />
                    </div>

                </div>

                <fieldset id="fieldsetViewSummary" runat="server" >
                    <p class="description">
                        <asp:Literal ID="lGroupDescription" runat="server"></asp:Literal>
                    </p>

                    <asp:Literal ID="lDetails" runat="server" />

                    <div class="actions">
                        <asp:LinkButton ID="lbEdit" runat="server" Text="Edit" CssClass="btn btn-primary" CausesValidation="false" OnClick="lbEdit_Click" />
                    </div>

                </fieldset>

            </div>

        </asp:Panel>
        
    </ContentTemplate>
</asp:UpdatePanel>
