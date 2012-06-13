<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Attributes.ascx.cs" Inherits="RockWeb.Blocks.Administration.Attributes" %>

<script type="text/javascript">

    var AttributeEntity = '<%= entity %>';
    var attribute = null;
        
    function editAttribute( attributeId ) {

        attribute = null;

        $('#<%= tbKey.ClientID %>').val('');
        $('#<%= tbName.ClientID %>').val('');
        $('#<%= tbCategory.ClientID %>').val('');
        $('#<%= tbDescription.ClientID %>').val('');
        $('#<%= ddlFieldType.ClientID %>').val('');
        $('#<%= tbDefaultValue.ClientID %>').val('');
        $('#<%= cbRequired.ClientID %>').removeAttr('checked');

        if (attributeId != 0) {

            $.ajax({
                type: 'GET',
                contentType: 'application/json',
                dataType: 'json',
                url: rock.baseUrl + 'REST/Core/Attribute/' + attributeId,
                success: function (getData, status, xhr) {

                    attribute = getData;

                    $('#<%= tbKey.ClientID %>').val(attribute.Key);
                    $('#<%= tbName.ClientID %>').val(attribute.Name);
                    $('#<%= tbCategory.ClientID %>').val(attribute.Category);
                    $('#<%= tbDescription.ClientID %>').val(attribute.Description);
                    $('#<%= ddlFieldType.ClientID %>').val(attribute.FieldTypeId);
                    $('#<%= tbDefaultValue.ClientID %>').val(attribute.DefaultValue);
                    if (attribute.Required)
                        $('#<%= cbRequired.ClientID %>').attr('checked', 'checked');

                    $('#modal-details').modal('show').bind('shown', function () {
                        $('#modal-details').appendTo('#<%= upSettings.ClientID %>');
                    });

                },
                error: function (xhr, status, error) {
                    alert(status + ' [' + error + ']: ' + xhr.responseText);
                }
            });
        }
        else {

            var category = $('#<%= ddlCategoryFilter.ClientID %>').val();
            category = category == '[All]' ? '' : category;
            $('#<%= tbCategory.ClientID %>').val(category);

            $('#modal-details').modal('show').bind('shown', function () {
                $('#modal-details').appendTo('#<%= upSettings.ClientID %>');
            });
        
        }

        return false;
    }

    Sys.Application.add_load(function () {

        $('#modal-details a.btn.primary').click(function () {

            if (Page_ClientValidate()) {

                var restAction = 'PUT';
                var restUrl = rock.baseUrl + 'REST/Core/Attribute/';

                if (attribute === null) {
                    restAction = 'POST';
                    attribute = new Object();
                    attribute.Id = 0;
                    attribute.System = false;
                    attribute.FieldTypeId = 0;
                    attribute.Entity = '<%= entity %>';
                    attribute.EntityQualifierColumn = '<%= entityQualifierColumn %>';
                    attribute.EntityQualifierValue = '<%= entityQualifierValue %>';
                    attribute.Order = 0;
                    attribute.GridColumn = false;
                }
                else {
                    restUrl += attribute.Id;
                }

                attribute.Key = $('#<%= tbKey.ClientID %>').val();
                attribute.Name = $('#<%= tbName.ClientID %>').val();
                attribute.Category = $('#<%= tbCategory.ClientID %>').val();
                attribute.Description = $('#<%= tbDescription.ClientID %>').val();
                attribute.FieldTypeId = $('#<%= ddlFieldType.ClientID %>').val();
                attribute.DefaultValue = $('#<%= tbDefaultValue.ClientID %>').val();
                attribute.Required = $('#<%= cbRequired.ClientID %>').is(':checked');

                $.ajax({
                    type: restAction,
                    contentType: 'application/json',
                    dataType: 'json',
                    data: JSON.stringify(attribute),
                    url: restUrl,
                    success: function (data, status, xhr) {

                        if (AttributeEntity == '') {
                            $.ajax({
                                type: 'PUT',
                                contentType: 'application/json',
                                dataType: 'json',
                                url: rock.baseUrl + 'REST/Core/Attribute/FlushGlobal',
                                error: function (xhr, status, error) {
                                    alert(status + ' [' + error + ']: ' + xhr.responseText);
                                }
                            });
                        }

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
                <legend>Filter Options</legend>
                <Rock:LabeledDropDownList ID="ddlCategoryFilter" runat="server" LabelText="Category" AutoPostBack="true" OnSelectedIndexChanged="ddlCategoryFilter_SelectedIndexChanged" />
            </fieldset>
        </div>

        <Rock:Grid ID="rGrid" runat="server" >
            <Columns>
                <asp:BoundField DataField="Id" HeaderText="Id" />
                <asp:BoundField DataField="Category" HeaderText="Category"  />
                <asp:BoundField DataField="Key" HeaderText="Key" />
                <asp:BoundField DataField="Name" HeaderText="Name" />
                <Rock:BoolField DataField="Required" HeaderText="Required"/>
                <asp:TemplateField>
                    <ItemStyle HorizontalAlign="Center" CssClass="grid-icon-cell edit"/>
                    <ItemTemplate>
                        <a href="#" onclick="editAttribute(<%# Eval("Id") %>);">Edit</a>
                    </ItemTemplate>
                </asp:TemplateField>
                <Rock:DeleteField OnClick="rGrid_Delete" />
            </Columns>
        </Rock:Grid>

        <asp:Button ID="btnRefresh" runat="server" Text="Save" style="display:none" onclick="btnRefresh_Click" />

    </asp:PlaceHolder>

    <div id="modal-details" class="modal hide fade">
        <div class="modal-header">
            <a href="#" class="close">&times;</a>
            <h3>Attribute</h3>
        </div>
        <div class="modal-body">
            <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert-message block-message error"/>
            <fieldset>
                <Rock:DataTextBox ID="tbKey" runat="server" SourceTypeName="Rock.Core.Attribute, Rock" PropertyName="Key" />
                <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Core.Attribute, Rock" PropertyName="Name" />
                <Rock:DataTextBox ID="tbCategory" runat="server" SourceTypeName="Rock.Core.Attribute, Rock" PropertyName="Category" />
                <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Core.Attribute, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
                <Rock:FieldTypeList ID="ddlFieldType" runat="server" SourceTypeName="Rock.Core.Attribute, Rock" PropertyName="FieldTypeId" LabelText="Field Type" />
                <Rock:DataTextBox ID="tbDefaultValue" runat="server" SourceTypeName="Rock.Core.Attribute, Rock" PropertyName="DefaultValue" />
                <Rock:LabeledCheckBox ID="cbRequired" runat="server" LabelText="Required" />
            </fieldset>
        </div>
        <div class="modal-footer">
            <a href="#" class="btn secondary">Cancel</a>
            <a href="#" class="btn primary">Save</a>
        </div>
    </div>

</ContentTemplate>
</asp:UpdatePanel>
