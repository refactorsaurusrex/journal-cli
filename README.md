# What is this?

`journal-cli` is a command line tool for anyone who journals using markdown files. 

...it's _most_ useful for people who journal with markdown files, love command lines tools, and think [Typora](https://typora.io/) is the bee's knees... but wish Typora had some sort of file tagging mechanism built in. 

# Background

Sometime late in 2017, I found myself flipping through _really_ old entries in my Google Calendar. Turns out I have entries in there going back to 2001. It was interesting taking a stroll down memory lane and seeing everything from the mundane ("Dentist Appt") to the exciting ("First day at new job!"). It got me thinking that I should make an effort to write down the highlights of my day _every day_ because future-me might be interested in knowing what I did on, say, December 13th, 2018. So I made the decision to start keeping a journal. 

For the first six months or so I used [Dynalist][dl], a tool I absolutely love. I was never particularly comfortable spelling out my life's details in a tool that didn't offer end-to-end encryption, but I stuck my head in the sand for as long as I could. Then in June of 2018 [Typeform][tf] - another tool I absolutely love - suffered a data breach, and some of my data was stolen. Luckily, the data taken from my account wasn't at all sensitive. But the incident cemented my fears that _nothing is safe in the cloud_. I resolved to find another, safer tool to use for journaling. 

My next stop was [Inkdrop][id]. It's not designed with journaling in mind, but I found it fairly suitable for that purpose - as long as you don't sync your files to Inkdrop's servers. Unfortunately, after a while Inkdrop felt too constraining. No cloud syncing meant I could only edit from a single computer. I was also irked by the Inkdrop developer's rigid opposition to adding any new features that [deviate even slightly from his narrowly defined product scope][id-journal]. I really wanted to use [Typora](https://typora.io/) - hands-down the best markdown editor available - but it lacked one really important feature. It offered no built-in mechanism for tagging files. Tags are essential for me because they basically constitute my journal's index.  

Finally, I had a realization. *Typora supports yaml front matter. I can use front matter to add tags to each file and create a command line tool to parse the tags from each journal file!* Thus, `journal-cli` was born. My current journaling setup is Typora for writing, [Cryptomator](https://cryptomator.org/) for encryption, Dropbox for synchronization, and `journal-cli`  for everything listed in the 'What can it do?' section below.

# Sooo... what can it do? 

> This is just a quick summary. Refer to the [wiki](https://github.com/refactorsaurusrex/journal-cli/wiki) for more detailed information. (If the wiki is lacking in some way, check back later. I'll be updating it more frequently than this readme.)

## Create an index of your journal

Run `Get-JournalIndex` to scan all your journal files and create an index of your entire journal. This allows you to:

- Display a list of all tags used in your journal.
- Sort tags by name or count, in ascending or descending order.
- List all journal entries containing specific tags.
- Show the headers from a subset of journal entries, for an overview of the topics contained in each entry. (Note: This assumes you use headers in your entries.)

## Create new journal entries

Run a single command to create a new journal entry for today and save the file in the appropriate year and month folders using a specific date-based naming convention. (Example: `2018.12.13.md`) 

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

Let's say you have a few dozen entries with the tag `family` and a few dozen more with the tag `family-drama`. Maybe you decide the latter really should be combined with the former. Use the `Rename-JournalTag` function to do exactly that. By default, this function creates backup copies of the original entries - just in case you change your mind. To permanently delete these backups, you can run `Remove-OldFiles`. (The backup files are saved with an `.old` file extension.)

# Getting Started

## Prerequisites

There are only two _real_ requirements in order to use `journal-cli`. First, you need to have a collection of `*.md` files.  They don't even need to be "journal" entries. They can be anything you want... as long as they are text-based and have use the "md" file extension. Second, at least some of those files must include Yaml front matter in the format shown below. The `tags` element is required, but you can create any tag names you want. That's it. 

> If you use Typora, you can go to the Paragraph menu and select "YAML Front Matter"

```yaml
  - tags:
    - MyTag
    - MyOtherTag
```

Most functions in this tool don't even care how your folders and files are laid out. However, `New-JournalEntry` is different. It assumes a directory structure like this:

```yaml
[rootDirectory]\year\month\year.month.day.md
```

If `journal-cli` actually attracts attention from people other than myself, alternative layouts can easily be added. 

## Installation

While `journal-cli` targets PowerShell 6, thus making it theoretically cross-platform, I haven't yet made any effort to get it working on Mac or Linux. So, for now, this is Windows only.

I also haven't published the code to any package management feeds yet. Eventually it will be installable from [Chocolatey](https://chocolatey.org/) and the [PowerShellGallery](https://www.powershellgallery.com). For the immediate term, the only way to get the tool is to clone the repo and run the code directly. Super lame, I know, but it won't be this way for long.

### Usage

- Open PowerShell. Type your desired function. Press `Enter`. Be amazed.
- Run `Get-Command -Module JournalCli` to display a list of all functions. 
- Run `Get-Help <function_name>` for syntax and usage notes for a particular function.

# A word about performance

I've only tested this tool with a few hundred files on a very fast machine. I've made no deliberate effort to maximize performance. If you run this against thousands (or more!) files and/or on a slower machine, I can't promise Ferrari-like processing speeds. But if the tool feels laggy, hit me up and let's fix it!

# Bugs / Suggestions

So far, I've written this tool with exactly one user in mind: **me**. That seems like a prudent choice since I think it's unlikely that many people will share my particular obsession with markdown, command line tools, and data security. That said, if you like the _idea_ behind `journal-cli` but don't like how specific things are implemented, let's talk. Open an [issue][issues] or [email me][profile]. I'm definitely open to genericizing the tool if folks are interested. Otherwise, I'll just keep targeting myself as the Single Most Important User. :)

[dl]: https://dynalist.io/
[tf]: https://www.typeform.com/
[id]: https://inkdrop.app/
[id-journal]: https://forum.inkdrop.app/t/save-the-currently-selected-notebook/883/6
[issues]: https://github.com/refactorsaurusrex/journal-cli/issues
[profile]: https://github.com/refactorsaurusrex
