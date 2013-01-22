<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DynamicReporting.ascx.cs" Inherits="RockWeb.Blocks.Administration.DynamicReporting" %>
<asp:UpdatePanel ID="upReporting" runat="server">
    <ContentTemplate>

        <div class="row-fluid">
            <div class="span6 well">

                <fieldset>
                    <legend>Details</legend>
                    <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.Report, Rock" PropertyName="Name" />
                    <Rock:DataTextBox  ID="tbDescription" runat="server" SourceTypeName="Rock.Model.Report, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
                </fieldset>
            </div>

            <div class="span6 well">

                <fieldset>
                    <legend>Filters</legend>
                    <div class="control-group">
                        <asp:Label ID="lblFilters" runat="server" AssociatedControlID="ddlFilters" CssClass="control-label" Text="Filters"></asp:Label>
                        <div class="controls input-append">
                            <asp:DropDownList ID="ddlFilters" runat="server" CssClass="span5"></asp:DropDownList>
                            <asp:Button ID="btnAdd" runat="server" CssClass="btn" Text="Add" />
                        </div>
                        <asp:PlaceHolder ID="phFilters" runat="server"></asp:PlaceHolder>
                    </div>
                </fieldset>
            </div>
        </div>

        <div>
            <asp:Label runat="server" ID="lblGridTitle" Text="Results" />
            <Rock:Grid ID="gResults" runat="server">
                <Columns>
                </Columns>
            </Rock:Grid>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
