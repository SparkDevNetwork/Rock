<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AttributeValues.ascx.cs" Inherits="RockWeb.Blocks.Administration.AttributeValues" %>

<script type="text/javascript">

    var AttributeId = null;
    var AttributeValueId = null;
    var AttributeValue = null;
        
    function editValue( id, valueId, value ) {

        AttributeId = id;
        AttributeValueId = valueId;

        $('#<%= tbValue.ClientID %>').val(value);

        $('#modal-details').modal('show').bind('shown', function () {
            $('#modal-details').appendTo('#<%= upSettings.ClientID %>');
        });
        
        return false;
    }

    Sys.Application.add_load(function () {

        $('#modal-details a.btn.primary').click(function () {

            if (Page_ClientValidate()) {

                var restAction = 'PUT';
                var restUrl = rock.baseUrl + 'REST/Core/AttributeValue/';

                attributeValue = new Object();
                attributeValue.Id = AttributeValueId;
                attributeValue.AttributeId = AttributeId;
                attributeValue.EntityId = <%= entityId %>;
                attributeValue.Value = $('#<%= tbValue.ClientID %>').val();

                if (attributeValue.Id === 0)
                    restAction = 'POST';
                else
                    restUrl += attributeValue.Id;

                $.ajax({
                    type: restAction,
                    contentType: 'application/json',
                    dataType: 'json',
                    data: JSON.stringify(attributeValue),
                    url: restUrl,
                    success: function (data, status, xhr) {

                        $('#modal-details').modal('hide');
                        $('#<%= btnRefresh.ClientID %>').click();

                    },
                    error: function (xhr, status, error) {

                        alert(status + ' [' + error + ']: ' + xhr.responseText);

                    }
                });
            }
        });

        $('#modal-details a.btn.secondary').click(function () {
            $('#modal-details').modal('hide');
        });

        $('#modal-details').modal({
            backdrop: true,
            keyboard: true
        });

    });

</script>

<asp:UpdatePanel ID="upSettings" runat="server">
<ContentTemplate>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert-message block-massage error"/>

    <asp:PlaceHolder ID="phContent" runat="server">

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
                <asp:TemplateField>
                    <ItemStyle HorizontalAlign="Center" CssClass="grid-icon-cell edit"/>
                    <ItemTemplate>
                        <a id="aEdit" runat="server">Edit</a>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </Rock:Grid>

        <asp:Button ID="btnRefresh" runat="server" Text="Save" style="display:none" onclick="btnRefresh_Click" />

    </asp:PlaceHolder>

    <div id="modal-details" class="modal hide fade">
        <div class="modal-header">
            <a href="#" class="close">&times;</a>
            <h3>Attribute Value</h3>
        </div>
        <div class="modal-body">
            <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert-message block-message error"/>
            <fieldset>
                <Rock:DataTextBox ID="tbValue" runat="server" SourceTypeName="Rock.Core.AttributeValue, Rock" PropertyName="Value" />
            </fieldset>
        </div>
        <div class="modal-footer">
            <a href="#" class="btn secondary">Cancel</a>
            <a href="#" class="btn primary">Save</a>
        </div>
    </div>

</ContentTemplate>
</asp:UpdatePanel>
