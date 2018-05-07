## Technical Overview 

clio is build upon the concept of a pipeline of different "passes", each extracting data from the previous and outputing it in a format either directly consuable or able to be passed onto the next.

These passes currently are:

- CommitFinder: Given a path and search range, determine which commits we should consider
- CommitParser: Given a set of commits, determine which potentially reference bugs and optionally determine confidence by cross referencing with Bugzilla/Github/VSTS's parser
- BugCollector: Given a set of ParsedCommits catagorize them into lists, handling duplicate references correctly

In the long term, CommitParser should really be broken into two steps.

## Workflow

- Main is found in EntryPoint.cs and it handles argument parsing and validation. 
- A clio object in instanced with the SearchOptions and SearchRange pased from arguments and handles setting up the pipeline.
- The pipeline gets a bit complicated with submodules (which require recursively calling Process) and --oldest-branch (which requires processing to calculate commits to ignore and a new --oldest to use).

Each pipeline step is an independent class, and in theory the entire clio.exe could be referenced as a library in other projects to reuse functionality.

## Explain

The --explain option outputs a significant amount of spam to try to explain what is going on under the hood. There are a number of places where it can be expanded, and many bug reports should likely.