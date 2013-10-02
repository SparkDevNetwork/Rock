<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EditPerson.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.EditPerson" %>

<asp:UpdatePanel ID="upEditPerson" runat="server">
    <ContentTemplate>

        <div class="row-fluid">

            <div class="span3">
                <Rock:ImageUploader ID="imgPhoto" runat="server" Label="Photo" />
            </div>

            <div class="span6">

                <fieldset>
                    <Rock:RockDropDownList ID="ddlTitle" runat="server" Label="Title"/>
                    <Rock:DataTextBox ID="tbGivenName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="GivenName" />
                    <Rock:DataTextBox ID="tbNickName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="NickName" />
                    <Rock:DataTextBox ID="tbMiddleName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="MiddleName" />
                    <Rock:DataTextBox ID="tbLastName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="LastName" />
                    <Rock:RockDropDownList ID="ddlSuffix" runat="server" Label="Suffix"/>
                    <Rock:DatePicker ID="dpBirthDate" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="BirthDate" />
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
                    <Rock:RockDropDownList ID="ddlRecordStatus" runat="server" Label="Record Status" AutoPostBack="true" OnSelectedIndexChanged="ddlRecordStatus_SelectedIndexChanged" /><br />
                    <Rock:RockDropDownList ID="ddlReason" runat="server" Label="Reason" Visible="false"></Rock:RockDropDownList>
                </fieldset>

            </div>

        </div>

    </ContentTemplate>
</asp:UpdatePanel>
