$request = [System.Net.WebRequest]::Create('http://rock.newspring.cc')
$response = $request.GetResponse()

$status = [int]$response.StatusCode
if ($status -eq 200) { 
    Write-Host "Site is loaded." 
}
else {
    Write-Host "Couldn't load the site."
}

$response.Close()