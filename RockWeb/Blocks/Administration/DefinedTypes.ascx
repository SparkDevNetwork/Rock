<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DefinedTypes.ascx.cs" Inherits="RockWeb.Blocks.Administration.DefinedTypes" %>

<script type="text/javascript">

    var type = null;
    var attribute = null;
    var value = null;
    
    function editType(clickedTypeId) {
    
        type = null;
                
        $('#<%= tbTypeName.ClientID %>').val('');
        $('#<%= tbTypeCategory.ClientID %>').val('');
        $('#<%= tbTypeDescription.ClientID %>').val('');
        $('#<%= ddlTypeFieldType.ClientID %>').val('');
        $('#<%= hfTypeId.ClientID %>').val(clickedTypeId);

        if (clickedTypeId != 0) {

            $.ajax({
                type: 'GET',
                contentType: 'application/json',
                dataType: 'json',
                url: rock.baseUrl + 'REST/Core/DefinedType/' + clickedTypeId,
                success: function (getData, status, xhr) {

                    type = getData;
                       
                    $('#<%= tbTypeName.ClientID %>').val( type.Name );
                    $('#<%= tbTypeCategory.ClientID %>').val( type.Category );
                    $('#<%= tbTypeDescription.ClientID %>').val( type.Description );
                    $('#<%= ddlTypeFieldType.ClientID %>').val( type.FieldTypeId );
                    $('#modal-types').modal('show').bind('shown', function () {
                        $('#modal-types').appendTo('#<%= upTypes.ClientID %>');
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
            $('#<%= tbTypeCategory.ClientID %>').val(category);
                        
            $('#modal-types').modal('show').bind('shown', function () {
                $('#modal-types').appendTo('#<%= upTypes.ClientID %>');
            });
        
        }

        return false;
    }

    function editValue(clickedValueId) {

        value = null;

        $('#<%= tbValueName.ClientID %>').val('');
        $('#<%= tbValueDescription.ClientID %>').val('');

        if (clickedValueId != 0) {

            $.ajax({
                type: 'GET',
                contentType: 'application/json',
                dataType: 'json',
                url: rock.baseUrl + 'REST/Core/DefinedValue/' + clickedValueId,
                success: function (getData, status, xhr) {

                    value = getData;

                    $('#<%= tbValueName.ClientID %>').val(value.Name);
                    $('#<%= tbValueDescription.ClientID %>').val(value.Description);
                    $('#modal-values').modal('show').bind('shown', function () {
                        $('#modal-values').appendTo('#<%= upValues.ClientID %>');
                    });
                },
                error: function (xhr, status, error) {
                    alert(status + ' [' + error + ']: ' + xhr.responseText);
                }
            });
        }
        else {

            $('#modal-values').modal('show').bind('shown', function () {
                $('#modal-values').appendTo('#<%= upValues.ClientID %>');
            });

        }

        return false;
    }

    function editAttribute(attributeId) {

        attribute = null;        

        $('#<%= tbAttributeKey.ClientID %>').val('');
        $('#<%= tbAttributeName.ClientID %>').val('');
        $('#<%= tbAttributeCategory.ClientID %>').val('');
        $('#<%= tbAttributeDescription.ClientID %>').val('');
        $('#<%= ddlAttributeFieldType.ClientID %>').val('');
        $('#<%= tbAttributeDefaultValue.ClientID %>').val('');
        $('#<%= cbAttributeGridColumn.ClientID %>').removeAttr('checked');
        $('#<%= cbAttributeRequired.ClientID %>').removeAttr('checked');

        if ( attributeId != 0 ) {

            $.ajax({
                type: 'GET',
                contentType: 'application/json',
                dataType: 'json',
                url: rock.baseUrl + 'REST/Core/Attribute/' + attributeId,
                success: function ( getData, status, xhr ) {
                    
                    attribute = getData;

                    $('#<%= tbAttributeKey.ClientID %>').val( attribute.Key );
                    $('#<%= tbAttributeName.ClientID %>').val( attribute.Name );
                    $('#<%= tbAttributeCategory.ClientID %>').val( attribute.Category );
                    $('#<%= tbAttributeDescription.ClientID %>').val( attribute.Description );
                    $('#<%= ddlAttributeFieldType.ClientID %>').val( attribute.FieldTypeId );
                    $('#<%= tbAttributeDefaultValue.ClientID %>').val( attribute.DefaultValue );
                    if (attribute.GridColumn)
                        $('#<%= cbAttributeGridColumn.ClientID %>').attr('checked', 'checked');
                    if ( attribute.Required )
                        $('#<%= cbAttributeRequired.ClientID %>').attr('checked', 'checked');
                    
                    $('#modal-attributes').modal('show').bind('shown', function () {
                        $('#modal-attributes').appendTo('#<%= upValues.ClientID %>');
                        $('#<%= upTypes.ClientID %>')
                    });
                },
                error: function (xhr, status, error) {
                    alert(status + ' [' + error + ']: ' + xhr.responseText);
                }
            });
        }

        else {

            $('#modal-attributes').modal('show').bind('shown', function () {
                $('#modal-attributes').appendTo('#<%= upAttributes.ClientID %>');
            });

        }

        return false;
    }

    Sys.Application.add_load(function () {

        $('a.btn.primary').click(function () {

            if (Page_ClientValidate()) {

                var restAction = 'PUT';
                var restUrl = null;
                var object = null;
                var parent = null;

                parent = $(this).parents('.modal');

                switch (parent.attr('Id')) {
                    case 'modal-types':

                        object = (type === null) ? new Object() : type;
                        restUrl = rock.baseUrl + 'REST/Core/DefinedType/';
                        object.Name = $('#<%= tbTypeName.ClientID %>').val();
                        object.Category = $('#<%= tbTypeCategory.ClientID %>').val();
                        object.Description = $('#<%= tbTypeDescription.ClientID %>').val();
                        object.FieldTypeId = $('#<%= ddlTypeFieldType.ClientID %>').val();

                        break;
                    case 'modal-values':

                        object = (value === null) ? new Object() : value;
                        restUrl = rock.baseUrl + 'REST/Core/DefinedValue/';
                        object.Name = $('#<%= tbValueName.ClientID %>').val();
                        object.Description = $('#<%= tbValueDescription.ClientID %>').val();
                        object.DefinedTypeId = $('#<%= hfTypeId.ClientID %>').val();
                        
                        break;
                    case 'modal-attributes':

                        object = (attribute === null) ? new Object() : attribute;
                        restUrl = rock.baseUrl + 'REST/Core/Attribute/';
                        object.Entity = '<%= entityTypeId %>';
                        object.EntityQualifierColumn = '<%= entityQualifierColumn %>';
                        object.Key = $('#<%= tbAttributeKey.ClientID %>').val();
                        object.Name = $('#<%= tbAttributeName.ClientID %>').val();
                        object.Category = $('#<%= tbAttributeCategory.ClientID %>').val();
                        object.Description = $('#<%= tbAttributeDescription.ClientID %>').val();
                        object.FieldTypeId = $('#<%= ddlAttributeFieldType.ClientID %>').val();
                        object.EntityQualifierValue = $('#<%= hfTypeId.ClientID %>').val();
                        object.DefaultValue = $('#<%= tbAttributeDefaultValue.ClientID %>').val();
                        object.GridColumn = $('#<%= cbAttributeGridColumn.ClientID %>').is(':checked');
                        object.Required = $('#<%= cbAttributeRequired.ClientID %>').is(':checked');

                        break;
                }

                if (object.Id === undefined) {
                    restAction = 'POST';
                    object.Id = 0;
                    object.System = false;
                    object.Order = 0;
                } else {
                    restUrl += object.Id;
                }

                $.ajax({
                    type: restAction,
                    contentType: 'application/json',
                    dataType: 'json',
                    data: JSON.stringify(object),
                    url: restUrl,
                    success: function (data, status, xhr) {
                        parent.modal('hide');
                        $('#<%= btnRefresh.ClientID %>').click();
                    },
                    error: function (xhr, status, error) {
                        alert(status + ' [' + error + ']: ' + xhr.responseText);
                    }
                });
            }
        });

        $('#modal-types, #modal-values, #modal-attributes').modal({
            backdrop: true,
            keyboard: true
        });

        $('a.btn.secondary').click(function () {
            $(this).parents('.modal').modal('hide');
        });

    });

</script>

<asp:UpdatePanel ID="upSettings" runat="server">
<ContentTemplate>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert-message block-message error"/>
    
    <asp:Panel ID="pnlContent" runat="server">

        <div class="grid-filter">
            <fieldset>
                <legend>Filter Options</legend>
                <Rock:LabeledDropDownList ID="ddlCategoryFilter" runat="server" LabelText="Category" AutoPostBack="true" OnSelectedIndexChanged="ddlCategoryFilter_SelectedIndexChanged" />
            </fieldset>
        </div>

        <Rock:Grid ID="rGridType" runat="server" ShowHeader="true" EmptyDataText="No Types Found" RowItemText="setting">
            <Columns>
                <asp:BoundField DataField="Id" HeaderText="Id" />
                <asp:TemplateField HeaderText="Name" showHeader="true" ItemStyle-Width="35%">
                    <ItemTemplate>
                        <asp:LinkButton runat="server" Text='<%#Eval("Name") %>' OnCommand="typeValues_Edit" CommandArgument='<%#Eval("ID")%>'/>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="Category" HeaderText="Category"  />
                <Rock:EditField OnClick="typeAttributes_Edit" />
                <asp:TemplateField>
                    <ItemStyle HorizontalAlign="Center" CssClass="grid-icon-cell tick"/>
                    <ItemTemplate>
                        <a href="#" onclick="editType(<%# Eval("Id") %>);">Edit</a>
                    </ItemTemplate>
                </asp:TemplateField>                
                <Rock:DeleteField OnClick="rGridType_Delete" />
            </Columns>
        </Rock:Grid>          
    </asp:Panel>

    <asp:Panel ID="pnlValues" runat="server" Visible="false">
    
        <asp:ValidationSummary runat="server" CssClass="failureNotification"/>

        <h3>Defined Values</h3>
                
        <div class="row">

        <Rock:Grid ID="rGridValue" runat="server" ShowHeader="true" EmptyDataText="No Default Values Found" >
            <Columns>
                <asp:BoundField DataField="Id" HeaderText="Id" />
                <asp:BoundField DataField="Name" HeaderText="Name" />
                <asp:BoundField DataField="Description" HeaderText="Description" />
                <asp:TemplateField>
                    <ItemStyle HorizontalAlign="Center" CssClass="grid-icon-cell edit"/>
                    <ItemTemplate>
                        <a href="#" onclick="editValue(<%# Eval("Id") %>);">Edit</a>
                    </ItemTemplate>
                </asp:TemplateField>
                <Rock:DeleteField OnClick="rGridValue_Delete" />
            </Columns>
        </Rock:Grid>

        </div>
        
        <asp:LinkButton id="btnValueClose" runat="server" Text="Done" CssClass="btn close" CausesValidation="false" OnClick="btnValueClose_Click" />
    </asp:Panel>

    <asp:Panel ID="pnlAttributes" runat="server" Visible="false">
  
        <asp:ValidationSummary runat="server" CssClass="failureNotification"/>

        <div class="row">

        <Rock:Grid ID="rGridAttribute" runat="server" ShowHeader="true" EmptyDataText="No Attributes Found">
            <Columns>
                <asp:BoundField DataField="Id" HeaderText="Id" />
                <asp:BoundField DataField="Category" HeaderText="Category"  />
                <asp:BoundField DataField="Name" HeaderText="Name" />
                <asp:BoundField DataField="Description" HeaderText="Description" />
                <Rock:BoolField DataField="GridColumn" HeaderText="Grid Column"/>
                <Rock:BoolField DataField="Required" HeaderText="Required"/>
                <asp:TemplateField>
                    <ItemStyle HorizontalAlign="Center" CssClass="grid-icon-cell edit"/>
                    <ItemTemplate>
                        <a href="#" onclick="editAttribute(<%# Eval("Id") %>);">Edit</a>
                    </ItemTemplate>
                </asp:TemplateField>
                <Rock:DeleteField OnClick="rGridAttribute_Delete" />
            </Columns>
        </Rock:Grid>

        </div>

        <asp:LinkButton id="btnAttributeClose" runat="server" Text="Done" CssClass="btn close" CausesValidation="false" OnClick="btnAttributeClose_Click" />
    </asp:Panel>

    <asp:HiddenField ID="hfTypeId" runat="server" />
    <asp:Button ID="btnRefresh" runat="server" Text="Save" style="display:none" onclick="btnRefresh_Click" />
    <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Error" Visible="false" />

</ContentTemplate>
</asp:UpdatePanel>

<asp:UpdatePanel ID="upTypes" runat="server">
<ContentTemplate>

    <div id="modal-types" class="modal hide fade">
        <div class="modal-header">
            <a href="#" class="close">&times;</a>
            <h3>Types</h3>
        </div>
        <div class="modal-body">
            <asp:ValidationSummary ID="valTypeSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert-message block-message error"/>   
            <fieldset>
               <Rock:DataTextBox ID="tbTypeName" runat="server" SourceTypeName="Rock.Core.DefinedType, Rock" PropertyName="Name" />
                <Rock:DataTextBox ID="tbTypeCategory" runat="server" SourceTypeName="Rock.Core.DefinedType, Rock" PropertyName="Category" />
                <Rock:DataTextBox ID="tbTypeDescription" runat="server" SourceTypeName="Rock.Core.DefinedType, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
                <Rock:FieldTypeList ID="ddlTypeFieldType" runat="server" SourceTypeName="Rock.Core.DefinedType, Rock" PropertyName="FieldType" />
            </fieldset>
        </div>
        <div class="modal-footer">
            <a href="#" class="btn">Cancel</a>
            <a href="#" class="btn btn-primary">Save</a>
        </div>
    </div>

</ContentTemplate>
</asp:UpdatePanel>

<asp:UpdatePanel ID="upValues" runat="server" >
<ContentTemplate>

    <div id="modal-values" class="modal hide fade">
        <div class="modal-header">
            <a href="#" class="close">&times;</a>
            <h3>Type Values</h3>
        </div>
        <div class="modal-body">
            <asp:ValidationSummary ID="valValueSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert-message block-message error"/>
            <fieldset>
                <Rock:DataTextBox ID="tbValueName" runat="server" SourceTypeName="Rock.Core.DefinedValue, Rock" PropertyName="Name" />
                <Rock:DataTextBox ID="tbValueDescription" runat="server" SourceTypeName="Rock.Core.DefinedValue, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
                <h4>Attribute Category</h4>
                <Rock:DataTextBox ID="tbValueGridColumn" runat="server" ReadOnly="true" SourceTypeName="Rock.Core.DefinedValue, Rock" PropertyName="Attributes" LabelText="Grid Attributes"/>
            </fieldset>
        </div>
        <div class="modal-footer">
            <a href="#" class="btn">Cancel</a>
            <a href="#" class="btn btn-primary">Save</a>
        </div>
    </div>

</ContentTemplate>
</asp:UpdatePanel>

<asp:UpdatePanel ID="upAttributes" runat="server" >
<ContentTemplate>

    <div id="modal-attributes" class="modal hide fade">
        <div class="modal-header">
            <a href="#" class="close">&times;</a>
            <h3>Attribute Values</h3>
        </div>
        <div class="modal-body">
            <asp:ValidationSummary ID="valAttributeSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert-message block-message error"/>
            <fieldset>
                <Rock:DataTextBox ID="tbAttributeKey" runat="server" SourceTypeName="Rock.Core.Attribute, Rock" PropertyName="Key" />
                <Rock:DataTextBox ID="tbAttributeName" runat="server" SourceTypeName="Rock.Core.Attribute, Rock" PropertyName="Name" />
                <Rock:DataTextBox ID="tbAttributeCategory" runat="server" SourceTypeName="Rock.Core.Attribute, Rock" PropertyName="Category" />
                <Rock:DataTextBox ID="tbAttributeDescription" runat="server" SourceTypeName="Rock.Core.Attribute, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
                <Rock:FieldTypeList ID="ddlAttributeFieldType" runat="server" SourceTypeName="Rock.Core.Attribute, Rock" PropertyName="FieldTypeId" LabelText="Field Type" />
                <Rock:DataTextBox ID="tbAttributeDefaultValue" runat="server" SourceTypeName="Rock.Core.Attribute, Rock" PropertyName="DefaultValue" />
                <Rock:LabeledCheckBox ID="cbAttributeGridColumn" runat="server" LabelText="Grid Column" />
                <Rock:LabeledCheckBox ID="cbAttributeRequired" runat="server" LabelText="Required" />
            </fieldset>
        </div>
        <div class="modal-footer">
            <a href="#" class="btn">Cancel</a>
            <a href="#" class="btn btn-primary">Save</a>
        </div>
    </div>

</ContentTemplate>
</asp:UpdatePanel>
