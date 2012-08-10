<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Metrics.ascx.cs" Inherits="RockWeb.Blocks.Administration.Metrics" %>

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

        <Rock:Grid ID="rGrid" runat="server" AllowSorting="true" >
            <Columns>
                <asp:BoundField DataField="Id" HeaderText="Id" SortExpression="Id" />
                <asp:BoundField DataField="Category" HeaderText="Category" SortExpression="Category" />
                <asp:BoundField DataField="Title" HeaderText="Title" />
                <asp:BoundField DataField="Description" HeaderText="Description" SortExpression="Description"/>
                <asp:BoundField DataField="MinValue" HeaderText="MinValue" SortExpression="MinValue"/>
                <asp:BoundField DataField="MaxValue" HeaderText="MaxValue" SortExpression="MaxValue"/>
                <asp:BoundField DataField="CollectionFrequency" HeaderText="CollectionFrequency" SortExpression="CollectionFrequency"/>
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
                    <legend><asp:Literal ID="lAction" runat="server"></asp:Literal> Metric</legend>
                    
                    <Rock:DataTextBox ID="tbCategory" runat="server" SourceTypeName="Rock.Core.Metric, Rock" PropertyName="Category" />
                    <Rock:DataTextBox ID="tbTitle" runat="server" SourceTypeName="Rock.Core.Metric, Rock" PropertyName="Title" />
                    <Rock:DataTextBox ID="tbSubtitle" runat="server" SourceTypeName="Rock.Core.Metric, Rock" PropertyName="Subtitle" />                   
                    <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Core.Metric, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
                </fieldset>

            </div>

            <div class="span6">

                <fieldset>
                    <legend>&nbsp;</legend>
                    <asp:PlaceHolder ID="phCollectionFrequency" runat="server"></asp:PlaceHolder>
                    <Rock:DataTextBox ID="tbMinValue" runat="server" SourceTypeName="Rock.Core.Metric, Rock" PropertyName="MinValue" />
                    <Rock:DataTextBox ID="tbMaxValue" runat="server" SourceTypeName="Rock.Core.Metric, Rock" PropertyName="MaxValue" />
                    <Rock:LabeledCheckBox ID="cbType" runat="server" LabelText="Allow Multiple Values" />                    
                    <Rock:DataTextBox ID="tbSource" runat="server" SourceTypeName="Rock.Core.Metric, Rock" PropertyName="Source" />
                    <Rock:DataTextBox ID="tbSourceSQL" runat="server" SourceTypeName="Rock.Core.Metric, Rock" PropertyName="SourceSQL" />
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
