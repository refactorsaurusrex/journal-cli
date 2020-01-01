[![Build status](https://ci.appveyor.com/api/projects/status/lhs9cnb68cxu0vuj/branch/master?svg=true)](https://ci.appveyor.com/project/refactorsaurusrex/journal-cli/branch/master) ![PowerShell Gallery](https://img.shields.io/powershellgallery/dt/journalcli)

# What is this?

`journal-cli` is a command line tool for people who want to journal with markdown files, love command line tools, and are highly averse to storing intimate information in the cloud unless it's encrypted.

# Background

Sometime late in 2017, I found myself flipping through _really_ old entries in my Google Calendar. Turns out I have entries in there going back to 2001. It was interesting taking a stroll down memory lane and seeing everything from the mundane ("Dentist Appt") to the exciting ("Date with Austin chick")\*. It got me thinking that I should make an effort to write down the highlights of my day _every day_ because future-me might be interested in knowing what I did on, say, December 13th, 2018. So I made the decision to start keeping a journal. 

For the first six months or so I used [Dynalist][dl], a tool I absolutely love. I was never particularly comfortable spelling out my life's details in a tool that didn't offer end-to-end encryption, but I stuck my head in the sand for as long as I could. Then in June of 2018 [Typeform][tf] - another tool I absolutely love - suffered a data breach, and some of my data was stolen. Luckily, the data taken from my account wasn't at all sensitive. But the incident cemented my fears that _nothing is safe in the cloud_. I resolved to find another, safer tool to use for journaling. 

My next stop was [Inkdrop][id]. It's not designed with journaling in mind, but I found it fairly suitable for that purpose - as long as you don't sync your files to Inkdrop's servers. Unfortunately, after a while Inkdrop felt too constraining. Disabling cloud synchronization meant I could only write entries from a single computer. I was also irked by the Inkdrop developer's rigid opposition to adding any new features that [deviate even slightly from his narrowly defined product scope][id-journal]. I really wanted to use [Typora](https://typora.io/) - hands-down the best markdown editor available - but it lacked one really important feature. It offered no built-in mechanism for tagging files. Tags are essential for me because they basically constitute my journal's index.  

Finally, I had a realization. *Typora supports yaml front matter. I can use front matter to add tags to each file and create a command line tool to parse the tags from each journal file!* Thus, `journal-cli` was born.

\* I now generally refer to that chick from Austin as "wife".

# Sooo... what can it do? 

> This is just a quick summary. Refer to the [main documentation site](https://journalcli.me) for further information. For complete syntactical documentation, refer to the [wiki](https://github.com/refactorsaurusrex/journal-cli/wiki).

## Create an index of your journal

Run `Get-JournalIndex` to scan all your journal files and create an index of your entire journal. This allows you to:

- Display a list of all tags used in your journal.
- Sort tags by name or count, in ascending or descending order.
- List all journal entries containing specific tags.
- Show the headers from a subset of journal entries, for an overview of the topics contained in each entry.
- Paginate through the entire text of your journal entries.

## Create new journal entries

Create new journal entries, right from your terminal, that are automatically saved in appropriate year and month directories using a specific date-based naming convention. (Example: `2018.12.13.md`) 

Forgot to create an entry for yesterday? No problem. Just pass in a `DateOffset` parameter like so:

```powershell
New-JournalEntry -DateOffset -1
```

## Create backups

Create a snapshot of your entire journal and save it to a zip file. Optionally, protect the zip file with a password.

## Open a random journal entry

What's the point of keeping a journal if you never re-read your entries? Run `Open-RandomJournalEntry` to open a randomly selected entry. Pass in one or more tag names to narrow down the collection of possible entries.

## List all entries by tag

Want to see every journal entry that was tagged `work`, `family`, or whatever? Run `Get-JournalEntriesByTag` and pass in one or more tags.

## Rename and/or consolidate tags

Let's say you have a few dozen entries with the tag `family` and a few dozen more with the tag `family-drama`. Maybe you decide the latter really should be combined with the former. Use the `Rename-JournalTag` function to do exactly that. 

## Complete version history

Every `journal-cli` command that alters the state of your journal automatically captures the changes with a git commit. Having a complete and permanent editing history allows you to easily view all changes made to specific entries, and undo most accidental changes  - such as a regrettable tag rename operation. You do not need to install git separately because it is fully integrated with `journal-cli`.

# How do I get it?

Check out the [Getting Started](https://journalcli.me/docs/getting-started) guide, which includes installation instructions.

# A word about performance

I've only tested this tool with a few hundred files on a very fast machine. I've made no deliberate effort to maximize performance. If you run this against thousands (or more!) files and/or on a slower machine, I can't promise Ferrari-like processing speeds. But if the tool feels laggy, [hit me up](https://github.com/refactorsaurusrex/journal-cli/issues) and let's fix it!

# Bugs / Suggestions

So far, I've written this tool with exactly one user in mind: **me**. That seems like a prudent choice since I'm not sure how many people out there share my particular obsession with markdown, command line tools, and data security. That said, if you like the idea behind `journal-cli` but feel something is missing or could be improved, let's talk. Open an [issue][issues] or [email me][profile]. (I can't promise I will accept changes that weren't agreed upon in advance, so please open the communication lines before writing any code.)

[dl]: https://dynalist.io/
[tf]: https://www.typeform.com/
[id]: https://inkdrop.app/
[id-journal]: https://forum.inkdrop.app/t/save-the-currently-selected-notebook/883/6
[issues]: https://github.com/refactorsaurusrex/journal-cli/issues
[profile]: https://github.com/refactorsaurusrex
