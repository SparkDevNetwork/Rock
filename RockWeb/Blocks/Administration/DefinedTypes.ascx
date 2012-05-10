<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DefinedTypes.ascx.cs" Inherits="RockWeb.Blocks.Administration.DefinedTypes" %>


<script type="text/javascript">

    var DefinedType = '<%= type %>';
    var type = null;

    var AttributeEntity = '<%= entity %>';
    var AttributeId = null;
    var AttributeValueId = null;
    var AttributeValue = null;
    var attribute = null;
    
    function editType( typeId ) {
        
        
        type = null;

        $('#<%= tbTypeName.ClientID %>').val('');
        $('#<%= tbTypeCategory.ClientID %>').val('');
        $('#<%= tbTypeDescription.ClientID %>').val('');
        
        if (typeId != 0) {

            $.ajax({
                type: 'GET',
                contentType: 'application/json',
                dataType: 'json',
                url: rock.baseUrl + 'REST/Core/DefinedType/' + typeId,
                success: function (getData, status, xhr) {

                    type = getData;

                    

                    $('#<%= tbTypeName.ClientID %>').val(type.Name);
                    $('#<%= tbTypeCategory.ClientID %>').val(type.Category);
                    $('#<%= tbTypeDescription.ClientID %>').val(type.Description);

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

            $('#<%= tbTypeCategory.ClientID %>').val(category);
            category = category == '[All]' ? '' : category;
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
                var restUrl = rock.baseUrl + 'REST/Core/DefinedType/';

                
                if (type === null) {
                    restAction = 'POST';
                    type = new Object();
                    type.Id = 0;
                    type.System = false;
                    type.FieldTypeId = 0;
                    type.Order = 0;
                    type.GridColumn = false;
                }
                else {
                    restUrl += type.Id;
                }

                type.Name = $('#<%= tbTypeName.ClientID %>').val();
                type.Category = $('#<%= tbTypeCategory.ClientID %>').val();
                type.Description = $('#<%= tbTypeDescription.ClientID %>').val();
                type.FieldTypeId = $('#<%= ddlTypeFieldType.ClientID %>').val();

                $.ajax({
                    type: restAction,
                    contentType: 'application/json',
                    dataType: 'json',
                    data: JSON.stringify(type),
                    url: restUrl,
                    success: function (data, status, xhr) {
                    
                        if (DefinedType == '')
                            $.ajax({
                                type: 'PUT',
                                contentType: 'application/json',
                                dataType: 'json',
                                url: rock.baseUrl + 'REST/Core/DefinedType/FlushGlobal',
                                error: function (xhr, status, error) {

                                    alert(status + ' [' + error + ']: ' + xhr.responseText);
                                }
                            });

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
                <asp:BoundField DataField="Name" HeaderText="Name" />
                <asp:BoundField DataField="Description" HeaderText="Description" />
                <asp:TemplateField>
                    <ItemStyle HorizontalAlign="Center" CssClass="grid-icon-cell tick"/>
                    <ItemTemplate>
                        <a href="#" onclick="editAttribute(<%# Eval("Id") %>);">Attributes</a>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField>
                    <ItemStyle HorizontalAlign="Center" CssClass="grid-icon-cell edit"/>
                    <ItemTemplate>
                        <a href="#" onclick="editType(<%# Eval("Id") %>);">Edit</a>
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
            <h3>Type</h3>
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
            <a href="#" class="btn secondary">Cancel</a>
            <a href="#" class="btn primary">Save</a>
        </div>
    </div>





</ContentTemplate>
</asp:UpdatePanel>
