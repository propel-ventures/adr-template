#!/usr/bin/env dotnet
// Build an ADR's index.html from its README.md.
//
// Each ADR folder holds two sources:
//     README.md    every section except the options table: Status, Problem statement,
//                  Decision criteria, an Options section that only links to the page, POC,
//                  Interesting combinations, Decision and Impact
//     index.html   the whole ADR; the source of truth for the options table, which sits
//                  between <!-- options:start --> and <!-- options:end -->
//
// This script converts the README with pandoc and writes it into index.html around the options
// block: everything before "## Options" goes between the readme:head markers, and everything after
// the Options section goes between the readme:tail markers. The <title> follows the README's H1.
// It never touches the options block.
//
//     .claude/skills/create-adr/assets/build.cs adrs/0001-short-title
//     .claude/skills/create-adr/assets/build.cs adrs/0001-short-title --check
//         (--check writes nothing, and exits 1 when index.html is out of date with README.md)

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.RegularExpressions;

var check = args.Contains("--check");
var rest = args.Where(a => a != "--check").ToArray();
if (rest.Length != 1)
    Fail("usage: build.cs <adr folder> [--check]");

var adr = Path.GetFullPath(rest[0]);
var readmePath = Path.Combine(adr, "README.md");
var pagePath = Path.Combine(adr, "index.html");
foreach (var p in new[] { readmePath, pagePath })
    if (!File.Exists(p))
        Fail($"missing {p}");

var readme = File.ReadAllText(readmePath);
var page = File.ReadAllText(pagePath);
if (!page.Contains("<!-- options:start -->"))
    Fail("index.html has no options block");

var title = Regex.Match(readme, @"^# (.+)$", RegexOptions.Multiline);
if (!title.Success)
    Fail("README.md has no H1 title");
var name = Regex.Replace(title.Groups[1].Value.Trim(), @"^\d+\.\s*", "");

var (head, tail) = SplitReadme(readme);
var updated = FillBlock(page, "readme:head", MarkdownToHtml(head));
updated = FillBlock(updated, "readme:tail", MarkdownToHtml(tail));
updated = new Regex("<title>.*?</title>").Replace(updated, _ => $"<title>{WebUtility.HtmlEncode(name)}</title>", 1);

if (check)
{
    if (updated != page)
    {
        Console.Error.WriteLine($"{pagePath} is out of date with README.md: run build.cs");
        return 1;
    }
    Console.WriteLine("index.html is up to date");
    return 0;
}

if (updated != page)
    File.WriteAllText(pagePath, updated);
Console.WriteLine($"Wrote {Path.GetRelativePath(Directory.GetCurrentDirectory(), pagePath)}");
return 0;

[DoesNotReturn]
static void Fail(string message)
{
    Console.Error.WriteLine($"build.cs: {message}");
    Environment.Exit(1);
}

static string MarkdownToHtml(string md)
{
    if (string.IsNullOrWhiteSpace(md))
        return "";
    var psi = new ProcessStartInfo("pandoc", ["-f", "gfm", "-t", "html5", "--columns=100000"])
    {
        RedirectStandardInput = true,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
    };
    using var pandoc = Process.Start(psi) ?? throw new InvalidOperationException("could not start pandoc");
    pandoc.StandardInput.Write(md);
    pandoc.StandardInput.Close();
    var output = pandoc.StandardOutput.ReadToEnd();
    var errors = pandoc.StandardError.ReadToEnd();
    pandoc.WaitForExit();
    if (pandoc.ExitCode != 0)
        Fail($"pandoc failed: {errors.Trim()}");
    // Every table gets its own scroll box, so the page never scrolls sideways.
    output = output.Replace("<table>", "<div class=\"table-wrap\"><table>").Replace("</table>", "</table></div>");
    return output.Trim();
}

// Return (head, tail): the README before "## Options", and after the Options section.
static (string Head, string Tail) SplitReadme(string text)
{
    var lines = Regex.Split(text, @"(?<=\n)");
    var starts = Enumerable.Range(0, lines.Length).Where(i => lines[i].TrimEnd() == "## Options").ToList();
    if (starts.Count != 1)
        Fail("README.md needs exactly one \"## Options\" heading");
    var start = starts[0];
    var end = Enumerable.Range(start + 1, lines.Length - start - 1).FirstOrDefault(i => lines[i].StartsWith("## "), lines.Length);
    return (string.Concat(lines[..start]), string.Concat(lines[end..]));
}

static string FillBlock(string page, string name, string body)
{
    var pattern = new Regex($@"(<!-- {Regex.Escape(name)}:start -->\n).*?(<!-- {Regex.Escape(name)}:end -->)", RegexOptions.Singleline);
    if (pattern.Matches(page).Count != 1)
        Fail($"index.html needs exactly one <!-- {name}:start --> ... <!-- {name}:end --> pair");
    return pattern.Replace(page, m => m.Groups[1].Value + (body.Length > 0 ? body + "\n" : "") + m.Groups[2].Value);
}
