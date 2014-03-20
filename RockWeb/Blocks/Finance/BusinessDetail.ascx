<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BusinessDetail.ascx.cs" Inherits="RockWeb.Blocks.Finance.BusinessDetail" %>

<asp:UpdatePanel ID="upnlBusinesses" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">
            <asp:HiddenField ID="hfBusinessId" runat="server" />
            <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

            <div class="banner"><h1><asp:Literal ID="lTitle" runat="server" /></h1></div>

            <div id="pnlEditDetails" runat="server">
                <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:DataTextBox ID="tbBusinessName" runat="server" Label="Title" TabIndex="1" 
                            SourceTypeName="Rock.Model.Person, Rock" PropertyName="BusinessName" ValidationGroup="businessDetail" />
                    </div>
                </div>
                
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbStreet1" runat="server" Label="Address Line 1" />
                        <Rock:RockTextBox ID="tbStreet2" runat="server" Label="Address Line 2" />
                        <div class="row">
                            <div class="col-lg-7">
                                <Rock:RockTextBox ID="tbCity" Label="City"  runat="server" />
                            </div>
                            <div class="col-lg-2">
                                <Rock:StateDropDownList ID="ddlState" Label="State" runat="server" UseAbbreviation="true" CssClass="input-mini" />
                            </div>
                            <div class="col-lg-3">
                                <Rock:RockTextBox ID="tbZip" Label="Zip" runat="server" CssClass="input-small" />
                            </div>
                        </div>
                    </div>
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbPhone" PrependText="<i class='fa fa-phone-square'></i>" runat="server" Text='<%# Eval("NumberFormatted")  %>' />
                        <Rock:RockTextBox ID="tbEmailAddress" runat="server" Label="Email Address" />
                        <!-- this is for contact information. in particular phone number and email address -->
                    </div>
                </div>

                <div class="actions">
                    <asp:LinkButton ID="lbSave" runat="server" Text="Save" CssClass="btn btn-primary btn-sm" OnClick="lbSave_Click" />
                    <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CssClass="btn btn-link btn-sm" CausesValidation="false" OnClick="lbCancel_Click" />
                </div>
            </div>

            <fieldset id="fieldsetViewSummary" runat="server">
                <div class="row">
                    <div class="col-md-12">
                        <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                    </div>
                    <div class="col-md-6">
                        <asp:Literal ID="lAddress" runat="server" />
                    </div>
                    <div class="col-md-6">
                        <asp:Literal ID="lContactInfo" runat="server" />
                    </div>
                </div>
                <div class="actions">
                    <asp:LinkButton ID="lbEdit" runat="server" Text="Edit" CssClass="btn btn-primary btn-sm" CausesValidation="false" OnClick="lbEdit_Click" />
                </div>
            </fieldset>

            <!-- this will be a grid labeled "Contacts". Which is really just a list of known relationships tied to the business. Just has to list the contact name. -->

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>