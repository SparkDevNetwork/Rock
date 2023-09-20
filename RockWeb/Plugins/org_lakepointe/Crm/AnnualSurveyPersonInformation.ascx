<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AnnualSurveyPersonInformation.ascx.cs" Inherits="RockWeb.Plugins.org_lakepointe.Crm.AnnualSurveyPersonInformation" %>
<style class="text/css">
    .btn.btn-sm {
        padding: 5px 10px;
        font-size: 14px;
        line-height: 1;
        border-radius: 0;
    }
</style>

<asp:HiddenField ID="hfPersonId" runat="server" />
<div class="row">
    <div class="panel panel-default">
        <div class="panel-heading">
            <span class="label label-campus pull-right"><asp:Literal ID="lFamilyRole" runat="server" /></span>
            <h3 class="panel-title">
                <asp:Literal ID="lName" runat="server" />
                <small><asp:LinkButton ID="btnRemove" runat="server" Visible="false" Text="(Remove)" OnClick="btnRemove_Click" CausesValidation="false" CssClass="btn btn-sm "/></small>
            </h3>
            
        </div>
        <div class="panel-body">
            <div class="row">
                <div class="col-xs-12">
                    <div class="row">
                        <asp:Panel ID="pnlPhoto" runat="server" Visible="true" CssClass="col-md-3">
                            <Rock:ImageEditor ID="imgPhoto" runat="server" Label="Photo" BinaryFileTypeGuid="03BD8476-8A9F-4078-B628-5B538F967AFC" />
                        </asp:Panel>
                        <div class="col-md-3">
                            <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" Required="true" RequiredErrorMessage="First Name is required." />
                        </div>
                        <div class="col-md-3">
                            <Rock:RockTextBox ID="tbNickName" runat="server" Label="Preferred Name" Required="false" />
                        </div>
                        <div class="col-md-3">
                            <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" Required="true" RequiredErrorMessage="Last Name is required." />
                        </div>

                    </div>

                    <div class="row">
                        <asp:Panel ID="pnlGroupRole" runat="server" Visible="true" CssClass="col-md-3">
                            <Rock:RockDropDownList ID="ddlGroupRole" runat="server" Label="Family Role" Required="true" Visible="false" RequiredErrorMessage="Family Role is Required." />
                        </asp:Panel>
                        <div class="col-md-3">
                            <Rock:RockRadioButtonList ID="rblGender" runat="server" Label="Gender" Required="true" RepeatDirection="Horizontal" RequiredErrorMessage="Gender is required." />
                        </div>
                        <asp:Panel ID="pnlMaritalStatus" runat="server" Visible="true" CssClass="col-md-3">
                            <Rock:DefinedValuePicker ID="dvpMaritalStatus" runat="server" Label="Marital Status" Required="true" RequiredErrorMessage="Marital Status is required" />
                        </asp:Panel>
                        <asp:Panel ID="pnlBirthDate" runat="server" CssClass="col-md-3">
                            <Rock:DatePicker ID="dpBirthDate" runat="server" Label="Birthdate" Required="true" RequiredErrorMessage="Birthdate is required." />
                        </asp:Panel>
                    </div>
                    <div class="row">
                        <asp:Panel ID="pnlGrade" runat="server" CssClass="col-md-3">
                            <Rock:GradePicker ID="ddlGradePicker" runat="server" Label="Grade" UseAbbreviation="true" UseGradeOffsetAsValue="true" />
                        </asp:Panel>
                        <asp:Panel ID="pnlActivelyAttending" runat="server" CssClass="col-md-3">
                            <Rock:RockDropDownList ID="ddlActive" runat="server" Label="Actively Attending Lake Pointe" Required="true" RequiredErrorMessage="Actively Attending is required.">
                                <asp:ListItem Value="0" Text="No" />
                                <asp:ListItem Value="1" Text="Yes" Selected="True" />
                            </Rock:RockDropDownList>
                        </asp:Panel>
                        <asp:Panel ID="pnlMemberOfFamily" runat="server" CssClass="col-md-3">
                            <Rock:RockDropDownList ID="ddlFamilyMember" runat="server" Label="Member of Family" Required="true" RequiredErrorMessage="Is Member of Immediate Family">
                                <asp:ListItem Value="0" Text="No" />
                                <asp:ListItem Value="1" Text="Yes" Selected="True" />
                            </Rock:RockDropDownList>
                        </asp:Panel>
                    </div>

                    <h4>Email</h4>
                    <asp:Panel ID="pnlEmail" runat="server" CssClass="row">

                        <div class="col-md-6">
                            <Rock:EmailBox ID="tbEmail" runat="server" Label="Email Address" Required="false" RequiredErrorMessage="Email is required." />
                        </div>
                        <asp:Panel ID="pnlEmailPreference" runat="server" CssClass="col-md-3">
                            <Rock:RockDropDownList ID="ddlEmailPreference" runat="server" Label="Email Preference" Required="false" RequiredErrorMessage="Email Preference is Required" />
                        </asp:Panel>
                    </asp:Panel>

                    <h4>Phone Numbers</h4>
                    <Rock:DynamicPlaceholder ID="phPhoneNumber" runat="server" />

                </div>
            </div>
        </div>
    </div>
</div>
