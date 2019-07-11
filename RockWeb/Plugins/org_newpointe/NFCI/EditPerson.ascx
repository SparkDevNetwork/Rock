<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EditPerson.ascx.cs" Inherits="RockWeb.Plugins.org_newpointe.NFCI.EditPerson" %>

<asp:UpdatePanel ID="upAddGroup" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pPersonError" runat="server" Visible="false" CssClass="alert alert-danger">
            <strong>
                <asp:Literal runat="server" ID="rlErrorTitle" /></strong>
            <asp:Literal runat="server" ID="rlErrorMessage" />
        </asp:Panel>

        <asp:Panel ID="pPersonEdit" runat="server" Visible="false" CssClass="panel panel-default">
            <div class="panel-body">

                    <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                    <h1>Personal Info</h1>

                    <!-- NickName -->
                    <Rock:DataTextBox ID="tbNickName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="NickName" />

                    <br />
                
                    <!-- Title -->
                    <Rock:DefinedValuePicker ID="dvpTitle" runat="server" Label="Title" />

                    <!-- First Name -->
                    <Rock:DataTextBox ID="tbFirstName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="FirstName" Required="true" />

                    <!-- Middle Name -->
                    <Rock:DataTextBox ID="tbMiddleName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="MiddleName" />

                    <!-- Last Name -->
                    <Rock:DataTextBox ID="tbLastName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="LastName" Required="true" />

                    <!-- Suffix -->
                    <Rock:DefinedValuePicker ID="dvpSuffix" runat="server" Label="Suffix" />

                    <br />
                
                    <!-- Gender -->
                    <Rock:RockRadioButtonList ID="rblGender" runat="server" Label="Gender" RepeatDirection="Horizontal" />

                    <!-- Birthday -->
                    <Rock:BirthdayPicker ID="bpBirthday" runat="server" Label="Birthday" />

                    <!-- Grade -->
                    <Rock:GradePicker ID="gpGrade" runat="server" UseGradeOffsetAsValue="true" />

                    <!-- Allergies -->
                    <Rock:RockTextBox ID="rtbAllergy" runat="server" Label="Allergies (Leave blank if none)" />

                    <br />

                    <h1>Contact Info</h1>

                    <h3>Phone</h3>

                    <asp:Repeater ID="rContactInfo" runat="server">
                        <ItemTemplate>
                            <div class="form-group phonegroup">
                                <div class="control-label col-sm-2 phonegroup-label text-right"><%# Rock.Web.Cache.DefinedValueCache.Get( (int)Eval("NumberTypeValueId")).Value  %></div>
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
                <div class="panel-footer actions clearfix">
                    <asp:LinkButton runat="server" ID="lbCancel" CssClass="btn btn-lg btn-default pull-left" Text="Cancel" OnClientClick="history.back();" CausesValidation="false"></asp:LinkButton>
                    <asp:LinkButton runat="server" ID="lbSubmit" CssClass="btn btn-lg btn-success pull-right" Text="Save" OnClick="lbSubmit_Click"></asp:LinkButton>
                </div>
            </div>
        </asp:Panel>

        <div class="panel panel-block">
            <div class="panel-body text-center">
                <asp:LinkButton runat="server" ID="lbRequestChange" CssClass="btn btn-lg btn-warning" Text="Request Additional Change" CausesValidation="false" OnClick="lbRequestChange_Click"></asp:LinkButton>
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
