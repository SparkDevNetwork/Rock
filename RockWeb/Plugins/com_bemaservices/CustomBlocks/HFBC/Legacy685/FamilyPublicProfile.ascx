<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FamilyPublicProfile.ascx.cs" Inherits="RockWeb.Plugins.org_hfbc.Legacy685.FamilyPublicProfile" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlGroupView" runat="server">
            <asp:Literal ID="lContent" runat="server"></asp:Literal>

            <asp:Literal ID="lDebug" runat="server"></asp:Literal>
        </asp:Panel>

        <asp:Panel ID="pnlGroupEdit" runat="server" Visible="false">
            <asp:ValidationSummary ID="vsGroupEdit" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

            <h2>
                <asp:Literal ID="lEditTitle" runat="server" />
            </h2>

            <div class="row">
                <div class="col-md-4">
                    <fieldset>
                        <Rock:CampusPicker ID="cpCampus" runat="server" Required="true" AutoPostBack="true" />
                    </fieldset>
                </div>
                <div class="col-md-8">
                    <asp:Panel ID="pnlPublicAddress" runat="server">
                        <fieldset>
                            <!--
                            <legend>
                                <asp:Literal ID="lAddressTitle" runat="server" Visible="false" />
                            </legend>
                            -->
                            <div class="clearfix">
                                <div class="pull-left margin-b-md">
                                    <asp:Literal ID="lPreviousAddress" runat="server" />
                                </div>
                                <div class="pull-right">
                                    <asp:LinkButton ID="lbMoved" CssClass="btn btn-default btn-xs" runat="server" OnClick="lbMoved_Click"><i class="fa fa-truck"></i> Moved</asp:LinkButton>
                                </div>
                            </div>

                            <asp:HiddenField ID="hfStreet1" runat="server" />
                            <asp:HiddenField ID="hfStreet2" runat="server" />
                            <asp:HiddenField ID="hfCity" runat="server" />
                            <asp:HiddenField ID="hfState" runat="server" />
                            <asp:HiddenField ID="hfPostalCode" runat="server" />
                            <asp:HiddenField ID="hfCountry" runat="server" />

                            <Rock:AddressControl ID="acAddress" runat="server" RequiredErrorMessage="Your Address is Required" />

                            <div class="margin-b-md">
                                <Rock:RockCheckBox ID="cbIsMailingAddress" runat="server" Text="This is my mailing address" Checked="true" />
                                <Rock:RockCheckBox ID="cbIsPhysicalAddress" runat="server" Text="This is my physical address" Checked="true" />
                            </div>
                        </fieldset>
                    </asp:Panel>
                </div>
            </div>

            </div>
            <div class="row">
                <div class="col-md-12">
                    <asp:PlaceHolder ID="phAttributes" runat="server" EnableViewState="true" />
                </div>
            </div>

            <div class="actions">
                <asp:Button ID="btnSaveGroup" runat="server" AccessKey="s" CssClass="btn btn-primary" Text="Save" OnClick="btnSaveGroup_Click" />
                <asp:LinkButton ID="lbCancelGroup" runat="server" AccessKey="c" CssClass="btn btn-link" OnClick="lbCancelGroup_Click" CausesValidation="false">Cancel</asp:LinkButton>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
