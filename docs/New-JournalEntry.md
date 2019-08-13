---
external help file: JournalCli.dll-Help.xml
Module Name: JournalCli
online version: https://github.com/refactorsaurusrex/journal-cli/wiki
schema: 2.0.0
---

# New-JournalEntry

## SYNOPSIS
Creates a new journal entry.

## SYNTAX

```
New-JournalEntry [-DateOffset <Int32>] [-RootDirectory <String>] [<CommonParameters>]
```

## DESCRIPTION
Creates a new markdown-based journal entry file in the specified root directory.

## EXAMPLES

### EXAMPLE 1
```
New-JournalEntry
```

### EXAMPLE 2
```
New-JournalEntry -DateOffset -1
```

## PARAMETERS

### -DateOffset
An integer representing the number of days to offset from today's date.
For example, -1 represents yesterday.
The default is 0, meaning today.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 0
Accept pipeline input: False
Accept wildcard characters: False
```

### -RootDirectory
{{ Fill RootDirectory Description }}

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

## OUTPUTS

## NOTES

## RELATED LINKS

[The journal-cli wiki.](https://github.com/refactorsaurusrex/journal-cli/wiki)

