<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Attributes.ascx.cs" Inherits="RockWeb.Blocks.Administration.Attributes" %>

<asp:UpdatePanel ID="upPanel" runat="server">
<ContentTemplate>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert-message block-massage error"/>

    <asp:Panel ID="pnlList" runat="server">

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

    </asp:Panel>

    <asp:Panel ID="pnlDetails" runat="server" Visible="false">

        <asp:HiddenField ID="hfId" runat="server" />

        <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert-message block-message error"/>

        <div class="row">

            <div class="span6">

                <fieldset>
                    <legend><asp:Literal ID="lAction" runat="server"></asp:Literal> Attribute</legend>
                    <Rock:DataTextBox ID="tbKey" runat="server" SourceTypeName="Rock.Core.Attribute, Rock" PropertyName="Key" />
                    <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Core.Attribute, Rock" PropertyName="Name" />
                    <Rock:DataTextBox ID="tbCategory" runat="server" SourceTypeName="Rock.Core.Attribute, Rock" PropertyName="Category" />
                    <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Core.Attribute, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
                </fieldset>

            </div>

            <div class="span6">

                <fieldset>
                    <legend>&nbsp;</legend>
                    <Rock:LabeledDropDownList ID="ddlFieldType" runat="server" LabelText="Field Type"></Rock:LabeledDropDownList>
                    <asp:PlaceHolder ID="phFieldTypeQualifiers" runat="server"></asp:PlaceHolder>
                    <Rock:DataTextBox ID="tbDefaultValue" runat="server" SourceTypeName="Rock.Core.Attribute, Rock" PropertyName="DefaultValue" />
                    <Rock:LabeledCheckBox ID="cbMultiValue" runat="server" LabelText="Allow Multiple Values" />
                    <Rock:LabeledCheckBox ID="cbRequired" runat="server" LabelText="Required" />
                </fieldset>

            </div>

        </div>

        <div class="actions">
            <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn primary" onclick="btnSave_Click" />
            <asp:LinkButton id="btnCancel" runat="server" Text="Cancel" CssClass="btn secondary" CausesValidation="false" OnClick="btnCancel_Click" />
        </div>

    </asp:Panel>

</ContentTemplate>
</asp:UpdatePanel>
