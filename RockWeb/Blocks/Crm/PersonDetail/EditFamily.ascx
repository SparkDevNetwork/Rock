<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EditFamily.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.EditFamily" %>

<asp:UpdatePanel ID="upEditFamily" runat="server">
    <ContentTemplate>

        <asp:ValidationSummary ID="valSummaryTop" runat="server"
            HeaderText="Please Correct the Following" CssClass="alert alert-error block-message error" />

        <Rock:NotificationBox ID="nbNotice" runat="server" Visible="false" />

        <div class="row-fluid">
            <div class="span4 form-horizontal">
                <fieldset>
                    <Rock:RockTextBox ID="tbFamilyName" runat="server" Label="Family Name" Required="true" CssClass="input-meduim" AutoPostBack="true" OnTextChanged="tbFamilyName_TextChanged" />
                </fieldset>
            </div>
            <div class="span4 form-horizontal">
                <fieldset>
                    <Rock:CampusPicker ID="cpCampus" runat="server" Required="true" AutoPostBack="true" OnSelectedIndexChanged="cpCampus_SelectedIndexChanged" />
                </fieldset>
            </div>
            <div class="span4 form-horizontal">
                <fieldset>
                    <Rock:RockDropDownList ID="ddlRecordStatus" runat="server" Label="Record Status" AutoPostBack="true" OnSelectedIndexChanged="ddlRecordStatus_SelectedIndexChanged" /><br />
                    <Rock:RockDropDownList ID="ddlReason" runat="server" Label="Reason" Visible="false" AutoPostBack="true" OnSelectedIndexChanged="ddlReason_SelectedIndexChanged"></Rock:RockDropDownList>
                </fieldset>

            </div>
        </div>

        <div class="persondetails-familybar">
            <div class="members">
                <ul class="clearfix">
                    <asp:ListView ID="lvMembers" runat="server">
                        <ItemTemplate>
                            <li>
                                <a href='<%# basePersonUrl + Eval("Id") %>'>
                                    <asp:Image ID="imgPerson" runat="server" />
                                    <div class="member">
                                        <h4><%# Eval("FirstName") %> <%# Eval("LastName") %></h4>
                                    </div>
                                </a>
                                <br />
                                <div>
                                    <asp:RadioButtonList ID="rblRole" runat="server" DataValueField="Id" DataTextField="Name" /></div>
                                <asp:LinkButton ID="lbNewFamily" runat="server" CssClass="btn btn-mini" CommandName="Move"><i class="icon-external-link"></i> Move to New Family</asp:LinkButton>
                                <asp:LinkButton ID="lbRemoveMember" runat="server" Visible="false" CssClass="btn btn-mini" CommandName="Remove"><i class="icon-remove"></i> Remove from Family</asp:LinkButton>
                            </li>
                        </ItemTemplate>
                    </asp:ListView>
                </ul>
            </div>
        </div>

        <p>
            <asp:LinkButton ID="lbAddPerson" runat="server" CssClass="btn" OnClick="lbAddPerson_Click"><i class="icon-user"></i> Add Person</asp:LinkButton>
        </p>

        <h4>Addresses</h4>
        <p>
            <asp:LinkButton ID="lbMoved" runat="server" CssClass="btn" OnClick="lbMoved_Click"><i class="icon-truck icon-flip-horizontal"></i> Family Moved</asp:LinkButton>
        </p>

        <Rock:Grid ID="gLocations" runat="server" AllowSorting="true" AllowPaging="false" DisplayType="Light">
            <Columns>
                <asp:TemplateField HeaderText="Type">
                    <ItemTemplate>
                        <%# Eval("LocationTypeName") %>
                    </ItemTemplate>
                    <EditItemTemplate>
                        <asp:DropDownList ID="ddlLocType" runat="server" CssClass="input-small" />
                    </EditItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Street">
                    <ItemTemplate>
                        <%# Eval("Street1") %><br />
                        <%# Eval("Street2") %>
                    </ItemTemplate>
                    <EditItemTemplate>
                        <asp:TextBox ID="tbStreet1" runat="server" Text='<%# Eval("Street1") %>' /><br />
                        <asp:TextBox ID="tbStreet2" runat="server" Text='<%# Eval("Street2") %>' />
                    </EditItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="City">
                    <ItemTemplate>
                        <%# Eval("City") %>
                    </ItemTemplate>
                    <EditItemTemplate>
                        <asp:TextBox ID="tbCity" runat="server" Text='<%# Eval("City") %>' />
                    </EditItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="State">
                    <ItemTemplate>
                        <%# Eval("State") %>
                    </ItemTemplate>
                    <EditItemTemplate>
                        <Rock:StateDropDownList ID="ddlState" runat="server" UseAbbreviation="true" CssClass="input-mini" />
                    </EditItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Zip">
                    <ItemTemplate>
                        <%# Eval("Zip") %>
                    </ItemTemplate>
                    <EditItemTemplate>
                        <asp:TextBox ID="tbZip" runat="server" Text='<%# Eval("Zip") %>' CssClass="input-small" />
                    </EditItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Mailing" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center">
                    <ItemTemplate>
                        <%# ((bool)Eval("IsMailing")) ? "<i class=\"icon-ok\"></i>" : "" %>
                    </ItemTemplate>
                    <EditItemTemplate>
                        <asp:CheckBox ID="cbMailing" runat="server" Checked='<%# Eval("IsMailing") %>' />
                    </EditItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Location" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center">
                    <ItemTemplate>
                        <%# ((bool)Eval("IsLocation")) ? "<i class=\"icon-ok\"></i>" : "" %>
                    </ItemTemplate>
                    <EditItemTemplate>
                        <asp:CheckBox ID="cbLocation" runat="server" Checked='<%# Eval("IsLocation") %>' />
                    </EditItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField ItemStyle-HorizontalAlign="Center" HeaderStyle-CssClass="span1" ItemStyle-Wrap="false">
                    <ItemTemplate>
                        <asp:LinkButton ID="lbEdit" runat="server" Text="Edit" CommandName="Edit" CssClass="btn btn-mini"><i class="icon-edit"></i></asp:LinkButton>
                    </ItemTemplate>
                    <EditItemTemplate>
                        <asp:LinkButton ID="lbSave" runat="server" Text="Save" CommandName="Update" CssClass="btn btn-mini btn-success"><i class="icon-check"></i></asp:LinkButton>
                        <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CommandName="Cancel" CssClass="btn btn-mini btn-warning" CausesValidation="false"><i class="icon-check-minus"></i></asp:LinkButton>
                    </EditItemTemplate>
                </asp:TemplateField>
                <Rock:DeleteField OnClick="gLocation_RowDelete" />
            </Columns>
        </Rock:Grid>

        <div class="actions">
            <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
            <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
        </div>

        <Rock:ConfirmPageUnload ID="confirmExit" runat="server" ConfirmationMessage="Changes have been made to this family that have not yet been saved." Enabled="false" />

        <Rock:ModalDialog ID="modalAddPerson" runat="server" Title="Add Person" Content-Height="380">
            <Content>

                <asp:HiddenField ID="hfActiveTab" runat="server" Value="Existing" />

                <ul class="nav nav-pills">
                    <li id="liExistingPerson" runat="server" class="active"><a href='#<%=divExistingPerson.ClientID%>' data-toggle="pill">Add Existing Person</a></li>
                    <li id="liNewPerson" runat="server"><a href='#<%=divNewPerson.ClientID%>' data-toggle="pill">Add New Person</a></li>
                </ul>

                <asp:ValidationSummary ID="valSummaryAddPerson" runat="server" ValidationGroup="modalAddPersonValidationGroup"
                    HeaderText="Please Correct the Following" CssClass="alert alert-error block-message error" />

                <div class="tab-content">

                    <div id="divExistingPerson" runat="server" class="tab-pane active">
                        <fieldset>
                            <Rock:PersonPicker2 ID="ppExistingPerson" runat="server" />
                            <Rock:RockCheckBox ID="cbRemoveOtherFamilies" runat="server" Checked="true" Text="Remove person from other families" />
                        </fieldset>
                    </div>

                    <div id="divNewPerson" runat="server" class="tab-pane">
                        <div class="row-fluid">
                            <div class="span4">
                                <fieldset>
                                    <Rock:RockTextBox ID="tbNewPersonFirstName" runat="server" Label="First Name" ValidationGroup="modalAddPersonValidationGroup" />
                                </fieldset>
                            </div>
                            <div class="span4">
                                <fieldset>
                                    <Rock:RockTextBox ID="tbNewPersonLastName" runat="server" Label="Last Name" ValidationGroup="modalAddPersonValidationGroup" />
                                </fieldset>
                            </div>
                        </div>
                        <div class="row-fluid">
                            <div class="span4">
                                <fieldset>
                                    <Rock:RockDropDownList ID="ddlNewPersonGender" runat="server" Label="Gender" />
                                </fieldset>
                            </div>
                            <div class="span4">
                                <fieldset>
                                    <Rock:DatePicker ID="dpNewPersonBirthDate" runat="server" Label="Birthdate" />
                                </fieldset>
                            </div>
                        </div>
                        <div class="row-fluid">
                            <div class="span4">
                                <fieldset>
                                    <Rock:RockRadioButtonList ID="rblNewPersonRole" runat="server" DataTextField="Name" DataValueField="Id" RepeatDirection="Horizontal" Label="Role" />
                                </fieldset>
                            </div>
                        </div>
                    </div>
                </div>

                <script>
                    Sys.Application.add_load(function () {
                        
                        $find('<%=modalAddPerson.ClientID%>').add_shown(function () {
                            enableRequiredField('<%=tbNewPersonFirstName.ClientID%>_rfv', false);
                            enableRequiredField('<%=tbNewPersonLastName.ClientID%>_rfv', false);
                        });

                        $('a[data-toggle="pill"]').on('shown', function (e) {
                            var tabHref = $(e.target).attr("href");
                            if (tabHref == '#<%=divExistingPerson.ClientID%>') {
                                $('#<%=hfActiveTab.ClientID%>').val('Existing');
                                enableRequiredField('<%=tbNewPersonFirstName.ClientID%>_rfv', false);
                                enableRequiredField('<%=tbNewPersonLastName.ClientID%>_rfv', false);
                            } else {
                                $('#<%=hfActiveTab.ClientID%>').val('New');
                                enableRequiredField('<%=tbNewPersonFirstName.ClientID%>_rfv', true);
                                enableRequiredField('<%=tbNewPersonLastName.ClientID%>_rfv', true);
                            }

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



