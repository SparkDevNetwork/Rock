<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AttributeValues.ascx.cs" Inherits="RockWeb.Blocks.Administration.AttributeValues" %>

<asp:UpdatePanel ID="upSettings" runat="server">
<ContentTemplate>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert-message block-message error"/>

    <div class="grid-filter">
        <fieldset>
            <legend>Filter</legend>
            <Rock:LabeledDropDownList ID="ddlCategoryFilter" runat="server" LabelText="Category" AutoPostBack="true" OnSelectedIndexChanged="ddlCategoryFilter_SelectedIndexChanged" />
        </fieldset>
    </div>

    <Rock:Grid ID="rGrid" runat="server">
        <Columns>
            <asp:BoundField DataField="Category" HeaderText="Category" />
            <asp:BoundField DataField="Name" HeaderText="Name" />
            <asp:BoundField DataField="Description" HeaderText="Description" />
            <asp:BoundField DataField="DefaultValue" HeaderText="Default Value" />
            <asp:TemplateField>
                <HeaderTemplate>Value</HeaderTemplate>
                <ItemTemplate><asp:Literal ID="lValue" runat="server"></asp:Literal></ItemTemplate>
            </asp:TemplateField>
            <Rock:EditField OnClick="rGrid_Edit" />
        </Columns>
    </Rock:Grid>

    <Rock:ModalDialog id="modalDetails" runat="server" Title="Attribute" >
    <Content>
        <asp:HiddenField ID="hfId" runat="server" />
        <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert-message block-message error"/>
        <fieldset>
            <div class="control-group">
                <label class="control-label"><asp:Literal ID="lCaption" runat="server"></asp:Literal></label>
                <div class="controls">
                    <asp:PlaceHolder ID="phEditControl" runat="server"></asp:PlaceHolder>
                </div>
            </div>
        </fieldset>
    </Content>
    </Rock:ModalDialog>

</ContentTemplate>
</asp:UpdatePanel>
