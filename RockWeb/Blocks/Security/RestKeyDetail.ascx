<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RestKeyDetail.ascx.cs" Inherits="RockWeb.Blocks.Security.RestKeyDetail" %>

<asp:UpdatePanel ID="upnlRestKeys" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">

            <div class="banner"><h1><asp:Literal ID="lTitle" runat="server" /></h1></div>

            <div id="pnlEditDetails" runat="server">
                <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                <asp:HiddenField ID="hfRestUserId" runat="server" />
                <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />
                <div class="row">
                    <div class="col-md-12">
                        <div class="row">
                            <div class="col-md-12">
                                <div class="control-label">Name</div>
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-sm-6">
                                <Rock:RockTextBox ID="tbName" runat="server" CssClass="form-group" />
                            </div>
                            <div class="col-sm-6">
                                <div class="row">
                                    <div class="col-xs-12">
                                        <asp:CheckBox ID="cbActive" runat="server" Checked="true" Text="Active" />
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-12">
                        <Rock:RockTextBox ID="tbDescription" runat="server" Label="Description" Help="Add a description for this REST API User" Rows="3" TextMode="MultiLine" />
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-12">
                        <div class="control-label">Key</div>
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbKey" runat="server" MaxLength="12" CssClass="form-group" />
                    </div>
                    <div class="col-md-6">
                        <Rock:BootstrapButton ID="lbGenerate" runat="server" Text="Generate Key" CssClass="btn btn-primary" OnClick="lbGenerate_Click" />
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-12"></div>
                </div>
                <div class="actions">
                    <asp:LinkButton ID="lbSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="lbSave_Click" />
                    <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="lbCancel_Click" />
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>