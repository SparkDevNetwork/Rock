<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AccountDetail.ascx.cs" Inherits="RockWeb.Plugins.rocks_pillars.PCOSync.AccountDetail" %>

<asp:UpdatePanel ID="upnlSettings" runat="server">
    <ContentTemplate>
         
        <asp:HiddenField ID="hfAccountId" runat="server" />

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server" Visible="false">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-calendar-check-o"></i> <asp:Literal ID="lTitle" runat="server" /></h1>
            </div>
            <div class="panel-body">

                <fieldset id="fieldsetViewDetails" runat="server">
                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                    <asp:Literal ID="lblDetails" runat="server" />
                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" AccessKey="m" Text="Edit" CssClass="btn btn-primary" CausesValidation="false" OnClick="btnEdit_Click" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="btnDelete_Click" CausesValidation="false" />
                        <asp:LinkButton ID="btnImportPCO" runat="server" Text="Configure Import Groups" CssClass="pull-right btn btn-link" CausesValidation="false" OnClick="btnImportPCO_Click" />
                    </div>
                </fieldset>

                <div id="pnlEditDetails" runat="server">
                    <asp:ValidationSummary ID="vsDetails" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
					<div class="alert alert-info">
						If adding a new account, you can get the Application Id and Secret by creating a Personal Access Token for your Planning Center Account. You do this from your Planning Center Online account. 
						Visit <a href='https://api.planningcenteronline.com/oauth/applications' target="_blank">https://api.planningcenteronline.com/oauth/applications</a> to create a token.">
					</div>
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="rocks.pillars.PCOSync.Model.PCOAccount, rocks.pillars.PCOSync" PropertyName="Name" Required="true" />
                            <Rock:RockCheckBox ID="cbPermissionDowngrade" runat="server" Label="Allow Permission Downgrade" Text="Yes" 
                                Help="Should the PCO Sync downgrade a person's permission in PCO if they are moved to a group in Rock with less privileges (e.g. Editor group to Viewer group)?" />

                        </div>
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbApplicationId" runat="server" SourceTypeName="rocks.pillars.PCOSync.Model.PCOAccount, rocks.pillars.PCOSync" PropertyName="ApplicationId" Label="PCO Application Id" Required="true" AutoPostBack="true" OnTextChanged="tbToken_TextChanged" />
                            <Rock:DataTextBox ID="tbSecret" runat="server" SourceTypeName="rocks.pillars.PCOSync.Model.PCOAccount, rocks.pillars.PCOSync" PropertyName="Secret" Label="PCO Secret" Required="true" AutoPostBack="true" OnTextChanged="tbToken_TextChanged" />
                            <Rock:RockDropDownList ID="ddlWelcomeEmailTemplate" runat="server" Label="Welcome Email" DataValueField="id" DataTextField="subject" />
                        </div>
                    </div>

                    <Rock:PanelWidget ID="wpAdvanced" runat="server" Title="Advanced Settings">
                        <Rock:NotificationBox ID="nbFields" runat="server" NotificationBoxType="Info" 
                            Text="Select how Rock's First and Nick name fields should be synced with Planning Center, and what optional fields should be included in the sync (First Name, Nick Name, Last Name, and Email are always synced)." />
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockRadioButtonList ID="rblNameMapping" runat="server" Label="Name Field Mapping">
                                    <asp:ListItem Text="Rock First Name -> PCO Given Name<br/>Rock Nick Name -> PCO First Name" Value="False" />
                                    <asp:ListItem Text="Rock First Name -> PCO First Name<br/>Rock Nick Name -> PCO Nick Name" Value="True" />
                                </Rock:RockRadioButtonList>
                            </div>
                            <div class="col-md-6">
                                <Rock:RockCheckBoxList ID="cblAddlFields" runat="server" Label="Additional Fields To Sync">
                                    <asp:ListItem Value="middle_name" Text="Middle Name" Selected="True" />
                                    <asp:ListItem Value="gender" Text="Gender" Selected="True" />
                                    <asp:ListItem Value="birthdate" Text="Birth Date" Selected="True" />
                                    <asp:ListItem Value="anniversary" Text="Anniversary" Selected="True" />
                                    <asp:ListItem Value="home_address" Text="Home Address" Selected="True" />
                                    <asp:ListItem Value="home_phone" Text="Home Phone" Selected="True" />
                                    <asp:ListItem Value="work_phone" Text="Work Phone" Selected="True" />
                                    <asp:ListItem Value="mobile_phone" Text="Mobile Phone" Selected="True"  />
                                    <asp:ListItem Value="photo_date" Text="Photo (Only Syncs from Rock to PCO)" Selected="True" />
                                </Rock:RockCheckBoxList>
                            </div>
                        </div>
                    </Rock:PanelWidget>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>
                </div>

                <asp:Panel ID="pnlAdd" runat="server" Visible="false" >

                    <Rock:NotificationBox ID="nbOverview" runat="server" NotificationBoxType="Info">
                        In order to import existing people from Planning Center, a group needs to be configured for each of the five permission levels. Once the groups have been added and configured, they can be selected here.
                        After selecting the groups, and saving your selection, the 'PCO Import' job can be run to import people from Planning Center and have them added to these groups.
                    </Rock:NotificationBox>

                    <Rock:NotificationBox ID="nbAdd" runat="server" Visible="false" NotificationBoxType="Success" />

                    <div class="row">
                        <asp:PlaceHolder ID="phAddGroups" runat="server" />
                    </div>

                    <div class="actions">
                        <asp:LinkButton id="btnSaveGroups" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSaveGroups_Click" />
                        <asp:LinkButton ID="btnCancelImport" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancelImport_Click" />
                    </div>

                </asp:Panel>
 
            </div>
                 
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
