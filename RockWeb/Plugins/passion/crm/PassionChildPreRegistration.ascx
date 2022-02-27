<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PassionChildPreRegistration.ascx.cs" Inherits="RockWeb.Plugins.passion.crm.PassionChildPreRegistration" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbError" runat="server" NotificationBoxType="Danger" />
        <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

        <div class="panel panel-block">


            <div class="panel-body">
                <div class="panel-heading">
                    <h1 class="text-center text-primary">Family Registration</h1>
                    <%--<h2 class="text-center">Atlanta</h2>--%>
                </div>
                <div class="col-md-6 col-md-offset-3 col-sm-6 col-sm-offset-3 col-xs-12">
                    <hr />
                    <asp:Panel ID="pnlVisit" runat="server">
                        <div class="panel-heading">
                            <h3 class="panel-title">Visit Information</h3>
                        </div>
                        <div class="panel-body">
                            <div class="row">
                                <asp:Panel CssClass="col-md-8 col-xs-12" runat="server" ID="pnlCampus">
                                    <Rock:CampusPicker ID="cpCampus" runat="server" CssClass="input-width-lg" Label="Campus" />
                                </asp:Panel>
                                <asp:Panel CssClass="col-md-8 col-xs-12" runat="server" ID="pnlPlannedDate">
                                    <Rock:DatePicker ID="dpPlannedDate" runat="server" Label="Planned Visit Date" AllowPastDateSelection="false" />
                                </asp:Panel>
                            </div>
                        </div>
                    </asp:Panel>

                    <asp:Panel ID="pnlChildren" runat="server" CssClass="panel-body">
                        <div class="row">
                            <h3 class="text-primary">Children</h3>
                            <Passion:PassionPreRegistrationChildren ID="prChildren" runat="server" OnAddChildClick="prChildren_AddChildClick" />
                        </div>
                    </asp:Panel>
                    <hr />
                    <asp:Panel ID="pnlParents" runat="server" CssClass="panel-body">
                        <div class="row">

                            <asp:HiddenField ID="hfFamilyGuid" runat="server" />
                            <asp:HiddenField ID="hfAdultGuid1" runat="server" />
                            <asp:HiddenField ID="hfAdultGuid2" runat="server" />

                            <h3 class="text-primary">First Adult</h3>
                            <div class="row">
                                <asp:Panel CssClass="col-sm-8 col-xs-12" runat="server" ID="pnlEmail1">
                                    <Rock:EmailBox ID="tbEmail1" runat="server" Label="Email" />
                                </asp:Panel>
                                <div class="col-sm-6 col-xs-12">
                                    <Rock:RockLiteral ID="lFirstName1" runat="server" Label="First Name" Visible="false" />
                                    <Rock:DataTextBox ID="tbFirstName1" runat="server" SourceTypeName="Rock.Model.Person" PropertyName="NickName" Label="First Name" />
                                </div>
                                <div class="col-sm-6 col-xs-12">
                                    <Rock:RockLiteral ID="lLastName1" runat="server" Label="Last Name" Visible="false" />
                                    <Rock:DataTextBox ID="tbLastName1" runat="server" SourceTypeName="Rock.Model.Person" PropertyName="LastName" />
                                </div>
                                <asp:Panel CssClass="col-sm-3 col-xs-12" runat="server" ID="pnlSuffix1">
                                    <Rock:DefinedValuePicker ID="dvpSuffix1" runat="server" Label="Suffix" />
                                </asp:Panel>
                                <asp:Panel CssClass="col-sm-8 col-xs-12" runat="server" ID="pnlGender1">
                                    <Rock:RockDropDownList ID="ddlGender1" runat="server" Label="Gender" />
                                </asp:Panel>
                                <div class="clearfix"></div>
                                <asp:Panel CssClass="col-sm-6 col-xs-12" runat="server" ID="pnlBirthDate1">
                                    <Rock:DatePicker ID="dpBirthDate1" runat="server" Label="Birthdate" AllowFutureDates="False" RequireYear="True" ShowOnFocus="false" StartView="decade" />
                                </asp:Panel>
                                <asp:Panel CssClass="col-sm-8 col-xs-12" runat="server" ID="pnlMaritalStatus1">
                                    <Rock:DefinedValuePicker ID="dvpMaritalStatus1" runat="server" Label="Marital Status" />
                                </asp:Panel>
                                <asp:Panel CssClass="col-sm-8 col-xs-12" runat="server" ID="pnlMobilePhone1">
                                    <Rock:PhoneNumberBox ID="pnMobilePhone1" runat="server" Label="Mobile Phone" />
                                </asp:Panel>
                                <Rock:DynamicPlaceholder ID="phAttributes1" runat="server" />
                            </div>

                            <hr />

                            <h3 class="text-primary">Second Adult</h3>
                            <div class="adult-2-fields">
                                <asp:HiddenField ID="hfSuffixRequired" runat="server" />
                                <asp:HiddenField ID="hfGenderRequired" runat="server" />
                                <asp:HiddenField ID="hfBirthDateRequired" runat="server" />
                                <asp:HiddenField ID="hfMaritalStatusRequired" runat="server" />
                                <asp:HiddenField ID="hfMobilePhoneRequired" runat="server" />
                                <asp:HiddenField ID="hfEmailRequired" runat="server" />

                                <div class="row">
                                    <asp:Panel CssClass="col-sm-8 col-xs-12 js-Adult2Required" runat="server" ID="pnlEmail2">
                                        <Rock:EmailBox ID="tbEmail2" runat="server" Label="Email" />
                                    </asp:Panel>
                                    <div class="col-sm-6 col-xs-12">
                                        <Rock:RockLiteral ID="lFirstName2" runat="server" Label="First Name" Visible="false" />
                                        <Rock:DataTextBox ID="tbFirstName2" runat="server" SourceTypeName="Rock.Model.Person" PropertyName="NickName" Label="First Name" />
                                    </div>
                                    <div class="col-sm-6 col-xs-12">
                                        <Rock:RockLiteral ID="lLastName2" runat="server" Label="Last Name" Visible="false" />
                                        <Rock:DataTextBox ID="tbLastName2" runat="server" SourceTypeName="Rock.Model.Person" PropertyName="LastName" />
                                    </div>
                                    <asp:Panel CssClass="col-sm-3 col-xs-12 js-Adult2Required" runat="server" ID="pnlSuffix2">
                                        <Rock:DefinedValuePicker ID="dvpSuffix2" runat="server" Label="Suffix" />
                                    </asp:Panel>
                                    <asp:Panel CssClass="col-sm-8 col-xs-12 js-Adult2Required" runat="server" ID="pnlGender2">
                                        <Rock:RockDropDownList ID="ddlGender2" runat="server" Label="Gender" />
                                    </asp:Panel>
                                    <asp:Panel CssClass="col-sm-6 col-xs-12 js-Adult2Required" runat="server" ID="pnlBirthDate2">
                                        <Rock:DatePicker ID="dpBirthDate2" runat="server" Label="Birthdate" AllowFutureDates="False" ShowOnFocus="false" StartView="decade" />
                                    </asp:Panel>
                                    <div class="clearfix"></div>
                                    <asp:Panel CssClass="col-sm-8 col-xs-12 js-Adult2Required" runat="server" ID="pnlMaritalStatus2">
                                        <Rock:DefinedValuePicker ID="dvpMaritalStatus2" runat="server" Label="Marital Status" />
                                    </asp:Panel>
                                    <asp:Panel CssClass="col-sm-8 col-xs-12 js-Adult2Required" runat="server" ID="pnlMobilePhone2">
                                        <Rock:PhoneNumberBox ID="pnMobilePhone2" runat="server" Label="Mobile Phone" />
                                    </asp:Panel>
                                    <Rock:DynamicPlaceholder ID="phAttributes2" runat="server" />
                                </div>
                            </div>

                            <hr />

                            <div class="row">
                                <div class="col-sm-12 col-xs-12">
                                    <Rock:AddressControl ID="acAddress" Label="Address" runat="server" UseStateAbbreviation="true" UseCountryAbbreviation="false" />
                                </div>
                                <div class="col-sm-8 col-xs-12">
                                    <Rock:DynamicPlaceholder ID="phFamilyAttributes" runat="server" />
                                </div>
                            </div>
                            <hr />
                        </div>
                    </asp:Panel>

                    <div class="col-sm-4 col-sm-offset-4 col-xs-12 text-center form-group">
                        <asp:LinkButton ID="lbSave" runat="server" Text="Save" CssClass="btn btn-primary btn-block" OnClick="btnSave_Click" />
                        <!--<asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" />-->
                    </div>
                </div>
            </div>
        </div>

        <script>

            function enableRequiredFields(enable) {

                $('.adult-2-fields').find("[id$='_rfv']").each(function () {
                    var domObj = $(this).get(0);
                    if (domObj != null) {
                        domObj.enabled = (enable != false);
                    }
                });
            }

        </script>
    </ContentTemplate>
</asp:UpdatePanel>
