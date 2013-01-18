<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RockAttributeEditor.ascx.cs" Inherits="RockWeb.Controls.RockAttributeEditor" %>
<asp:HiddenField ID="hfAttributeGuid" runat="server" />
<asp:HiddenField ID="hfAttributeId" runat="server" />
<fieldset>
    <legend>
        <asp:Literal ID="lAttributeActionTitle" runat="server"></asp:Literal>
    </legend>
    <div class="row-fluid">

        <div class="span6">
            <Rock:DataTextBox ID="tbAttributeName" runat="server" SourceTypeName="Rock.Model.Attribute, Rock" PropertyName="Name" />
            <Rock:DataTextBox ID="tbAttributeKey" runat="server" SourceTypeName="Rock.Model.Attribute, Rock" PropertyName="Key" />
            <Rock:DataTextBox ID="tbAttributeCategory" runat="server" SourceTypeName="Rock.Model.Attribute, Rock" PropertyName="Category" />
            <Rock:DataTextBox ID="tbAttributeDescription" runat="server" SourceTypeName="Rock.Model.Attribute, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
        </div>

        <div class="span6">
            <Rock:DataDropDownList ID="ddlAttributeFieldType" runat="server" LabelText="Field Type" SourceTypeName="Rock.Model.FieldType, Rock"
                PropertyName="Name" DataValueField="Id" DataTextField="Name" OnSelectedIndexChanged="ddlAttributeFieldType_SelectedIndexChanged" AutoPostBack="true" />
            <asp:PlaceHolder ID="phAttributeFieldTypeQualifiers" runat="server" />
            <p>Default Value</p>
            <asp:PlaceHolder ID="phAttributeDefaultValue" runat="server" EnableViewState="false" />
            <Rock:LabeledCheckBox ID="cbAttributeMultiValue" runat="server" LabelText="Allow Multiple Values" />
            <Rock:LabeledCheckBox ID="cbAttributeRequired" runat="server" LabelText="Required" />
            <Rock:LabeledCheckBox ID="cbShowInGrid" runat="server" LabelText="Show in Grid" />
        </div>

    </div>
</fieldset>

<div class="actions">
    <asp:LinkButton ID="btnSaveAttribute" runat="server" Text="OK" CssClass="btn btn-primary" OnClick="btnSaveAttribute_Click" />
    <asp:LinkButton ID="btnCancelAttribute" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancelAttribute_Click" />
</div>
