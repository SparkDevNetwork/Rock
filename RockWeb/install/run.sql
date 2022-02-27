DECLARE @CMSConfigurationPageGuid AS UNIQUEIDENTIFIER = 'b4a24ab7-9369-4055-883f-4f4892c39ae3'
DECLARE @PAGEGUID AS UNIQUEIDENTIFIER = 'c5207c19-8dc4-40d3-b8e1-c28de60febcf'
DECLARE @BLOCKGUID AS UNIQUEIDENTIFIER = '6d690cd1-a6c6-42d6-8dd9-c20859e97eaa'
DECLARE @HTMLCONTENTGUID AS UNIQUEIDENTIFIER = 'fe9bd229-ca98-45b3-a501-b4af8007848b'
DECLARE @CMSConfigurationPageId AS INT = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = @CMSConfigurationPageGuid)
DECLARE @HTMLCONTENT as VARCHAR(max) = '{% stylesheet %}
i>i {font-size:0px;}
{% endstylesheet%}

<div style="display:flex; flex-direction:row; justify-contents:space-around; align-items:flex-end;">
    <div class="col-md-8" >
        <h2><i class="fa fa-searchengin"></i> Looking for an Available Icon</h2>
        <p>Use the text box to filter icons by name and when you find one you like, click it to copy the i tag to your clipboard. You can select a size to add the relative size to the clicked icon.</p>
        <div style="display:flex; flex-direction:column;">
    <input id="filter" type="text" class="col-sm-12" style="font-size:1.2em; padding:10px;">
    
</div>
    </div>
<div style="display:flex; flex-grow:2;flex-direction:column; justify-content:space-around; align-items:baseline;">
    <div style="display:flex; flex-grow:2;flex-direction:row; justify-content:space-around; align-items:baseline; width:100%">

        <div><label for="fa-1x"><i class="fa fa-rockrms"></i></label>
        <input type="radio" id="fa-1x" name="size"  checked="checked"></div>
        
        <div><label for="fa-2x"><i class="fa fa-2x fa-rockrms"></i></label>
        <input type="radio" id="fa-2x" name="size"></div>
        
        <div><label for="fa-3x"><i class="fa fa-3x fa-rockrms"></i></label>
        <input type="radio" id="fa-3x" name="size"></div>
        
        <div><label for="fa-4x"><i class="fa fa-4x fa-rockrms"></i></label>
        <input type="radio" id="fa-4x" name="size"></div>
    
        <div><label for="fa-5x"><i class="fa fa-5x fa-rockrms"></i>
        <input type="radio" id="fa-5x" name="size" ></div>
    </div>
</div>

</div>
<hr>
<div id="icons" style="display:flex; flex-direction:row; flex-wrap: wrap; justify-content:space-around;"></div>


<script>
var my_url; 
$(''head'').find(''link'').each(function(el){
    var sheet = $(this).attr(''href'')
    var n = sheet.indexOf("bootstrap.css");
    if(n >0 ){
        my_url = sheet;
    }

});
var count = 0;
fetch(my_url)
  .then(response => response.text())
  .then(text => {
    var res = text.split(".fa");
    res.forEach(function(icon){
      var test = icon.indexOf(":before")
      if(test > 0) {

        var n = icon.indexOf(":before");
        icon = icon.substring(0, n != -1 ? n : icon.length);
        
        var icon = icon.replace(''%2C'',''''); 
        if (icon.charAt(0) == ''-''){
        count++;
            var cleanicon = icon.replace(''%2C'','''')
            var cleanicon = ''<i style="margin:20px; cursor:pointer;" class="icon fa fa-5x fa'' +  cleanicon + ''" data-title="fa''+cleanicon+''" data-toggle="tooltip" title="fa''+cleanicon+''"  onclick="copy(this)" style="cursor:pointer;"><i class="fa fa'' +  cleanicon + ''"></i></i>''
        
        $(''#icons'').append(cleanicon);
        }
      }
    })
  });
  
 
function insert(str, index, value) {
    return str.substr(0, index) + value + str.substr(index);
}

function copy(el){
var size = document.querySelector(''input[name = "size"]:checked'').id;
var item = el.innerHTML;
var index = item.indexOf("fa") + 2;
item = insert(item, index, '' ''+ size);

  var copycontent = document.createElement("input");

  copycontent.setAttribute("value", item);

  document.body.appendChild(copycontent);

  copycontent.select();

  document.execCommand("copy");

  document.body.removeChild(copycontent);
}


$("#filter").keyup(function () {
    var filterText = $("#filter").val().toLowerCase();
     $(".icon").each(function () {
        var id = $(this);
        id.toggle(id.attr(''title'').toLowerCase().indexOf(filterText) > -1)
    });
    
    
});
</script>'

Insert Into Page (
    [InternalName],
    [ParentPageId],
    [PageTitle],
    [IsSystem],
    [LayoutId],
    [RequiresEncryption],
    [EnableViewState],
    [PageDisplayTitle],
    [PageDisplayBreadCrumb],
    [PageDisplayIcon],
    [PageDisplayDescription],
    [DisplayInNavWhen],
    [MenuDisplayDescription],
    [MenuDisplayIcon],
    [MenuDisplayChildPages],
    [BreadCrumbDisplayName],
    [BreadCrumbDisplayIcon],
    [Order],
    [OutputCacheDuration],
    [IconCssClass],
    [IncludeAdminFooter],
    [Guid],
    [BrowserTitle],
    [CreatedDateTime],
    [ModifiedDateTime],
    [AllowIndexing]
)


VALUES (
    'Available Font Awesome Fonts',
    @CMSConfigurationPageId,
    'Available Font Awesome Fonts',
    0,
    12,
    0,
    1,
    1,
    1,
    1,
    1,
    0,
    0,
    0,
    1,
    1,
    0,
    20,
    0,
    'fa fa-searchengin',
    1,
    @PAGEGUID,
    'Available Font Awesome Fonts',
    GETDATE(),
    GETDATE (),
    0
)

declare @newpageid as Int = (Select SCOPE_IDENTITY())

Insert Into Block (
    [IsSystem],
    [PageId],
    [BlockTypeId],
    [Zone],
    [Order],
    [Name],
    [OutputCacheDuration],
    [Guid],
    [CreatedDateTime],
    [ModifiedDateTime]
    )
VALUES (
    0,
    @newpageid,
    6,
    'Main',
    0,
    'Font Awesome Icons',
    0,
    @BLOCKGUID,
    GetDate(),
    GetDate()
    )

declare @newblock as Int = (Select SCOPE_IDENTITY())

Insert Into HTMLContent (
    [BlockId],
    [Version],
    [Content],
    [IsApproved],
    [ApprovedDateTime],
    [Guid],
    [CreatedDateTime],
    [ModifiedDateTime]
)
VALUES (
    @newblock,
    1,
    @HTMLCONTENT,
    1,
    GetDate(),
    @HTMLCONTENTGUID,
    GetDate(),
    GetDate()
);