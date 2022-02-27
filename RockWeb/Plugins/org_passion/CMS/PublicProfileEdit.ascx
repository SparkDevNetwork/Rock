<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PublicProfileEdit.ascx.cs" Inherits="RockWeb.Blocks.Cms.PublicProfileEdit" %>

<asp:UpdatePanel ID="upContent" runat="server">
    <ContentTemplate>

        <script>
            $(function () {
                $(".photo a").fluidbox();
            });
        </script>

        <div style="padding: 15px;" class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-user"></i>&nbsp;My Account</h1>
            </div>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbNotAuthorized" runat="server" Text="You must be logged in to view your account." NotificationBoxType="Danger" Visible="false" />
                <asp:Panel ID="pnlView" runat="server">
                    
                        <div class="row">

                        <div class="col-sm-3">
                            <div class="photo">
                                <asp:Literal ID="lImage" runat="server" />
                            </div>
                        </div>

                        <div class="col-sm-9">
                            <h1 class="title name">
                                <asp:Literal ID="lName" runat="server" /><div class="pull-right">
                                    <Rock:RockDropDownList ID="ddlGroup" runat="server" DataTextField="Name" DataValueField="Id" OnSelectedIndexChanged="ddlGroup_SelectedIndexChanged" AutoPostBack="true" Visible="false" />
                                </div>
                            </h1>

                            <div class="row">
                                <div class="col-md-6">
                                    <ul class="person-demographics list-unstyled">
                                        <li>
                                            <asp:Literal ID="lAge" runat="server" /></li>
                                        <li>
                                            <asp:Literal ID="lGender" runat="server" /></li>
                                        <li>
                                            <asp:Literal ID="lMaritalStatus" runat="server" /></li>
                                        <li>
                                            <asp:Literal ID="lGrade" runat="server" /></li>
                                    </ul>
                                </div>
                                <div class="col-md-6 form-group">
                                    <div class="form-group">
                                        <ul class="phone-list list-unstyled">
                                            <asp:Repeater ID="rptPhones" runat="server">
                                                <ItemTemplate>
                                                    <li><%# (bool)Eval("IsUnlisted") ? "Unlisted" : FormatPhoneNumber( Eval("CountryCode"), Eval("Number") ) %> <small><%# Eval("NumberTypeValue.Value") %></small></li>
                                                </ItemTemplate>
                                            </asp:Repeater>
                                        </ul>
                                    </div>
                                    <div class="form-group">
                                        <a ID="lEmail" runat="server" />
                                    </div>

                                </div>
                            </div>
                            <asp:Literal ID="lAddress" runat="server" />
                            <div class="row">
                                <asp:Repeater ID="rptPersonAttributes" runat="server">
                                    <ItemTemplate>
                                        <div class="col-md-6 form-group">
                                            <b><%# Eval("Name") %></b></br><%# Eval("Value") %>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>
                            <div class="row">
                                <div class="col-md-12">
                                    <asp:Literal ID="lFamilyHeader" runat="server" Text="<h4>Family Information</h4>" Visible="false" />
                                </div>
                                <asp:Repeater ID="rptGroupAttributes" runat="server">
                                    <ItemTemplate>
                                        <div class="col-md-6 form-group">
                                            <b><%# Eval("Name") %></b></br><%# Eval("Value") %>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>
                            <br />
                            <div class="row">
                                <div class="col-xs-12 col-md-4">
                            <asp:LinkButton ID="lbEditPerson" runat="server" CssClass="btn btn-primary btn-block btn-sm" OnClick="lbEditPerson_Click" CausesValidation="false"> Update</asp:LinkButton>
                                </div>
                            </div>
                            
                        </div>
                    </div>
                    
                    
                    <hr />

                    <h2>
                        <asp:Literal ID="lGroupName" runat="server" />
                    </h2>
                    <asp:Repeater ID="rptGroupMembers" runat="server" OnItemDataBound="rptGroupMembers_ItemDataBound" OnItemCommand="rptGroupMembers_ItemCommand">
                        <ItemTemplate>
                            <div class="row">
                                <div style="margin-top: 20px;" class="col-md-3">
                                    <div class="photo">
                                        <asp:Literal ID="lGroupMemberImage" runat="server" />
                                    </div>
                                </div>
                                <div class="col-md-9">
                                    <h3>
                                                <asp:Literal ID="lGroupMemberName" runat="server" /></h3>
                                    <div class="row">
                                        <div class="col-md-6">
                                            
                                            <ul class="person-demographics list-unstyled">
                                                <li>
                                                    <asp:Literal ID="lAge" runat="server" /></li>
                                                <li>
                                                    <asp:Literal ID="lGender" runat="server" /></li>
                                                <li>
                                                    <asp:Literal ID="lMaritalStatus" runat="server" /></li>
                                                <li>
                                                    <asp:Literal ID="lGrade" runat="server" /></li>
                                            </ul>
                                        </div>
                                       
                                        <div class="col-md-6 form-group">
                                            <ul class="phone-list list-unstyled">
                                                <asp:Repeater ID="rptGroupMemberPhones" runat="server">
                                                    <ItemTemplate>
                                                        <li><%# (bool)Eval("IsUnlisted") ? "Unlisted" : FormatPhoneNumber( Eval("CountryCode"), Eval("Number") ) %> <small><%# Eval("NumberTypeValue.Value") %></small></li>
                                                    </ItemTemplate>
                                                </asp:Repeater>
                                            </ul>
                                            <asp:Literal ID="lGroupMemberEmail" runat="server" />
                                        </div>
                                    </div>
                                    <div class="row form-group">
                                        <asp:Repeater ID="rptGroupMemberAttributes" runat="server">
                                            <ItemTemplate>
                                                <div class="col-md-6 form-group">
                                                    <b><%# Eval("Name") %></b></br><%# Eval("Value") %>
                                                </div>
                                            </ItemTemplate>
                                        </asp:Repeater>
                                    </div>
                                    <div class="row">
                                        <div class="col-xs-12 col-md-4">
                                            <asp:LinkButton ID="lbEditGroupMember" runat="server" CssClass="btn btn-primary btn-block btn-sm" CommandArgument='<%# Eval("Person.Guid") %>' CommandName="Update"> Update</asp:LinkButton>
                                        </div>
                                        
                                    </div>
                                </div>
                            </div>
                            <br />
                        </ItemTemplate>
                    </asp:Repeater>
                    <hr />

                    <div class="row">

                        <div class="col-xs-12 col-md-6 col-md-offset-3 form-group">

                    
                    <asp:LinkButton ID="lbAddGroupMember" runat="server" CssClass="btn btn-success btn-block btn-sm" OnClick="lbAddGroupMember_Click"> + New Family Member</asp:LinkButton>

                    <asp:LinkButton ID="lbRequestChanges" runat="server" CssClass="hidden btn btn-primary btn-block btn-sm" OnClick="lbRequestChanges_Click"> Request Additional Changes</asp:LinkButton>

                        </div>

                    </div>
                    
                </asp:Panel>

                <asp:HiddenField ID="hfPersonGuid" runat="server" />

                <asp:Panel ID="pnlEdit" runat="server">
                    <asp:ValidationSummary ID="valValidation" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                    
                    <asp:Panel ID="pnlProfile" runat="server">
                        <div class="panel-heading clearfix">
                            <h1 class="pull-left">Profile Information</h1>
                        </div>
                        <div class="panel-body">


                            <div class="col-md-7">
                                <Rock:ImageEditor ID="imgPhoto" runat="server" BinaryFileTypeGuid="03BD8476-8A9F-4078-B628-5B538F967AFC" />
                            </div>



                            <div class="col-md-6">
                                <Rock:RockDropDownList ID="ddlTitle" runat="server" CssClass="hidden input-width-md" />
                                <Rock:DataTextBox ID="tbFirstName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="FirstName" Required="true" />
                                <Rock:DataTextBox ID="tbLastName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="LastName" Required="true" />
                                <Rock:DataTextBox ID="tbNickName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="NickName" Label="Goes by" />
                                <Rock:RockDropDownList ID="ddlSuffix" CssClass="hidden input-width-md" runat="server" />
                                <Rock:BirthdayPicker ID="bpBirthDay" runat="server" Label="Birthday" />
                                <Rock:RockRadioButtonList ID="rblRole" runat="server" DataTextField="Name" DataValueField="Id" RepeatDirection="Horizontal" Label="Role" Visible="false" AutoPostBack="true" OnSelectedIndexChanged="rblRole_SelectedIndexChanged" />
                                <div class="row">
                                    <div class="col-md-6">
                                        <Rock:RockRadioButtonList ID="rblGender" runat="server" RepeatDirection="Horizontal" Label="Gender" Required="true">
                                            <asp:ListItem Text="Male" Value="Male" />
                                            <asp:ListItem Text="Female" Value="Female" />
                                        </Rock:RockRadioButtonList>
                                    </div>
                                    <div class="col-md-6">
                                        <%-- This YearPicker is needed for the GradePicker to work --%>
                                        <div style="display: none;">
                                            <Rock:YearPicker ID="ypGraduation" runat="server" Label="Graduation Year" Help="High School Graduation Year." />
                                        </div>
                                        <Rock:GradePicker ID="ddlGradePicker" runat="server" Label="Grade" UseAbbreviation="true" UseGradeOffsetAsValue="true" Visible="false" />
                                    </div>
                                </div>
                            </div>
                        
                            </div>
                        </asp:Panel>

                    <hr />

                    <asp:Panel ID="pnlContact" runat="server">
                        <div class="panel-heading clearfix">
                            <h3 class="pull-left">Contact Info</h3>
                        </div>
                        <div class="panel-body">
                            <asp:Repeater ID="rContactInfo" runat="server">
                                <ItemTemplate>
                                    <div class="form-group">
                                        <div class="control-label col-md-12">
                                            <label><%# Eval("NumberTypeValue.Value")  %></label></div>
                                        <div class="controls col-md-10">
                                            <div class="row">
                                                <div class="col-md-7 form-group">
                                                    <asp:HiddenField ID="hfPhoneType" runat="server" Value='<%# Eval("NumberTypeValueId")  %>' />
                                                    <Rock:PhoneNumberBox ID="pnbPhone" runat="server" CountryCode='<%# Eval("CountryCode")  %>' Number='<%# Eval("NumberFormatted")  %>' />
                                                </div>
                                                <div class="col-md-5">
                                                    <div class="row">
                                                        <div class="col-md-6">
                                                            <asp:CheckBox ID="cbSms" runat="server" Text="SMS" Checked='<%# (bool)Eval("IsMessagingEnabled") %>' CssClass="js-sms-number" />
                                                        </div>
                                                        <div class="col-md-6">
                                                            <asp:CheckBox ID="cbUnlisted" runat="server" Text="Unlisted" Checked='<%# (bool)Eval("IsUnlisted") %>' />
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>

                            <div class="form-group">
                                <div class="controls col-md-10 form-group">
                                    <Rock:DataTextBox ID="tbEmail" PrependText="<i class='fa fa-envelope'></i>" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="Email" Label="Email" />
                                </div>
                            </div>

                            <div class="form-group col-md-10">

                                <div class="form-group controls well">
                                    <Rock:RockRadioButtonList ID="rblEmailPreference" runat="server" RepeatDirection="Horizontal" Label="Email Preference">
                                        <asp:ListItem Text="Email Allowed" Value="EmailAllowed" />
                                        <asp:ListItem Text="No Mass Emails" Value="NoMassEmails" />
                                        <asp:ListItem Text="Do Not Email" Value="DoNotEmail" />
                                    </Rock:RockRadioButtonList>

                                    <Rock:RockRadioButtonList ID="rblCommunicationPreference" runat="server" RepeatDirection="Horizontal" Label="Communication Preference">
                                        <asp:ListItem Text="Email" Value="1" />
                                        <asp:ListItem Text="SMS" Value="2" />
                                    </Rock:RockRadioButtonList>
                                </div>

                            </div>
                        </div>
                        <hr />
                    </asp:Panel>

                    <asp:Panel ID="pnlAddress" runat="server">
                        <fieldset>
                            <legend>
                                <div class="panel-heading">
                                    <h3 id="lAddressTitle" runat="server"></h3>
                                </div>
                            </legend>
                            <div class="panel-body">
                                <div class="clearfix col-md-10">
                                    <div class="pull-left margin-b-md">
                                        <asp:Literal ID="lPreviousAddress" runat="server" />
                                    </div>
                                    <div class="pull-left form-group">
                                        <asp:LinkButton ID="lbMoved" CssClass="btn btn-info btn-sm" runat="server" OnClick="lbMoved_Click"><i class="fa fa-truck"></i> Moved</asp:LinkButton>
                                    </div>
                                </div>

                                <asp:HiddenField ID="hfStreet1" runat="server" />
                                <asp:HiddenField ID="hfStreet2" runat="server" />
                                <asp:HiddenField ID="hfCity" runat="server" />
                                <asp:HiddenField ID="hfState" runat="server" />
                                <asp:HiddenField ID="hfPostalCode" runat="server" />
                                <asp:HiddenField ID="hfCountry" runat="server" />

                                <div class="form-group col-md-10">

                                     <Rock:AddressControl ID="acAddress" runat="server" RequiredErrorMessage="Your Address is Required" />

                                    <div class="margin-b-md">
                                        <Rock:RockCheckBox ID="cbIsMailingAddress" runat="server" Text="This is my mailing address" Checked="true" />
                                        <Rock:RockCheckBox ID="cbIsPhysicalAddress" runat="server" Text="This is my physical address" Checked="true" />
                                    </div>

                                </div>
                               
                            </div>

                        </fieldset>
                    </asp:Panel>

                   

                    <asp:Panel ID="pnlPersonAttributes" runat="server">
                        <div class="panel-heading clearfix">
                            <h3 class="pull-left">Additional Information</h3>
                        </div>
                        <div class="panel-body">
                            <div class="col-md-12 col-xs-12">
                                <Rock:DynamicPlaceholder ID="phPersonAttributes" runat="server" />
                            </div>
                        </div>
                        <hr />
                    </asp:Panel>

                    <asp:Panel ID="pnlFamilyAttributes" runat="server">
                        <div class="panel-heading clearfix">
                            <h4 class="panel-title pull-left">Family Information</h4>
                        </div>
                        <div class="panel-body">
                            <div class="col-md-12 col-xs-12">
                            <Rock:DynamicPlaceholder ID="phFamilyAttributes" runat="server" />
                                </div>
                        </div>
                        <hr />
                    </asp:Panel>

                    <div class="actions text-center">
                        <div class="col-xs-12 col-md-6 col-md-offset-3">
                            <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Update" CssClass="btn btn-primary btn-block" OnClick="btnSave_Click" />

                        </div>
                        <div class="col-xs-12 col-md-6 col-md-offset-3">
                            <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                        </div>
                    </div>

                </asp:Panel>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
