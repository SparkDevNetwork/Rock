<%@ Control Language="C#" AutoEventWireup="true" CodeFile="VolunteerApplication.ascx.cs" Inherits="RockWeb.Plugins.org_lakepointe.VolunteerApplication.VolunteerApplication" %>
<%@ Import Namespace="RockWeb.Plugins.org_lakepointe.VolunteerApplication" %>

<style>
    .form-group.required > .control-label::after {
        font-weight: 900;
    }

    body {
        color: unset;
    }

    [style*="--aspect-ratio"] > :first-child {
        width: 100%;
    }
        [style*="--aspect-ratio"] > img {  
        height: auto;
    } 
    @supports (--custom:property) {
        [style*="--aspect-ratio"] {
            position: relative;
        }
        [style*="--aspect-ratio"]::before {
            content: "";
            display: block;
            padding-bottom: calc(100% / (var(--aspect-ratio)));
        }  
        [style*="--aspect-ratio"] > :first-child {
            position: absolute;
            top: 0;
            left: 0;
            height: 100%;
        }  
    }

    /* SNS 1/27/2023 -- This turns off the Rock admin toolbar at the bottom of the page. */
    /* It was necessary to prevent PuppeteerSharp from printing a black box at the bottom of each page when rendering to PDF. */
    /* Tried wrapping in an @media print {} clause, but it appears PS doesn't stiplulate the "print" predicate even when explicitly told to. */
    /* @media print { */
        .js-cms-admin-footer {
            display: none;
        }
    /* } */
</style>

