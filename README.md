# MyUpdate

It is an updater executable in Windows Form View or in Console Application.
When launch it can detect if an update is avaiable on a server, download a zip file, replace the unzip files and restart the executable

## Get Starting - Server side

Go to the folder where the update will be.
add the update.xml and update.zip files
 - The update.zip file contains all files related to the new update
 - The update.xml file is the update information. Version, url download, zip file md5, description and launch arguments.

Example update.xml
```xml
<?xml version="1.0"?>
<MyUpdate>
	<update appId="MyApp">
		<version>2.0.0.0</version>
		<url>https://myapps.update.com/my-app/update.zip</url>
		<fileName>update.zip</fileName>
		<md5>0B109C95A339DA3A93BA6BF532F69530</md5>
		<description>Initial update</description>
		<launchArgs>newVersion-2.0.0.0</launchArgs>
	</update>
</MyUpdate>
```

#### MD5

MD5, for Message Digest 5, is a cryptographic hash function that allows you to obtain the digital fingerprint of a file

### How to get md5 from a file

In Powershell by using the command line bellow "get-filehash"
```
get-filehash -path "C:\WorkPlace\MyUpdate\MyApp.exe" -Algorithm MD5 | format-list
```
The example above will be :
Algorithm : MD5
Hash      : 0B109C95A339DA3A93BA6BF532F69530
Path      : C:\WorkPlace\MyUpdate\MyApp.exe


In PHP by using hash_file function.
```php
// hash_file(string $algo, string $filename, bool $binary = false): string|false
<?php
/* Create a file to calculate your digital footprint */
file_put_contents('example.txt', 'The quick brown pin jumped over the lazy dog.');

echo hash_file('md5', 'example.txt');
?>
```
The example above will be :
2dfe052a8caca3db869ede6ae544cd5d

## Run Application

The project is in development actually.
The window form view is completed, however the console application version is in development.
Further modification will occur in the future.


###### version 0.5.2 (10/2021 - Paris)
###### Developped by JDevs