﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SystemInfo.ascx.cs" Inherits="RockWeb.Blocks.Administration.SystemInfo" %>

<script language="javascript">

    $(document).ready(function () {

        $('#show-cache-objects').click(function () {
            $('#cache-objects').toggle('slow', function () {
                if ($('#modal-scroll-container').length) {
                    $('#modal-scroll-container').tinyscrollbar_update('relative');
                }
            });
        });

        $('a.show-pill').click(function () {
            $('ul.nav-pills > li').attr('class', '');
            $(this).parent().attr('class', 'active');
            $('div.tabContent > div').hide('slow');
            $('#' + $(this).attr('pill')).show('slow');
        });
    });

</script>

<ul class="nav nav-pills" >
    <li class='active'><a pill="version-info" class="show-pill" href="#">Version Info</a></li>
    <li><a pill="memory-cache" class="show-pill" href="#">Memory Cache</a></li>
</ul>

<div class="tabContent" >

    <div id="version-info">
        Rock Version Info will eventually go here!
    </div>

    <div id="memory-cache" style="display:none">
        
        <div id="cache-details">
            <asp:Literal ID="lCacheOverview" runat="server"></asp:Literal>
        </div>
        
        <a id="show-cache-objects" href="#">Show Cache Objects</a>
        
        <div id="cache-objects" style="display:none">
            <asp:Literal ID="lCacheObjects" runat="server"></asp:Literal>
        </div>

    </div>

</div>