<asp:UpdatePanel ID="upVolunteerApplication" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbWarning" runat="server" NotificationBoxType="Danger" />
        <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
        <asp:Literal ID="lProgress" runat="server" Text="This should have been replaced by a Progress Bar." />
        <asp:Panel ID="pnlLanguageChoice" runat="server" Visible="true">
            <h1 class="text-center">Language/Idioma</h1>
            <div class="row">
                <div class="col-md-12">
                    <asp:Literal ID="lEnglishPrompt" runat="server" Text="Please select your preferred language" /><br />
                    <asp:Literal ID="lSpanishPrompt" runat="server" Text="Selecciona tu idioma preferido" />
                    <Rock:RockDropDownList ID="ddlLanguage" runat="server" Enabled="true" Label="" AutoPostBack="true">
                        <asp:ListItem Text="English" Value="English" Selected="False"></asp:ListItem>
                        <asp:ListItem Text="Espa&ntilde;ol" Value="Spanish" Selected="False"></asp:ListItem>
                    </Rock:RockDropDownList>
                </div>
            </div>
        </asp:Panel>
        <asp:Panel ID="pnlPersonalInfo" runat="server" Visible="false">
            <asp:Panel ID="pnlParentNotification" runat="server" Visible="false">
                <hr />
                <div class="row">
                    <div class="col-md-12">
                        <Rock:RockCheckBox ID="cbParentNotification" runat="server" Visible="true" Enabled="true" Checked="false" />
                    </div>
                </div>
                <hr />
            </asp:Panel>
            <h1 class="text-center"><asp:Literal ID="PageTitlePersonalInformation" runat="server" /></h1>
            <div class="row">
                <div class="col-md-12">
                    <asp:Literal ID="PersonalInformationInstructions" runat="server" />
                </div>
            </div>
            <div class="row">
                <div class="col-md-12">
                    <h4>
                        <asp:Literal ID="lSectionTitleParentInfo" runat="server" /></h4>
                </div>
            </div>
            <div class="row">
                <div class="col-md-4">
                    <Rock:RockTextBox ID="tbFirstName" runat="server" Visible="true" Required="true" Enabled="false" />
                </div>
                <div class="col-md-4">
                    <Rock:RockTextBox ID="tbLastName" runat="server" Visible="true" Required="true" Enabled="false" />
                </div>
                <div class="col-md-4">
                    <Rock:RockDropDownList ID="ddlGender" runat="server" Visible="true" Required="true" Enabled="false" />
                </div>
            </div>
            <div class="row">
                <div class="col-md-4">
                    <Rock:PhoneNumberBox ID="tbPhone" runat="server" Visible="true" Required="true" Enabled="false" />
                </div>
                <div class="col-md-4">
                    <Rock:EmailBox ID="tbEmail" runat="server" Visible="true" Required="true" Enabled="false" />
                </div>
                <div class="col-md-4">
                    <Rock:DatePartsPicker ID="dpBirthDate" runat="server" Visible="true" Required="true" AllowFutureDates="false" Enabled="false" />
                </div>
            </div>
            <div class="row">
                <div class="col-md-12">
                    <asp:Literal ID="lPreferredContactInstructions" runat="server" />
                </div>
            </div>
            <div class="row">
                <div class="col-md-12">
                    <Rock:RockRadioButtonList ID="rblContactMethod" runat="server" Required="true" Enabled="true">
                        <asp:ListItem Text="Call" Value="1" Selected="False"></asp:ListItem>
                        <asp:ListItem Text="Email" Value="2" Selected="False"></asp:ListItem>
                        <asp:ListItem Text="Text" Value="3" Selected="False"></asp:ListItem>
                    </Rock:RockRadioButtonList>
                </div>
            </div>
            <p style="page-break-after: always;">&nbsp;</p>
        </asp:Panel>
        <asp:Panel ID="pnlMinorInfo" runat="server" Visible="false">
            <p style="page-break-before: always;">&nbsp;</p>
            <div class="row">
                <div class="col-md-12">
                    <h4>
                        <asp:Literal ID="lSectionTitleMinorInfo" runat="server" /></h4>
                </div>
            </div>
            <div class="row">
                <div class="col-md-12">
                    <Rock:RockDropDownList ID="ddlMinor" runat="server" AutoPostBack="true" />
                </div>
            </div>
            <div class="row">
                <div class="col-md-4">
                    <Rock:RockTextBox ID="tbKidFirstName" runat="server" Visible="true" Required="true" Enabled="false" />
                </div>
                <div class="col-md-4">
                    <Rock:RockTextBox ID="tbKidLastName" runat="server" Visible="true" Required="true" Enabled="false" />
                </div>
                <div class="col-md-4">
                    <Rock:RockDropDownList ID="ddlKidGender" runat="server" Visible="true" Required="true" Enabled="true" />
                </div>
            </div>
            <div class="row">
                <div class="col-md-4">
                    <Rock:PhoneNumberBox ID="tbKidPhone" runat="server" Visible="true" Required="false" Enabled="true" />
                </div>
                <div class="col-md-4">
                    <Rock:EmailBox ID="tbKidEmail" runat="server" Visible="true" Required="false" Enabled="true" />
                </div>
                <div class="col-md-4">
                    <Rock:DatePartsPicker ID="dpKidBirthDate" runat="server" Visible="true" Required="true" AllowFutureDates="false" Enabled="true" />
                </div>
            </div>
            <p style="page-break-after: always;">&nbsp;</p>
        </asp:Panel>
        <asp:Panel ID="pnlVolunteerOpportunitiesForMinors" runat="server" Visible="false">
            <p style="page-break-before: always;">&nbsp;</p>
            <h1 class="text-center"><asp:Literal ID="lSectionTitleVolunteerOpportunitiesForMinors" runat="server" /></h1>
            <div class="row" style="margin-top: 20px">
                <div class="col-xs-12">
                    <asp:Panel ID="pnlOpportunitiesEnglish" runat="server" Visible="true">
                        <!-- #include file="VolunteerOpportunitiesForMinors.inc" -->
                    </asp:Panel>
                    <asp:Panel ID="pnlOpportunitiesSpanish" runat="server" Visible="false">
                        <!-- #include file="VolunteerOpportunitiesForMinors.es.inc" -->
                    </asp:Panel>
                </div>
            </div>
            <p style="page-break-after: always;">&nbsp;</p>
        </asp:Panel>
        <asp:Panel ID="pnlMinistry" runat="server" Visible="false">
            <p style="page-break-before: always;">&nbsp;</p>
            <h1 class="text-center"><asp:Literal ID="lPageTitleCampusAndMinistry" runat="server" /></h1>
            <div class="row">
                <div class="col-md-12">
                    <Rock:CampusPicker ID="cpCampus" runat="server" />
                </div>
            </div>
            <div class="row">
                <div class="col-md-12">
                    <Rock:RockCheckBoxList ID="cblMinistries" runat="server" DataTextField="Name" DataValueField="Id" />
                </div>
            </div>
            <div class="row">
                <div class="col-md-12">
                    <Rock:RockTextBox ID="tbOther" runat="server" Visible="true" Required="false" />
                </div>
            </div>
            <p style="page-break-after: always;">&nbsp;</p>
        </asp:Panel>
        <asp:Panel ID="pnlBackground" runat="server" Visible="false">
            <p style="page-break-before: always;">&nbsp;</p>
            <h1 class="text-center">
                <asp:Literal ID="lPageTitleBackground" runat="server" /></h1>
            <h2 class="text-center"><asp:Literal ID="lPageSubtitleBackground" runat="server" /></h2>
            <div class="row">
                <div class="col-md-6">
                    <Rock:RockRadioButtonList ID="rblChurchHome" runat="server" Enabled="true" Required="true">
                        <asp:ListItem Value="Yes" Selected="False"></asp:ListItem>
                        <asp:ListItem Value="No" Selected="False"></asp:ListItem>
                    </Rock:RockRadioButtonList>
                </div>
                <div class="col-md-6">
                    <Rock:RockTextBox ID="tbChurchYears" runat="server" Visible="true" Required="false" />
                    <Rock:RockTextBox ID="tbChurchHome" runat="server" Visible="true" Required="false" />
                </div>
            </div>
            <div class="row">
                <div class="col-md-12">
                    <asp:Literal ID="lGridDescription" runat="server" />
                    <Rock:AttributeMatrixEditor ID="ameHistory" runat="server" AttributeMatrixTemplateId="9" />
                    <%--translate field names in AME?--%>
                </div>
            </div>
            <div class="row">
                <div class="col-md-12">
                    <Rock:RockTextBox ID="tbMotivation" runat="server" TextMode="MultiLine" Rows="5" Visible="true" Required="true" />
                </div>
            </div>
            <div class="row">
                <div class="col-md-6">
                    <Rock:RockRadioButtonList ID="rblAgeGroupOrGender" runat="server" Enabled="true" Required="true">
                        <asp:ListItem Value="Yes" Selected="False"></asp:ListItem>
                        <asp:ListItem Value="No" Selected="False"></asp:ListItem>
                        <asp:ListItem Value="Not Applicable" Selected="False"></asp:ListItem>
                    </Rock:RockRadioButtonList>
                </div>
                <div class="col-md-6">
                    <Rock:RockTextBox ID="rtbAgeGroupOrGender" runat="server" TextMode="MultiLine" Rows="5" Visible="true" Required="false" />
                </div>
            </div>
            <div class="row">
                <div class="col-md-12">
                    <asp:Literal ID="lConfidentialDisclaimer" runat="server" />
                </div>
            </div>
            <div class="row">
                <div class="col-md-6">
                    <Rock:RockRadioButtonList ID="rblHasAbused" runat="server" Enabled="true" Required="true">
                        <asp:ListItem Value="Yes" Selected="False"></asp:ListItem>
                        <asp:ListItem Value="No" Selected="False"></asp:ListItem>
                    </Rock:RockRadioButtonList>
                </div>
                <div class="col-md-6">
                    <Rock:RockTextBox ID="rtbHasAbused" runat="server" TextMode="MultiLine" Rows="5" Visible="true" Required="false" />
                </div>
            </div>
            <div class="row">
                <div class="col-md-6">
                    <Rock:RockRadioButtonList ID="rblHasBeenAccused" runat="server" Enabled="true" Required="true">
                        <asp:ListItem Value="Yes" Selected="False"></asp:ListItem>
                        <asp:ListItem Value="No" Selected="False"></asp:ListItem>
                    </Rock:RockRadioButtonList>
                </div>
                <div class="col-md-6">
                    <Rock:RockTextBox ID="rtbHasBeenAccused" runat="server" TextMode="MultiLine" Rows="5" Visible="true" Required="false" />
                </div>
            </div>
            <div class="row">
                <div class="col-md-6">
                    <Rock:RockRadioButtonList ID="rblPsycho" runat="server" Enabled="true" Required="true">
                        <asp:ListItem Value="Yes" Selected="False"></asp:ListItem>
                        <asp:ListItem Value="No" Selected="False"></asp:ListItem>
                    </Rock:RockRadioButtonList>
                </div>
                <div class="col-md-6">
                    <Rock:RockTextBox ID="rtbPsycho" runat="server" TextMode="MultiLine" Rows="5" Visible="true" Required="false" />
                </div>
            </div>
            <div class="row">
                <div class="col-md-6">
                    <Rock:RockRadioButtonList ID="rblHindrance" runat="server" Enabled="true" Required="true" Visible="true">
                        <asp:ListItem Value="Yes" Selected="False"></asp:ListItem>
                        <asp:ListItem Value="No" Selected="False"></asp:ListItem>
                    </Rock:RockRadioButtonList>
                </div>
                <div class="col-md-6">
                    <Rock:RockTextBox ID="rtbHindrance" runat="server" TextMode="MultiLine" Rows="5" Visible="true" Required="false" />
                </div>
            </div>
            <p style="page-break-after: always;">&nbsp;</p>
        </asp:Panel>
        <asp:Panel ID="pnlPersonalHistory" runat="server" Visible="false">
            <p style="page-break-before: always;">&nbsp;</p>
            <h1 class="text-center">
                <asp:Literal ID="lPageTitleReferences" runat="server" /></h1>
            <div class="row">
                <div class="col-md-12">
                    <asp:Literal ID="lReferenceDescription" runat="server" />
                    <Rock:AttributeMatrixEditor ID="ameReferences" runat="server" AttributeMatrixTemplateId="10" />
                    <asp:Literal ID="lReferenceFooter" runat="server" />
                </div>
            </div>
            <p style="page-break-after: always;">&nbsp;</p>
        </asp:Panel>
        <asp:Panel ID="pnlCoreBeliefs" runat="server" Visible="false">
            <p style="page-break-before: always;">&nbsp;</p>
            <h1 class="text-center">
                <asp:Literal ID="lPageTitleBelief" runat="server" /></h1>
            <div class="row" style="margin-top: 20px">
                <div class="col-xs-12">
                    <asp:Panel ID="pnlBeliefEnglish" runat="server" Visible="true">
                        <!-- #include file="Beliefs.inc" -->
                    </asp:Panel>
                    <asp:Panel ID="pnlBeliefSpanish" runat="server" Visible="false">
                        <!-- #include file="Beliefs.es.inc" -->
                    </asp:Panel>
                </div>
            </div>
            <hr />
            <div class="row">
                <div class="col-md-12" style="margin-left: 5%;">
                    <Rock:RockRadioButtonList ID="rblBelief" runat="server" Enabled="true" Label="">
                        <asp:ListItem Value="1" Selected="False"></asp:ListItem>
                        <asp:ListItem Value="2" Selected="False"></asp:ListItem>
                    </Rock:RockRadioButtonList>
                </div>
            </div>
            <p style="page-break-after: always;">&nbsp;</p>
        </asp:Panel>
        <asp:Panel ID="pnlSafetyPolicy1" runat="server" Visible="false">
            <p style="page-break-before: always;">&nbsp;</p>
            <div class="row">
                <div class="col-md-12">
                    <asp:Literal ID="lSafetyPolicyInstruction" runat="server" />
                </div>
            </div>
            <h1 class="text-center">
                <asp:Literal ID="lPageTitleSafetyPolicy1" runat="server" /></h1>
            <div class="row" style="margin-top: 20px">
                <div class="col-xs-12">
                    <asp:Panel ID="pnlSafety1English" runat="server" Visible="true">
                        <!-- #include file="SafetyPolicyPage1.inc" -->
                    </asp:Panel>
                    <asp:Panel ID="pnlSafety1Spanish" runat="server" Visible="false">
                        <!-- #include file="SafetyPolicyPage1.es.inc" -->
                    </asp:Panel>
                </div>
            </div>
            <asp:HyperLink ID="hlPolicyPDF" runat="server" Visible="true" Target="_blank" CssClass="btn btn-default" />
            <p style="page-break-after: always;">&nbsp;</p>
        </asp:Panel>
        <asp:Panel ID="pnlSafetyPolicy7" runat="server" Visible="false">
            <p style="page-break-before: always;">&nbsp;</p>
            <h1 class="text-center">
                <asp:Literal ID="lPageTitleSafetyPolicy7" runat="server" Visible="false" /></h1>
            <div class="row" style="margin-top: 20px">
                <div class="col-xs-12">
                    <asp:Panel ID="pnlSafety7English" runat="server" Visible="true">
                        <!-- #include file="SafetyPolicyPage7.inc" -->
                    </asp:Panel>
                    <asp:Panel ID="pnlSafety7Spanish" runat="server" Visible="false">
                        <!-- #include file="SafetyPolicyPage7.es.inc" -->
                    </asp:Panel>
                </div>
            </div>
            <p style="page-break-after: always;">&nbsp;</p>
            <p style="page-break-before: always;">&nbsp;</p>
            <asp:Panel ID="pnlStudentVolunteerAgreement" runat="server" Visible="false">
                <h1 class="text-center"><asp:Literal ID="lStudentVolunteerAgreement" runat="server" /></h1>
                <h3 class="text-center"><asp:Literal ID="lSignatureNote" runat="server" /></h3>
                <Rock:RockCheckBoxList ID="rcblStudentAgreement" runat="server" Enabled="true">
                    <asp:ListItem Value="1" Selected="False" />
                    <asp:ListItem Value="2" Selected="False" />
                    <asp:ListItem Value="3" Selected="False" />
                    <asp:ListItem Value="4" Selected="False" />
                    <asp:ListItem Value="5" Selected="False" />
                    <asp:ListItem Value="6" Selected="False" />
                </Rock:RockCheckBoxList>
                <p style="page-break-after: always;">&nbsp;</p>
                <p style="page-break-before: always;">&nbsp;</p>
                <h1 class="text-center"><asp:Literal ID="lSectionTitleNoticeOfRequirements" runat="server" /></h1>
                <p><asp:Literal ID="lNoticeOfRequirements" runat="server" /></p>
                <Rock:RockCheckBoxList ID="rcblNoticeOfRequirements" runat="server" Enabled="true">
                    <asp:ListItem Value="1" Selected="False" />
                </Rock:RockCheckBoxList>
                <div class="row">
                    <div class="col-md-12">
                        <h3 class="text-center"><asp:Literal ID="lSectionTitleParentGuardianApproval" runat="server" /></h3>
                        <Rock:RockCheckBoxList ID="rcblParentGuardianApproval" runat="server" Enabled="true" Label="">
                            <asp:ListItem Value="1" Selected="False"></asp:ListItem>
                            <asp:ListItem Value="2" Selected="False"></asp:ListItem>
                            <asp:ListItem Value="3" Selected="False"></asp:ListItem>
                            <asp:ListItem Value="4" Selected="False"></asp:ListItem>
                            <asp:ListItem Value="5" Selected="False"></asp:ListItem>
                        </Rock:RockCheckBoxList>
                    </div>
                </div>
                <asp:Panel ID="pnlSignatureParentGuardian" runat="server" Visible="false">
                    <div class="row">
                        <div class="col-md-12">
                            <h3><asp:Literal ID="lApplicantFullName" runat="server" /></h3>
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-7">
                            <h3 style="text-transform: unset;"><asp:Literal ID="lParentGuardianSignature" runat="server" /></h3>
                            <h3><asp:Literal ID="lParentGuardianFullName" runat="server" /></h3>
                        </div>
                        <div class="col-md-5">
                            <h3 style="text-transform: unset;"><asp:Literal ID="lParentGuardianDate" runat="server" /></h3>
                        </div>
                    </div>
                </asp:Panel>
            </asp:Panel>
            <asp:Panel ID="pnlAckAndAg" runat="server" Visible="false">
                <div class="row">
                    <div class="col-md-12">
                        <h3 class="text-center">
                            <asp:Literal ID="lSectionTitleAckAndAg" runat="server" /></h3>
                        <Rock:RockCheckBoxList ID="rcblAgree" runat="server" Enabled="true">
                            <asp:ListItem Value="1" Selected="False" />
                            <asp:ListItem Value="2" Selected="False" />
                            <asp:ListItem Value="3" Selected="False" />
                        </Rock:RockCheckBoxList>
                    </div>
                </div>
            </asp:Panel>
            <asp:Panel ID="pnlAckAndAgLevel2" runat="server" Visible="false">
                <div class="row">
                    <div class="col-md-12">
                        <h3 class="text-center">
                            <asp:Literal ID="lSectionTitleAckAndAgLevel2" runat="server" /></h3>
                        <Rock:RockCheckBoxList ID="rcblAgreeL2" runat="server" Enabled="true">
                            <asp:ListItem Value="1" Selected="False" />
                            <asp:ListItem Value="2" Selected="False" />
                        </Rock:RockCheckBoxList>
                    </div>
                </div>
            </asp:Panel>
            <asp:Panel ID="pnlAckAndAgLevel0" runat="server" Visible="false">
                <div class="row">
                    <div class="col-md-12">
                        <h3 class="text-center">
                            <asp:Literal ID="lSectionTitleAckAndAgLevel0" runat="server" /></h3>
                        <Rock:RockCheckBoxList ID="rcblAgreeL0" runat="server" Enabled="true">
                            <asp:ListItem Value="1" Selected="False" />
                            <asp:ListItem Value="3" Selected="False" />
                            <asp:ListItem Value="2" Selected="False" />
                        </Rock:RockCheckBoxList>
                    </div>
                </div>
            </asp:Panel>
            <p style="page-break-after: always;">&nbsp;</p>
            <asp:Panel ID="pnlAdultRelease" runat="server" Visible="false">
                <div class="row">
                    <asp:Panel runat="server" class="col-md-12">
                        <h3 class="text-center"><asp:Literal ID="lSectionTitleAdultRelease" runat="server" /></h3>
                        <asp:Panel ID="pnlInternationalStudent" runat="server" Visible="false">
                            <div class="row">
                                <div class="col-md-12">
                                    <Rock:RockRadioButtonList ID="rblInternationalStudent" runat="server" Enabled="true" Required="true" OnSelectedIndexChanged="rblInternationalStudent_SelectedIndexChanged" AutoPostBack="true">
                                        <asp:ListItem Value="Yes" Selected="False"></asp:ListItem>
                                        <asp:ListItem Value="No" Selected="False"></asp:ListItem>
                                    </Rock:RockRadioButtonList>
                                </div>
                            </div>
                            <asp:Panel ID="pnlInternationalStudentDetails" runat="server" Visible="false">
                                <div class="row">
                                    <div class="col-md-3">
                                        <Rock:RockTextBox ID="rtbSurname" runat="server" Visible="true" Required="true" Enabled="true" />
                                    </div>
                                    <div class="col-md-3">
                                        <Rock:RockTextBox ID="rtbGivenName" runat="server" Visible="true" Required="true" Enabled="true" />
                                    </div>
                                    <div class="col-md-6">
                                        <Rock:RockTextBox ID="rtbMothersMaidenName" runat="server" Visible="true" Required="true" Enabled="true" />
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-md-6">
                                        <Rock:RockTextBox ID="rtbOtherNames" runat="server" Visible="true" Required="true" Enabled="true" />
                                    </div>
                                    <div class="col-md-3">
                                        <Rock:RockTextBox ID="rtbPlaceOfBirth" runat="server" Visible="true" Required="true" Enabled="true" />
                                    </div>
                                    <div class="col-md-3">
                                        <Rock:DatePartsPicker ID="dppDateOfBirth" runat="server" Visible="true" Required="true" AllowFutureDates="false" Enabled="false" />
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-md-6">
                                        <Rock:RockTextBox ID="rtbCurrentAddress" runat="server" TextMode="MultiLine" Rows="5" Required="true" Visible="true" />
                                    </div>
                                    <div class="col-md-6">
                                        <Rock:RockTextBox ID="rtbYearsInTX" runat="server" Visible="true" Required="true" Enabled="true" />
                                        <Rock:RockTextBox ID="rtbMonthsAtLakepointe" runat="server" Visible="true" Required="true" Enabled="true" />
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-md-6">
                                        <Rock:RockTextBox ID="rtbPriorAddress" runat="server" TextMode="MultiLine" Rows="5" Required="true" Visible="true" />
                                    </div>
                                    <div class="col-md-6">
                                        <Rock:FileUploader ID="rfuIDFront" runat="server" Required="true" Enabled="true" />
                                        <Rock:RockTextBox ID="rtbSocialSecurityNumber" runat="server" Visible="true" Required="false" />
                                    </div>
                                </div>
                            </asp:Panel>
                        </asp:Panel>
                        <asp:Panel ID="pnlSSN" runat="server" Visible="false">
                            <div class="row">
                                <div class="col-md-12">
                                    <Rock:RockRadioButtonList ID="rblSSN" runat="server" Enabled="true">
                                        <asp:ListItem Value="Yes" Selected="False"></asp:ListItem>
                                        <asp:ListItem Value="No" Selected="False"></asp:ListItem>
                                    </Rock:RockRadioButtonList>
                                </div>
                            </div>
                        </asp:Panel>
                        <Rock:RockCheckBoxList ID="rcblAdultRelease" runat="server" Enabled="true" Label="">
                            <asp:ListItem Value="1" Selected="False"></asp:ListItem>
                            <asp:ListItem Value="2" Selected="False"></asp:ListItem>
                            <asp:ListItem Value="3" Selected="False"></asp:ListItem>
                            <asp:ListItem Value="4" Selected="False"></asp:ListItem>
                        </Rock:RockCheckBoxList>
                    </asp:Panel>
                </div>
            </asp:Panel>
            <asp:Panel ID="pnlMinorRelease" runat="server" Visible="false">
                <div class="row">
                    <div class="col-md-12">
                        <h3 class="text-center"><asp:Literal ID="lSectionTitleMinorRelease" runat="server" /></h3>
                        <Rock:RockCheckBoxList ID="rcblMinorRelease" runat="server" Enabled="true" Label="">
                            <asp:ListItem Value="1" Selected="False"></asp:ListItem>
                            <asp:ListItem Value="3" Selected="False"></asp:ListItem>
                            <asp:ListItem Value="4" Selected="False"></asp:ListItem>
                        </Rock:RockCheckBoxList>
                        <Rock:RockTextBox ID="rtbMinorSignature" runat="server" Visible="true" Required="true" Enabled="true" />
                        <h4 style="color:red;"><asp:Literal ID="lSignatureInstructionsPDF" runat="server" Visible="false" /></h4>
                    </div>
                </div>
            </asp:Panel>
        </asp:Panel>
        <asp:Panel ID="pnlSignature" runat="server" Visible="false">
            <h3 class="text-center"><asp:Literal ID="lPageTitleSignNow" runat="server" /></h3>
            <%-- Put the signature document html in an Iframe so it doesn't inherit styling from the page --%>
            <div class="styled-scroll">
                <asp:Panel ID="pnlIframeSignatureDocumentHTML" class="signaturedocument-container" runat="server">
                    <iframe id="iframeSignatureDocumentHTML" name="signature-document-html-iframe" class="signaturedocument-iframe js-signaturedocument-iframe" runat="server" src="javascript: window.frameElement.getAttribute('srcdoc');" style="width: 100%"></iframe>
                </asp:Panel>
            </div>
            <h4 style="color:red;"><asp:Literal ID="lSignatureInstructions" runat="server" Visible="false" /></h4>
            <Rock:ElectronicSignatureControl ID="escElectronicSignatureControl" runat="server" OnCompleteSignatureClicked="btnSignSignature_Click" CssClass="well" />
            <script type="text/javascript">
                function resizeIframe(el) {
                    el.style.height = el.contentWindow.document.documentElement.scrollHeight + 'px';
                }
            </script>
        </asp:Panel>
        <asp:Panel ID="pnlConfirmation" runat="server" Visible="false">
            <h1 class="text-center">
                <asp:Literal ID="lPageTitleConfirmation" runat="server" /></h1>
            <div class="row" style="margin-top: 20px">
                <div class="col-xs-12">
                    <asp:Literal ID="lConfirmationMessage" runat="server" />
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlLockedApplication" runat="server" Visible="false">
            <h1 class="text-center">
                <asp:Literal ID="lPageTitleLocked" runat="server" />
            </h1>
            <div class="row">
                <div class="col-md-12">
                    <asp:Literal ID="lCantUpdate" runat="server" /><br />
                </div>
            </div>
            <div class="row" style="margin-top: 20px">
                <div class="col-xs-12">
                    <asp:LinkButton ID="lbDownload" runat="server" Visible="false" CausesValidation="false" CssClass="btn btn-default"></asp:LinkButton>
                    <asp:LinkButton ID="lbRequestReopen" runat="server" Visible="true" CausesValidation="false" CssClass="btn btn-default pull-right"></asp:LinkButton>
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlNavigation" runat="server" Visible="true">
            <div class="row" style="margin-top: 20px">
                <div class="col-xs-12">
                    <p style="text-align: right;"><asp:Literal ID="lClickDoneFirst" runat="server" Visible="false" /></p>
                </div>
            </div>
            <div class="row" style="margin-top: 20px">
                <div class="col-xs-12">
                    <asp:LinkButton ID="lbPrevious" runat="server" Visible="true" CausesValidation="false" CssClass="btn btn-default"></asp:LinkButton>
                    <asp:LinkButton ID="lbNext" runat="server" Visible="true" CausesValidation="true" CssClass="btn btn-default pull-right"></asp:LinkButton>
                </div>
            </div>
            <div class="row" style="margin-top: 20px">
                <div class="col-xs-12">
                    <asp:Literal ID="lFooterMessage" runat="server" />
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
