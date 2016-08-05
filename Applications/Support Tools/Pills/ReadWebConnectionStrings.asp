<!DOCTYPE html>
<html>
	<head>
		<title>Rock Diagnose - Rock Pill</title>
		<link rel='stylesheet' href='//fonts.googleapis.com/css?family=Open+Sans:300,400,600,700' type='text/css' />
		<link rel="stylesheet" href="//netdna.bootstrapcdn.com/bootstrap/3.0.0/css/bootstrap.min.css" />
		<link href="//netdna.bootstrapcdn.com/font-awesome/4.1.0/css/font-awesome.min.css" rel="stylesheet" />
		
		<script src="//code.jquery.com/jquery-1.9.0.min.js"></script>
		
		<link href="//rockrms.blob.core.windows.net/pills/Styles/rock-pills.css" rel="stylesheet" />
		<link rel="shortcut icon" href="//rockrms.blob.core.windows.net/pills/Images/favicon.ico" />

	</head>
	<body>
	
		<div id="content">
            <h1 id="logo"></h1>
	
			<h1>Rock Pill</h1>
			
			<div id="console">
				<ul>
					<%
						dsn = "Provider='sqloledb';" & ImportConnectionString("web.ConnectionStrings.config", "RockContext", false)
                        Response.Write( dsn )
					%>
				</ul>
			</div>
			
			
		</div>
	</body>
</html>

<%
Function ImportConnectionString(webConfig, attrName, reformatDSN)
    Dim oXML, oNode, oChild, oAttr, dsn
    Set oXML=Server.CreateObject("Microsoft.XMLDOM")
    oXML.Async = "false"
    oXML.Load(Server.MapPath(webConfig))
    Set oNode = oXML.GetElementsByTagName("connectionStrings").Item(0) 
    Set oChild = oNode.GetElementsByTagName("add")
    ' Get the first match
    For Each oAttr in oChild 
        If  oAttr.getAttribute("name") = attrName then
            dsn = oAttr.getAttribute("connectionString")
            If reformatDSN Then
                ' Optionally reformat the connection string (adjust as needed)
                dsn = Replace(dsn, "User ID=", "UID=")
                dsn = Replace(dsn, "Password=", "PWD=")
                dsn = Replace(dsn, "Data Source=", "Server=")
                dsn = Replace(dsn, "Initial Catalog=", "Database=")
                dsn = Replace(dsn, "Persist Security Info=True;", "")
                dsn = "Provider=MSDASQL;Driver={SQL Server};" & dsn
            End If
            ImportConnectionString = dsn
            Exit Function
        End If
    Next
End Function
%>