<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ShortLinkDetail.ascx.cs" Inherits="RockWeb.Blocks.Crm.ShortLinkDetail" %>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>
        
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server" >

        <asp:HiddenField ID="hfShortLinkId" runat="server" />

        <div class="panel-heading">
            <h1 class="panel-title"><i class="fa fa-link"></i> Shortened Link</h1>
        </div>
        <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
        <div class="panel-body">

            <asp:ValidationSummary ID="ValidationSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
            <Rock:NotificationBox ID="nbError" runat="server" NotificationBoxType="Danger" Visible="false" />

            <div id="pnlEditDetails" runat="server">

                <div class="row">
                    <div class="col-sm-6">
                        <Rock:RockDropDownList ID="ddlSite" runat="server" Label="Site" Required="true" 
                            RequiredErrorMessage="Site is Required" Help="The site to use for the short link." />
                        <asp:HiddenField ID="hfSiteUrl" runat="server" />
                    </div>
                    <div class="col-sm-6">
                        <Rock:RockTextBox ID="tbToken" runat="server" Label="Token" Required="true" RequiredErrorMessage="A Token is Required" Help="The token to use for the short link. Must be unique" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-sm-12">
                        <Rock:RockTextBox ID="tbUrl" runat="server" Label="URL" Required="true" RequiredErrorMessage="A URL is Required" Help="The URL that short link will direct users to."  />
                    </div>
                </div>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" ValidationGroup="ShortLinkDetail" ></asp:LinkButton>
                    <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" OnClick="btnCancel_Click" CausesValidation="false"></asp:LinkButton>
                </div>

            </div>

                <fieldset id="fieldsetViewDetails" runat="server">

                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                    <div class="row">
                        <div class="col-sm-6">
                            <Rock:RockLiteral ID="lSite" runat="server" Label="Site" />
                        </div>
                        <div class="col-sm-6">
                            <Rock:RockLiteral ID="lToken" runat="server" Label="Token" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-sm-12">
                            <Rock:RockLiteral ID="lUrl" runat="server" Label="Url" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" AccessKey="m" ToolTip="Alt+m" Text="Edit" CssClass="btn btn-primary" CausesValidation="false" OnClick="btnEdit_Click" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link btn-sm" CausesValidation="false" OnClick="btnDelete_Click"/>
                        <button id="btnCopy" runat="server"
                            data-toggle="tooltip" data-placement="top" data-trigger="hover" data-delay="250" title="Copy to Clipboard"
                            class="btn btn-default js-copy-clipboard pull-right" 
                            onclick="$(this).attr('data-original-title', 'Copied').tooltip('show').attr('data-original-title', 'Copy to Clipboard');return false;">
                            <i class='fa fa-clone'></i>
                        </button>
                    </div>

                </fieldset>
        </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
