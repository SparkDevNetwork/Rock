<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EditPerson.ascx.cs" Inherits="RockWeb.Plugins.org_newpointe.NFCI.EditPerson" %>

<asp:UpdatePanel ID="upAddGroup" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-pencil"></i>Edit Person
                </h1>
            </div>
            <asp:Panel ID="pPersonError" runat="server" Visible="false" CssClass="panel-body">
                <div class="alert alert-danger"><strong>Error</strong> There is no group selected or you do not have permission to view or edit this group.</div>
            </asp:Panel>
            <asp:Panel ID="pPersonInfo" runat="server" Visible="false">
                <div class="panel-body">

                    <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                    <h1>Personal Info</h1>

                    <Rock:DataTextBox ID="tbNickName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="NickName" />

                    <br />

                    <div class="row">
                        <div class="col-md-3">
                            <Rock:DefinedValuePicker ID="dvpTitle" runat="server" Label="Title" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-5">
                            <Rock:DataTextBox ID="tbFirstName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="FirstName" />
                        </div>
                        <div class="col-md-2">
                            <Rock:DataTextBox ID="tbMiddleName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="MiddleName" />
                        </div>
                        <div class="col-md-5">
                            <Rock:DataTextBox ID="tbLastName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="LastName" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-3">
                            <Rock:DefinedValuePicker ID="dvpSuffix" runat="server" Label="Suffix" />
                        </div>
                    </div>

                    <br />

                    <Rock:RockRadioButtonList ID="rblGender" runat="server" Label="Gender" RepeatDirection="Horizontal" />

                    <Rock:BirthdayPicker ID="bpBirthday" runat="server" Label="Birthday" />

                    <Rock:GradePicker ID="gpGrade" runat="server" UseGradeOffsetAsValue="true" />

                    <Rock:RockTextBox ID="rtbAllergy" runat="server" Label="Allergy" />

                    <br />
                    <h1>Contact Info</h1>
                    <h3>Email</h3>

                    <Rock:EmailBox ID="ebEmail" runat="server" />

                    <h3>Phone</h3>

                    <div class="row">
                        <asp:Repeater ID="rContactInfo" runat="server">
                            <ItemTemplate>
                                <div class="form-group phonegroup">
                                    <div class="control-label col-sm-2 phonegroup-label text-right"><%# Rock.Web.Cache.DefinedValueCache.Read( (int)Eval("NumberTypeValueId")).Value  %></div>
                                    <div class="controls col-sm-10 phonegroup-number">
                                        <div class="row">
                                            <div class="col-sm-9">
                                                <asp:HiddenField ID="hfPhoneType" runat="server" Value='<%# Eval("NumberTypeValueId")  %>' />
                                                <Rock:PhoneNumberBox ID="pnbPhone" runat="server" CountryCode='<%# Eval("CountryCode") %>' Number='<%# Eval("NumberFormatted")  %>' />
                                            </div>
                                            <div class="col-sm-3">
                                                <div class="row">
                                                    <div class="col-xs-12">
                                                        <asp:CheckBox ID="cbUnlisted" runat="server" Text="Unlisted" Checked='<%# (bool)Eval("IsUnlisted") %>' />
                                                    </div>
                                                </div>
                                            </div>

                                        </div>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>


                </div>
                <div class="panel-footer actions clearfix">
                    <asp:LinkButton runat="server" ID="lbCancel" CssClass="btn btn-lg btn-default pull-left" Text="Cancel" OnClientClick="history.back();" CausesValidation="false"></asp:LinkButton>
                    <asp:LinkButton runat="server" ID="lbSubmit" CssClass="btn btn-lg btn-success pull-right" Text="Save" OnClick="lbSubmit_Click"></asp:LinkButton>
                </div>
            </asp:Panel>
        </div>
        <div class="panel panel-block">
            <div class="panel-body text-center">
                <asp:LinkButton runat="server" ID="lbRequestChange" CssClass="btn btn-lg btn-warning" Text="Request Additional Change" CausesValidation="false" OnClick="lbRequestChange_Click"></asp:LinkButton>
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
