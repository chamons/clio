# clio
Generate release notes from git history

## Building

git submodule update --recursive --init
nuget restore clio.sln
make release

The contents of dist are relocatable where ever you desire.

## Quickstart

./dist/clio

```
clio [options] path
One action (--list-commits, --list-bugs, --format-notes) must be selected.

  -h, -?, --help             Displays the help
  -l, --list-commits         List commits that would be considered
  -b, --list-bugs            List bugs discovered instead of formatting release
                               notes
  -f, --format-notes=VALUE   Output formatted release notes of a given type    
                               (Mac iOS Android)
  -o, --output=VALUE         Path to output release notes (Defaults to current
                               directory)
      --oldest=VALUE         Starting to consider
      --newest=VALUE         Ending to consider
      --single=VALUE         Analyze just a single commit
      --exclude-oldest       Exclude oldest item from range considered (
                               included by default)
      --ignore-low-bugs=VALUE
                             Ignore any bug references to bugs with IDs less
                               than 1000 (Defaults to true)
      --explain              Explain why each commit is considered a bug
      --disable-bugzilla     Disable bugzilla validation of bugs. May increase
                               false positive bugs but drastically reduce time
                               taken.
```

The three current actions (beyond help) are:

- list-commits: List the commits that are under consideration based upon the path given, --oldest, and --newest
- list-bugs: Print a markdown formatted table of the bugs found in the commit range in question. A secondary set of "potential" bugs may come after, which will require manual verification.
- format-notes: Instance up full release notes based upon saved templates and insert the bug list directly. PR are welcome to add additional formats.

## Why the name clio?

From [wikipedia](https://en.wikipedia.org/wiki/Clio) - In Greek mythology, Clio also spelled Kleio, is the muse of history...Clio, sometimes referred to as "the Proclaimer", is often represented with an open scroll of parchment scroll or a set of tablets. The name is etymologically derived from the Greek root κλέω/κλείω (meaning "to recount," "to make famous," or "to celebrate").
