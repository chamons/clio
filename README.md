# clio

Clio automates searaching git history to determine what bug fixes went in to a given release.

It currently understand the following formats in commit notes:

```
https://bugzilla.xamarin.com/show_bug.cgi?id=####
bugzilla ####
bug ####
fix ####
bxc ####

with an optional # before the number for the last four.
```

and will cross reference bugzilla to verify the existance of referenced bugs when possible.

## Building

```
make prepare
make release
```

The contents of dist are relocatable where ever you desire.

## Quickstart

```
./dist/clio [ACTION] [OPTIONS] PATH_TO_GIT_CHECKOUT
```

## Examples:

**What bugs were fixed after a given hash**:

``` --oldest:bcfddc9 clio-test-data/```

**What mono bugs were fixed in a release branch (clio currently does not understand tags)**:

```--list-bugs --oldest:`git rev-list -1 mono-5.2.0.213` --newest:origin/2017-04 mono/```

**What bugs were fixed after d15-3 branched to current d15-4, excluding cherry picks to d15-3, including submodule bumped**:

```--list-bugs --submodules --oldest-branch:d15-3 --newest=d15-4 xamarin-macios```

**Start release notes for a release based on a built in template**:

```--bugzilla:private --oldest-branch:d15-3 --newest=d15-4 --format-notes:Mac -o:xamarin.mac_3.8.md xamarin-macios/```


## Actions

The three current actions (beyond help) are:

- list-commits: List the commits that are under consideration based upon the path given, --oldest, and --newest
- list-bugs: Print a markdown formatted table of the bugs found in the commit range in question. A secondary set of "potential" bugs may come after, which will require manual verification.
- format-notes: Instance up full release notes based upon saved templates and insert the bug list directly. PR are welcome to add additional formats.

## Bugzilla Validation

By default, clio will contact bugzilla.xamarin.com and verify potential bugs against the public bug list. This takes significant amount of time, but reduces false positives and provides better title information in many cases.

Two additional options can change this behavor (beyond --bugzilla:public which is default)

- --bugzilla:private - Log into bugzilla with credentials stored in ~/.bugzilla or the ```BUGZILLA_LOGIN``` ```BUGZILLA_PASSWORD``` environmental variables, which allows private bugs to be listed and verified
- --bugzilla:disable - Disable all bugzilla validation. Will drastically improve speed but may reduce bug sorting quality.

By default bugzilla valiation is set to public, which means private bugs will be called out as "Potential bugs". Additionally, they will not have bugzilla titles added as additional descriptions to choose from in bug listings. This may be desired behavior, if private bugs are not considered release notes appropriate. This behavior can be changed with the aforementioned ```--bugzilla:private``` option.

## Full Argument Listing

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

## "Under the hood" documentation

[TechnicalOverview.md](docs/TechnicalOverview.md)

## Why the name clio?

From [wikipedia](https://en.wikipedia.org/wiki/Clio) - In Greek mythology, Clio also spelled Kleio, is the muse of history...Clio, sometimes referred to as "the Proclaimer", is often represented with an open scroll of parchment scroll or a set of tablets. The name is etymologically derived from the Greek root κλέω/κλείω (meaning "to recount," "to make famous," or "to celebrate").
