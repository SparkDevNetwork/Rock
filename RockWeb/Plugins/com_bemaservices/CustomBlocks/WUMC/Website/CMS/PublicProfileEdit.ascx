<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PublicProfileEdit.ascx.cs" Inherits="RockWeb.Plugins.com_bemadev.Cms.PublicProfileEdit" %>

<asp:UpdatePanel ID="upContent" runat="server">
    <ContentTemplate>

        <script>
            $(function () {
                $(".photo a").fluidbox();
            });
        </script>

       
           <div class="edit-profile">
                <asp:HiddenField ID="hfPersonId" runat="server" />
                <Rock:NotificationBox ID="nbNotAuthorized" runat="server" Text="You must be logged in to view your account." NotificationBoxType="Danger" Visible="false" />
                <asp:Panel ID="pnlView" runat="server">
                    <div class="well">

                    <div class="row">

                        <div class="col-sm-3">
                            <div class="photo">
                                <asp:Literal ID="lImage" runat="server" />
                            </div>
                        </div>

                        <div class="col-sm-9">
                            <h4 class="title name">
                                <asp:Literal ID="lName" runat="server" /><div class="pull-right">
                                    <Rock:RockDropDownList ID="ddlGroup" runat="server" DataTextField="Name" DataValueField="Id" OnSelectedIndexChanged="ddlGroup_SelectedIndexChanged" AutoPostBack="true" Visible="false" />
                                </div>
                            </h4>
							<asp:Literal ID="lAddress" runat="server" />
							<div class="row">
							  <div class="col-md-12 mb-10">
                                    <ul class="phone-list list-unstyled" style="margin-bottom:0px;">
                                        <asp:Repeater ID="rptPhones" runat="server">
                                            <ItemTemplate>
                                                <li><%# (bool)Eval("IsUnlisted") ? "Unlisted" : FormatPhoneNumber( Eval("CountryCode"), Eval("Number") ) %> <small><%# Eval("NumberTypeValue.Value") %></small></li>
                                            </ItemTemplate>
                                        </asp:Repeater>
                                    </ul>
                                    <asp:Literal ID="lEmail" runat="server" />
                                </div>
							</div>	
                            <div class="row">
                                <div class="col-md-12 mb-10">
                                    <ul class="person-demographics list-unstyled">
                                        <li class="mb-10">
                                            <asp:Literal ID="lAge" runat="server" /></li>
										<li class="mb-10">
                                            <asp:Literal ID="lConnection" runat="server" /></li>
										<li class="mb-10">
                                            <asp:Literal ID="lEmployer" runat="server" /></li>
										<li class="mb-10">
                                            <asp:Literal ID="lPosition" runat="server" /></li>
                                        <li class="hidden">
                                            <asp:Literal ID="lGender" runat="server" /></li>
                                        <li class="hidden">
                                            <asp:Literal ID="lMaritalStatus" runat="server" /></li>
                                        <li class="hidden">
                                            <asp:Literal ID="lGrade" runat="server" /></li>
                                    </ul>
                                </div>          
                            </div>
                            
                       
                            <div class="row hidden">
                                <asp:Repeater ID="rptPersonAttributes" runat="server">
                                    <ItemTemplate>
                                        <div class="col-md-6">
                                            <b><%# Eval("Name") %></b></br><small><%# Eval("Value") %></small>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>
                            <div class="row hidden">
                                <div class="col-md-12">
                                    <asp:Literal ID="lFamilyHeader" runat="server" Text="<h4>Family Information</h4>" Visible="false" />
                                </div>
                                <asp:Repeater ID="rptGroupAttributes" runat="server">
                                    <ItemTemplate>
                                        <div class="col-md-6">
                                            <b><%# Eval("Name") %></b></br><small><%# Eval("Value") %></small>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>
                    
                            <asp:LinkButton ID="lbEditPerson" runat="server" CssClass="btn btn-primary" OnClick="lbEditPerson_Click" CausesValidation="false"><i class="fa fa-pencil" style="margin-left:6px;"></i> Update</asp:LinkButton>
                        </div>
                    </div>

                </div>

                <div class="well">
                    <h4>
                       Family <asp:Literal ID="lGroupName" runat="server" Visible="false"/>
                    </h4>
                    <asp:Repeater ID="rptGroupMembers" runat="server" OnItemDataBound="rptGroupMembers_ItemDataBound" OnItemCommand="rptGroupMembers_ItemCommand">
                        <ItemTemplate>
                            <div class="row">
                                <div class="col-md-1 ">
                                    <div class="photo">
                                        <asp:Literal ID="lGroupMemberImage" runat="server" />
                                    </div>
                                </div>
                                <div class="col-md-11">
                                    <div class="row">
                                        <div class="col-md-3">
                                            <b class="inline-block">
                                                <asp:Literal ID="lGroupMemberName" runat="server" /></b><br>
                                           <asp:LinkButton ID="lbEditGroupMember" runat="server" CssClass="btn btn-primary" CommandArgument='<%# Eval("PersonId") %>' CommandName="Update"> <i class="fa fa-pencil" style="margin-left:6px;"></i> Update</asp:LinkButton>

                                        </div>
                                        <div class="col-md-4 hidden">
                                            <ul class="person-demographics list-unstyled">
                                                <li>
                                                    <asp:Literal ID="lAge" runat="server" /></li>
                                                <li class="hidden">
                                                    <asp:Literal ID="lGender" runat="server" Visible="false"/></li>
                                                <li class="hidden">
                                                    <asp:Literal ID="lMaritalStatus" runat="server" Visible="false" /></li>
                                                <li class="hidden">
                                                    <asp:Literal ID="lGrade" runat="server" Visible="false" /></li>
                                            </ul>
                                        </div>
                                        <div class="col-md-4 hidden">
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
                                    <div class="row hidden">
                                        <asp:Repeater ID="rptGroupMemberAttributes" runat="server">
                                            <ItemTemplate>
                                                <div class="col-md-6">
                                                    <b><%# Eval("Name") %></b></br><small><%# Eval("Value") %></small>
                                                </div>
                                            </ItemTemplate>
                                        </asp:Repeater>
                                    </div>
                                </div>
                            </div>
							<hr/ style="border-style: dashed;">
 
                        </ItemTemplate>
                    </asp:Repeater>
                    <asp:LinkButton ID="lbAddGroupMember" runat="server" CssClass="btn btn-xs btn-link" OnClick="lbAddGroupMember_Click"><i class="fa fa-plus"></i> Add New Family Member</asp:LinkButton><br/>
					<a href="https://people.thewoodlandsumc.org/page/648" target="_blank"  Class="btn btn-xs btn-link" style="text-align: left;"><i class="fa fa-envelope"></i> Click here to report additional changes</a>

                    <asp:LinkButton ID="lbRequestChanges" runat="server" CssClass="btn btn-xs" OnClick="lbRequestChanges_Click"> Request Additional Changes</asp:LinkButton>
                </asp:Panel>

                <asp:Panel ID="pnlEdit" runat="server" CssClass="well">
                    <asp:ValidationSummary ID="valValidation" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                    <div class="row">

                        <div class="col-md-3">
                            <Rock:ImageEditor ID="imgPhoto" runat="server" Label="Photo" BinaryFileTypeGuid="03BD8476-8A9F-4078-B628-5B538F967AFC" />
                        </div>

                        <div class="col-md-9">
                            <Rock:RockDropDownList ID="ddlTitle" runat="server" CssClass="input-width-md" Label="Title" />
                            <Rock:DataTextBox ID="tbFirstName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="FirstName" Required="true" />
                            <Rock:DataTextBox ID="tbNickName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="NickName" />
                            <Rock:DataTextBox ID="tbMiddleName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="MiddleName" />
                            <Rock:DataTextBox ID="tbLastName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="LastName" Required="true" />
                            <Rock:RockDropDownList ID="ddlSuffix" CssClass="input-width-md" runat="server" Label="Suffix" />
                            <Rock:BirthdayPicker ID="bpBirthDay" runat="server" Label="Birthday" />
                            <Rock:RockRadioButtonList ID="rblRole" runat="server" DataTextField="Name" DataValueField="Id" RepeatDirection="Horizontal" Label="Role" Visible="false" AutoPostBack="true" OnSelectedIndexChanged="rblRole_SelectedIndexChanged" />
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockRadioButtonList ID="rblGender" runat="server" RepeatDirection="Horizontal" Label="Gender" Required="true">
                                        <asp:ListItem Text="Male" Value="Male" />
                                        <asp:ListItem Text="Female" Value="Female" />
                                        <asp:ListItem Text="Unknown" Value="Unknown" />
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
                    <hr />

					<asp:Panel ID="pnlPersonAttributes" runat="server">
                        <div class="panel-heading clearfix">
                            <h4 class="panel-title pull-left">Additional Information</h4>
                        </div>
                        <div class="panel-body">
                            <asp:PlaceHolder ID="phPersonAttributes" runat="server" EnableViewState="false"></asp:PlaceHolder>
                        </div>
                        <hr />
                    </asp:Panel>

                    <asp:Panel ID="pnlFamilyAttributes" runat="server">
                        <div class="panel-heading clearfix">
                            <h4 class="panel-title pull-left">Family Information</h4>
                        </div>
                        <div class="panel-body">
                            <asp:PlaceHolder ID="phFamilyAttributes" runat="server" EnableViewState="false"></asp:PlaceHolder>
                        </div>
                        <hr />
                    </asp:Panel>

                    <h3>Contact Info</h3>
                    <div class="form-horizontal">
                        <asp:Repeater ID="rContactInfo" runat="server">
                            <ItemTemplate>
                                <div class="form-group">
                                    <div class="control-label col-md-2"><%# Eval("NumberTypeValue.Value")  %></div>
                                    <div class="controls col-md-10">
                                        <div class="row">
                                            <div class="col-md-7">
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
                            <div class="control-label col-md-2">Email</div>
                            <div class="controls col-md-10">
                                <Rock:DataTextBox ID="tbEmail" PrependText="<i class='fa fa-envelope'></i>" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="Email" Label="" />
                            </div>
                        </div>

                        <div class="form-group">
                            <div class="controls col-md-10 col-md-offset-2">
                                <Rock:RockRadioButtonList ID="rblEmailPreference" runat="server" RepeatDirection="Horizontal" Label="Email Preference">
                                    <asp:ListItem Text="Email Allowed" Value="EmailAllowed" />
                                    <asp:ListItem Text="No Mass Emails" Value="NoMassEmails" />
                                    <asp:ListItem Text="Do Not Email" Value="DoNotEmail" />
                                </Rock:RockRadioButtonList>
                                
                                <Rock:RockRadioButtonList ID="rblCommunicationPreference" runat="server" RepeatDirection="Horizontal" Label="Communication Preference" visible="false" >
                                    <asp:ListItem Text="Email" Value="1" />
                                    <asp:ListItem Text="SMS" Value="2" />
                                </Rock:RockRadioButtonList>
                            </div>
                        </div>
                    </div>

                    <asp:Panel ID="pnlAddress" runat="server">
                        <fieldset>
                            <legend class="hidden">
                                <asp:Literal ID="lAddressTitle" runat="server" Visible="false" /></legend>

                            <div class="clearfix margin-b-lg" :>
                                <div class="pull-left">
                                    <asp:Literal ID="lPreviousAddress" runat="server" />
                                </div>

                            </div>
							
							<div class="clearfix">
									<asp:LinkButton ID="lbMoved" CssClass="btn btn-primary" runat="server" OnClick="lbMoved_Click"><i class="fa fa-truck"></i> UPDATE ADDRESS</asp:LinkButton>
								</div>

                            <asp:HiddenField ID="hfStreet1" runat="server" />
                            <asp:HiddenField ID="hfStreet2" runat="server" />
                            <asp:HiddenField ID="hfCity" runat="server" />
                            <asp:HiddenField ID="hfState" runat="server" />
                            <asp:HiddenField ID="hfPostalCode" runat="server" />
                            <asp:HiddenField ID="hfCountry" runat="server" />

                            <Rock:AddressControl ID="acAddress" runat="server" RequiredErrorMessage="Your Address is Required" visible="false" />

                            <div class="margin-b-md">
                                <Rock:RockCheckBox ID="cbIsMailingAddress" runat="server" Text="This is my mailing address" Checked="true" />
                                <Rock:RockCheckBox ID="cbIsPhysicalAddress" runat="server" Text="This is my physical address" Checked="true" />
                            </div>
                        </fieldset>
                    </asp:Panel>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>

                </asp:Panel>
            </div>
         </div>
    </ContentTemplate>
</asp:UpdatePanel>
