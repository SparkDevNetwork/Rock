<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DataViewDetail.ascx.cs" Inherits="RockWeb.Blocks.Reporting.DataViewDetail" %>
<script>
    Sys.Application.add_load(function () {
        $('a.filter-view-state').click(function () {
            $header = $(this).parent().parent();
            $header.siblings('.widget-content').slideToggle();
            $header.children('div.pull-left').children('div').slideToggle();
            $enabled = $header.children('input.filter-expanded');
            $enabled.val($enabled.val() == "True" ? "False" : "True");
            $('i', this).toggleClass('icon-chevron-down');
            $('i', this).toggleClass('icon-chevron-up');
        });
    });
</script>
<asp:UpdatePanel ID="upDataView" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server">

            <fieldset>
                <legend><asp:Literal ID="lName" runat="server" /></legend>
                <Rock:LabeledText ID="ltAppliedTo" runat="server" LabelText="Applied To" />
                <Rock:LabeledText ID="ltDescription" runat="server" LabelText="Description" />
                <Rock:LabeledText ID="ltFilter" runat="server" LabelText="Filter" />
            </fieldset>

            <div class="actions">
                <asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click"/>
                <asp:LinkButton ID="btnBack" runat="server" Text="Back" CssClass="btn" OnClick="btnBack_Click" />
            </div>

        </asp:Panel>

        <asp:Panel ID="pnlEdit" runat="server" Visible="false">

            <fieldset>
                <legend>Details</legend>
                <Rock:DataDropDownList ID="ddlEntityType" runat="server" SourceTypeName="Rock.Model.DataView, Rock" PropertyName="EntityTypeId" DataTextField="FriendlyName" LabelText="Applied To" DataValueField="Id" />
                <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.DataView, Rock" PropertyName="Name" CssClass="" />
                <Rock:DataTextBox  ID="tbDescription" runat="server" SourceTypeName="Rock.Model.DataView, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
            </fieldset>

            <asp:PlaceHolder ID="phFilters" runat="server"></asp:PlaceHolder>

            <div class="actions">
                <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>

        </asp:Panel>
        
    </ContentTemplate>
</asp:UpdatePanel>
