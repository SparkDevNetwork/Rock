<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MultiEventRegistrationWizard.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.Event.MultiEventRegistrationWizard" %>

<style>
    iframe {
        width: 100%;
        height: 800px;
        overflow: hidden;
        border-style: none;
    }

    .template-row {
        display: -webkit-box;
        display: -webkit-flex;
        display: -ms-flexbox;
        display: flex;
        flex-wrap: wrap;
    }

        .template-row > [class*='col-'] {
            display: flex;
            flex-direction: column;
        }
</style>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:HiddenField ID="hfTriggerScroll" runat="server" Value="" />
        <asp:HiddenField ID="hfAllowNavigate" runat="server" Value="" />

        <asp:ValidationSummary ID="vsSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
        <Rock:NotificationBox ID="nbPaymentValidation" runat="server" NotificationBoxType="Danger" Visible="false" />

        <Rock:NotificationBox ID="nbMain" runat="server" Visible="false"></Rock:NotificationBox>
        <Rock:NotificationBox ID="nbWaitingList" runat="server" Visible="false" NotificationBoxType="Warning" />

        <asp:Panel ID="pnlStart" runat="server" Visible="false" CssClass="registrationentry-intro">
            <asp:Literal ID="lPreHtmlInstructions" runat="server" />
            <asp:Literal ID="lInstructions" runat="server" />

            <h2>
                <asp:Literal ID="lCategoryName" runat="server" /></h2>
            <asp:Literal ID="lCategoryDetails" runat="server" />
            </br>
        </br>
            <div class="row">
                <div class="col-md-offset-4 col-md-4">
                    <asp:HiddenField ID="hfGradeRequired" runat="server" />
                    <asp:HiddenField ID="hfChildrenOnly" runat="server" />

                    <asp:Repeater ID="rFamilyMembers" runat="server" OnItemDataBound="rFamilyMembers_ItemDataBound" OnItemCommand="rFamilyMembers_ItemCommand">
                        <HeaderTemplate>
                            <b>
                                <asp:Label ID="lFamilyMembersLabel" runat="server" Text="Who would you like to register?" /></b>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <asp:HiddenField ID="hfPersonAliasId" runat="server" />
                            <div class="row" style="text-align: left;">
                                <div class="col-md-10">
                                    <asp:CheckBox ID="cbFamilyMember" runat="server" />
                                    <asp:Panel ID="divFamilyText" runat="server" Style="margin-top: 10px; margin-bottom: 10px;">
                                        <asp:Literal ID="lFamilyMember" runat="server" />
                                    </asp:Panel>
                                </div>
                                <div class="col-md-2">
                                    <asp:LinkButton ID="lbEditGroupMember" runat="server" CssClass="btn btn-danger btn-xs" CommandArgument='<%# Eval("Guid") %>' CommandName="Update" Visible="false"> Edit</asp:LinkButton>
                                </div>
                            </div>
                        </ItemTemplate>
                        <FooterTemplate>
                            <asp:LinkButton ID="lbAddGroupMember" runat="server" CssClass="btn btn-default btn-xs" Text="Add Family Member" OnClick="lbAddGroupMember_Click" />
                        </FooterTemplate>
                    </asp:Repeater>
                </div>
            </div>

            <asp:Panel ID="pnlEditFamilyMember" runat="server" Visible="false">
                </br>
                <div class="row">
                    <asp:HiddenField ID="hfMemberId" runat="server" />

                    <div class="well col-md-offset-2 col-md-8">
                        <Rock:NotificationBox ID="nbBirthdate" runat="server" Visible="false" NotificationBoxType="Danger" />

                        <div style="text-align: center;">
                        </div>
                        <div class="row">
                            <div class="col-md-3">
                                <Rock:ImageEditor ID="imgPhoto" runat="server" Label="Photo" Required="true" ButtonText="<i class='fa fa-camera'></i>" BinaryFileTypeGuid="03BD8476-8A9F-4078-B628-5B538F967AFC" />
                            </div>
                            <div class="col-md-9">
                                <div class="row">
                                    <div class="col-md-6">
                                        <Rock:RockTextBox ID="rtbMemberFirstName" runat="server" Label="First Name" Required="true" />
                                        <Rock:DatePicker ID="dpMemberBirthDate" runat="server" Label="Birthday" Required="true" />
                                    </div>
                                    <div class="col-md-6">
                                        <Rock:RockTextBox ID="rtbMemberLastName" runat="server" Label="Last Name" Required="true" />
                                        <Rock:GradePicker ID="dvpMemberGrade" runat="server" Label="Grade" Required="true" />
                                    </div>
                                </div>
                                <Rock:GroupRolePicker ID="rtbMemberRelationship" runat="server" Label="Role" GroupTypeId="10" Required="true" />
                                <Rock:DefinedValuePicker ID="dvpMemberSchool" runat="server" Label="School" DefinedTypeId="34" />
                            </div>
                        </div>

                        <asp:LinkButton ID="lbSaveGroupMember" runat="server" CssClass="btn btn-primary" Text="Save" OnClick="lbSaveGroupMember_Click" />
                        <asp:LinkButton ID="lbCancelGroupMember" runat="server" CssClass="btn btn-default" Text="Cancel" OnClick="lbCancelGroupMember_Click" CausesValidation="false" />

                    </div>
                </div>
            </asp:Panel>

            <div class="actions">
                <Rock:BootstrapButton ID="lbHowManyNext" runat="server" AccessKey="n" ToolTip="Alt+n" Text="Next" DataLoadingText="Next" CssClass="btn btn-primary pull-right" CausesValidation="true" OnClick="lbHowManyNext_Click" />
            </div>
            <asp:Literal ID="lPostHtmlInstructions" runat="server" />
        </asp:Panel>

        <asp:Panel ID="pnlSelectRegistrations" runat="server" Visible="false">
            <h1 style="text-align: center; color: red;">
                <asp:Literal ID="lCurrentRegistrantName" runat="server" /></h1>
            <h2 style="text-align: center;">Which Daycation weeks would you like to register for?</h2>
            <h4 style="text-align: center;">A non-refundable $50 Admin fee will be applied to your registration.</h4>
            <h5 style="text-align: center;">
                <asp:Literal ID="lSpecialtyLink" runat="server" /></h5>
            <div class="row template-row">
                <asp:Repeater ID="rptRegistrations" runat="server" OnItemDataBound="rptRegistrations_ItemDataBound">
                    <ItemTemplate>
                        <div id="divItem" runat="server" class="col-md-3">
                            <div class="panel panel-default">
                                <div class="panel-heading" style="text-align: center;">
                                    <asp:Literal ID="lTemplateName" runat="server" />
                                    <asp:HiddenField ID="hfInstanceId" runat="server" />
                                    <Rock:RockCheckBox ID="cbTemplate" runat="server" AutoPostBack="true" Visible="false" OnCheckedChanged="cbTemplate_CheckedChanged" />
                                </div>
                                <div class="panel-body">
                                    <Rock:RockRadioButtonList ID="rrbRegistrationInstances" FormGroupCssClass="list-group-item" runat="server" DataValueField="Id" DataTextField="Name" TextAlign="Left" />
                                </div>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
            <div class="actions">
                <asp:LinkButton ID="lbSelectRegistrationPrev" runat="server" AccessKey="p" ToolTip="Alt+p" Text="Previous" CssClass="btn btn-default pull-left" CausesValidation="false" OnClick="lbSelectRegistrationPrev_Click" Visible="false" />
                <Rock:BootstrapButton ID="lbSelectRegistrationNext" runat="server" AccessKey="n" ToolTip="Alt+n" Text="Next" DataLoadingText="Next" CssClass="btn btn-primary pull-right" CausesValidation="true" OnClick="lbSelectRegistrationNext_Click" />
            </div>
        </asp:Panel>
        <%-- Prompt for any Registration Attributes that should be prompted for before entering registrations--%>
        <asp:Panel ID="pnlRegistrationAttributesStart" runat="server" Visible="false" CssClass="registrationentry-registration-attributes">

            <h1>
                <asp:Literal ID="lRegistrationAttributesStartTitle" runat="server" /></h1>

            <asp:Panel ID="pnlRegistrationAttributesStartProgressBar" runat="server">
                <div class="progress">
                    <div class="progress-bar" role="progressbar" aria-valuenow="<%=CurrentRegistrationInformation.PercentComplete%>" aria-valuemin="0" aria-valuemax="100" style="width: <%=CurrentRegistrationInformation.PercentComplete%>%;">
                        <span class="sr-only"><%=CurrentRegistrationInformation.PercentComplete%>% Complete</span>
                    </div>
                </div>
            </asp:Panel>

            <Rock:AttributeValuesContainer ID="avcRegistrationAttributesStart" runat="server" />
            <div class="actions">
                <asp:LinkButton ID="btnRegistrationAttributesStartPrev" runat="server" AccessKey="p" ToolTip="Alt+p" Text="Previous" CssClass="btn btn-default" CausesValidation="false" OnClick="btnRegistrationAttributesStartPrev_Click" />
                <Rock:BootstrapButton ID="btnRegistrationAttributesStartNext" runat="server" AccessKey="n" ToolTip="Alt+n" Text="Next" DataLoadingText="Next" CssClass="btn btn-primary pull-right" CausesValidation="true" OnClick="btnRegistrationAttributesStartNext_Click" />
            </div>
        </asp:Panel>

        <%-- Prompt for information on each Registration --%>
        <asp:Panel ID="pnlRegistrant" runat="server" Visible="false" CssClass="registrationentry-registrant">


            <h1 style="text-align: center;">
                <asp:Literal ID="lRegistrantTitle" runat="server" />
            </h1>
            <div class="row">
                <div class="col-md-3">
                    <asp:Literal ID="lRegistrantSidebar" runat="server" />
                </div>
                <div class="col-md-9">
                    <Rock:NotificationBox ID="nbType" runat="server" NotificationBoxType="Warning" />

                    <asp:Panel ID="pnlRegistrantFields" runat="server">

                        <asp:Panel ID="pnlFamilyOptions" runat="server" CssClass="well js-registration-same-family">
                            <Rock:RockRadioButtonList ID="rblFamilyOptions" runat="server" Label="Individual is in the same immediate family as" RepeatDirection="Vertical" Required="true" RequiredErrorMessage="Answer to which family is required." DataTextField="Value" DataValueField="Key" />
                        </asp:Panel>

                        <asp:PlaceHolder ID="phRegistrantControls" runat="server" />

                        <div id="divFees" runat="server" class="well registration-additional-options">
                            <h4>
                                <asp:Literal ID="lRegistrantFeeCaption" runat="server" /></h4>
                            <asp:PlaceHolder ID="phFees" runat="server" />
                        </div>
                </div>
        </asp:Panel>

        <asp:Panel ID="pnlDigitalSignature" runat="server" Visible="false">
            <Rock:NotificationBox ID="nbDigitalSignature" runat="server" NotificationBoxType="Info"></Rock:NotificationBox>
            <asp:HiddenField ID="hfRequiredDocumentLinkUrl" runat="server" />
            <asp:HiddenField ID="hfRequiredDocumentQueryString" runat="server" />

            <iframe id="iframeRequiredDocument" frameborder="0"></iframe>
            <span style="display: none">
                <asp:LinkButton ID="lbRequiredDocumentNext" runat="server" Text="Required Document Return" OnClick="lbRequiredDocumentNext_Click" CausesValidation="false"></asp:LinkButton>
            </span>

        </asp:Panel>

        <div class="actions">
            <asp:LinkButton ID="lbRegistrantPrev" runat="server" AccessKey="p" ToolTip="Alt+p" Text="Previous" CssClass="btn btn-default" CausesValidation="false" OnClick="lbRegistrantPrev_Click" />
            <Rock:BootstrapButton ID="lbRegistrantNext" runat="server" AccessKey="n" ToolTip="Alt+n" Text="Next" DataLoadingText="Next" CssClass="btn btn-primary pull-right" CausesValidation="true" OnClick="lbRegistrantNext_Click" />
        </div>
        </div>
        </asp:Panel>

        <%-- Prompt for any Registration Attributes that should be prompted for after entering registrations--%>
        <asp:Panel ID="pnlRegistrationAttributesEnd" runat="server" Visible="false" CssClass="registrationentry-registration-attributes">

            <h1>
                <asp:Literal ID="lRegistrationAttributesEndTitle" runat="server" /></h1>

            <asp:Panel ID="pnlRegistrationAttributesEndProgressBar" runat="server">
                <div class="progress">
                    <div class="progress-bar" role="progressbar" aria-valuenow="<%=CurrentRegistrationInformation.PercentComplete%>" aria-valuemin="0" aria-valuemax="100" style="width: <%=CurrentRegistrationInformation.PercentComplete%>%;">
                        <span class="sr-only"><%=CurrentRegistrationInformation.PercentComplete%>% Complete</span>
                    </div>
                </div>
            </asp:Panel>

            <Rock:AttributeValuesContainer ID="avcRegistrationAttributesEnd" runat="server" />
            <div class="actions">
                <asp:LinkButton ID="btnRegistrationAttributesEndPrev" runat="server" AccessKey="p" ToolTip="Alt+p" Text="Previous" CssClass="btn btn-default" CausesValidation="false" OnClick="btnRegistrationAttributesEndPrev_Click" />
                <Rock:BootstrapButton ID="btnRegistrationAttributesEndNext" runat="server" AccessKey="n" ToolTip="Alt+n" Text="Next" DataLoadingText="Next" CssClass="btn btn-primary pull-right" CausesValidation="true" OnClick="btnRegistrationAttributesEndNext_Click" />
            </div>
        </asp:Panel>

        <%-- Summary and Payment --%>
        <asp:Panel ID="pnlSummaryAndPayment" runat="server" Visible="false" CssClass="registrationentry-summary">

            <h1>
                <asp:Literal ID="lSummaryAndPaymentTitle" runat="server" /></h1>


            <asp:Panel ID="pnlRegistrarInfoPrompt" runat="server" CssClass="well">

                <h4>This
                    <asp:Literal ID="lRegistrationTermPrompt" runat="server" />
                    Was Completed By</h4>
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbYourFirstName" runat="server" Label="First Name" CssClass="js-your-first-name" Required="true" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbYourLastName" runat="server" Label="Last Name" Required="true" />
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-6">
                        <Rock:EmailBox ID="tbConfirmationEmail" runat="server" Label="Send Confirmation Emails To" Required="true" />
                        <Rock:RockCheckBox ID="cbUpdateEmail" runat="server" Text="Should Your Account Be Updated To Use This Email Address?" Visible="false" Checked="true" />
                        <asp:Literal ID="lUpdateEmailWarning" runat="server" Text="Note: Your account will automatically be updated with this email address." Visible="false" />
                    </div>
                    <div class="col-md-6">
                        <asp:Panel ID="pnlRegistrarFamilyOptions" runat="server" CssClass="js-registration-same-family">
                            <Rock:RockRadioButtonList ID="rblRegistrarFamilyOptions" runat="server" Label="You are in the same immediate family as" RepeatDirection="Horizontal" Required="true" DataTextField="Value" DataValueField="Key" RequiredErrorMessage="Answer to which family is required." />
                        </asp:Panel>
                    </div>
                </div>

            </asp:Panel>

            <asp:Panel ID="pnlRegistrarInfoUseLoggedInPerson" runat="server" CssClass="well" Visible="false">
                <h4>This
                    <asp:Literal ID="lRegistrationTermLoggedInPerson" runat="server" />
                    Was Completed By</h4>
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockLiteral ID="lUseLoggedInPersonFirstName" runat="server" Label="First Name" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockLiteral ID="lUseLoggedInPersonLastName" runat="server" Label="Last Name" />
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockLiteral ID="lUseLoggedInPersonEmail" runat="server" Label="Email" />
                        <Rock:EmailBox ID="tbUseLoggedInPersonEmail" runat="server" Label="Email" Required="true" />
                    </div>
                    <div class="col-md-6">
                    </div>
                </div>
            </asp:Panel>

            <asp:Panel ID="pnlRegistrantsReview" CssClass="margin-b-md" runat="server" Visible="false">
                <asp:Repeater ID="rptrRegistrantEvents" runat="server" OnItemDataBound="rptrRegistrantEvents_ItemDataBound">
                    <ItemTemplate>
                        <asp:Literal ID="lRegistrantsReview" runat="server" />
                        <ul>
                            <asp:Repeater ID="rptrRegistrantsReview" runat="server">
                                <ItemTemplate>
                                    <li><strong><%# Eval("RegistrantName")  %></strong></li>
                                </ItemTemplate>
                            </asp:Repeater>
                        </ul>
                    </ItemTemplate>
                </asp:Repeater>

            </asp:Panel>
            <asp:Panel ID="pnlWaitingListReview" CssClass="margin-b-md" runat="server" Visible="false">
                <asp:Repeater ID="rptrWaitingListEvents" runat="server" OnItemDataBound="rptrWaitingListEvents_ItemDataBound">
                    <ItemTemplate>
                        <asp:Literal ID="lWaitingListReview" runat="server" />
                        <ul>
                            <asp:Repeater ID="rptrWaitingListReview" runat="server">
                                <ItemTemplate>
                                    <li><strong><%# Eval("RegistrantName")  %></strong></li>
                                </ItemTemplate>
                            </asp:Repeater>
                        </ul>
                    </ItemTemplate>
                </asp:Repeater>
            </asp:Panel>

            <asp:Panel ID="pnlCostAndFees" runat="server">

                <h4>Payment Summary</h4>

                <Rock:NotificationBox ID="nbDiscountCode" runat="server" Visible="false" NotificationBoxType="Warning"></Rock:NotificationBox>

                <div class="clearfix">
                    <div id="divDiscountCode" runat="server" class="form-group pull-right">
                        <label class="control-label">
                            <asp:Literal ID="lDiscountCodeLabel" runat="server" /></label>
                        <div class="input-group">
                            <Rock:RockTextBox ID="tbDiscountCode" runat="server" CssClass="form-control input-width-md input-sm"></Rock:RockTextBox>
                            <asp:LinkButton ID="lbDiscountApply" runat="server" CssClass="btn btn-default btn-sm margin-l-sm" Text="Apply" OnClick="lbDiscountApply_Click" CausesValidation="false"></asp:LinkButton>
                        </div>
                    </div>
                </div>

                <asp:Repeater ID="rptRegistrationFees" runat="server" OnItemDataBound="rptRegistrationFees_ItemDataBound">
                    <ItemTemplate>
                        <asp:HiddenField ID="hfRegistrationInstanceId" runat="server" />
                        <h3>
                            <asp:Literal ID="lRegistrationInstanceName" runat="server" /></h3>
                        <div class="fee-table">
                            <asp:Repeater ID="rptFeeSummary" runat="server">
                                <HeaderTemplate>
                                    <div class="row hidden-xs fee-header">
                                        <div class="col-sm-6">
                                            <strong>Description</strong>
                                        </div>

                                        <div runat="server" class="col-sm-3 fee-value" visible='<%# (IsDiscountColumnShown) %>'>
                                            <strong>Discounted Amount</strong>
                                        </div>

                                        <div class="col-sm-3 fee-value">
                                            <strong>Amount</strong>
                                        </div>

                                    </div>
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <div class="row fee-row-<%# Eval("Type").ToString().ToLower() %>">
                                        <div class="col-sm-6 fee-caption">
                                            <%# Eval("Description") %>
                                        </div>

                                        <div runat="server" class="col-sm-3 fee-value" visible='<%# (IsDiscountColumnShown) %>'>
                                            <span class="visible-xs-inline">Discounted Amount:</span> <%# Rock.Web.Cache.GlobalAttributesCache.Value( "CurrencySymbol" )%> <%# string.Format("{0:N}", Eval("DiscountedCost")) %>
                                        </div>

                                        <div class="col-sm-3 fee-value">
                                            <span class="visible-xs-inline">Amount:</span> <%# Rock.Web.Cache.GlobalAttributesCache.Value( "CurrencySymbol" )%> <%# string.Format("{0:N}", Eval("Cost")) %>
                                        </div>

                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>

                <div class="row fee-totals">
                    <div class="col-sm-offset-8 col-sm-4 fee-totals-options">
                        <asp:HiddenField ID="hfTotalCost" runat="server" />
                        <Rock:RockLiteral ID="lTotalCost" runat="server" Label="Total Cost" />

                        <%-- For Partial Payments... --%>

                        <asp:HiddenField ID="hfMinimumDue" runat="server" />
                        <Rock:RockLiteral ID="lMinimumDue" runat="server" Label="Minimum Due Today" />

                        <div class="form-right">
                            <Rock:CurrencyBox ID="nbAmountPaid" runat="server" CssClass="input-width-md amount-to-pay" NumberType="Currency" Label="Amount To Pay Today" Required="true" />
                        </div>

                        <Rock:RockLiteral ID="lRemainingDue" runat="server" Label="Amount Remaining" />


                        <%-- For Payoff --%>
                        <Rock:RockLiteral ID="lAmountDue" runat="server" Label="Amount Due" Visible="false" />
                    </div>
                </div>


            </asp:Panel>

            <asp:Panel ID="pnlPaymentInfo" runat="server" CssClass="well">

                <asp:Literal ID="lPaymentInfoTitle" runat="server" />

                <Rock:RockRadioButtonList ID="rblSavedCC" runat="server" CssClass="radio-list margin-b-lg" RepeatDirection="Vertical" DataValueField="Id" DataTextField="Name" AutoPostBack="true" />

                <div id="divNewCard" runat="server" class="radio-content">
                    <Rock:RockTextBox ID="txtCardFirstName" runat="server" Label="First Name on Card" Visible="false"></Rock:RockTextBox>
                    <Rock:RockTextBox ID="txtCardLastName" runat="server" Label="Last Name on Card" Visible="false"></Rock:RockTextBox>
                    <Rock:RockTextBox ID="txtCardName" runat="server" Label="Name on Card" Visible="false"></Rock:RockTextBox>
                    <Rock:RockTextBox ID="txtCreditCard" runat="server" Label="Card Number" MaxLength="19" CssClass="credit-card" />
                    <ul class="card-logos list-unstyled">
                        <li class="card-visa"></li>
                        <li class="card-mastercard"></li>
                        <li class="card-amex"></li>
                        <li class="card-discover"></li>
                    </ul>
                    <div class="row">
                        <div class="col-sm-6">
                            <Rock:MonthYearPicker ID="mypExpiration" runat="server" Label="Expiration Date" />
                        </div>
                        <div class="col-sm-6">
                            <Rock:RockTextBox ID="txtCVV" Label="Card Security Code" CssClass="input-width-xs" runat="server" MaxLength="4" />
                        </div>
                    </div>
                    <Rock:AddressControl ID="acBillingAddress" runat="server" Label="Billing Address" UseStateAbbreviation="true" UseCountryAbbreviation="false" ShowAddressLine2="false" />
                </div>

            </asp:Panel>

            <div class="actions">
                <asp:LinkButton ID="lbSummaryPrev" runat="server" AccessKey="p" ToolTip="Alt+p" Text="Previous" CssClass="btn btn-default" CausesValidation="false" Visible="false" OnClick="lbSummaryPrev_Click" />
                <Rock:BootstrapButton ID="lbSummaryNext" runat="server" AccessKey="n" ToolTip="Alt+n" Text="Pay Minimum" DataLoadingText="Next" CssClass="btn btn-primary pull-right" CausesValidation="true" OnClick="lbSummaryNext_Click" />
                <Rock:BootstrapButton ID="lbPayNowNext" runat="server" Style="margin: 15px !important;" AccessKey="d" ToolTip="Alt+d" Text="Pay Total" DataLoadingText="Next" CssClass="btn btn-primary pull-right" CausesValidation="true" OnClick="lbPayNowNext_Click" />
                <asp:LinkButton ID="lbPaymentPrev" runat="server" AccessKey="p" ToolTip="Alt+p" Text="Previous" CssClass="btn btn-default" Visible="false" CausesValidation="false" OnClick="lbPaymentPrev_Click" />
                <asp:Label ID="aStep2Submit" runat="server" ClientIDMode="Static" CssClass="btn btn-primary pull-right" Text="Finish" />
            </div>

            <iframe id="iframeStep2" src="<%=Step2IFrameUrl%>" style="display: none"></iframe>

            <asp:HiddenField ID="hfStep2AutoSubmit" runat="server" Value="false" />
            <asp:HiddenField ID="hfIsPaidInFull" runat="server" Value="false" />
            <asp:HiddenField ID="hfStep2Url" runat="server" />
            <asp:HiddenField ID="hfStep2ReturnQueryString" runat="server" />
            <span style="display: none">
                <asp:LinkButton ID="lbStep2Return" runat="server" Text="Step 2 Return" OnClick="lbStep2Return_Click" CausesValidation="false"></asp:LinkButton>
            </span>

        </asp:Panel>

        <asp:Panel ID="pnlSuccess" runat="server" Visible="false">

            <h1>
                <asp:Literal ID="lSuccessTitle" runat="server" /></h1>

            <asp:Panel ID="pnlSuccessProgressBar" runat="server">
                <div class="progress">
                    <div class="progress-bar" role="progressbar" aria-valuenow="<%=CurrentRegistrationInformation.PercentComplete%>" aria-valuemin="0" aria-valuemax="100" style="width: <%=CurrentRegistrationInformation.PercentComplete%>%;">
                        <span class="sr-only"><%=CurrentRegistrationInformation.PercentComplete%>% Complete</span>
                    </div>
                </div>
            </asp:Panel>

            <asp:Literal ID="lSuccess" runat="server" />
            <asp:Literal ID="lSuccessDebug" runat="server" Visible="false" />

            <asp:Panel ID="pnlSaveAccount" runat="server" Visible="false">
                <div class="well">
                    <legend>Make Payments Even Easier</legend>
                    <fieldset>
                        <Rock:RockCheckBox ID="cbSaveAccount" runat="server" Text="Save account information for future transactions" CssClass="toggle-input" />
                        <div id="divSaveAccount" runat="server" class="toggle-content">
                            <Rock:RockTextBox ID="txtSaveAccount" runat="server" Label="Name for this account" CssClass="input-large"></Rock:RockTextBox>

                            <asp:PlaceHolder ID="phCreateLogin" runat="server" Visible="false">

                                <div class="control-group">
                                    <div class="controls">
                                        <div class="alert alert-info">
                                            <b>Note:</b> For security purposes you will need to login to use your saved account information.  To create
	    			                a login account please provide a user name and password below. You will be sent an email with the account 
	    			                information above as a reminder.
                                        </div>
                                    </div>
                                </div>

                                <Rock:RockTextBox ID="txtUserName" runat="server" Label="Username" CssClass="input-medium" />
                                <Rock:RockTextBox ID="txtPassword" runat="server" Label="Password" CssClass="input-medium" TextMode="Password" />
                                <Rock:RockTextBox ID="txtPasswordConfirm" runat="server" Label="Confirm Password" CssClass="input-medium" TextMode="Password" />

                            </asp:PlaceHolder>

                            <Rock:NotificationBox ID="nbSaveAccount" runat="server" Visible="false" NotificationBoxType="Danger"></Rock:NotificationBox>

                            <div id="divSaveActions" runat="server" class="actions">
                                <asp:LinkButton ID="lbSaveAccount" runat="server" Text="Save Account" CssClass="btn btn-primary" OnClick="lbSaveAccount_Click" />
                            </div>
                        </div>
                    </fieldset>
                </div>
            </asp:Panel>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
