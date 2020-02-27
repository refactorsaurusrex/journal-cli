[![PowerShell Gallery Version](https://img.shields.io/powershellgallery/v/JournalCli?style=for-the-badge&logo=powershell)](https://www.powershellgallery.com/packages/JournalCli) [![PowerShell Gallery](https://img.shields.io/powershellgallery/dt/journalcli?style=for-the-badge&label=Downloads&logo=powershell)](https://www.powershellgallery.com/packages/JournalCli) [![MyGet](https://img.shields.io/myget/journal-cli/v/JournalCli?label=Beta&style=for-the-badge&logo=powershell)](https://www.myget.org/feed/journal-cli/package/nuget/JournalCli) 

[![Build status](https://img.shields.io/appveyor/build/refactorsaurusrex/journal-cli/master?style=for-the-badge&logo=appveyor)](https://ci.appveyor.com/project/refactorsaurusrex/journal-cli/branch/master) [![Gitter](https://img.shields.io/gitter/room/journal-cli/community?style=for-the-badge&logo=gitter)](https://gitter.im/journal-cli/community)

# What is this?

`journal-cli` is a *cross-platform* tool for people who want to journal with markdown, love command line tools, and are highly averse to storing intimate information in the cloud unless it's encrypted.

# Background

Sometime late in 2017, I found myself flipping through _really_ old entries in my Google Calendar. Turns out I have entries in there dating back to 2001. It was interesting taking a stroll down memory lane and seeing everything from the mundane ("Dentist Appt") to the exciting ("Date with Austin chick")\*. It got me thinking that I should make an effort to write down the highlights of my day _every day_ because future-me might be interested in knowing what I did on, say, December 13th, 2018. So I made the decision to start keeping a daily journal. 

For the first six months or so I used [Dynalist][dl], a tool I absolutely love. I was never particularly comfortable spelling out my life's details in a tool that didn't offer end-to-end encryption, but I stuck my head in the sand for as long as I could. Then in June of 2018 [Typeform][tf] - a surveying app I've used - suffered a breach and some of my data was stolen. Luckily, the data taken from my account wasn't sensitive. But the incident cemented my fears that _nothing is safe in the cloud_. I resolved to find another, safer tool to use for journaling. 

My next stop was [Inkdrop][id]. It's not designed with journaling in mind, but I found it fairly suitable for that purpose - as long as you don't sync your files to Inkdrop's servers. Unfortunately, after a while Inkdrop felt too constraining. Disabling cloud synchronization meant I could only write entries from a single computer. I was also irked by the Inkdrop developer's rigid opposition to adding any new features that [deviate even slightly from his narrowly defined product scope][id-journal]. I really wanted to use [Typora](https://typora.io/) - hands-down the best markdown editor available - but it lacked one really important feature. It offered no built-in mechanism for tagging files. Tags are essential for me because they basically constitute my journal's index.  

Finally, I had a realization. *Typora supports yaml front matter. I can use front matter to add tags to each file and create a command line tool to parse the tags from each journal file!* Thus, `journal-cli` was born.

\* I now generally refer to that chick from Austin as "wife".

# Sooo... what can it do? 

> This is just a quick summary. Refer to the [main documentation site](https://journalcli.me) for further information. For complete syntactical documentation, refer to the [wiki](https://github.com/refactorsaurusrex/journal-cli/wiki).

## Create an index of your journal

Use `Get-JournalIndex` and `Get-JournalEntriesByTag` to scan all your journal files and create an index of your entire journal. This allows you to:

- Display a list of all tags used in your journal.
- Sort tags by name or count, in ascending or descending order.
- List all journal entries containing specific tags.
- Show the headers from a subset of journal entries, for an overview of the topics contained in each entry.
- Paginate through the entire text of your journal entries.

## Create new journal entries

Create new journal entries, right from your terminal, that are automatically saved in appropriate year and month directories using a specific date-based naming convention and edit them in your favorite markdown editor.

```powershell
New-JournalEntry -Tags work,play
```

Forgot to create an entry for yesterday? No problem. Just pass in a `-DateOffset` parameter like so:

```powershell
New-JournalEntry -DateOffset -1 -Tags work,play
```

Use the `-Date` parameter to create an entry for a specific date:

```powershell
New-JournalEntry -Date '2017.4.25' -Tags work,play
```

You can also skip the markdown editor and write entries directly from your terminal window:

```powershell
Add-JournalEntryContent "I went to work and it was swell." -Tags work
```

The `Add-JournalEntryContent` cmdlet has the same `-Date` and `-DateOffset` parameters as `New-JournalEntry`.

## Create new _compiled_ journal entries

You can dynamically create new entries composed of an arbitrary set of original entries. For example, maybe you want a complete history of all your ski trip notes in a single file:

```powershell
New-CompiledJournalEntry -Tags skiing
```

This will create a single markdown file that contains *every journal entry written* that's tagged `skiing`. Or maybe you want to create a single entry for the entire year of 2019. 

```powershell
New-CompiledJournalEntry -From '2019.1.1' -To '2019.12.31'
```

You can also use the `-Entries` parameter to pass in a set of `IJournalEntry` objects created by creative usage of `Get-JournalIndex` or `Get-JournalEntriesByTag`. 

## Read entries from your terminal

There are several ways to peruse journal entry content directly from your terminal window.

```powershell
# Displays entry names, tags, and headers.
Get-JournalEntriesByTag -Tags skiing | select -ExpandProperty entries

# Displays entry names, tags, and body content.
Get-JournalEntriesByTag -Tags skiing -IncludeBodies | select -ExpandProperty entries

# Displays full content of retrieved entries one page at a time.
Get-JournalEntriesByTag -Tags skiing -IncludeBodies | 
  select -ExpandProperty entries | 
  select -ExpandProperty Body |
  more
```

## Write entries from your terminal

Use the new `Add-JournalEntryContent` cmdlet to write entries directly from your terminal. 

```powershell
Add-JournalEntryContent "Today I went to work, and it was swell."
```

By default, new content will be entered under the standard H1 date header. But you can also specify a header name with the `-Header` parameter. If the header already exists, your content will be appended to it. Otherwise, the new header will be appended to the bottom of the entry along with the associated content. You can also add tags with the `-Tags` parameter, and target an entry date other than `Today` by using the `-Date` and/or `-DateOffset` parameters.

## Write notes to your future self

Use the `-Readme` parameter of the `New-JournalEntry` cmdlet to add a specified date in the future when you want future-you to re-read the entry. Run `Get-JournalReadmeEntries` to display a list of entries with expired readme dates. (Future entries are hidden by default.) In a future release, you'll be able to export this list to Google Calendar so you can receive reminders to re-read your entries at the set date and time.

Further documentation on this feature can be found [here](https://journalcli.me/docs/features#valid-readme-expressions). 

## Create backups

Create a snapshot of your entire journal and save it to a zip file. Optionally, protect the zip file with a password.

## Open random journal entries

What's the point of keeping a journal if you never re-read your entries? Run `Open-RandomJournalEntry` to open a randomly selected entry. Pass in one or more tag names to narrow down the collection of possible entries.

## List all entries by tag

Want to see every journal entry that was tagged `work`, `family`, or whatever? Run `Get-JournalEntriesByTag` and pass in one or more tags.

## Rename and/or consolidate tags

Let's say you have a few dozen entries with the tag `family` and a few dozen more with the tag `family-drama`. Maybe you decide the latter really should be combined with the former. Use the `Rename-JournalTag` function to do exactly that. 

## Version History

Every `journal-cli` command that alters the state of your journal automatically captures the changes with a git commit. Having a complete and permanent editing history allows you to easily view all changes made to specific entries, and undo most accidental changes  - such as a regrettable tag rename operation. You do not need to install git separately because it is fully integrated with `journal-cli`.

# How do I get it?

`journal-cli` runs anywhere PowerShell 6 runs, including on Windows, Linux, and MacOS systems. Install it from the PowerShell Gallery:

```powershell
> Install-Module JournalCli 
```

For additional details and instructions, check out the [Getting Started](https://journalcli.me/docs/getting-started) guide.

## Beta Versions

New beta versions are published on every merge to master. To see what changes are in the current beta, look at the [Ready For Release column](https://github.com/refactorsaurusrex/journal-cli/projects/1) of the `journal-cli` roadmap. If you'd like to try out these features before they're released to the PowerShell Gallery, follow the steps below.

First, make sure you've completely removed all versions installed from the PowerShell Gallery. The reason for this is all beta releases are versioned `0.0.X`, where `x` is the [build number](https://ci.appveyor.com/project/refactorsaurusrex/journal-cli). That means beta versions will always have a lower semantic version number than the official releases, which can cause problems with module autoloading and upgrades if you have both beta and non-beta versions installed. You can use this command to remove all versions:

```powershell
Get-InstalledModule "JournalCli" -AllVersions | Uninstall-Module

# If JournalCli has already been loaded into your PowerShell session, 
# this will cause an error. Restart your terminal and try again.
```

Next, register the beta feed:

```powershell
Register-PSRepository -Name "JournalCliBeta" -SourceLocation "https://www.myget.org/F/journal-cli/api/v2"
```

Last, install the module from the beta feed:

```powershell
Install-Module -Name "JournalCli" -Repository "JournalCliBeta" 
```

# A word about performance

I've only tested this tool with a few hundred files on a very fast machine. I've made no deliberate effort to maximize performance. If you run this against thousands (or more!) files and/or on a slower machine, I can't promise Ferrari-like processing speeds. But if the tool feels laggy, [hit me up](https://github.com/refactorsaurusrex/journal-cli/issues) and let's fix it!

# Bugs / Suggestions

So far, I've written this tool with exactly one user in mind: **me**. That seems like a prudent choice since I'm not sure how many people out there share my particular obsession with markdown, command line tools, and data security. That said, if you like the idea behind `journal-cli` but feel something is missing or could be improved, let's talk. Open an [issue][issues],  [email me][profile], or post a message on [gitter](https://gitter.im/journal-cli/community). I can't promise I will accept changes that weren't agreed upon in advance, so please open the communication lines before writing any code.

[dl]: https://dynalist.io/
[tf]: https://www.typeform.com/
[id]: https://inkdrop.app/
[id-journal]: https://forum.inkdrop.app/t/save-the-currently-selected-notebook/883/6
[issues]: https://github.com/refactorsaurusrex/journal-cli/issues
[profile]: https://github.com/refactorsaurusrex
