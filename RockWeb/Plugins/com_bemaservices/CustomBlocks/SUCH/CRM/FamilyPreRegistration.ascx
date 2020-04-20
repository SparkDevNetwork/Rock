<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FamilyPreRegistration.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.CustomBlocks.SUCH.Crm.FamilyPreRegistration" %>
<style>
    .panel-default>.panel-heading {
        border:none;
        background:#3ab5e6;
        font-size:22px;
        border-radius:0px;
    }

    .panel-title {
        font-size:18px;
        text-transform:uppercase;
        font-weight:bold;
        color:#fff;
        padding:20px;
    }

    .panel {
        border-radius:0px;
    }

    .btn-primary {
        background:#3ab5e6;
        border:#3ab5e6;
        color:#fff;
    }

    hr {
        border-color:#ddd;
    }
</style>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbError" runat="server" NotificationBoxType="Danger" />
        <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

        <div class="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">Family Pre-Registration</h1>
            </div>
            <div class="panel-body">

                <asp:Panel ID="pnlVisit" runat="server" CssClass="panel panel-default">
                    <div class="panel-heading">
                        <h3 class="panel-title">Visit Information</h3>
                    </div>
                    <div class="panel-body">
                        <div class="row">
                            <asp:Panel CssClass="col-md-3" runat="server" ID="pnlCampus">
                                <Rock:CampusPicker ID="cpCampus" runat="server" CssClass="input-width-lg" Label="Campus"  Required="True"  />
                            </asp:Panel>
                            <asp:Panel CssClass="col-md-3" runat="server" ID="pnlPlannedDate">
                                <Rock:DatePicker ID="dpPlannedDate" runat="server" Label="First Time Visit Date" AllowPastDateSelection="false" Required="True" />
                            </asp:Panel>
							<asp:Panel CssClass="col-md-3" runat="server" ID="PnlServiceTime">
                                <Rock:RockDropDownList ID="ddlServiceTimes" runat="server" CssClass="input-width-lg" Label="Service Time"  Required="True"  /> 
                            </asp:Panel>
                            	<asp:Panel CssClass="col-md-3" runat="server" ID="PnlGuest">
                            <Rock:RockDropDownList ID="dllGuest" runat="server" CssClass="input-width-lg" Label="Are you visiting from out of town?"  Required="True"  />
                            </asp:Panel>
                        </div>
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlParents" runat="server" CssClass="panel panel-default">
                    <div class="panel-heading">
                        <h3 class="panel-title">Adult Information</h3>
                    </div>
                    <div class="panel-body">

                        <asp:HiddenField ID="hfFamilyGuid" runat="server" />
                        <asp:HiddenField ID="hfAdultGuid1" runat="server" />
                        <asp:HiddenField ID="hfAdultGuid2" runat="server" />

                        <h4>First Adult</h4>
                        <div class="row">
                            <div class="col-sm-3">
                                <Rock:RockLiteral ID="lFirstName1" runat="server" Label="First Name" Visible="false" />
                                <Rock:DataTextBox ID="tbFirstName1" runat="server" SourceTypeName="Rock.Model.Person" PropertyName="NickName" Label="First Name" />
                            </div>
                            <div class="col-sm-3">
                                <Rock:RockLiteral ID="lLastName1" runat="server" Label="Last Name" Visible="false" />
                                <Rock:DataTextBox ID="tbLastName1" runat="server" SourceTypeName="Rock.Model.Person" PropertyName="LastName" />
                            </div>
                            <asp:Panel CssClass="col-sm-3" runat="server" ID="pnlSuffix1">
                                <Rock:DefinedValuePicker ID="dvpSuffix1" runat="server" Label="Suffix" />
                            </asp:Panel>
                            <asp:Panel CssClass="col-sm-3" runat="server" ID="pnlGender1">
                                <Rock:RockDropDownList ID="ddlGender1" runat="server" Label="Gender" />
                            </asp:Panel>
                            <asp:Panel CssClass="col-sm-3" runat="server" ID="pnlBirthDate1">
                                <Rock:DatePicker ID="dpBirthDate1" runat="server" Label="Birthdate" AllowFutureDates="False" RequireYear="True" ShowOnFocus="false" StartView="decade" />
                            </asp:Panel>
                            <asp:Panel CssClass="col-sm-3" runat="server" ID="pnlMaritalStatus1">
                                <Rock:DefinedValuePicker ID="dvpMaritalStatus1" runat="server" Label="Marital Status" />
                            </asp:Panel>
                            <asp:Panel CssClass="col-sm-3" runat="server" ID="pnlMobilePhone1">
                                <Rock:PhoneNumberBox ID="pnMobilePhone1" runat="server" Label="Mobile Phone" />
                            </asp:Panel>
                            <asp:Panel CssClass="col-sm-6" runat="server" ID="pnlEmail1">
                                <Rock:EmailBox ID="tbEmail1" runat="server" Label="Email" />
                            </asp:Panel>
                            <Rock:DynamicPlaceholder ID="phAttributes1" runat="server" />
                        </div>

                        <hr />

                        <h4>Second Adult</h4>
                        <div class="adult-2-fields">
                            <asp:HiddenField ID="hfSuffixRequired" runat="server" />
                            <asp:HiddenField ID="hfGenderRequired" runat="server" />
                            <asp:HiddenField ID="hfBirthDateRequired" runat="server" />
                            <asp:HiddenField ID="hfMaritalStatusRequired" runat="server" />
                            <asp:HiddenField ID="hfMobilePhoneRequired" runat="server" />
                            <asp:HiddenField ID="hfEmailRequired" runat="server" />

                            <div class="row">
                                <div class="col-sm-3">
                                    <Rock:RockLiteral ID="lFirstName2" runat="server" Label="First Name" Visible="false" />
                                    <Rock:DataTextBox ID="tbFirstName2" runat="server" SourceTypeName="Rock.Model.Person" PropertyName="NickName" Label="First Name" />
                                </div>
                                <div class="col-sm-3">
                                    <Rock:RockLiteral ID="lLastName2" runat="server" Label="Last Name" Visible="false" />
                                    <Rock:DataTextBox ID="tbLastName2" runat="server" SourceTypeName="Rock.Model.Person" PropertyName="LastName" />
                                </div>
                                <asp:Panel CssClass="col-sm-3 js-Adult2Required" runat="server" ID="pnlSuffix2">
                                    <Rock:DefinedValuePicker ID="dvpSuffix2" runat="server" Label="Suffix" />
                                </asp:Panel>
                                <asp:Panel CssClass="col-sm-3 js-Adult2Required" runat="server" ID="pnlGender2">
                                    <Rock:RockDropDownList ID="ddlGender2" runat="server" Label="Gender" />
                                </asp:Panel>
                                <asp:Panel CssClass="col-sm-3 js-Adult2Required" runat="server" ID="pnlBirthDate2">
                                    <Rock:DatePicker ID="dpBirthDate2" runat="server" Label="Birthdate" AllowFutureDates="False" ShowOnFocus="false" StartView="decade" />
                                </asp:Panel>
                                <asp:Panel CssClass="col-sm-3 js-Adult2Required" runat="server" ID="pnlMaritalStatus2">
                                    <Rock:DefinedValuePicker ID="dvpMaritalStatus2" runat="server" Label="Marital Status" />
                                </asp:Panel>
                                <asp:Panel CssClass="col-sm-3 js-Adult2Required" runat="server" ID="pnlMobilePhone2">
                                    <Rock:PhoneNumberBox ID="pnMobilePhone2" runat="server" Label="Mobile Phone" />
                                </asp:Panel>
                                <asp:Panel CssClass="col-sm-6 js-Adult2Required" runat="server" ID="pnlEmail2">
                                    <Rock:EmailBox ID="tbEmail2" runat="server" Label="Email" />
                                </asp:Panel>
                                <Rock:DynamicPlaceholder ID="phAttributes2" runat="server" />
                            </div>
                        </div>

                        <hr />

                        <div class="row">
                            <div class="col-sm-6">
                                <Rock:AddressControl ID="acAddress" Label="Address" runat="server" UseStateAbbreviation="true" UseCountryAbbreviation="false" />
                            </div>
                            <div class="col-sm-6">
                                <Rock:DynamicPlaceholder ID="phFamilyAttributes" runat="server" />
                            </div>
                        </div>

                         <div class="row ">
                            <div class="col-sm-12">
                                <label>Additional Notes</label><br/>
                                <asp:TextBox ID="tbNewNote" runat="server" TextMode="MultiLine" width="100%" ></asp:TextBox>
                            </div>
                        </div>

                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlChildren" runat="server" CssClass="panel panel-default">
                    <div class="panel-heading">
                        <h3 class="panel-title">Children</h3>
                    </div>
                    <div class="panel-body">
                        <Rock:PreRegistrationChildren ID="prChildren" runat="server" OnAddChildClick="prChildren_AddChildClick" />
                    </div>
                </asp:Panel>

                <div class="actions">
                    <asp:LinkButton ID="lbSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" />
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

