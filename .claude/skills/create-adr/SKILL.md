---
name: create-adr
description: 'Create, update and close Architecture Decision Records (ADRs). Each ADR is a folder with a README.md (every section except the options table) and an index.html page (the whole ADR, and the source of truth for the options table), viewed locally in a browser. Use when the user asks to write, create, draft, update or decide an ADR or decision document.'
argument-hint: 'Topic or problem statement for the ADR'
user-invocable: true
disable-model-invocation: false
---

# Create and maintain an ADR

## The format

Each ADR is a folder `adrs/NNNN-short-slug/` with two sources:

| File | Holds | Edit it for |
| --- | --- | --- |
| `README.md` | Status (at the top), Problem statement, Decision criteria, Options (two links only), POC, Interesting combinations, Decision, Impact | every section except the options table |
| `index.html` | the whole ADR: the README sections, built in, plus the options table between `<!-- options:start -->` and `<!-- options:end -->` | the options table only |

`.claude/skills/create-adr/assets/build.cs` is a .NET file-based app. It converts the README with `pandoc`, copies it into `index.html` around the options block, and sets the page `<title>` from the README's H1. It never touches the options block. It needs the .NET 10 SDK and `pandoc`.

Section rules:

- **Status** comes first, with the event table. Use one of: Proposed, In POC, Accepted, Rejected, Deprecated, Superseded by ADR NNNN.
- **Options** in the README holds exactly two links, both to `index.html`: one to view it, one to edit the table. The table itself lives only in `index.html`.
- **POC** lists the options that go through a proof of concept. When none do, write "None: the options table settles the decision." and delete the table.
- **Interesting combinations** lists pairs of options that fit together naturally and look better together on paper than either does alone. List only strong pairs, and at most three, with the criteria each pair passes that each option fails alone. When no pair stands out, write "None." and delete the table.
- **Decision** and **Impact** keep the placeholder text while the Status is Proposed or In POC. Fill them in only when the Status becomes Accepted or Rejected.

## Create a new ADR

- where to look = `assets/template.md` and `assets/template.html` in this skill, `adrs/` for existing numbers, and `git ls-remote --heads origin` plus `gh pr list --state open` for numbers already taken on open branches (skip these two when the repo has no remote)
- definition of done = a folder `adrs/NNNN-short-slug/` with `README.md` and `index.html`, and a new line in the root `README.md` under `## List of ADRs`
- self-check = `build.cs <folder> --check` prints "index.html is up to date"; the page script parses; the README has no options table; Decision and Impact hold placeholders unless the Status is Accepted or Rejected

1. Collect the problem statement, the decision criteria with their Pass, Partial and Fail bars, the options, any POC candidates, and any strong pairs of options. Ask the user for each piece that is missing, and leave a bracketed `[unknown: ...]` gap for anything they cannot answer yet.
2. Pick `NNNN` as one more than the highest number in `adrs/` and on open branches or PRs. Start at `0001` when `adrs/` is empty. When an open PR already uses the next number, tell the user and ask which number to take.
3. Copy `assets/template.md` to `adrs/NNNN-short-slug/README.md`, and `assets/template.html` to `adrs/NNNN-short-slug/index.html`.
4. Fill the README sections. Start the H1 with the number: `# 1. Title`.
5. Fill the options table in `index.html`:
   - The first header cell is `Aspect`. Each other header is `N. Option name`.
   - The first row is **Description**. Each later row is one criterion, named `N. Criterion` to match the README.
   - Start every verdict cell with 🟢, 🟡 or 🔴, then the verdict word in bold. The page scores options from these marks.
   - Put each cell's evidence and source links in the cell as `•` lines joined with `<br>`.
   - Edit the `#options-config` JSON: one filter for each criterion a reader may want to hide failures on, the tie-break rows in priority order, and optionally `defaultBest` (a number) to open the page showing only the best N options until the viewer changes the view.
6. Run `.claude/skills/create-adr/assets/build.cs adrs/NNNN-short-slug`.
7. Add the ADR to the root `README.md` under `## List of ADRs`, with a link to its `index.html` for the options table.
8. Run the self-check above, then report the folder and any `[unknown: ...]` gaps left. Tell the user they can open `index.html` in a browser to see the page.

## Update an ADR

- where to look = the ADR's `README.md` for prose, and its `index.html` options block for options
- definition of done = the source edited and the page rebuilt
- self-check = `build.cs <folder> --check` prints "index.html is up to date"

1. Edit the prose in `README.md`, or the options table in `index.html`. Update `Last Updated` in the README's event table.
2. Run `.claude/skills/create-adr/assets/build.cs adrs/NNNN-short-slug`.

## Close an ADR with a decision

- where to look = the ADR's `README.md`, the options table and the POC results
- definition of done = Status Accepted (or Rejected) with its date, and Decision and Impact filled in
- self-check = the Decision names one option and cites criteria by number; the Impact covers both good and bad effects

1. Set the Status to Accepted or Rejected, and fill in `Accepted` in the event table.
2. Write the Decision: the option chosen, and why it beat the runners-up, citing criteria by number.
3. Write the Impact: the effect on other systems, processes and future work, good and bad.
4. Rebuild as in [Update an ADR](#update-an-adr).
