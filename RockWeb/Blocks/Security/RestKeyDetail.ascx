<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RestKeyDetail.ascx.cs" Inherits="RockWeb.Blocks.Security.RestKeyDetail" %>

<asp:UpdatePanel ID="upnlBusinesses" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">

            <div class="banner"><h1><asp:Literal ID="lTitle" runat="server" /></h1></div>

            <div id="pnlEditDetails" runat="server">
                <div class="row">
                    <div class="col-md-12">
                        <div class="row">
                            <div class="col-sm-6">
                                <Rock:RockTextBox ID="tbName" runat="server" Label="Name" />
                            </div>
                            <div class="col-sm-6">
                                <div class="row">
                                    <div class="col-xs-12">
                                        <asp:CheckBox ID="cbActive" runat="server" Text="Active" />
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                        <div class="row">
                            <div class="col-md-12">
                                <div class="row">
                                    <div class="col-sm-7">
                                        <Rock:PhoneNumberBox ID="pnbPhone" runat="server" CountryCode='<%# Eval("CountryCode") %>' Number='<%# Eval("NumberFormatted")  %>' TabIndex="7" />
                                    </div>
                                    <div class="col-sm-2">
                                        <div class="row">
                                            <div class="col-xs-6">
                                                <asp:CheckBox ID="cbSms"  runat="server" Text="Sms" Checked='<%# (bool)Eval("IsMessagingEnabled") %>' TabIndex="8" />
                                            </div>
                                            <div class="col-xs-6">
                                                <asp:CheckBox ID="cbUnlisted" runat="server" Text="Unlisted" Checked='<%# (bool)Eval("IsUnlisted") %>' TabIndex="9" />
                                            </div>
                                        </div>
                                    </div>
                                    <div class="col-sm-3"></div>
                                </div>
                            </div>
                        </div>











                <div class="row">
                    <div class="col-md-12">
                        <Rock:RockTextBox ID="tbDescription" runat="server" Label="Description" Help="Add a description for this API Key" Rows="3" />
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbKey" runat="server" Label="Key" />
                    </div>
                    <div class="col-md-6">
                        <asp:LinkButton ID="lbGenerate" runat="server" Text="Generate Key" CssClass="btn btn-primary" OnClick="lbGenerate_Click" />
                    </div>
                </div>
                <div class="actions">
                    <asp:LinkButton ID="lbSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="lbSave_Click" />
                    <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="lbCancel_Click" />
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>