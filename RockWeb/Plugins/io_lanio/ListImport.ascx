<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ListImport.ascx.cs" Inherits="RockWeb.Plugins.io_Lanio.ListImport" %>

<asp:UpdatePanel ID="upnlUpload" runat="server">
    <Triggers>
       <asp:PostBackTrigger ControlID="btnUpload" />
    </Triggers>
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-upload" aria-hidden="true"></i> Upload File</h1>
            </div>
            <div class="panel-body">
              
                        <div class="row">
                            <div class="col-md-6">
                                <label>1. Choose a csv or xlsx file.</label><br>
                                <asp:FileUpload runat="server" ID="fuFile" />
                                <a href="#more" data-toggle="collapse" style="margin-left: 25px;">more info</a>
                                <div id="more" class="collapse" style="margin-left: 25px;">
                                    <p>The following fields can be imported<br />
                                        <ul>
                                            <li>FirstName</li>
                                            <li>LastName</li>
                                            <li>Email</li>
                                            <li>Phone<p><small>Phone may be in the following formats<br />123123123412<br />(123) 123-1234<br />123-123-1234 </small></p></li>
                                            <li>DOB<p><small>Date of birth may be in the following format<br />1/1/2000<br />2009/02/26<br />February 26, 2009<br />2002-02-10</small></p></li>
                                            <li>ForeignKey</li>
                                            <li>Graduation<<small>THe year that a perosn graduates. This will alsodetermin their grade.</small>/li>
                                            <li>Gender</li>
                                            <li>Id</li>
                                        </ul>
                                    </p>
                                </div>
                                <br />
                                <asp:Button runat="server" class="btn btn-primary" style="margin-top: 25px;" ID="btnUpload" OnClick="btnUpload_OnClick" Text="Next" />

                             </div>
                            <div class="col-md-6">
                                
                            </div>
                        </div>

                    </div>

        </div>

    </ContentTemplate>
</asp:UpdatePanel>


<asp:UpdatePanel ID="upnlSync" runat="server">
     <Triggers>
       <asp:PostBackTrigger ControlID="btnSync" />
    </Triggers>
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-random" aria-hidden="true"></i> Found Fields</h1>
            </div>
            <div class="panel-body">
                <div class="alert alert-info" role="alert">
                 <p> We were unsure how to import the file.</p><p>These are the fields that were found in the file. Tell us how the fields from your file match Rock by <strong>dragging your fields on the left into Rocks fields on the right.</strong> </p>
               </div>
                <div class="row">
                    <div class="col-md-3">
                            <%foreach (var s in colHeadings)
                                { %>
                                <div class="draggable btn btn-primary">
                                   <i class="fa fa-bars" aria-hidden="true"></i> <span><%=s%></span>
                                </div>
                                <br />
                            <%} %>
                    </div>

                    <div class="col-md-3">
                        <asp:TextBox runat="server" class="drop" ID="txtFirstName" placeholder="First Name*" ></asp:TextBox> <br>
                        <asp:TextBox runat="server" class="drop" ID="txtLastName" placeholder="Last Name*"></asp:TextBox>   <br>
                        <asp:TextBox runat="server" class="drop" ID="txtEmail" placeholder="Email"></asp:TextBox>          <br>
                        <asp:TextBox runat="server" class="drop" ID="txtPhone" placeholder="Phone"></asp:TextBox>           <br>
                        <asp:TextBox runat="server" class="drop" ID="txtDOB" placeholder="Date of Birth"></asp:TextBox>     <br>
                        <asp:TextBox runat="server" class="drop" ID="txtGender" placeholder="Gender"></asp:TextBox>         <br />
                        <asp:TextBox runat="server" class="drop" ID="txtGraduation" placeholder="Graduation Year"></asp:TextBox>         <br />
                        <asp:TextBox runat="server" class="drop" ID="txtForeignKey" placeholder="ForeignKey"></asp:TextBox> <br />
                        <asp:TextBox runat="server" class="drop" ID="txtId" placeholder="RockId" ></asp:TextBox>            <br>
                        
                        <asp:Button runat="server" ID="btnSync" Text="Next" OnClick="btnSync_OnClick" style="margin-top: 25px;" class="btn btn-primary" />
                    </div>
                 </div>
                <asp:HiddenField runat="server" ID="hdnFile" />
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>

