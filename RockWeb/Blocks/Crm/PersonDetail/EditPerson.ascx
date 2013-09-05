<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EditPerson.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.EditPerson" %>

<asp:UpdatePanel ID="upEditPerson" runat="server">
    <ContentTemplate>

        <div class="row-fluid">

            <div class="span3">
                <Rock:ImageUploader ID="imgPhoto" runat="server" LabelText="Photo" />
            </div>

            <div class="span6">

                <fieldset>
                    <Rock:LabeledDropDownList ID="ddlTitle" runat="server" LabelText="Title"/>
                    <Rock:DataTextBox ID="tbGivenName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="GivenName" />
                    <Rock:DataTextBox ID="tbNickName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="NickName" />
                    <Rock:DataTextBox ID="tbMiddleName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="MiddleName" />
                    <Rock:DataTextBox ID="tbLastName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="LastName" />
                    <Rock:LabeledDropDownList ID="ddlSuffix" runat="server" LabelText="Suffix"/>
                    <Rock:DatePicker ID="dpBirthDate" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="BirthDate" />
                    <Rock:DatePicker ID="dpAnniversaryDate" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="AnniversaryDate" />

                    <Rock:LabeledRadioButtonList ID="rblGender" runat="server" RepeatDirection="Horizontal" LabelText="Gender">
                        <asp:ListItem Text="Male" Value="Male" />
                        <asp:ListItem Text="Female" Value="Female" />
                        <asp:ListItem Text="Unknown" Value="Unknown" />
                    </Rock:LabeledRadioButtonList>

                    <Rock:LabeledRadioButtonList ID="rblMaritalStatus" runat="server" RepeatDirection="Horizontal" LabelText="Marital Status" />
                    <Rock:LabeledRadioButtonList ID="rblStatus" runat="server" LabelText="Person Status" />

                </fieldset>

                <fieldset>
                    <legend>Contact Info</legend>

                    <div class="form-horizontal">
                        <asp:Repeater ID="rContactInfo" runat="server">
                            <ItemTemplate>
                                <div class="control-group">
                                    <div class="control-label"><%# Eval("NumberTypeValue.Name")  %></div>
                                    <div class="controls">
                                        <asp:HiddenField ID="hfPhoneType" runat="server" Value='<%# Eval("NumberTypeValueId")  %>' />
                                        <asp:TextBox ID="tbPhone" runat="server" Text='<%# Eval("NumberFormatted")  %>' />
                                        <asp:CheckBox ID="cbUnlisted" runat="server" Text="unlisted" Checked='<%# (bool)Eval("IsUnlisted") %>' />
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>

                    <Rock:DataTextBox ID="tbEmail" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="Email" />

                </fieldset>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>

            </div>

            <div class="span3">

                <fieldset>
                    <Rock:LabeledDropDownList ID="ddlRecordStatus" runat="server" LabelText="Record Status" AutoPostBack="true" OnSelectedIndexChanged="ddlRecordStatus_SelectedIndexChanged" /><br />
                    <Rock:LabeledDropDownList ID="ddlReason" runat="server" LabelText="Reason" Visible="false"></Rock:LabeledDropDownList>
                </fieldset>

            </div>

        </div>

    </ContentTemplate>
</asp:UpdatePanel>
