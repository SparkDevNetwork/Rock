<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AppleTvPageDetail.ascx.cs" Inherits="RockWeb.Blocks.Tv.AppleTvPageDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlBlock" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-tv"></i> 
                    <asp:Literal ID="lBlockTitle" runat="server" Text="New Page" />
                </h1>

                <div class="panel-labels">
                    <button id="btnCopyToClipboard" runat="server"
                        data-toggle="tooltip" data-placement="top" data-trigger="hover" data-delay="250" title="Copy Page Guid to Clipboard"
                        class="btn btn-info btn-xs btn-copy-to-clipboard"
                        onclick="$(this).attr('data-original-title', 'Copied').tooltip('show').attr('data-original-title', 'Copy Page Guid to Clipboard');return false;">
                        <i class='fa fa-clipboard'></i>
                    </button>
                </div>
            </div>

            <div class="panel-body">

                <Rock:NotificationBox ID="nbMessages" runat="server" NotificationBoxType="Warning" />

                <asp:Panel ID="pnlEdit" runat="server" >

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="tbPageName" runat="server" Label="Page Name" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbShowInMenu" runat="server" Label="Show In Menu" Checked="true" />
                        </div>
                    </div>

                    <Rock:RockTextBox ID="tbDescription" runat="server" Label="Description" TextMode="MultiLine" Rows="3" />

                    <Rock:CodeEditor ID="ceTvml" runat="server" EditorHeight="600" Label="Page TVML" />

                    <fieldset class="mb-4">
                        <Rock:CacheabilityPicker ID="cpCacheSettings" runat="server" Label="" />
                    </fieldset>

                    <Rock:AttributeValuesContainer ID="avcAttributes" runat="server" />

                    <div class="actions">
                        <asp:Button ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:Button ID="btnSaveAndEdit" runat="server" Text="Save Continue Editing" CssClass="btn btn-link" OnClick="btnSaveAndEdit_Click" />
                        <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" OnClick="btnCancel_Click" />
                    </div>
                    
                </asp:Panel>
                
            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>