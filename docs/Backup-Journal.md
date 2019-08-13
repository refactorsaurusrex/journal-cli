---
external help file: JournalCli.dll-Help.xml
Module Name: JournalCli
online version: https://github.com/refactorsaurusrex/journal-cli/wiki
schema: 2.0.0
---

# Backup-Journal

## SYNOPSIS
Creates a complete backup of your journal entries.

## SYNTAX

```
Backup-Journal [-BackupLocation <String>] [-Password <String>] [-SaveLocation] [-SavePassword]
 [-RootDirectory <String>] [<CommonParameters>]
```

## DESCRIPTION
Creates a password-protected zip archive of all files contained in the journal root directory and saves it to the specified location.

## EXAMPLES

### EXAMPLE 1
```
Backup-Journal
```

Backup your journal using previously saved location and password. If a password and save location have not both been previously saved, the command will fail.

### EXAMPLE 2
```
Backup-Journal -BackupLocation C:\My\Secret\Location -SaveLocation -Password "secret123" -SavePassword
```

Backup your journal to the specified location with the provided password, and save the location and password values for future use.

## PARAMETERS

### -BackupLocation
The path to directory where backups should be saved. This is required if a location has not been previously saved.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Password
The password to apply to the archive. If none is provided, the archive will not be protected.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -RootDirectory
The path to the root directory for the journal. This is required if it has not been previously saved using the `Set-DefaultJournalLocation` cmdlet.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SaveLocation
Saves the specified location for future use.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -SavePassword
Saves the specified password for future use.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

## OUTPUTS

## NOTES

## RELATED LINKS

[The journal-cli wiki](https://github.com/refactorsaurusrex/journal-cli/wiki)

