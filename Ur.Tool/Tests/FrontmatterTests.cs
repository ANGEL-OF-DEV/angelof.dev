using Ur.Tool.Infra;

namespace Ur.Tool.Tests;

public class FrontmatterTests
{
  [Test]
  public async Task Parses_frontmatter_and_keys()
  {
    var doc = @"---
id: X
schema_id: doc
schema_version: 1
content_version: 1
status: enacted
date: 2026-01-24T00:00:00Z
steward: s
type: doc
title: t
---
# Hello
";
    var parsed = Frontmatter.ParseYamlFrontmatter(doc);

    await Assert.That(parsed.HasFrontmatter).IsTrue();
    await Assert.That(parsed.Error).IsNull();
    await Assert.That(parsed.Keys.ContainsKey("id")).IsTrue();
    await Assert.That(parsed.Keys["schema_id"]).IsEqualTo("doc");
  }

  [Test]
  public async Task Reports_missing_terminator()
  {
    var doc = @"---
id: X
";
    var parsed = Frontmatter.ParseYamlFrontmatter(doc);

    await Assert.That(parsed.HasFrontmatter).IsTrue();
    await Assert.That(parsed.Error).IsNotNull();
  }
}
