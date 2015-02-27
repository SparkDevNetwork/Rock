<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EditFamily.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.EditFamily" %>

<asp:UpdatePanel ID="upEditFamily" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbInvalidFamily" runat="server" Visible="false" />

        <div class="panel panel-block" id="pnlEditFamily" runat="server">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-users"></i> <asp:Literal ID="lBanner" runat="server" /></h1>
            </div>
            <div class="panel-body">

                <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                <div class="row">
                    <div class="col-md-4">
                        <fieldset>
                            <Rock:RockTextBox ID="tbFamilyName" runat="server" Label="Family Name" Required="true" CssClass="input-meduim" AutoPostBack="true" OnTextChanged="tbFamilyName_TextChanged" />
                        </fieldset>
                    </div>
                    <div class="col-md-4">
                        <fieldset>
                            <Rock:CampusPicker ID="cpCampus" runat="server" Required="true" AutoPostBack="true" OnSelectedIndexChanged="cpCampus_SelectedIndexChanged" />
                        </fieldset>
                    </div>
                    <div class="col-md-4">
                        <fieldset>
                            <Rock:RockDropDownList ID="ddlRecordStatus" runat="server" Label="Record Status" AutoPostBack="true" OnSelectedIndexChanged="ddlRecordStatus_SelectedIndexChanged" /><br />
                            <Rock:RockDropDownList ID="ddlReason" runat="server" Label="Reason" Visible="false" AutoPostBack="true" OnSelectedIndexChanged="ddlReason_SelectedIndexChanged"></Rock:RockDropDownList>
                        </fieldset>

                    </div>
                </div>

                <div class="panel panel-widget editfamily-list">
                    <div class="panel-heading clearfix">
                        <h3 class="panel-title pull-left">Family Members</h3>
                        <div class="pull-right">
                            <asp:LinkButton ID="lbAddPerson" runat="server" CssClass="btn btn-action btn-xs" OnClick="lbAddPerson_Click" CausesValidation="false"><i class="fa fa-user"></i> Add Person</asp:LinkButton>
                        </div>
                    </div>
                    <div class="panel-body">
                        <ul class="groupmembers">
                            <asp:ListView ID="lvMembers" runat="server">
                                <ItemTemplate>
                                    <li class="member">
                                        <div class="person-image" id="divPersonImage" runat="server"></div>
                                        <div class="member-information">
                                            <h4><%# Eval("NickName") %> <%# Eval("LastName") %></h4>
                                            
                                              <asp:RadioButtonList ID="rblRole" runat="server" DataValueField="Id" DataTextField="Name" />
                                            
                                        </div>
                                        <div class="actions">
                                            <asp:LinkButton ID="lbNewFamily" runat="server" CssClass="btn btn-default btn-move btn-xs" CommandName="Move"><i class="fa fa-external-link"></i> Move to New Family</asp:LinkButton>
                                            <asp:LinkButton ID="lbRemoveMember" runat="server" Visible="false" CssClass="btn btn-remove btn-xs" CommandName="Remove"><i class="fa fa-times"></i> Remove from Family</asp:LinkButton>
                                        </div>
                                    </li>
                                </ItemTemplate>
                            </asp:ListView>
                        </ul>
                    </div>
                </div>

                <div class="panel panel-widget">
                    <div class="panel-heading clearfix">
                        <h4 class="panel-title pull-left">Addresses</h4>
                        <div class="pull-right">
                            <asp:LinkButton ID="lbMoved" runat="server" CssClass="btn btn-action btn-xs" OnClick="lbMoved_Click" CausesValidation="false"><i class="fa fa-truck fa-flip-horizontal"></i> Family Moved</asp:LinkButton>
                        </div>
                    </div>

                    <div class="panel-body">
                
                        <div class="grid grid-panel">
                            <Rock:Grid ID="gLocations" runat="server" AllowSorting="true" AllowPaging="false" DisplayType="Light">
                                <Columns>
                                    <Rock:RockTemplateField HeaderText="Type">
                                        <ItemTemplate>
                                            <%# Eval("LocationTypeName") %>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <Rock:RockDropDownList ID="ddlLocType" runat="server" DataTextField="Value" DataValueField="Id" />
                                        </EditItemTemplate>
                                    </Rock:RockTemplateField>
                                    <Rock:RockTemplateField HeaderText="Address">
                                        <ItemTemplate>
                                            <%# Eval("FormattedAddress") %><br />
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <Rock:AddressControl ID="acAddress" runat="server" Required="true"/>
                                        </EditItemTemplate>
                                    </Rock:RockTemplateField>
                                    <Rock:RockTemplateField HeaderText="Mailing" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center">
                                        <ItemTemplate>
                                            <%# ((bool)Eval("IsMailing")) ? "<i class=\"fa fa-check\"></i>" : "" %>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:CheckBox ID="cbMailing" runat="server" Checked='<%# Eval("IsMailing") %>' />
                                        </EditItemTemplate>
                                    </Rock:RockTemplateField>
                                    <Rock:RockTemplateField HeaderText="Map Location" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center">
                                        <ItemTemplate>
                                            <%# ((bool)Eval("IsLocation")) ? "<i class=\"fa fa-check\"></i>" : "" %>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <%# ((bool)Eval("IsLocation")) ? "<i class=\"fa fa-check\"></i>" : "" %>
                                            <asp:CheckBox ID="cbLocation" runat="server" Checked='<%# Eval("IsLocation") %>' Visible='<%# !(bool)Eval("IsLocation") %>' />
                                        </EditItemTemplate>
                                    </Rock:RockTemplateField>
                                    <Rock:RockTemplateField ItemStyle-HorizontalAlign="Center" HeaderStyle-CssClass="span1" ItemStyle-CssClass="grid-columncommand" ItemStyle-Wrap="false">
                                        <ItemTemplate>
                                            <asp:LinkButton ID="lbEdit" runat="server" Text="Edit" CommandName="Edit" CssClass="btn btn-default btn-sm" CausesValidation="false"><i class="fa fa-pencil"></i></asp:LinkButton>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:LinkButton ID="lbSave" runat="server" Text="Save" CommandName="Update" CssClass="btn btn-sm btn-success"><i class="fa fa-check"></i></asp:LinkButton>
                                            <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CommandName="Cancel" CssClass="btn btn-sm btn-warning" CausesValidation="false"><i class="fa fa-minus"></i></asp:LinkButton>
                                        </EditItemTemplate>
                                    </Rock:RockTemplateField>
                                    <Rock:DeleteField OnClick="gLocation_RowDelete" />
                                </Columns>
                            </Rock:Grid>
                        </div>

                    </div>
                </div>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="btnDelete_Click" CausesValidation="false" />
                </div>

            </div>
        </div>
        
        <Rock:ConfirmPageUnload ID="confirmExit" runat="server" ConfirmationMessage="Changes have been made to this family that have not yet been saved." Enabled="false" />

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="modalAddPerson" runat="server" Title="Add Person" ValidationGroup="AddPerson" >
            <Content>

                <asp:HiddenField ID="hfActiveTab" runat="server" />

                <ul class="nav nav-pills">
                    <li id="liNewPerson" runat="server" class="active"><a href='#<%=divNewPerson.ClientID%>' data-toggle="pill">Add New Person</a></li>
                    <li id="liExistingPerson" runat="server"><a href='#<%=divExistingPerson.ClientID%>' data-toggle="pill">Add Existing Person</a></li>
                </ul>

                <asp:ValidationSummary ID="valSummaryAddPerson" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="AddPerson"/>

                <div class="tab-content">

                    <div id="divNewPerson" runat="server" class="tab-pane active">
                        <div class="row">
                            <div class="col-sm-4">
                                <Rock:RockDropDownList ID="ddlNewPersonTitle" runat="server" Label="Title" ValidationGroup="AddPerson" CssClass="input-width-md" />
                                <Rock:RockTextBox ID="tbNewPersonFirstName" runat="server" Label="First Name" ValidationGroup="AddPerson" />
                                <Rock:RockTextBox ID="tbNewPersonLastName" runat="server" Label="Last Name" ValidationGroup="AddPerson" />
                                <Rock:RockDropDownList ID="ddlNewPersonSuffix" runat="server" Label="Suffix" ValidationGroup="AddPerson" CssClass="input-width-md" />
                            </div>
                            <div class="col-sm-4">
                                <Rock:RockDropDownList ID="ddlNewPersonConnectionStatus" runat="server" Label="Connection Status" ValidationGroup="AddPerson"/>
                                <Rock:RockRadioButtonList ID="rblNewPersonRole" runat="server" DataTextField="Name" DataValueField="Id" RepeatDirection="Horizontal" Label="Role" ValidationGroup="AddPerson"/>
                                <Rock:RockRadioButtonList ID="rblNewPersonGender" runat="server" Required="true" Label="Gender" RepeatDirection="Horizontal" ValidationGroup="AddPerson"/>
                            </div>
                            <div class="col-sm-4">
                                <Rock:DatePicker ID="dpNewPersonBirthDate" runat="server" Label="Birthdate" ValidationGroup="AddPerson"/>
                                <Rock:GradePicker ID="ddlGradePicker" runat="server" Label="Grade" ValidationGroup="AddPerson" UseAbbreviation="true" UseGradeOffsetAsValue="true" />
                                <Rock:RockDropDownList ID="ddlNewPersonMaritalStatus" runat="server" DataTextField="Name" DataValueField="Id" RepeatDirection="Horizontal" Label="Marital Status"  ValidationGroup="AddPerson"/>
                            </div>
                        </div>

                    </div>

                    <div id="divExistingPerson" runat="server" class="tab-pane">
                        <fieldset>
                            <Rock:PersonPicker ID="ppPerson" runat="server" Label="Person" Required="true" ValidationGroup="AddPerson" />
                            <Rock:RockCheckBox ID="cbRemoveOtherFamilies" runat="server" Checked="true" Text="Remove person from other families" ValidationGroup="AddPerson"/>
                        </fieldset>
                    </div>

                </div>


                <script>
                    Sys.Application.add_load(function () {

                        $('#<%=modalAddPerson.ServerSaveLink.ClientID%>').on('click', function () {

                            // if Save was clicked, set the fields that should be validated based on what tab they are on
                            if ($('#<%=hfActiveTab.ClientID%>').val() == "Existing") {
                                enableRequiredField('<%=ppPerson.RequiredFieldValidator.ClientID%>', true)
                                enableRequiredField('<%=tbNewPersonFirstName.RequiredFieldValidator.ClientID%>', false);
                                enableRequiredField('<%=tbNewPersonLastName.RequiredFieldValidator.ClientID%>', false);
                                enableRequiredField('<%=rblNewPersonRole.RequiredFieldValidator.ClientID%>', false);
                                enableRequiredField('<%=rblNewPersonGender.RequiredFieldValidator.ClientID%>', false);
                                enableRequiredField('<%=ddlNewPersonConnectionStatus.RequiredFieldValidator.ClientID%>', false);
                            }
                            else {
                                enableRequiredField('<%=ppPerson.RequiredFieldValidator.ClientID%>', false)
                                enableRequiredField('<%=tbNewPersonFirstName.RequiredFieldValidator.ClientID%>', true);
                                enableRequiredField('<%=tbNewPersonLastName.RequiredFieldValidator.ClientID%>', true);
                                enableRequiredField('<%=rblNewPersonRole.RequiredFieldValidator.ClientID%>', true);
                                enableRequiredField('<%=rblNewPersonGender.RequiredFieldValidator.ClientID%>', true);
                                enableRequiredField('<%=ddlNewPersonConnectionStatus.RequiredFieldValidator.ClientID%>', true);
                            }

                            // update the scrollbar since our validation box could show
                            setTimeout(function () {
                                Rock.dialogs.updateModalScrollBar('<%=valSummaryAddPerson.ClientID%>');
                            });
                        })

                        $('a[data-toggle="pill"]').on('shown.bs.tab', function (e) {
                            var tabHref = $(e.target).attr("href");
                            if (tabHref == '#<%=divExistingPerson.ClientID%>') {
                                $('#<%=hfActiveTab.ClientID%>').val('Existing');
                            } else {
                                $('#<%=hfActiveTab.ClientID%>').val('New');
                            }

                            // if the validation error summary is shown, hide it when they switch tabs
                            $('#<%=valSummaryAddPerson.ClientID%>').hide();
                        });

                    })

                    function enableRequiredField(validatorId, enable) {
                        var jqObj = $('#' + validatorId);

                        if (jqObj != null) {
                            var domObj = jqObj.get(0);
                            if (domObj != null) {
                                ValidatorEnable(domObj, enable);
                            }
                        }
                    }

                </script>

            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>



