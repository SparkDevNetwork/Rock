/* 
  Updates the InteractionDeviceType table with DeviceTypeData that isn't already in it
  This script can safely be run multiple times
 */

DECLARE @deviceTypeData TABLE (
    [Name] [nvarchar](250) NULL
    , [DeviceTypeData] [nvarchar](max) NULL
    , [ClientType] [nvarchar](25) NULL
    , [OperatingSystem] [nvarchar](100) NULL
    , [Application] [nvarchar](100) NULL
    , [Guid] [uniqueidentifier] NOT NULL
    )

INSERT INTO @deviceTypeData (
    [Name]
    , [DeviceTypeData]
    , [ClientType]
    , [OperatingSystem]
    , [Application]
    , [Guid]
    )
VALUES (
    'Windows 10 - Chrome 69.0.3497'
    , 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.100 Safari/537.36'
    , 'Desktop'
    , 'Windows 10'
    , 'Chrome 69.0.3497'
    , newid()
    )
    , (
    'Windows 7 - Chrome 51.0.2704'
    , 'Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36'
    , 'Desktop'
    , 'Windows 7'
    , 'Chrome 51.0.2704'
    , newid()
    )
    , (
    'Windows 7 - Chrome 52.0.2743'
    , 'Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.116 Safari/537.36'
    , 'Desktop'
    , 'Windows 7'
    , 'Chrome 52.0.2743'
    , newid()
    )
    , (
    'Mac OS X 10.11.6 - Safari 9.1.2'
    , 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_11_6) AppleWebKit/601.7.7 (KHTML, like Gecko) Version/9.1.2 Safari/601.7.7'
    , 'Desktop'
    , 'Mac OS X 10.11.6'
    , 'Safari 9.1.2'
    , newid()
    )
    , (
    'Windows 10 - Chrome 52.0.2743'
    , 'Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.116 Safari/537.36'
    , 'Desktop'
    , 'Windows 10'
    , 'Chrome 52.0.2743'
    , newid()
    )
    , (
    'Windows 10 - Chrome 51.0.2704'
    , 'Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36'
    , 'Desktop'
    , 'Windows 10'
    , 'Chrome 51.0.2704'
    , newid()
    )
    , (
    'Other - Other'
    , ''
    , 'None'
    , 'Other'
    , 'Other'
    , newid()
    )
    , (
    'Mac OS X 10.11 - Firefox 47.0'
    , 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10.11; rv:47.0) Gecko/20100101 Firefox/47.0'
    , 'Desktop'
    , 'Mac OS X 10.11'
    , 'Firefox 47.0'
    , newid()
    )
    , (
    'Windows 10 - Chrome 70.0.3538'
    , 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.110 Safari/537.36'
    , 'Desktop'
    , 'Windows 10'
    , 'Chrome 70.0.3538'
    , newid()
    )
    , (
    'Other - Other'
    , 'IIS Application Initialization Preload'
    , 'Desktop'
    , 'Other'
    , 'Other'
    , newid()
    )
    , (
    'Windows 10 - Chrome 71.0.3578'
    , 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36'
    , 'Desktop'
    , 'Windows 10'
    , 'Chrome 71.0.3578'
    , newid()
    )
    , (
    'Windows 7 - IE 8.0'
    , 'Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0)'
    , 'Desktop'
    , 'Windows 7'
    , 'IE 8.0'
    , newid()
    )
    , (
    'Windows - Firefox 31.0'
    , 'Mozilla/5.0(WindowsNT6.1;rv:31.0)Gecko/20100101Firefox/31.0'
    , 'Desktop'
    , 'Windows'
    , 'Firefox 31.0'
    , newid()
    )
    , (
    'Windows 7 - Chrome 41.0.2228'
    , 'Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36'
    , 'Desktop'
    , 'Windows 7'
    , 'Chrome 41.0.2228'
    , newid()
    )
    , (
    'Other - Python-urllib 2.6'
    , 'Python-urllib/2.6'
    , 'Desktop'
    , 'Other'
    , 'Python-urllib 2.6'
    , newid()
    )
    , (
    'Windows 7 - IE 9.0'
    , 'Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64)'
    , 'Desktop'
    , 'Windows 7'
    , 'IE 9.0'
    , newid()
    )
    , (
    'Other - Other'
    , 'HTTP Banner Detection (https://security.ipip.net)'
    , 'Mobile'
    , 'Other'
    , 'Other'
    , newid()
    )
    , (
    'Windows XP - IE 6.0'
    , 'Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1)'
    , 'Desktop'
    , 'Windows XP'
    , 'IE 6.0'
    , newid()
    )
    , (
    'Windows XP - Sogou Explorer 1.0'
    , 'Mozilla/5.0 (Windows NT 5.2) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36 SE 2.X MetaSr 1.0'
    , 'Desktop'
    , 'Windows XP'
    , 'Sogou Explorer 1.0'
    , newid()
    )
    , (
    'Linux - Firefox 52.0'
    , 'Mozilla/5.0 (X11; Linux x86_64; rv:52.0) Gecko/20100101 Firefox/52.0'
    , 'Desktop'
    , 'Linux'
    , 'Firefox 52.0'
    , newid()
    )
    , (
    'Windows 7 - Chrome 49.0.2623'
    , 'Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/49.0.2623.105 Safari/537.36'
    , 'Desktop'
    , 'Windows 7'
    , 'Chrome 49.0.2623'
    , newid()
    )
    , (
    'Windows 7 - Firefox 45.0'
    , 'Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:28.0) Gecko/20100101 Firefox/45.0'
    , 'Desktop'
    , 'Windows 7'
    , 'Firefox 45.0'
    , newid()
    )
    , (
    'Windows 8 - Chrome 59.0.3036'
    , 'Mozilla/5.0 (Windows NT 6.2;en-US) AppleWebKit/537.32.36 (KHTML, live Gecko) Chrome/59.0.3036.108 Safari/537.32'
    , 'Desktop'
    , 'Windows 8'
    , 'Chrome 59.0.3036'
    , newid()
    )
    , (
    'Ubuntu - Firefox 58.0'
    , 'Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:58.0) Gecko/20100101 Firefox/58.0'
    , 'Desktop'
    , 'Ubuntu'
    , 'Firefox 58.0'
    , newid()
    )
    , (
    'Ubuntu - Chromium 28.0.1500'
    , 'Mozilla/5.0 (X11; Linux i686) AppleWebKit/537.36 (KHTML, like Gecko) Ubuntu Chromium/28.0.1500.52 Chrome/28.0.1500.52 Safari/537.36'
    , 'Desktop'
    , 'Ubuntu'
    , 'Chromium 28.0.1500'
    , newid()
    )
    , (
    'Windows XP - IE 8.0'
    , 'Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 5.2; Trident/4.0)'
    , 'Desktop'
    , 'Windows XP'
    , 'IE 8.0'
    , newid()
    )
    , (
    'Windows Vista - IE 7.0'
    , 'Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.0)'
    , 'Desktop'
    , 'Windows Vista'
    , 'IE 7.0'
    , newid()
    )
    , (
    'Windows 10 - Chrome 66.0.3359'
    , 'Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.117 Safari/537.36'
    , 'Desktop'
    , 'Windows 10'
    , 'Chrome 66.0.3359'
    , newid()
    )
    , (
    'Windows 10 - Chrome 68.0.3440'
    , 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36'
    , 'Desktop'
    , 'Windows 10'
    , 'Chrome 68.0.3440'
    , newid()
    )
    , (
    'Windows 10 - Chrome 57.0.2987'
    , 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/57.0.2987.133 Safari/537.36'
    , 'Desktop'
    , 'Windows 10'
    , 'Chrome 57.0.2987'
    , newid()
    )
    , (
    'Mac OS X 10.11.5 - Chrome 50.0.2661'
    , 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_11_5) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.102 Safari/537.36'
    , 'Desktop'
    , 'Mac OS X 10.11.5'
    , 'Chrome 50.0.2661'
    , newid()
    )
    , (
    'Windows 8 - Chrome 58.0.3103'
    , 'Mozilla/5.0 (Windows NT 6.2;en-US) AppleWebKit/537.32.36 (KHTML, live Gecko) Chrome/58.0.3103.99 Safari/537.32'
    , 'Desktop'
    , 'Windows 8'
    , 'Chrome 58.0.3103'
    , newid()
    )
    , (
    'Ubuntu - Firefox 47.0'
    , 'Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:47.0) Gecko/20100101 Firefox/47.0'
    , 'Desktop'
    , 'Ubuntu'
    , 'Firefox 47.0'
    , newid()
    )
    , (
    'Ubuntu - Firefox 48.0'
    , 'Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:48.0) Gecko/20100101 Firefox/48.0'
    , 'Desktop'
    , 'Ubuntu'
    , 'Firefox 48.0'
    , newid()
    )
    , (
    'Windows 7 - Firefox 28.0'
    , 'Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:28.0) Gecko/20100101 Firefox/28.0'
    , 'Desktop'
    , 'Windows 7'
    , 'Firefox 28.0'
    , newid()
    )
    , (
    'Linux - Chrome 34.0.1847'
    , 'Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/34.0.1847.137 Safari/537.36'
    , 'Desktop'
    , 'Linux'
    , 'Chrome 34.0.1847'
    , newid()
    )
    , (
    'Windows 7 - Chrome 55.0.2883'
    , 'Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36'
    , 'Desktop'
    , 'Windows 7'
    , 'Chrome 55.0.2883'
    , newid()
    )
    , (
    'Windows 10 - Chrome 89.0.4389'
    , 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.90 Safari/537.36'
    , 'Desktop'
    , 'Windows 10'
    , 'Chrome 89.0.4389'
    , newid()
    )
    , (
    'Windows 10 - Other'
    , 'Microsoft Office/16.0 (Windows NT 10.0; Microsoft Outlook 16.0.13801; Pro)'
    , 'Desktop'
    , 'Windows 10'
    , 'Other'
    , newid()
    )
    , (
    'Windows 10 - Edge 95.0.1020'
    , 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/95.0.4638.54 Safari/537.36 Edg/95.0.1020.30'
    , 'Desktop'
    , 'Windows 10'
    , 'Edge 95.0.1020'
    , newid()
    )
    , (
    'Windows 10 - Chrome 95.0.4638'
    , 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/95.0.4638.69 Safari/537.36'
    , 'Desktop'
    , 'Windows 10'
    , 'Chrome 95.0.4638'
    , newid()
    )
    , (
    'Windows 10 - Chrome 60.0.3112'
    , 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.113 Safari/537.36'
    , 'Desktop'
    , 'Windows 10'
    , 'Chrome 60.0.3112'
    , newid()
    )
    , (
    'Linux - Other'
    , 'Linux Gnu (cow)'
    , 'Desktop'
    , 'Linux'
    , 'Other'
    , newid()
    )
    , (
    'Windows 10 - Chrome 102.0.0'
    , 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/102.0.0.0 Safari/537.36'
    , 'Desktop'
    , 'Windows 10'
    , 'Chrome 102.0.0'
    , newid()
    )
    , (
    'Other - Python Requests 2.28'
    , 'python-requests/2.28.0'
    , 'Desktop'
    , 'Other'
    , 'Python Requests 2.28'
    , newid()
    )
    , (
    'Windows 7 - Chrome 74.0.3729'
    , 'Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko)Chrome/74.0.3729.169 Safari/537.36'
    , 'Desktop'
    , 'Windows 7'
    , 'Chrome 74.0.3729'
    , newid()
    )
    , (
    'Linux - Chrome 98.0.4758'
    , 'Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36'
    , 'Desktop'
    , 'Linux'
    , 'Chrome 98.0.4758'
    , newid()
    )
    , (
    'iOS 2.2.1 - Mobile Safari 3.1.1'
    , 'Mozilla/5.0 (iPod; U; CPU iPhone OS 2_2_1 like Mac OS X; en-us) AppleWebKit/525.18.1 (KHTML, like Gecko) Version/3.1.1 Mobile/5H11a Safari/525.20'
    , 'Mobile'
    , 'iOS 2.2.1'
    , 'Mobile Safari 3.1.1'
    , newid()
    )
    , (
    'Windows 8 - Chrome 55.0.2883'
    , 'Mozilla/5.0 (Windows NT 6.2; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36'
    , 'Desktop'
    , 'Windows 8'
    , 'Chrome 55.0.2883'
    , newid()
    )
    , (
    'Android 7.0 - Chrome Mobile 69.0.3497'
    , 'Mozilla/5.0 (Linux; Android 7.0; TRT-LX3 Build/HUAWEITRT-LX3) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.100 Mobile Safari/537.36'
    , 'Mobile'
    , 'Android 7.0'
    , 'Chrome Mobile 69.0.3497'
    , newid()
    )
    , (
    'Windows 10 - Chrome 78.0.3904'
    , 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3904.108 Safari/537.36'
    , 'Desktop'
    , 'Windows 10'
    , 'Chrome 78.0.3904'
    , newid()
    )
    , (
    'Mac OS X - Safari'
    , 'Mozilla/5.0 (Macintosh; U; PPC Mac OS X; en) AppleWebKit/312.5.2 (KHTML, like Gecko) Safari/312.3.3'
    , 'Desktop'
    , 'Mac OS X'
    , 'Safari'
    , newid()
    )
    , (
    'Linux - Chrome 11.0.696'
    , 'Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/534.24 (KHTML, like Gecko) Chrome/11.0.696.3 Safari/534.24'
    , 'Desktop'
    , 'Linux'
    , 'Chrome 11.0.696'
    , newid()
    )
    , (
    'Mac OS X 10.11.6 - Safari 11.0'
    , 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_11_6) AppleWebKit/604.1.38 (KHTML, like Gecko) Version/11.0 Safari/604.1.38'
    , 'Desktop'
    , 'Mac OS X 10.11.6'
    , 'Safari 11.0'
    , newid()
    )
    , (
    'Mac OS X 10.15.7 - Chrome 102.0.0'
    , 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/102.0.0.0 Safari/537.36'
    , 'Desktop'
    , 'Mac OS X 10.15.7'
    , 'Chrome 102.0.0'
    , newid()
    )
    , (
    'Linux - Chrome 86.0.4'
    , 'Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4 240.111 Safari/537.36'
    , 'Desktop'
    , 'Linux'
    , 'Chrome 86.0.4'
    , newid()
    )
    , (
    'Windows 7 - Chrome 50.0.2661'
    , 'Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.102 Safari/537.36'
    , 'Desktop'
    , 'Windows 7'
    , 'Chrome 50.0.2661'
    , newid()
    )
    , (
    'Windows 8.1 - Firefox 43.0'
    , 'Mozilla/5.0 (Windows NT 6.3; WOW64; rv:43.0) Gecko/20100101 Firefox/43.0'
    , 'Desktop'
    , 'Windows 8.1'
    , 'Firefox 43.0'
    , newid()
    )
    , (
    'Windows 10 - Chrome 92.0.4515'
    , 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.131 Safari/537.36'
    , 'Desktop'
    , 'Windows 10'
    , 'Chrome 92.0.4515'
    , newid()
    )
    , (
    'Mac OS X 10.14.5 - Chrome 74.0.3729'
    , 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_14_5) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36'
    , 'Desktop'
    , 'Mac OS X 10.14.5'
    , 'Chrome 74.0.3729'
    , newid()
    )
    , (
    'Windows 10 - Firefox 45.0'
    , 'Mozilla/5.0 (Windows NT 10.0; WOW64; rv:45.0) Gecko/20100101 Firefox/45.0'
    , 'Desktop'
    , 'Windows 10'
    , 'Firefox 45.0'
    , newid()
    )
    , (
    'Mac OS X 10.11 - Chrome 47.0.2526'
    , 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_11) AppleWebKit/601.1.27 (KHTML, like Gecko) Chrome/47.0.2526.106 Safari/601.1.27'
    , 'Desktop'
    , 'Mac OS X 10.11'
    , 'Chrome 47.0.2526'
    , newid()
    )
    , (
    'Android 9.0 - Android 9.0'
    , 'Dalvik/2.1.0 (Linux; U; Android 9.0; ZTE BA520 Build/MRA58K)'
    , 'Tablet'
    , 'Android 9.0'
    , 'Android 9.0'
    , newid()
    )
    , (
    'Windows XP - Opera 9.01'
    , 'Opera/9.01 (Windows NT 5.1)'
    , 'Desktop'
    , 'Windows XP'
    , 'Opera 9.01'
    , newid()
    )
    , (
    'Android 4.4.2 - UC Browser 11.0.5'
    , 'Mozilla/5.0 (Linux; U; Android 4.4.2; en-US; HM NOTE 1W Build/KOT49H) AppleWebKit/534.30 (KHTML, like Gecko) Version/4.0 UCBrowser/11.0.5.850 U3/0.8.0 Mobile Safari/534.30'
    , 'Mobile'
    , 'Android 4.4.2'
    , 'UC Browser 11.0.5'
    , newid()
    )
    , (
    'Windows 10 - Firefox 101.0'
    , 'Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:101.0) Gecko/20100101 Firefox/101.0'
    , 'Desktop'
    , 'Windows 10'
    , 'Firefox 101.0'
    , newid()
    )
    , (
    'Other - Go-http-client 1.1'
    , 'Go-http-client/1.1'
    , 'Desktop'
    , 'Other'
    , 'Go-http-client 1.1'
    , newid()
    )
    , (
    'Android 6.0 - Chrome Mobile 52.0.2429'
    , 'Mozilla/5.0 (Linux; Android 6.0; HTC One M9 Build/MRA177068) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2429.98 Mobile Safari/537.3'
    , 'Mobile'
    , 'Android 6.0'
    , 'Chrome Mobile 52.0.2429'
    , newid()
    )
    , (
    'Windows 10 - Chrome 76.0.3809'
    , 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/76.0.3809.71 Safari/537.36'
    , 'Desktop'
    , 'Windows 10'
    , 'Chrome 76.0.3809'
    , newid()
    )
    , (
    'iOS 15.1.1 - Mobile Safari 15.1'
    , 'Mozilla/5.0 (iPhone; CPU iPhone OS 15_1_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/15.1 Mobile/15E148 Safari/604.1'
    , 'Mobile'
    , 'iOS 15.1.1'
    , 'Mobile Safari 15.1'
    , newid()
    )
    , (
    'Linux - Firefox 10.0'
    , 'Mozilla/5.0 (X11; Linux i686; rv:10.0) Gecko/20100101 Firefox/10.0'
    , 'Desktop'
    , 'Linux'
    , 'Firefox 10.0'
    , newid()
    )
    , (
    'Windows 10 - Chrome 102.0.5005'
    , 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/102.0.5005.63 Safari/537.36'
    , 'Desktop'
    , 'Windows 10'
    , 'Chrome 102.0.5005'
    , newid()
    )
    , (
    'Windows 7 - Firefox 59.0'
    , 'Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:59.0) Gecko/20100101 Firefox/59.0'
    , 'Desktop'
    , 'Windows 7'
    , 'Firefox 59.0'
    , newid()
    )
    , (
    'Windows 10 - Edge 102.0.1245'
    , 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/102.0.5005.124 Safari/537.36 Edg/102.0.1245.41'
    , 'Desktop'
    , 'Windows 10'
    , 'Edge 102.0.1245'
    , newid()
    )
    , (
    'Android 2.2 - Android 2.2'
    , 'Mozilla/5.0 (Linux; U; Android 2.2; ja-jp; SC-02B Build/FROYO) AppleWebKit/533.1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1'
    , 'Mobile'
    , 'Android 2.2'
    , 'Android 2.2'
    , newid()
    )
    , (
    'Mac OS X 10.15.7 - Safari 15.4'
    , 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/15.4 Safari/605.1.15'
    , 'Desktop'
    , 'Mac OS X 10.15.7'
    , 'Safari 15.4'
    , newid()
    )
    , (
    'Linux - Chrome 81.0.4044'
    , 'Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/81.0.4044.129 Safari/537.36'
    , 'Desktop'
    , 'Linux'
    , 'Chrome 81.0.4044'
    , newid()
    )
    , (
    'Windows XP - Chrome 60.0.3112'
    , 'Mozilla/5.0 (Windows NT 5.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.90 Safari/537.36'
    , 'Desktop'
    , 'Windows XP'
    , 'Chrome 60.0.3112'
    , newid()
    )
    , (
    'Windows 10 - Chrome 101.0.4951'
    , 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/101.0.4951.67 Safari/537.36'
    , 'Desktop'
    , 'Windows 10'
    , 'Chrome 101.0.4951'
    , newid()
    )
    , (
    'Windows 7 - Other'
    , 'Mozilla/5.0 (Windows NT 6.1; WOW64) SkypeUriPreview Preview/0.5 skype-url-preview@microsoft.com'
    , 'Desktop'
    , 'Windows 7'
    , 'Other'
    , newid()
    )
    , (
    'Windows 10 - Edge 91.0.864'
    , 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.114 Safari/537.36 Edg/91.0.864.54'
    , 'Desktop'
    , 'Windows 10'
    , 'Edge 91.0.864'
    , newid()
    )
    , (
    'Mac OS X 10.12.1 - Chrome 54.0.2840'
    , 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_12_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.98 Safari/537.36'
    , 'Desktop'
    , 'Mac OS X 10.12.1'
    , 'Chrome 54.0.2840'
    , newid()
    )
    , (
    'Mac OS X 10.13.6 - Chrome 102.0.0'
    , 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_13_6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/102.0.0.0 Safari/537.36'
    , 'Desktop'
    , 'Mac OS X 10.13.6'
    , 'Chrome 102.0.0'
    , newid()
    )
    , (
    'Ubuntu - Firefox 76.0'
    , 'Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:76.0) Gecko/20100101 Firefox/76.0'
    , 'Desktop'
    , 'Ubuntu'
    , 'Firefox 76.0'
    , newid()
    )
    , (
    'Linux - Chrome 93.0.4577'
    , 'Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/93.0.4577.63 Safari/537.36'
    , 'Desktop'
    , 'Linux'
    , 'Chrome 93.0.4577'
    , newid()
    )
    , (
    'Windows 10 - Chrome 83.0.4103'
    , 'Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.116 Safari/537.36'
    , 'Desktop'
    , 'Windows 10'
    , 'Chrome 83.0.4103'
    , newid()
    )
    , (
    'Windows 10 - Chrome 90.0.4430'
    , 'Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.72 Safari/537.36'
    , 'Desktop'
    , 'Windows 10'
    , 'Chrome 90.0.4430'
    , newid()
    )
    , (
    'Android 6.0 - Chrome Mobile WebView 69.0.3497'
    , 'Mozilla/5.0 (Linux; Android 6.0; CAM-L03 Build/HUAWEICAM-L03; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/69.0.3497.100 Mobile Safari/537.36'
    , 'Mobile'
    , 'Android 6.0'
    , 'Chrome Mobile WebView 69.0.3497'
    , newid()
    )
    , (
    'Linux - Chrome 83.0.4103'
    , 'Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.97 Safari/537.36'
    , 'Desktop'
    , 'Linux'
    , 'Chrome 83.0.4103'
    , newid()
    )
    , (
    'Linux - Chrome 26.0.1410'
    , 'Mozilla/5.0 (Linux; NetCast; U) AppleWebKit/537.31 (KHTML, like Gecko) Chrome/26.0.1410.33 Safari/537.31 SmartTV/5.0'
    , 'Desktop'
    , 'Linux'
    , 'Chrome 26.0.1410'
    , newid()
    )
    , (
    'Windows 10 - Chrome 88.0.4324'
    , 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.190 Safari/537.36'
    , 'Desktop'
    , 'Windows 10'
    , 'Chrome 88.0.4324'
    , newid()
    )
    , (
    'Other - Python Requests 2.26'
    , 'python-requests/2.26.0'
    , 'Desktop'
    , 'Other'
    , 'Python Requests 2.26'
    , newid()
    )
    , (
    'Windows 7 - IE 11.0'
    , 'Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko'
    , 'Desktop'
    , 'Windows 7'
    , 'IE 11.0'
    , newid()
    )
    , (
    'Windows 10 - Firefox 65.0'
    , 'Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:65.0) Gecko/20100101 Firefox/65.0'
    , 'Desktop'
    , 'Windows 10'
    , 'Firefox 65.0'
    , newid()
    )
    , (
    'Other - curl 7.64.0'
    , 'curl/7.64.0'
    , 'Desktop'
    , 'Other'
    , 'curl 7.64.0'
    , newid()
    )
    , (
    'Android 4.1.1 - Android 4.1.1'
    , 'Mozilla/5.0 (Linux; Android 4.1.1; Nexus 7 Build/JRO03D)'
    , 'Tablet'
    , 'Android 4.1.1'
    , 'Android 4.1.1'
    , newid()
    )
    , (
    'Windows 7 - Firefox 77.0'
    , 'Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:77.0) Gecko/20100101 Firefox/77.0'
    , 'Desktop'
    , 'Windows 7'
    , 'Firefox 77.0'
    , newid()
    )
    , (
    'Other - curl 7.58.0'
    , 'curl/7.58.0'
    , 'Desktop'
    , 'Other'
    , 'curl 7.58.0'
    , newid()
    )
    , (
    'Other - curl 7.83.0'
    , 'curl/7.83.0'
    , 'Desktop'
    , 'Other'
    , 'curl 7.83.0'
    , newid()
    )
    , (
    'Linux - Chrome 41.0.2227'
    , 'Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2227.0 Safari/537.36'
    , 'Desktop'
    , 'Linux'
    , 'Chrome 41.0.2227'
    , newid()
    )
    , (
    'Mac OS X 10.15.7 - Edge 102.0.1245'
    , 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/102.0.5005.124 Safari/537.36 Edg/102.0.1245.44'
    , 'Desktop'
    , 'Mac OS X 10.15.7'
    , 'Edge 102.0.1245'
    , newid()
    )
    , (
    'iOS 15.5 - Rock Mobile Latest 1.0.18'
    , ''
    , 'Mobile'
    , 'iOS 15.5'
    , 'Rock Mobile Latest 1.0.18'
    , newid()
    )
    , (
    'Mac OS X 10.6 - Firefox 3.6.8'
    , 'Mozilla/5.0 (Macintosh; U; Intel Mac OS X 10.6; fr; rv:1.9.2.8) Gecko/20100722 Firefox/3.6.8'
    , 'Desktop'
    , 'Mac OS X 10.6'
    , 'Firefox 3.6.8'
    , newid()
    )
    , (
    'iOS 15.5 - Edge Mobile 102.0.1245'
    , 'Mozilla/5.0 (iPhone; CPU iPhone OS 15_5 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) EdgiOS/102.0.1245.39 Version/15.0 Mobile/15E148 Safari/604.1'
    , 'Mobile'
    , 'iOS 15.5'
    , 'Edge Mobile 102.0.1245'
    , newid()
    )
    , (
    'Android 7.0 - Chrome Mobile WebView 60.0.3112'
    , 'Mozlila/5.0 (Linux; Android 7.0; SM-G892A Bulid/NRD90M; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/60.0.3112.107 Moblie Safari/537.36'
    , 'Tablet'
    , 'Android 7.0'
    , 'Chrome Mobile WebView 60.0.3112'
    , newid()
    )
    , (
    'Ubuntu - Firefox 97.0'
    , 'Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:97.0) Gecko/20100101 Firefox/97.0'
    , 'Desktop'
    , 'Ubuntu'
    , 'Firefox 97.0'
    , newid()
    )
    , (
    'Other - masscan 1.3'
    , 'masscan/1.3 (https://github.com/robertdavidgraham/masscan)'
    , 'Desktop'
    , 'Other'
    , 'masscan 1.3'
    , newid()
    )
    , (
    'Windows 7 - Chrome 43.0.2357'
    , 'Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/43.0.2357.124 Safari/537.36'
    , 'Desktop'
    , 'Windows 7'
    , 'Chrome 43.0.2357'
    , newid()
    )
    , (
    'Windows 10 - Chrome 87.0.4280'
    , 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.66 Safari/537.36'
    , 'Desktop'
    , 'Windows 10'
    , 'Chrome 87.0.4280'
    , newid()
    )
    , (
    'Windows 7 - Chrome 72.0.3626'
    , 'Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/72.0.3626.109 Safari/537.36'
    , 'Desktop'
    , 'Windows 7'
    , 'Chrome 72.0.3626'
    , newid()
    )
    , (
    'Linux - Firefox 84.0'
    , 'Mozilla/5.0 (X11; Linux x86_64; rv:84.0) Gecko/20100101 Firefox/84.0'
    , 'Desktop'
    , 'Linux'
    , 'Firefox 84.0'
    , newid()
    )
    , (
    'Windows 10 - Chrome 45.0.2454'
    , 'Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/45.0.2454.101 Safari/537.36'
    , 'Desktop'
    , 'Windows 10'
    , 'Chrome 45.0.2454'
    , newid()
    )
    , (
    'Windows 7 - Chrome 63.0.3239'
    , 'Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.132 Safari/479B76'
    , 'Desktop'
    , 'Windows 7'
    , 'Chrome 63.0.3239'
    , newid()
    )
    , (
    'Windows 7 - Chrome 100.0.4896'
    , 'Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.4896.127 Safari/537.36'
    , 'Desktop'
    , 'Windows 7'
    , 'Chrome 100.0.4896'
    , newid()
    )
    , (
    'Windows 10 - Chrome 88.0.4240'
    , 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4240.193 Safari/537.36'
    , 'Desktop'
    , 'Windows 10'
    , 'Chrome 88.0.4240'
    , newid()
    )
    , (
    'Windows 10 - Chrome 103.0.5060'
    , 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.5060.53 Safari/537.36'
    , 'Desktop'
    , 'Windows 10'
    , 'Chrome 103.0.5060'
    , newid()
    )
    , (
    'Windows 10 - Chrome 80.0.3987'
    , 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.149 Safari/537.36'
    , 'Desktop'
    , 'Windows 10'
    , 'Chrome 80.0.3987'
    , newid()
    )
    , (
    'Mac OS X 10.15.7 - Edge 101.0.1210'
    , 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/101.0.4951.41 Safari/537.36 Edg/101.0.1210.32'
    , 'Desktop'
    , 'Mac OS X 10.15.7'
    , 'Edge 101.0.1210'
    , newid()
    )
    , (
    'Windows 10 - Chrome 74.0.3729'
    , 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36'
    , 'Desktop'
    , 'Windows 10'
    , 'Chrome 74.0.3729'
    , newid()
    )
    , (
    'Windows 8 - Chrome 53.0.3076'
    , 'Mozilla/5.0 (Windows NT 6.2;en-US) AppleWebKit/537.32.36 (KHTML, live Gecko) Chrome/53.0.3076.68 Safari/537.32'
    , 'Desktop'
    , 'Windows 8'
    , 'Chrome 53.0.3076'
    , newid()
    )
    , (
    'Android 9 - Chrome Mobile 76.0.3809'
    , 'Mozilla/5.0 (Linux; Android 9; GM1910) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/76.0.3809.111 Mobile Safari/537.36'
    , 'Mobile'
    , 'Android 9'
    , 'Chrome Mobile 76.0.3809'
    , newid()
    )
    , (
    'Linux - Firefox 83.0'
    , 'Mozilla/5.0 (X11; Linux x86_64; rv:83.0) Gecko/20100101 Firefox/83.0'
    , 'Desktop'
    , 'Linux'
    , 'Firefox 83.0'
    , newid()
    )
    , (
    'Mac OS X 10.9.2 - Other'
    , 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_9_2)'
    , 'Desktop'
    , 'Mac OS X 10.9.2'
    , 'Other'
    , newid()
    )
    , (
    'Mac OS X 10.15.7 - Chrome 103.0.0'
    , 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.0.0 Safari/537.36'
    , 'Desktop'
    , 'Mac OS X 10.15.7'
    , 'Chrome 103.0.0'
    , newid()
    )
    , (
    'iOS 15.5 - Mobile Safari 15.5'
    , 'Mozilla/5.0 (iPhone; CPU iPhone OS 15_5 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/15.5 Mobile/15E148 Safari/604.1'
    , 'Mobile'
    , 'iOS 15.5'
    , 'Mobile Safari 15.5'
    , newid()
    )
    , (
    'Mac OS X 10.8.3 - Chrome 26.0.1410'
    , 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_8_3) AppleWebKit/537.31 (KHTML, like Gecko) Chrome/26.0.1410.65 Safari/537.31'
    , 'Desktop'
    , 'Mac OS X 10.8.3'
    , 'Chrome 26.0.1410'
    , newid()
    )
    , (
    'iOS 15.3.1 - Mobile Safari 15.3'
    , 'Mozilla/5.0 (iPhone; CPU iPhone OS 15_3_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/15.3 Mobile/15E148 Safari/604.1'
    , 'Mobile'
    , 'iOS 15.3.1'
    , 'Mobile Safari 15.3'
    , newid()
    )
    , (
    'Linux - HeadlessChrome 80.0.3987'
    , 'Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) HeadlessChrome/80.0.3987.163 Safari/537.36'
    , 'Desktop'
    , 'Linux'
    , 'HeadlessChrome 80.0.3987'
    , newid()
    )
    , (
    'Other - Python Requests 2.27'
    , 'python-requests/2.27.1'
    , 'Desktop'
    , 'Other'
    , 'Python Requests 2.27'
    , newid()
    )
    , (
    'Windows 8 - Chrome 27.0.1453'
    , 'Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/27.0.1453.93 Safari/537.36'
    , 'Desktop'
    , 'Windows 8'
    , 'Chrome 27.0.1453'
    , newid()
    )
    , (
    'Other - Python-urllib 3.7'
    , 'Python-urllib/3.7'
    , 'Desktop'
    , 'Other'
    , 'Python-urllib 3.7'
    , newid()
    )
    , (
    'Windows 7 - Firefox 8.0'
    , 'Mozilla/5.0 (Windows NT 6.1; WOW64; rv:8.0) Gecko/20100101 Firefox/8.0'
    , 'Desktop'
    , 'Windows 7'
    , 'Firefox 8.0'
    , newid()
    )
    , (
    'Windows 10 - Chrome 79.0'
    , 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0 Safari/537.36'
    , 'Desktop'
    , 'Windows 10'
    , 'Chrome 79.0'
    , newid()
    )
    , (
    'Windows 10 - Chrome 84.0.4147'
    , 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko; compatible; BW/1.1; bit.ly/2W6Px8S; d37c47cefe) Chrome/84.0.4147.105 Safari/537.36'
    , 'Desktop'
    , 'Windows 10'
    , 'Chrome 84.0.4147'
    , newid()
    )
    , (
    'Windows NT - Chrome 95.0.4247'
    , 'Mozilla/5.0 (Windows NT 10.9; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/95.0.4247.22 Safari/537.36'
    , 'Desktop'
    , 'Windows NT'
    , 'Chrome 95.0.4247'
    , newid()
    )
    , (
    'Android 6 - Chrome Mobile 90.0.4193'
    , 'Mozilla/5.0 (Linux; Android 6; CAG-L02 Build/HUAWEICAG-L02) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4193.58 Mobile Safari/537.36'
    , 'Mobile'
    , 'Android 6'
    , 'Chrome Mobile 90.0.4193'
    , newid()
    )
    , (
    'Windows 10 - Chrome 81.0.4044'
    , 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/81.0.4044.113 Safari/537.36'
    , 'Desktop'
    , 'Windows 10'
    , 'Chrome 81.0.4044'
    , newid()
    )
    , (
    'Windows 10 - Firefox 79.0'
    , 'Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:79.0) Gecko/20100101 Firefox/79.0'
    , 'Desktop'
    , 'Windows 10'
    , 'Firefox 79.0'
    , newid()
    )
    , (
    'Other - curl 7.54.0'
    , 'curl/7.54.0'
    , 'Desktop'
    , 'Other'
    , 'curl 7.54.0'
    , newid()
    )
    , (
    'Windows 10 - Chrome 103.0.0'
    , 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.0.0 Safari/537.36'
    , 'Desktop'
    , 'Windows 10'
    , 'Chrome 103.0.0'
    , newid()
    )
    , (
    'Windows XP - Firefox 9.0.1'
    , 'Mozilla/5.0 (Windows NT 5.1; rv:9.0.1) Gecko/20100101 Firefox/9.0.1'
    , 'Desktop'
    , 'Windows XP'
    , 'Firefox 9.0.1'
    , newid()
    )
    , (
    'Other - Scrapy 2.5.1'
    , 'Scrapy/2.5.1 (+https://scrapy.org)'
    , 'Desktop'
    , 'Other'
    , 'Scrapy 2.5.1'
    , newid()
    )
    , (
    'Mac OS X 10.8.3 - Chrome 27.0.1453'
    , 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_8_3) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/27.0.1453.93 Safari/537.36'
    , 'Desktop'
    , 'Mac OS X 10.8.3'
    , 'Chrome 27.0.1453'
    , newid()
    )
    , (
    'Other - Python Requests 2.18'
    , 'python-requests/2.18.3'
    , 'Desktop'
    , 'Other'
    , 'Python Requests 2.18'
    , newid()
    )
    , (
    'Windows 10 - Firefox 67.0'
    , 'Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:67.0) Gecko/20100101 Firefox/67.0'
    , 'Desktop'
    , 'Windows 10'
    , 'Firefox 67.0'
    , newid()
    )
    , (
    'iOS 10.3.1 - Mobile Safari 10.0'
    , 'Mozilla/5.0 (iPhone; CPU iPhone OS 10_3_1 like Mac OS X) AppleWebKit/603.1.30 (KHTML, like Gecko) Version/10.0 Mobile/14E304 Safari/602.1'
    , 'Mobile'
    , 'iOS 10.3.1'
    , 'Mobile Safari 10.0'
    , newid()
    )
    , (
    'Other - curl 7.64.1'
    , 'curl/7.64.1'
    , 'Desktop'
    , 'Other'
    , 'curl 7.64.1'
    , newid()
    )
    , (
    'Mac OS X 10.15.7 - Chrome 96.0.4664'
    , 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.110 Safari/537.36'
    , 'Desktop'
    , 'Mac OS X 10.15.7'
    , 'Chrome 96.0.4664'
    , newid()
    )
    , (
    'Mac OS X 11.0.0 - Chrome 87.0.4280'
    , 'Mozilla/5.0 (Macintosh; Intel Mac OS X 11_0_0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36'
    , 'Desktop'
    , 'Mac OS X 11.0.0'
    , 'Chrome 87.0.4280'
    , newid()
    )
    , (
    'Mac OS X 10.15.7 - Edge 92.0.902'
    , 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.159 Safari/537.36 Edg/92.0.902.84'
    , 'Desktop'
    , 'Mac OS X 10.15.7'
    , 'Edge 92.0.902'
    , newid()
    )
    , (
    'Windows XP - Other'
    , 'Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:x.x.x) Gecko/20041107 Firefox/x.x'
    , 'Desktop'
    , 'Windows XP'
    , 'Other'
    , newid()
    )
    , (
    'Mac OS X 10.13.6 - Chrome 80.0.3987'
    , 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_13_6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.116 Safari/537.36'
    , 'Desktop'
    , 'Mac OS X 10.13.6'
    , 'Chrome 80.0.3987'
    , newid()
    )
    , (
    'iOS 7.1.2 - Mobile Safari 4.0.5'
    , 'Mozilla/5.0 (iPad; CPU OS 7_1_2 like Mac OS X; en-US) AppleWebKit/531.5.2 (KHTML, like Gecko) Version/4.0.5 Mobile/8B116 Safari/6531.5.2'
    , 'Tablet'
    , 'iOS 7.1.2'
    , 'Mobile Safari 4.0.5'
    , newid()
    )
    , (
    'Windows 10 - Edge 103.0.1264'
    , 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.5060.53 Safari/537.36 Edg/103.0.1264.37'
    , 'Desktop'
    , 'Windows 10'
    , 'Edge 103.0.1264'
    , newid()
    )
    , (
    'Other - UC Browser 7.0.2'
    , 'Openwave/ UCWEB7.0.2.37/28/999'
    , 'Desktop'
    , 'Other'
    , 'UC Browser 7.0.2'
    , newid()
    )
    , (
    'Android 8.0.0 - Chrome Mobile 78.0.3904'
    , 'Mozilla/5.0 (Linux; Android 8.0.0;) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3904.108 Mobile Safari/537.36'
    , 'Mobile'
    , 'Android 8.0.0'
    , 'Chrome Mobile 78.0.3904'
    , newid()
    )
    , (
    'Android 6.0 - Chrome Mobile 81.0.4044'
    , 'Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/81.0.4044.138 Mobile Safari/537.36'
    , 'Mobile'
    , 'Android 6.0'
    , 'Chrome Mobile 81.0.4044'
    , newid()
    )
    , (
    'Windows 7 - Firefox 4.0.1'
    , 'Mozilla/5.0 (Windows NT 6.1; rv:2.0.1) Gecko/20100101 Firefox/4.0.1'
    , 'Desktop'
    , 'Windows 7'
    , 'Firefox 4.0.1'
    , newid()
    )
    , (
    'Mac OS X 10.15.7 - Edge 103.0.1264'
    , 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.5060.53 Safari/537.36 Edg/103.0.1264.37'
    , 'Desktop'
    , 'Mac OS X 10.15.7'
    , 'Edge 103.0.1264'
    , newid()
    )
    , (
    'Windows 10 - Chrome 83.0.4098'
    , 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4098.0 Safari/537.36'
    , 'Desktop'
    , 'Windows 10'
    , 'Chrome 83.0.4098'
    , newid()
    )
    , (
    'Linux - Chrome 80.0.3987'
    , 'Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.116 Safari/537.36'
    , 'Desktop'
    , 'Linux'
    , 'Chrome 80.0.3987'
    , newid()
    )
    , (
    'Windows 10 - Edge 18.17763'
    , 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/536.36 (KHTML, like Gecko) Chrome/64.0.3282.140 Safari/537.36 Edge/18.17763'
    , 'Desktop'
    , 'Windows 10'
    , 'Edge 18.17763'
    , newid()
    )
    , (
    'Windows 7 - Opera 52.0.2871'
    , 'Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/65.0.3325.181 Safari/537.36 OPR/52.0.2871.99'
    , 'Desktop'
    , 'Windows 7'
    , 'Opera 52.0.2871'
    , newid()
    )
    , (
    'Windows 10 - Opera 42.0.2393'
    , 'Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36 OPR/42.0.2393.94'
    , 'Desktop'
    , 'Windows 10'
    , 'Opera 42.0.2393'
    , newid()
    )
    , (
    'Mac OS X 10.13 - Firefox 73.0'
    , 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10.13; rv:61.0) Gecko/20100101 Firefox/73.0'
    , 'Desktop'
    , 'Mac OS X 10.13'
    , 'Firefox 73.0'
    , newid()
    )
    , (
    'Other - masscan'
    , 'masscan-ng/1.3 (https://github.com/bi-zone/masscan-ng)'
    , 'Desktop'
    , 'Other'
    , 'masscan'
    , newid()
    )
    , (
    'Windows Vista - Chrome 41.0.2227'
    , 'Mozilla/5.0 (Windows NT 6.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2227.1 Safari/537.36'
    , 'Desktop'
    , 'Windows Vista'
    , 'Chrome 41.0.2227'
    , newid()
    )
    , (
    'iOS 11.0 - Mobile Safari 11.0'
    , 'Mozilla/5.0 (iPhone; CPU OS 11_0 like Mac OS X) AppleWebKit/604.1.25 (KHTML, like Gecko) Version/11.0 Mobile/15A372 Safari/604.1'
    , 'Mobile'
    , 'iOS 11.0'
    , 'Mobile Safari 11.0'
    , newid()
    )
    , (
    'Mac OS X 10.15.7 - Safari 15.1'
    , 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/15.1 Safari/605.1.15'
    , 'Desktop'
    , 'Mac OS X 10.15.7'
    , 'Safari 15.1'
    , newid()
    )
    , (
    'Mac OS X 10.13.6 - Chrome 91.0.4472'
    , 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_13_6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.114 Safari/537.36'
    , 'Desktop'
    , 'Mac OS X 10.13.6'
    , 'Chrome 91.0.4472'
    , newid()
    )
    , (
    'iOS 11.3 - Mobile Safari UI/WKWebView'
    , 'Mozilla/5.01724933 Mozilla/5.0 (iPhone; CPU iPhone OS 11_3 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Mobile/15E302'
    , 'Mobile'
    , 'iOS 11.3'
    , 'Mobile Safari UI/WKWebView'
    , newid()
    )
    , (
    'Windows 10 - Chrome 83.0.4086'
    , 'Mozilla/5.0 (Windows NT 10.0; ) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4086.0 Safari/537.36'
    , 'Desktop'
    , 'Windows 10'
    , 'Chrome 83.0.4086'
    , newid()
    )
    , (
    'Windows 8 - Chrome 57.0.3023'
    , 'Mozilla/5.0 (Windows NT 6.2;en-US) AppleWebKit/537.32.36 (KHTML, live Gecko) Chrome/57.0.3023.73 Safari/537.32'
    , 'Desktop'
    , 'Windows 8'
    , 'Chrome 57.0.3023'
    , newid()
    )
    , (
    'Android 6.0.1 - QQ Browser Mobile 8.7'
    , 'Mozilla/5.0 (Linux; U; Android 6.0.1; zh-cn; HUAWEI RIO-UL00 Build/HUAWEIRIO-UL00) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/57.0.2987.132 MQQBrowser/8.7 Mobile Safari/537.36'
    , 'Mobile'
    , 'Android 6.0.1'
    , 'QQ Browser Mobile 8.7'
    , newid()
    )
    , (
    'Windows 7 - Firefox 3.6.8'
    , 'Mozilla/5.0 (Windows; U; Windows NT 6.1; en-GB; rv:1.9.2.8) Gecko/20100722 Firefox/3.6.8 ( .NET CLR 3.5.30729; .NET4.0C)'
    , 'Desktop'
    , 'Windows 7'
    , 'Firefox 3.6.8'
    , newid()
    )
    , (
    'Windows XP - Chrome 41.0.2224'
    , 'Mozilla/5.0 (Windows NT 5.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2224.3 Safari/537.36'
    , 'Desktop'
    , 'Windows XP'
    , 'Chrome 41.0.2224'
    , newid()
    )
    , (
    'Linux - Chrome 89.0.4389'
    , 'Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.82 Safari/537.36'
    , 'Desktop'
    , 'Linux'
    , 'Chrome 89.0.4389'
    , newid()
    )
    , (
    'Windows 7 - Chrome 42.0.2311'
    , 'Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.90 Safari/537.36'
    , 'Desktop'
    , 'Windows 7'
    , 'Chrome 42.0.2311'
    , newid()
    )
    , (
    'Windows 7 - Chrome 99.0.4844'
    , 'Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.4844.84 Safari/537.36'
    , 'Desktop'
    , 'Windows 7'
    , 'Chrome 99.0.4844'
    , newid()
    )
    , (
    'Windows 8.1 - Chrome 60.0.3112'
    , 'Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.90 Safari/537.36 React.org'
    , 'Desktop'
    , 'Windows 8.1'
    , 'Chrome 60.0.3112'
    , newid()
    )
    , (
    'Windows XP - Opera 8.01'
    , 'Mozilla/5.0 (Windows NT 5.1; U; en) Opera 8.01'
    , 'Desktop'
    , 'Windows XP'
    , 'Opera 8.01'
    , newid()
    )
    , (
    'Windows 10 - Chrome 100.0.4896'
    , 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.4896.127 Safari/537.36'
    , 'Desktop'
    , 'Windows 10'
    , 'Chrome 100.0.4896'
    , newid()
    )
    , (
    'Windows XP - Firefox 3.6.12'
    , 'Mozilla/5.0 (Windows; U; Windows NT 5.1; ru; rv:1.9.2.12) Gecko/20101026 Firefox/3.6.12 YB/5.0.3'
    , 'Desktop'
    , 'Windows XP'
    , 'Firefox 3.6.12'
    , newid()
    )
    , (
    'Windows XP - Chrome 14.0.835'
    , 'Mozilla/5.0 (Windows NT 5.1) AppleWebKit/535.1 (KHTML, like Gecko) Chrome/14.0.835.202 Safari/535.1'
    , 'Desktop'
    , 'Windows XP'
    , 'Chrome 14.0.835'
    , newid()
    )
    , (
    'Windows XP - Opera 11.00'
    , 'Opera/9.80 (Windows NT 5.1; U; ru) Presto/2.7.62 Version/11.00'
    , 'Desktop'
    , 'Windows XP'
    , 'Opera 11.00'
    , newid()
    )
    , (
    'Other - curl 7.29.0'
    , 'curl/7.29.0'
    , 'Desktop'
    , 'Other'
    , 'curl 7.29.0'
    , newid()
    )
    , (
    'Mac OS X 10.15.3 - Chrome 80.0.3987'
    , 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_3) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.106 Safari/537.36'
    , 'Desktop'
    , 'Mac OS X 10.15.3'
    , 'Chrome 80.0.3987'
    , newid()
    )
    , (
    'Ubuntu 7.10 - Firefox 2.0.0'
    , 'Mozilla/5.0 (X11; U; Linux i686; fr; rv:1.8.1.8) Gecko/20071022 Ubuntu/7.10 (gutsy) Firefox/2.0.0.11'
    , 'Desktop'
    , 'Ubuntu 7.10'
    , 'Firefox 2.0.0'
    , newid()
    )
    , (
    'Other - libwww-perl 6.61'
    , 'libwww-perl/6.61'
    , 'Mobile'
    , 'Other'
    , 'libwww-perl 6.61'
    , newid()
    )
    , (
    'Windows 7 - Chrome 65.0.3325'
    , 'Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/65.0.3325.52 Safari/537.36'
    , 'Desktop'
    , 'Windows 7'
    , 'Chrome 65.0.3325'
    , newid()
    )
    , (
    'Mac OS X 10.10.1 - Chrome 39.0.2171'
    , 'Mozilla/5.0 (Macintosh;                 Intel Mac OS X 10_10_1) AppleWebKit/537.36 (KHTML,                 like Gecko) Chrome/39.0.2171.95 Safari/537.36'
    , 'Desktop'
    , 'Mac OS X 10.10.1'
    , 'Chrome 39.0.2171'
    , newid()
    )
    , (
    'Mac OS X 10.15.4 - Chrome 83.0.4103'
    , 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_4) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.97 Safari/537.36'
    , 'Desktop'
    , 'Mac OS X 10.15.4'
    , 'Chrome 83.0.4103'
    , newid()
    )
    , (
    'iOS 13.2.3 - Mobile Safari 13.0.3'
    , 'Mozilla/5.0 (iPhone; CPU iPhone OS 13_2_3 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/13.0.3 Mobile/15E148 Safari/604.1'
    , 'Mobile'
    , 'iOS 13.2.3'
    , 'Mobile Safari 13.0.3'
    , newid()
    )
    , (
    'Other - curl 7.68.0'
    , 'curl/7.68.0'
    , 'Desktop'
    , 'Other'
    , 'curl 7.68.0'
    , newid()
    )
    , (
    'Windows 10 - Chrome 99.0.4844'
    , 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.4844.84 Safari/537.36'
    , 'Desktop'
    , 'Windows 10'
    , 'Chrome 99.0.4844'
    , newid()
    )
    , (
    'Mac OS X 10.15 - Firefox 101.0'
    , 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10.15; rv:101.0) Gecko/20100101 Firefox/101.0'
    , 'Desktop'
    , 'Mac OS X 10.15'
    , 'Firefox 101.0'
    , newid()
    )
    , (
    'Android 12.0 - Rock.Mobile.Android 1.0'
    , ''
    , 'Mobile'
    , 'Android 12.0'
    , 'Rock.Mobile.Android 1.0'
    , newid()
    )
    , (
    'Linux - Chrome 100.0.4896'
    , 'Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.4896.88 Safari/537.36'
    , 'Desktop'
    , 'Linux'
    , 'Chrome 100.0.4896'
    , newid()
    )
    , (
    'Mac OS X 10.15.7 - Chrome 103.0.5060'
    , 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.5060.53 Safari/537.36'
    , 'Desktop'
    , 'Mac OS X 10.15.7'
    , 'Chrome 103.0.5060'
    , newid()
    )
    , (
    'Mac OS X 10.15.7 - Safari 15.5'
    , 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/15.5 Safari/605.1.15'
    , 'Desktop'
    , 'Mac OS X 10.15.7'
    , 'Safari 15.5'
    , newid()
    )
    , (
    'Other - curl 7.81.0'
    , 'curl/7.81.0'
    , 'Desktop'
    , 'Other'
    , 'curl 7.81.0'
    , newid()
    )
    , (
    'Mac OS X 10.15.7 - Chrome 100.0.4896'
    , 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.4896.127 Safari/537.36'
    , 'Desktop'
    , 'Mac OS X 10.15.7'
    , 'Chrome 100.0.4896'
    , newid()
    )
    , (
    'Other - Python Requests 2.22'
    , 'python-requests/2.22.0'
    , 'Desktop'
    , 'Other'
    , 'Python Requests 2.22'
    , newid()
    )
    , (
    'Windows 10 - Firefox 83.0'
    , 'Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:83.0) Gecko/20100101 Firefox/83.0'
    , 'Desktop'
    , 'Windows 10'
    , 'Firefox 83.0'
    , newid()
    )
    , (
    'Windows 7 - Firefox 29.0'
    , 'Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:25.0) Gecko/20100101 Firefox/29.0'
    , 'Desktop'
    , 'Windows 7'
    , 'Firefox 29.0'
    , newid()
    )
    , (
    'Windows 7 - Chrome 76.0.3809'
    , 'Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/76.0.3809.132 Safari/537.36'
    , 'Desktop'
    , 'Windows 7'
    , 'Chrome 76.0.3809'
    , newid()
    )
    , (
    'Other - curl 7.67.0'
    , 'curl/7.67.0'
    , 'Desktop'
    , 'Other'
    , 'curl 7.67.0'
    , newid()
    )
    , (
    'Windows 7 - Chrome 28.0.1468'
    , 'Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/28.0.1468.0 Safari/537.36'
    , 'Desktop'
    , 'Windows 7'
    , 'Chrome 28.0.1468'
    , newid()
    )
    , (
    'Windows 10 - Chrome 79.0.3945'
    , 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.130 Safari/537.36'
    , 'Desktop'
    , 'Windows 10'
    , 'Chrome 79.0.3945'
    , newid()
    )
    , (
    'Other - Firefox 35.0'
    , 'Mozilla/5.0 Firefox/35.0'
    , 'Desktop'
    , 'Other'
    , 'Firefox 35.0'
    , newid()
    )
    , (
    'Windows 8 - Chrome 32.0.1667'
    , 'Mozilla/5.0 (Windows NT 6.2; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/32.0.1667.0 Safari/537.36'
    , 'Desktop'
    , 'Windows 8'
    , 'Chrome 32.0.1667'
    , newid()
    )
    , (
    'Linux - GooglePlusBot'
    , 'Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36 Google-PageRenderer Google (+https://developers.google.com/+/web/snippet/)'
    , 'Desktop'
    , 'Linux'
    , 'GooglePlusBot'
    , newid()
    )
    , (
    'Linux - Firefox 73.0'
    , 'Mozilla/5.0 (X11; Linux x86_64; rv:73.0) Gecko/20100101 Firefox/73.0'
    , 'Desktop'
    , 'Linux'
    , 'Firefox 73.0'
    , newid()
    )
    , (
    'Android 11 - Chrome Mobile 102.0.0'
    , 'Mozilla/5.0 (Linux; Android 11; SM-A125U) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/102.0.0.0 Mobile Safari/537.36'
    , 'Mobile'
    , 'Android 11'
    , 'Chrome Mobile 102.0.0'
    , newid()
    )
    , (
    'Mac OS X 10.13.6 - Chrome 103.0.0'
    , 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_13_6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.0.0 Safari/537.36'
    , 'Desktop'
    , 'Mac OS X 10.13.6'
    , 'Chrome 103.0.0'
    , newid()
    )

INSERT INTO InteractionDeviceType (
    [Name]
    , [DeviceTypeData]
    , [ClientType]
    , [OperatingSystem]
    , [Application]
    , [Guid]
    )
SELECT *
FROM @deviceTypeData
WHERE [Name] NOT IN (
        SELECT [Name]
        FROM InteractionDeviceType
        )
