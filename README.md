# Architecture Decision Records

> An Architecture Decision Record (ADR) is a short document that captures and explains a single decision relevant to a product or ecosystem. Documents should be short, just a couple of pages, and contain the decision, the context for making it, and significant ramifications. They should not be modified if the decision is changed, but linked to a superseding decision.
>
> Martin Fowler, [Architecture Decision Record](https://martinfowler.com/bliki/ArchitectureDecisionRecord.html)

This repository is a starter kit for recording architecture, workflow and design decisions in ADR format. Use it as a GitHub template: create a new repository from it for each client or product, then add ADRs with the `create-adr` skill.

## Structure
- `adrs/`: every decision. Each ADR is a numbered folder (for example `adrs/0001-short-title/`) with a `README.md` and an `index.html`.
- `.claude/skills/create-adr/`: the Claude Code skill that creates, updates and closes ADRs. Its `assets/` hold the README template (`template.md`), the page template (`template.html`) and `build.cs`, which builds each ADR's page from its README.

## Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download) to run `build.cs`
- [pandoc](https://pandoc.org/installing.html), which `build.cs` calls to convert Markdown

## List of ADRs
_None yet._

## How to add a new ADR

Run this in Claude Code, from this repo:

```
/create-adr [topic or problem statement]
```

The `create-adr` skill asks you for anything it is missing: the problem, the criteria, the options and any POC candidates. Then it:

1. Picks the next free ADR number (it checks open branches and PRs too).
2. Creates `adrs/NNNN-short-slug/` with a `README.md` and an `index.html`.
3. Adds the ADR to the list above.

Each ADR has two sources:

- `README.md` holds every section except the options table.
- `index.html` holds the options table, and is the page you open in a browser to read the whole ADR.

After you edit either one, rebuild the page:

```
.claude/skills/create-adr/assets/build.cs adrs/NNNN-short-slug
```
