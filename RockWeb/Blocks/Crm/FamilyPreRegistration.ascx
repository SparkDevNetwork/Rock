<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FamilyPreRegistration.ascx.cs" Inherits="RockWeb.Blocks.Crm.FamilyPreRegistration" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbError" runat="server" NotificationBoxType="Danger" />
        <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

        <div class="panel panel-block">

            <div class="panel-body">

                <Rock:PanelWidget ID="pwVisit" runat="server" Title="Visit Information" Expanded="true">
                    <div class="row">
                        <asp:Panel CssClass="col-md-5" runat="server" ID="pnlCampus">
                            <Rock:CampusPicker ID="cpCampus" runat="server" CssClass="input-width-lg" Label="Campus" />
                        </asp:Panel>
                        <asp:Panel CssClass="col-md-5" runat="server" ID="pnlPlannedDate">
                            <Rock:DatePicker ID="dpPlannedDate" runat="server" Label="Planned Visit Date" AllowPastDateSelection="false" />
                        </asp:Panel>
                    </div>
                </Rock:PanelWidget>

                <Rock:PanelWidget ID="pwParents" runat="server" Title="Adult Information" Expanded="true">

                    <asp:HiddenField ID="hfFamilyGuid" runat="server" />
                    <asp:HiddenField ID="hfAdultGuid1" runat="server" />
                    <asp:HiddenField ID="hfAdultGuid2" runat="server" />

                    <h4>
                        <asp:Literal ID="lAdultHeading1" runat="server" /></h4>
                    <div class="row">
                        <div class="col-md-3">
                            <Rock:DataTextBox ID="tbFirstName1" runat="server" SourceTypeName="Rock.Model.Person" PropertyName="NickName" Label="First Name" />
                        </div>
                        <div class="col-md-3">
                            <Rock:DataTextBox ID="tbLastName1" runat="server" SourceTypeName="Rock.Model.Person" PropertyName="LastName" />
                        </div>
                        <asp:Panel CssClass="col-md-2" runat="server" ID="pnlSuffix1">
                            <Rock:DefinedValuePicker ID="dvpSuffix1" runat="server" Label="Suffix" />
                        </asp:Panel>
                        <asp:Panel CssClass="col-md-2" runat="server" ID="pnlGender1">
                            <Rock:RockDropDownList ID="ddlGender1" runat="server" Label="Gender" />
                        </asp:Panel>
                        <asp:Panel CssClass="col-md-2" runat="server" ID="pnlBirthDate1">
                            <Rock:DatePicker ID="dpBirthDate1" runat="server" Label="Birthdate" AllowFutureDateSelection="False" ForceParse="false" />
                        </asp:Panel>
                    </div>
                    <div class="row">
                        <asp:Panel class="col-md-3" runat="server" ID="pnlEmail1">
                            <Rock:EmailBox ID="tbEmail1" runat="server" Label="Email" />
                        </asp:Panel>
                        <asp:Panel class="col-md-3" runat="server" ID="pnlMobilePhone1">
                            <Rock:PhoneNumberBox ID="pnMobilePhone1" runat="server" Label="Mobile Phone" />
                        </asp:Panel>
                        <Rock:DynamicPlaceholder ID="phAttributes1" runat="server" />
                    </div>

                    <hr />

                    <h4>
                        <asp:Literal ID="lAdultHeading2" runat="server" /></h4>
                    <div class="row">
                        <div class="col-md-3">
                            <Rock:DataTextBox ID="tbFirstName2" runat="server" SourceTypeName="Rock.Model.Person" PropertyName="NickName" Label="First Name" />
                        </div>
                        <div class="col-md-3">
                            <Rock:DataTextBox ID="tbLastName2" runat="server" SourceTypeName="Rock.Model.Person" PropertyName="LastName" />
                        </div>
                        <asp:Panel CssClass="col-md-2" runat="server" ID="pnlSuffix2">
                            <Rock:DefinedValuePicker ID="dvpSuffix2" runat="server" Label="Suffix" />
                        </asp:Panel>
                        <asp:Panel CssClass="col-md-2" runat="server" ID="pnlGender2">
                            <Rock:RockDropDownList ID="ddlGender2" runat="server" Label="Gender" />
                        </asp:Panel>
                        <asp:Panel CssClass="col-md-2" runat="server" ID="pnlBirthDate2">
                            <Rock:DatePicker ID="dpBirthDate2" runat="server" Label="Birthdate" AllowFutureDateSelection="False" ForceParse="false" />
                        </asp:Panel>
                    </div>
                    <div class="row">
                        <asp:Panel class="col-md-3" runat="server" ID="pnlEmail2">
                            <Rock:EmailBox ID="tbEmail2" runat="server" Label="Email" />
                        </asp:Panel>
                        <asp:Panel class="col-md-3" runat="server" ID="pnlMobilePhone2">
                            <Rock:PhoneNumberBox ID="pnMobilePhone2" runat="server" Label="Mobile Phone" />
                        </asp:Panel>
                        <Rock:DynamicPlaceholder ID="phAttributes2" runat="server" />
                    </div>

                    <hr />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:AddressControl ID="acAddress" Label="Address" runat="server" UseStateAbbreviation="true" UseCountryAbbreviation="false" />
                        </div>
                        <asp:Panel ID="pnlFamilyAttributes" runat="server" CssClass="col-md-6">
                            <div class="row">
                                <Rock:DynamicPlaceholder ID="phFamilyAttributes" runat="server" />
                            </div>
                        </asp:Panel>
                    </div>
                </Rock:PanelWidget>

                <Rock:PanelWidget ID="pwChildren" runat="server" Title="Children" Expanded="true">
                    <Rock:PreRegistrationChildren ID="prChildren" runat="server" OnAddChildClick="prChildren_AddChildClick" />
                </Rock:PanelWidget>

                <div class="actions">
                    <asp:LinkButton ID="lbSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" />
                </div>

            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
