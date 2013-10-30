<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EditPerson.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.EditPerson" %>

<asp:UpdatePanel ID="upEditPerson" runat="server">
    <ContentTemplate>

        <div class="row">

            <div class="col-md-3">
                <Rock:ImageUploader ID="imgPhoto" runat="server" Label="Photo" />
            </div>

            <div class="col-md-6">

                <fieldset>
                    <Rock:RockDropDownList ID="ddlTitle" runat="server" CssClass="input-width-md" Label="Title"/>
                    <Rock:DataTextBox ID="tbGivenName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="GivenName" />
                    <Rock:DataTextBox ID="tbNickName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="NickName" />
                    <Rock:DataTextBox ID="tbMiddleName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="MiddleName" />
                    <Rock:DataTextBox ID="tbLastName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="LastName" />
                    <Rock:RockDropDownList ID="ddlSuffix" CssClass="input-width-md" runat="server" Label="Suffix"/>
                    <Rock:BirthdayPicker ID="bpBirthDay" runat="server" Label="Birthday" />
                    <Rock:DatePicker ID="dpAnniversaryDate" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="AnniversaryDate" />

                    <Rock:RockRadioButtonList ID="rblGender" runat="server" RepeatDirection="Horizontal" Label="Gender">
                        <asp:ListItem Text="Male" Value="Male" />
                        <asp:ListItem Text="Female" Value="Female" />
                        <asp:ListItem Text="Unknown" Value="Unknown" />
                    </Rock:RockRadioButtonList>

                    <Rock:RockRadioButtonList ID="rblMaritalStatus" runat="server" RepeatDirection="Horizontal" Label="Marital Status" />
                    <Rock:RockRadioButtonList ID="rblStatus" runat="server" Label="Person Status" />

                </fieldset>

                <fieldset>
                    <legend>Contact Info</legend>

                    <div class="form-horizontal">
                        <asp:Repeater ID="rContactInfo" runat="server">
                            <ItemTemplate>
                                <div class="form-group">
                                    <div class="control-label col-md-2"><%# Eval("NumberTypeValue.Name")  %></div>
                                    <div class="controls col-md-10">
                                        <div class="row">
                                            <div class="col-sm-9">
                                                <asp:HiddenField ID="hfPhoneType" runat="server" Value='<%# Eval("NumberTypeValueId")  %>' />
                                                <Rock:DataTextBox ID="tbPhone" PrependText="<i class='icon-phone-sign'></i>" runat="server" Text='<%# Eval("NumberFormatted")  %>' />
                                            </div>    
                                            <div class="col-sm-3">
                                                <asp:CheckBox ID="cbSms" runat="server" Text="sms" Checked='<%# (bool)Eval("IsMessagingEnabled") %>' />
                                                <asp:CheckBox ID="cbUnlisted" runat="server" Text="unlisted" Checked='<%# (bool)Eval("IsUnlisted") %>' />
                                            </div>

                                        </div>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>

                    <Rock:DataTextBox ID="tbEmail" PrependText="<i class=' icon-envelope'></i>" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="Email" />

                </fieldset>

                <fieldset>
                    <legend>Contribution Info</legend>
                    <Rock:RockDropDownList ID="ddlGivingGroup" runat="server" Label="Combine Giving With" Help="The family that this person's gifts should be combined with for contribution statements and reporting.  If left blank, their contributions will not be grouped with their family" /> 
                </fieldset>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>

            </div>

            <div class="col-md-3">

                <fieldset>
                    <Rock:RockDropDownList ID="ddlRecordStatus" runat="server" Label="Record Status" AutoPostBack="true" OnSelectedIndexChanged="ddlRecordStatus_SelectedIndexChanged" /><br />
                    <Rock:RockDropDownList ID="ddlReason" runat="server" Label="Reason" Visible="false"></Rock:RockDropDownList>
                </fieldset>

            </div>

        </div>

    </ContentTemplate>
</asp:UpdatePanel>
