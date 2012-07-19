<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Attributes.ascx.cs" Inherits="RockWeb.Blocks.Administration.Attributes" %>

<asp:UpdatePanel ID="upPanel" runat="server">
<ContentTemplate>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert-message block-massage error"/>

    <div class="grid-filter">
        <fieldset>
            <legend>Filter Options</legend>
            <Rock:LabeledDropDownList ID="ddlCategoryFilter" runat="server" LabelText="Category" AutoPostBack="true" OnSelectedIndexChanged="ddlCategoryFilter_SelectedIndexChanged" />
        </fieldset>
    </div>

    <Rock:Grid ID="rGrid" runat="server" >
        <Columns>
            <asp:BoundField DataField="Id" HeaderText="Id" />
            <asp:BoundField DataField="Category" HeaderText="Category"  />
            <asp:BoundField DataField="Name" HeaderText="Name" />
            <asp:BoundField DataField="FieldType" HeaderText="Type" />
            <Rock:BoolField DataField="IsMultiValue" HeaderText="Multi-Value"/>
            <Rock:BoolField DataField="IsRequired" HeaderText="Required"/>
            <Rock:EditField OnClick="rGrid_Edit" />
            <Rock:DeleteField OnClick="rGrid_Delete" />
        </Columns>
    </Rock:Grid>

    <Rock:ModalDialog id="modalDetails" runat="server" Title="Attribute" >
    <Content>
        <asp:HiddenField ID="hfId" runat="server" />
        <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert-message block-message error"/>
        <fieldset>
            <Rock:DataTextBox ID="tbKey" runat="server" SourceTypeName="Rock.Core.Attribute, Rock" PropertyName="Key" />
            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Core.Attribute, Rock" PropertyName="Name" />
            <Rock:DataTextBox ID="tbCategory" runat="server" SourceTypeName="Rock.Core.Attribute, Rock" PropertyName="Category" />
            <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Core.Attribute, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
            <Rock:LabeledFieldType ID="ddlFieldType" runat="server" LabelText="Field Type" />
            <Rock:DataTextBox ID="tbDefaultValue" runat="server" SourceTypeName="Rock.Core.Attribute, Rock" PropertyName="DefaultValue" />
            <Rock:LabeledCheckBox ID="cbMultiValue" runat="server" LabelText="Allow Multiple Values" />
            <Rock:LabeledCheckBox ID="cbRequired" runat="server" LabelText="Required" />
        </fieldset>
    </Content>
    </Rock:ModalDialog>

</ContentTemplate>
</asp:UpdatePanel>
