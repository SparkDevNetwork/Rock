<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AnnualSurvey.ascx.cs" Inherits="RockWeb.Plugins.org_lakepointe.Crm.AnnualSurvey" %>
<%@ Register TagPrefix="LPC" TagName="PersonInformation" Src="~/Plugins/org_lakepointe/Crm/AnnualSurveyPersonInformation.ascx" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbError" runat="server" NotificationBoxType="Danger" />
        <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
        <asp:HiddenField ID="hfRespondant" runat="server" />

        <a href="#" id="hlTopOfForm" style="color:transparent;font-size:1px;line-height:1px;">top</a>

        <%-- Welcome Panel --%>
        <asp:Panel ID="pnlWelcome" runat="server" Visible="false">
            <div class="row">
                <div class="col-xs-12">
                    <asp:Literal ID="lWelcome" runat="server" />
                    <asp:Literal ID="lWelcomeDebug" Visible="false" runat="server" />
                </div>
            </div>
        </asp:Panel>

        <%--Family Panel--%>
        <asp:Panel ID="pnlFamily" runat="server" Visible="false">
            <div class="row">
                <div class="col-xs-12">
                    <asp:Literal ID="lFamilyIntro" runat="server" />
                    <asp:Literal ID="lFamilyDebug" runat="server" />
                </div>
            </div>
            <div class="row">
                <div class="col-xs-12" style="margin: 0 auto;">
                    <div class="panel panel-default">
                        <div class="panel-heading">
                            <h3 class="panel-title">
                                <asp:Literal ID="lFamilyTitle" runat="server" /></h3>
                        </div>
                        <div class="panel-body">
                            <div class="row">
                                <div class="col-xs-12">
                                    <Rock:CampusPicker ID="cpFamilyCampus" runat="server" CssClass="input-width-lg" Label="Campus our family attends" Required="true" RequiredErrorMessage="Please select a Campus" ValidationGroup="FamilyInfo"  />
                                    <Rock:AddressControl ID="apHomeAddress" runat="server" Label="Home Address" Required="true" RequiredErrorMessage="Please enter your home address" ValidationGroup="FamilyInfo" Country="US" Style="margin-bottom:0px;" />
                                    <div class="row">
                                        <div class="col-md-6" >
                                            <Rock:RockCheckBox ID="cbIsMailingAddress" Text="This is my mailing address" runat="server" />
                                        </div>
                                        <div class="col-md-6" >
                                            <Rock:RockCheckBox ID="cbIsPhysicalAddress" Text="This is my physical address" runat="server" />
                                        </div>
                                    </div>
                                    <Rock:RockCheckBox ID="cbNoLongerAttend" runat="server" Label="Check if you no longer attend Lake Pointe" />
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </asp:Panel>

        <%-- Family Members Panel --%>
        <div id="pnlFamilyMembers" runat="server" visible="false" >
            <div class="row">

                <div class="col-xs-12" style="padding-left:0; padding-right:0;">
                    <asp:Literal ID="lFamilyMember" runat="server" />
                    <asp:Literal ID="lFamilyMemberDebug" runat="server" />
                </div>
                <div class="col-xs-12" style="padding-left:0; padding-right: 0; padding-bottom:10px">
                    <div class="btn-group pull-right">
                        <button type="button" class="btn btn-sm btn-primary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                            Add Family Member <span class="caret"></span>
                        </button>
                        <ul class="dropdown-menu">
                            <li id="liAddAdult" runat="server" ><asp:LinkButton ID="btnAddAdult" Text="Adult" runat="server" OnClick="btnAddFamilyMember_Click" CausesValidation="false" CommandArgument="Adult" /></li>
                            <li><asp:LinkButton ID="btnAddChild" Text="Child" runat="server" CommandArgument="Child" CausesValidation="false" OnClick="btnAddFamilyMember_Click" /></li>
                        </ul>
                    </div>
                </div>
            </div>
            <div class="row">
                <div class="col-xs-12">
                    <Rock:DynamicPlaceholder ID="phFamilyMembers" runat="server" />
                </div>
            </div>
        </div>

        <%--Ask More Questions Panels--%> 
        <asp:Panel ID="pnlAskMoreQuestions" runat="server" Visible="false">
            <div class="row">
                <div class="col-md-6" style="margin: 0 auto;">
                    <Rock:RockRadioButtonList ID="rblAskMoreQuestions" runat="server" Label="Would you be willing to answer a few more questions?" Required="true" RepeatDirection="Horizontal" RequiredErrorMessage="Please select Yes or No.">
                        <asp:ListItem Value="Yes" Text="Yes" Selected="True" />
                        <asp:ListItem Value="No" Text="No" />
                    </Rock:RockRadioButtonList>
                </div>

            </div>
        </asp:Panel>

        <%-- Survey Questions Panel  --%>
        <asp:Panel ID="pnlSurveyQuestions" runat="server" Visible="false">
            <div class="row">
                <div class="col-md-12">
                    <div class="panel panel-default">
                        <div class="panel-heading">
                            <h3 class="panel-title">
                                <asp:Literal ID="lSurveyQuestionsTitle" runat="server" />
                            </h3>
                        </div>
                        <div class="panel-body">
                            <div class="row">
                                <div class="col-xs-12">
                                    <asp:Literal ID="lSurveyQuestionsIntro" runat="server" />
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-xs-12">
                                   <Rock:DynamicPlaceholder ID="phSurveyQuestions" runat="server" />
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </asp:Panel>

        <%--No longer attends panel--%>
        <asp:Panel ID="pnlNoLongerAttends" runat="server" Visible="false">
            <div class="row">
                <div class="col-xs-12">
                    <asp:Literal ID="lNoLongerAttends" runat="server" />
                    <asp:Literal ID="lNoLongerAttendsDebug" runat="server" Visible="false" />
                </div>
            </div>
        </asp:Panel>

        <%--Survey Complete Panel--%>
        <asp:Panel ID="pnlSurveyComplete" runat="server" Visible="false">
            <div class="row">
                <div class="col-xs-12">
                    <asp:Literal ID="lSurveyComplete" runat="server" />
                    <asp:Literal ID="lSurveyCompleteDebug" runat="server" Visible="false" />
                </div>
            </div>
        </asp:Panel>


        <%--Button Panel - Keep at the bottom of the page--%>
        <asp:Panel ID="pnlButtons" runat="server" Visible="true" CssClass="actions">
            <div class="row">
                <div class="col-xs-12">
                    <span class="pull-left">
                        <Rock:BootstrapButton ID="btnPrevious" runat="server" Visible="false" CssClass="btn btn-action" Text="Back" OnClick="btnPrevious_Click" CausesValidation="false" />
                    </span>
                    <span class="pull-right">
                        <Rock:BootstrapButton ID="btnNext" runat="server" Visible="true" CssClass="btn btn-primary" Text="Next" DataLoadingText="Next" OnClick="btnNext_Click" CausesValidation="false" />
                    </span>

                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
