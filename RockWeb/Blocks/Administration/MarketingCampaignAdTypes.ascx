<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MarketingCampaignAdTypes.ascx.cs" Inherits="RockWeb.Blocks.Administration.MarketingCampaignAdTypes" %>

<asp:UpdatePanel ID="upMarketingCampaignAdType" runat="server">
    <ContentTemplate>
        <script type="text/javascript">
            function populateAttributeKey() {
                // if the attribute key hasn't been filled in yet, populate it with the attribute name minus whitespace
                var keyValue = $('#<%=tbAttributeKey.ClientID%>').val();
                if (keyValue == '') {
                    var nameValue = $('#<%=tbAttributeName.ClientID%>').val();
                    nameValue = nameValue.replace(/\s+/g, '');
                    $('#<%=tbAttributeKey.ClientID%>').val(nameValue);
                }
            }
        </script>
        <asp:Panel ID="pnlList" runat="server">
            <Rock:ModalAlert ID="mdGridWarning" runat="server" />
            <Rock:Grid ID="gMarketingCampaignAdType" runat="server" AllowSorting="true" OnRowSelected="gMarketingCampaignAdType_Edit">
                <Columns>
                    <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                    <Rock:BoolField DataField="IsSystem" HeaderText="System" SortExpression="IsSystem" />
                    <Rock:DeleteField OnClick="gMarketingCampaignAdType_Delete" />
                </Columns>
            </Rock:Grid>
        </asp:Panel>

        <asp:Panel ID="pnlDetails" runat="server" Visible="false">

            <asp:HiddenField ID="hfMarketingCampaignAdTypeId" runat="server" />

            <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="alert alert-error" />

            <fieldset>
                <legend>
                    <i id="iconIsSystem" runat="server" class="icon-eye-open"></i>
                    <asp:Literal ID="lActionTitle" runat="server" />
                </legend>
                <div class="row-fluid">
                    <div class="span6">
                        <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.MarketingCampaignAdType, Rock" PropertyName="Name" />
                        <Rock:LabeledDropDownList ID="ddlDateRangeType" runat="server" LabelText="Date Range Type" />
                    </div>
                    <div class="span6">
                        <Rock:Grid ID="gMarketingCampaignAdAttributeTypes" runat="server" AllowPaging="false" DisplayType="Light">
                            <Columns>
                                <asp:BoundField DataField="Name" HeaderText="Attribute Types" />
                                <Rock:EditField OnClick="gMarketingCampaignAdAttributeType_Edit" />
                                <Rock:DeleteField OnClick="gMarketingCampaignAdAttributeType_Delete" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </div>
            </fieldset>

            <div class="actions">
                <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>

        </asp:Panel>

        <asp:Panel ID="pnlAdTypeAttribute" runat="server" Visible="false">
            <asp:HiddenField ID="hfAttributeGuid" runat="server" />
            <fieldset>
                <legend>
                    <asp:Literal ID="lAttributeActionTitle" runat="server"></asp:Literal>
                </legend>
                <div class="row-fluid">

                    <div class="span6">
                        <Rock:DataTextBox ID="tbAttributeName" runat="server" SourceTypeName="Rock.Model.Attribute, Rock" PropertyName="Name" onblur="populateAttributeKey()" />
                        <Rock:DataTextBox ID="tbAttributeKey" runat="server" SourceTypeName="Rock.Model.Attribute, Rock" PropertyName="Key" />
                        <Rock:DataTextBox ID="tbAttributeCategory" runat="server" SourceTypeName="Rock.Model.Attribute, Rock" PropertyName="Category" />
                        <Rock:DataTextBox ID="tbAttributeDescription" runat="server" SourceTypeName="Rock.Model.Attribute, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
                    </div>

                    <div class="span6">
                        <Rock:DataDropDownList ID="ddlAttributeFieldType" runat="server" LabelText="Field Type" SourceTypeName="Rock.Model.FieldType, Rock" PropertyName="Name" DataValueField="Id" DataTextField="Name" />
                        <asp:PlaceHolder ID="phAttributeFieldTypeQualifiers" runat="server"></asp:PlaceHolder>
                        <Rock:DataTextBox ID="tbAttributeDefaultValue" runat="server" SourceTypeName="Rock.Model.Attribute, Rock" PropertyName="DefaultValue" />
                        <Rock:LabeledCheckBox ID="cbAttributeMultiValue" runat="server" LabelText="Allow Multiple Values" />
                        <Rock:LabeledCheckBox ID="cbAttributeRequired" runat="server" LabelText="Required" />
                    </div>

                </div>
            </fieldset>

            <div class="actions">
                <asp:LinkButton ID="btnSaveAttribute" runat="server" Text="OK" CssClass="btn btn-primary" OnClick="btnSaveAttribute_Click" />
                <asp:LinkButton ID="btnCancelAttribute" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancelAttribute_Click" />
            </div>

        </asp:Panel>

        <Rock:NotificationBox ID="nbWarning" runat="server" Title="Warning" NotificationBoxType="Warning" Visible="false" />

    </ContentTemplate>
</asp:UpdatePanel>
