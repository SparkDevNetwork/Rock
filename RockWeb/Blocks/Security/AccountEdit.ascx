<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AccountEdit.ascx.cs" Inherits="RockWeb.Blocks.Security.AccountEdit" %>

<asp:UpdatePanel ID="upEditPerson" runat="server">
    <ContentTemplate>

        <asp:ValidationSummary ID="valValidation" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

        <div class="row">

            <div class="col-md-3">
                <Rock:ImageEditor ID="imgPhoto" runat="server" Label="Photo" BinaryFileTypeGuid="03BD8476-8A9F-4078-B628-5B538F967AFC" />
            </div>

            <div class="col-md-9">

                <fieldset>
                    <Rock:RockDropDownList ID="ddlTitle" runat="server" CssClass="input-width-md" Label="Title"/>
                    <Rock:DataTextBox ID="tbFirstName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="FirstName" />
                    <Rock:DataTextBox ID="tbNickName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="NickName" />
                    <Rock:DataTextBox ID="tbLastName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="LastName" />
                    <Rock:RockDropDownList ID="ddlSuffix" CssClass="input-width-md" runat="server" Label="Suffix"/>
                    <Rock:BirthdayPicker ID="bpBirthDay" runat="server" Label="Birthday" />
                    <Rock:RockRadioButtonList ID="rblGender" runat="server" RepeatDirection="Horizontal" Label="Gender">
                        <asp:ListItem Text="Male" Value="Male" />
                        <asp:ListItem Text="Female" Value="Female" />
                        <asp:ListItem Text="Unknown" Value="Unknown" />
                    </Rock:RockRadioButtonList>
                </fieldset>

                <fieldset>
                    <legend>Contact Info</legend>

                    <div class="form-horizontal">
                        <asp:Repeater ID="rContactInfo" runat="server">
                            <ItemTemplate>
                                <div class="form-group">
                                    <div class="control-label col-sm-2"><%# Eval("NumberTypeValue.Value")  %></div>
                                    <div class="controls col-sm-10">
                                        <div class="row">
                                            <div class="col-sm-7">
                                                <asp:HiddenField ID="hfPhoneType" runat="server" Value='<%# Eval("NumberTypeValueId")  %>' />
                                                <Rock:PhoneNumberBox ID="pnbPhone" runat="server" CountryCode='<%# Eval("CountryCode")  %>' Number='<%# Eval("NumberFormatted")  %>' />
                                            </div>    
                                            <div class="col-sm-5">
                                                <div class="row">
                                                    <div class="col-xs-6">
                                                        <asp:CheckBox ID="cbSms"  runat="server" Text="SMS" Checked='<%# (bool)Eval("IsMessagingEnabled") %>' CssClass="js-sms-number"  />
                                                    </div>
                                                    <div class="col-xs-6">
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

                    <Rock:DataTextBox ID="tbEmail" PrependText="<i class='fa fa-envelope'></i>" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="Email" />

                </fieldset>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>

            </div>

        </div>

    </ContentTemplate>
</asp:UpdatePanel>