<asp:UpdatePanel ID="upnlGroup" runat="server">
    <Triggers>
       <asp:PostBackTrigger ControlID="btnGroup" />
        <asp:PostBackTrigger ControlID="btnSkip" />
    </Triggers>
    <ContentTemplate>
        
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-upload" aria-hidden="true"></i> Group Settings</h1>
            </div>
            <div class="panel-body">
                        
                    <div class="row">
                        <div class="col-md-12">
                            <div class="alert alert-info" role="alert">
                                <p>Choose whether or not to import into a group</p>
                            </div>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <button class="btn btn-primary" type="button" data-toggle="collapse" data-target="#<%= chooseGroup.ClientID %>" aria-expanded="false" aria-controls="<%= chooseGroup.ClientID %>">
                                Choose a group
                            </button>
                            <div class="collapse" runat="server"  id="chooseGroup">
                                <div class="well">
                                    <div class="row">
                                        <div class="col-md-6">
                                            <Rock:GroupPicker ID="gpGroup" runat="server"  Label="Choose a group" OnSelectItem="gpGroup_SelectedIndexChanged"/>
                                            <div class="checkbox">
                                                <label data-toggle="collapse" data-target="#<%=newGroup.ClientID %>">
                                                    <asp:CheckBox runat="server" ID="chkNew" oncheckedchanged="chkNew_CheckedChanged" AutoPostBack="true"/> Create a new group. 
                                                </label>
                                            </div>
                                            <div id="newGroup" runat="server" class="collapse" >
                                                <small>This group will be placed under the selected group</small>
                                                <asp:TextBox runat="server"  ID="txtGroupName" placeholder="group name" ></asp:TextBox>
                                            </div>
                                            <br />
                                            
                                            <br />
                                            <!--<h1>Role:<%=grpTypeRole.GroupRoleId %> type:<%=grpTypeRole.GroupTypeId %></h1>-->

                                            <asp:Button runat="server" ID="btnGroup" OnClick="btnGroup_OnClick" Text="Upload" />
                                        </div>
                                        <div class="col-md-6">
                                            <Rock:GroupTypePicker ID="gtpType" runat="server" Lable="Group Type"  OnSelectedIndexChanged="gtpType_SelectedIndexChanged" AutoPostBack="true" />
                                            <Rock:GroupRolePicker ID="grpTypeRole" runat="server" GroupTypeID="0"  Lable="Group Role"  />
                                        </div>
                                    </div>
                                </div>
                            </div>
                                
                        </div>
                           
                        <div class="col-md-6">
                            <asp:Button runat="server" class="btn btn-primary" ID="btnSkip"  OnClick="btnSkip_OnClick"  Text="Skip"/>
                        </div>
                    </div>

            </div>

        </div>


    </ContentTemplate>
</asp:UpdatePanel>

<asp:UpdatePanel ID="upnlSuccess" runat="server">
   
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"<i class="fa fa-star" aria-hidden="true"></i> Finish</h1>
            </div>
            <asp:Panel runat="server" class="panel-body">
                Success!
            </asp:Panel>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>

<asp:UpdatePanel ID="upnlFailure" runat="server">
   
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-exclamation" aria-hidden="true"></i> Failure</h1>
            </div>
            <asp:Panel runat="server" class="panel-body">
            <asp:label runat="server" ID="lbError" ></asp:label>
                Oops there was a problem. The csv file must contain at least 
                <ul>
                    <li>First Name</li>
                    <li>Last Name</li>
                    <li>Email</li>
                </ul>
            </asp:Panel>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
<style>
    .draggable {
        z-index: 100;
        border: 1px solid rgb(106, 106, 106);
        padding: 4px 6px;
        border-radius: 4px;
        margin-bottom: 3px;
        cursor: grab;
    }
    .drop{
        padding: 7px;
    }
    #<%= newGroup.ClientID %> {
        margin-left: 35px;
    }    
    .checkbox{
        margin-top: 0px;
        display:inline-block;
    }
    .ui-state-hover{
        background-color: green;
    }
</style>
  
<link rel="stylesheet" href="https://code.jquery.com/ui/1.12.1/themes/base/jquery-ui.css">
<script src="https://code.jquery.com/ui/1.12.1/jquery-ui.js"></script>


<script>
   $(".draggable").draggable({
        revert: true
    });
    $(".drop").droppable({
        drop: function (event, ui) {
            $(this).val($.trim(ui.draggable.text()));
        },
        classes: {
            "ui-droppable-hover": "ui-state-hover"
        }
    })
 

    $(function () {
        var availableTags = [
            <% foreach(var s in groups){ %>
            <%= "\""+s+"\"," %>
            <% } %>
        ];
         
        $("#<%= txtGroupName.ClientID%>").autocomplete({
            source: availableTags
        });

        <% if(IsPostBack) {%>
        $("#<%= gpGroup.ClientID %>").addClass("in");
        <% } %>
    });
        
</script>
