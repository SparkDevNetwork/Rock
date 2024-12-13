<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EntityTypes.ascx.cs" Inherits="RockWeb.Blocks.Core.EntityTypes" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-square"></i> Entity List</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="gfSettings" runat="server">
                        <Rock:RockTextBox ID="tbSearch" runat="server" Label="Entity Type or Name Contains" />
                    </Rock:GridFilter>
                    <Rock:Grid ID="gEntityTypes" runat="server" AllowSorting="true">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Entity Type" SortExpression="Name" />
                            <Rock:RockBoundField DataField="FriendlyName" HeaderText="Friendly Name" SortExpression="FriendlyName" />
                            <Rock:BoolField DataField="IsCommon" HeaderText="Common" SortExpression="IsCommon" />
                            <Rock:RockTemplateFieldUnselected>
                                <HeaderStyle CssClass="grid-columncommand" />
                                <ItemStyle CssClass="grid-columncommand" HorizontalAlign="Center"/>
                                <ItemTemplate>
                                    <a id="aSecure" runat="server" class="btn btn-sm btn-square btn-security"><i class="fa fa-lock"></i></a>
                                </ItemTemplate>
                            </Rock:RockTemplateFieldUnselected>
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>



        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="mdEdit" runat="server" Title="Entity" OnCancelScript="clearActiveDialog();">
            <Content>
                <asp:HiddenField ID="hfEntityTypeId" runat="server" />
                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.EntityType, Rock" PropertyName="Name" Label="Entity Type Name" />
                <Rock:DataTextBox ID="tbFriendlyName" runat="server" SourceTypeName="Rock.Model.EntityType, Rock" PropertyName="FriendlyName" Label="Friendly Name" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbCommon" runat="server" Label="Common" Help="There are various places that a user is prompted for an entity type.  'Common' entities will be listed first for the user to easily find them" />
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbIsRelatedToInteraction" runat="server" Label="Track Interaction" Help="When enabled this will track which interaction was active when instances of this entity type are created." />
                        </div>
                    </div>
                </div>

                <Rock:CodeEditor ID="ceIndexResultsTemplate" runat="server" Label="Index Results Template" EditorTheme="Rock" EditorMode="Lava" EditorHeight="200"
                    Help="The Lava used by the Universal Search feature to display results. Available merge fields include CurrentPerson, IndexDocument and DisplayOptions." />
                <Rock:CodeEditor ID="ceIndexDocumentUrl" runat="server" Label="Index Document URL Pattern" EditorTheme="Rock" EditorMode="Lava" EditorHeight="200"
                    Help="The Lava used by the Universal Search feature to determine the Rock URL of the document. Available merge fields include CurrentPerson, IndexDocument and DisplayOptions." />
                <Rock:CodeEditor ID="ceLinkUrl" runat="server" Label="Link URL Pattern" EditorTheme="Rock" EditorMode="Lava" EditorHeight="80"
                    Help="The Lava used when Rock needs to create a URL to an entity based just on the entity type (ie. Tag Report). Available merge field is Entity." />
            </Content>
        </Rock:ModalDialog>

        <Rock:NotificationBox ID="nbWarning" runat="server" Title="Warning" NotificationBoxType="Warning" Visible="false" />

    </ContentTemplate>
</asp:UpdatePanel>