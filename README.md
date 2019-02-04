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

To parse commits, clio must have a range to consider. There are two formats available:

- `--base` `--branch` - This uses git cherry to determine what commits would need to be cherry-picked from base to branch to bring branch "up to speed".
    - This case remove "merge echos" that raw hash ranges can introduce
- `--oldest` `--newest` - This passes a raw range of commits to git for consideration. Equivilant to git log OLD_HASH..NEW_HASH.
    - Do note that due to how git handles merges, merge parents from release ago could show up in this list.


## Actions

The two current actions (beyond help) are:

- list-commits: List the commits that are under consideration based upon the path given, --oldest, and --newest
- list-bugs: Print a markdown formatted table of the bugs found in the commit range in question. A secondary set of "potential" bugs may come after, which will require manual verification.

## Bugzilla Validation

By default, clio will contact bugzilla.xamarin.com and verify potential bugs against the public bug list. This takes significant amount of time, but reduces false positives and provides better title information in many cases.

Two additional options can change this behavor (beyond --bugzilla:public which is default)

- --bugzilla:private - Log into bugzilla with credentials stored in ~/.bugzilla or the ```BUGZILLA_LOGIN``` ```BUGZILLA_PASSWORD``` environmental variables, which allows private bugs to be listed and verified
- --bugzilla:disable - Disable all bugzilla validation. Will drastically improve speed but may reduce bug sorting quality.

By default bugzilla valiation is set to public, which means private bugs will be called out as "Potential bugs". Additionally, they will not have bugzilla titles added as additional descriptions to choose from in bug listings. This may be desired behavior, if private bugs are not considered release notes appropriate. This behavior can be changed with the aforementioned ```--bugzilla:private``` option.

## Github Validation

To enable github validation both `--github` and `--github-pat` need to be passed, as github has strict rate limiting that will be hit even in the most trivial case. See the [documentation](https://help.github.com/articles/creating-a-personal-access-token-for-the-command-line/) for details on how to create a PAT.

## VSTS Validation

To enable VSTS validation `--vsts=enable` and `--vsts-path` need to be passed, for similar reasons as github.

## Full Argument Listing

```
clio [options] path
--list-bugs is the default option when none of (--list-commits, --list-bugs) selected.

  -h, -?, --help             Displays the help
  -l, --list-commits         List commits that would be considered
  -b, --list-bugs            List bugs discovered instead of formatting release
                               notes
  -x, --export-bugs          Export bugs discovered instead of formatting
                               release notes
      --explain-commit=VALUE Parse a single commit and explain.
      --output-file=VALUE    Output file for export-bugs
      --oldest=VALUE         Starting hash to consider (hash range mode)
      --newest=VALUE         Ending hash to consider (hash range mode)
      --base=VALUE           Starting base to consider (branch/base mode)
      --branch=VALUE         Ending branch to consider (branch/base mode)
      --explain              Explain why each commit is considered a bug
      --vsts=VALUE           Determines if VSTS issues should be validated or
                               not (enable, disable). The default is `disable`
      --github-pat=VALUE     Sets the PAT required to access github issues
      --github-pat-file=VALUE
                             Sets a file to read to use for github-path
      --vsts-pat=VALUE       Sets the PAT required to access VSTS issues
      --github=VALUE         Project to search issues of, such as xamarin/
                               xamarin-macios. Must be '/' seperated
      --ignore-vsts          Ignores VSTS issues and does not attempt to parse
                               commits for VSTS issues
      --additional-bug-info  Print additional information on each bug for list-
                               bugs
      --split-enhancement=VALUE
                             Split out enhancement bugs from others in listing (
                               defaults to true)
      --collect-authors      Generate a list of unique authors to commits listed
      --ignore=VALUE         Commit hashes to ignore
  @file                      Read response file for more options.
```

## "Under the hood" documentation

[TechnicalOverview.md](docs/TechnicalOverview.md)

## Why the name clio?

From [wikipedia](https://en.wikipedia.org/wiki/Clio) - In Greek mythology, Clio also spelled Kleio, is the muse of history...Clio, sometimes referred to as "the Proclaimer", is often represented with an open scroll of parchment scroll or a set of tablets. The name is etymologically derived from the Greek root κλέω/κλείω (meaning "to recount," "to make famous," or "to celebrate").
