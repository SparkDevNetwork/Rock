<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RestKeyDetail.ascx.cs" Inherits="RockWeb.Blocks.Security.RestKeyDetail" %>

<asp:UpdatePanel ID="upnlRestKeys" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-key"></i> <asp:Literal ID="lTitle" runat="server" /></h1>
            </div>
            <div class="panel-body">

                <div id="pnlEditDetails" runat="server">
                    <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                    <asp:HiddenField ID="hfRestUserId" runat="server" />
                    <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />
                    <div class="row">
                        <div class="col-sm-6">
                            <Rock:RockTextBox ID="tbName" runat="server" Label="Name" />
                        </div>
                        <div class="col-sm-6">
                            <Rock:RockCheckBox ID="cbActive" runat="server" Checked="true" Label="Active" />
                        </div>

                        <div class="col-md-12">
                            <Rock:RockTextBox ID="tbDescription" runat="server" Label="Description" Help="Add a description for this REST API User" Rows="3" TextMode="MultiLine" />
                        </div>

                        <div class="col-md-6">
                            <div class="d-flex align-items-end">
                                <div class="flex-fill">
                                    <Rock:RockTextBox Label="Key" Help="Enter a key to use for a access token, or use the 'Generate Key' button to create a random 24 digit key." ID="tbKey" runat="server" MaxLength="24" />
                                </div>
                                <div class="form-group">
                                    <Rock:BootstrapButton ID="lbGenerate" runat="server" Text="Generate Key" CssClass="btn btn-primary ml-2" OnClick="lbGenerate_Click" />
                                </div>
                            </div>
                        </div>
                        <div class="col-md-6">

                        </div>
                    </div>
                    <div class="actions">
                        <asp:LinkButton ID="lbSave" runat="server" data-shortcut-key="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="lbSave_Click" />
                        <asp:LinkButton ID="lbCancel" runat="server" data-shortcut-key="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="lbCancel_Click" />
                    </div>
                </div>

            </div>


        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>