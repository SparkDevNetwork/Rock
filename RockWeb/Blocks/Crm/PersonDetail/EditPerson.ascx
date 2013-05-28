<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EditPerson.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.EditPerson" %>

<asp:UpdatePanel ID="upEditPerson" runat="server">
    <ContentTemplate>

        <fieldset>
            <Rock:DataTextBox id="tbGivenName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="GivenName"  />
            <Rock:DataTextBox id="tbNickName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="NickName"  />
            <Rock:DataTextBox id="tbMiddleName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="MiddleName"  />
            <Rock:DataTextBox id="tbLastName" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="LastName"  />
            <Rock:DatePicker ID="dpBirthDate" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="BirthDate" />
            <Rock:DatePicker ID="dpAnniversaryDate" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="AnniversaryDate" />

            <Rock:LabeledRadioButtonList id="rblGender" runat="server">
                <asp:ListItem Text="Male" Value="1"/>
                <asp:ListItem Text="Female" Value="2" />
                <asp:ListItem Text="Unknown" Value="0" />
            </Rock:LabeledRadioButtonList>

            <Rock:LabeledRadioButtonList ID="rblMaritalStatus" runat="server" />
            <Rock:LabeledRadioButtonList ID="rblStatus" runat="server" />

        </fieldset>

        <fieldset>
            <legend>Contact Info</legend>
            <ul>
                <asp:Repeater ID="rContactInfo" runat="server">
                    <ItemTemplate>
                        <li>
                            <Rock:LabeledTextBox ID="tbPhone" runat="server" />
                            <asp:CheckBox ID="cbUnlisted" runat="server" Text="unlisted" />
                        </li>
                    </ItemTemplate> 
                </asp:Repeater>
            </ul>
        </fieldset>

        <fieldset>
            <Rock:DataTextBox id="tbEmail" runat="server" SourceTypeName="Rock.Model.Person, Rock" PropertyName="Email"  />
        </fieldset>>

        <div class="actions">
            <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
            <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
        </div>

</div>
<div class="actions" style="display: none;">
</div>

    </ContentTemplate>
</asp:UpdatePanel>
