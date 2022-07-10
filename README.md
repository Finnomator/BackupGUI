# BackupGUI
A wrapper for [Microsoft Robocopy](https://docs.microsoft.com/en-us/windows-server/administration/windows-commands/robocopy).

## Installation
- Visit the [release page](https://github.com/Finnomator/BackupGUI/releases/tag/Release-1.0)
- Download `BackupGUI-Release-X.X.zip`
- Extract the file
- Run setup.exe

## Usage
- Choose a path where the backup should be created
- Add paths to be backed up
- Click 'Start Backup'

## Important
Used Robocopy parameters:
|Parameter|Description|
|---|---|
|/s|Copies subdirectories. This option automatically **excludes empty directories**.|
|/mir|**Mirrors** a directory tree -> All modified files will be **overwriten**.|

[Learn more](https://docs.microsoft.com/en-us/windows-server/administration/windows-commands/robocopy)
