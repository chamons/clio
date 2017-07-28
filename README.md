# clio
Generate release notes from git history

## Building

```
make prepare
make release
```

The contents of dist are relocatable where ever you desire.

## Quickstart

```
./dist/clio
```

```
clio [options] path
--list-bugs is the default option when none of (--list-commits, --list-bugs, --format-notes) selected.

  -h, -?, --help             Displays the help
  -l, --list-commits         List commits that would be considered
  -b, --list-bugs            List bugs discovered instead of formatting release
                               notes
  -f, --format-notes=VALUE   Output formatted release notes of a given type    
                               (Mac iOS Android)
  -o, --output=VALUE         Path to output release notes (Defaults to current
                               directory)
      --oldest=VALUE         Starting hash to consider
      --newest=VALUE         Ending hash to consider
      --oldest-branch=VALUE  Starting branch to consider. Finds the last commit
                               in master before branch, and ignore all bugs
                               fixed in master that are also fixed in this
                               branch.
      --single=VALUE         Analyze just a single commit
      --exclude-oldest       Exclude oldest item from range considered (
                               included by default)
      --ignore-low-bugs=VALUE
                             Ignore any bug references to bugs with IDs less
                               than 1000 (Defaults to true)
      --explain              Explain why each commit is considered a bug
      --bugzilla=VALUE       What level should bugzilla queries be made at     
                               (Public, Private, Disable)
      --sort-bug-list=VALUE  Sort bug list by id number (Defaults to true)
      --additional-bug-info  Print additional information on each bug for list-
                               bugs
      --submodules           Query submodules as well
  @file                      Read response file for more options.
```

The three current actions (beyond help) are:

- list-commits: List the commits that are under consideration based upon the path given, --oldest, and --newest
- list-bugs: Print a markdown formatted table of the bugs found in the commit range in question. A secondary set of "potential" bugs may come after, which will require manual verification.
- format-notes: Instance up full release notes based upon saved templates and insert the bug list directly. PR are welcome to add additional formats.

## Bugzilla Validation

By default, clio will contact bugzilla.xamarin.com and verify potential bugs against the public bug list. This takes significant amount of time, but reduces false positives and provides better title information in many cases.

Two additional options can change this behavor (beyond --bugzilla:public which is default)

- --bugzilla:private - Log into bugzilla with credentials stored in ~/.bugzilla or the ```BUGZILLA_LOGIN``` ```BUGZILLA_PASSWORD``` environmental variables, which allows private bugs to be listed and verified
- --bugzilla:disable - Disable all bugzilla validation. Will drastically improve speed but may reduce bug sorting quality.

## Why the name clio?

From [wikipedia](https://en.wikipedia.org/wiki/Clio) - In Greek mythology, Clio also spelled Kleio, is the muse of history...Clio, sometimes referred to as "the Proclaimer", is often represented with an open scroll of parchment scroll or a set of tablets. The name is etymologically derived from the Greek root κλέω/κλείω (meaning "to recount," "to make famous," or "to celebrate").
