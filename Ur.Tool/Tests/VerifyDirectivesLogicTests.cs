using Ur.Tool.Commands.Verify;

namespace Ur.Tool.Tests;

public class VerifyDirectivesLogicTests
{
  [Test]
  public async Task Fails_when_missing_founding_ids()
  {
    var root = Path.Combine(Path.GetTempPath(), "urtool-tests", Guid.NewGuid().ToString("n"));
    Directory.CreateDirectory(root);

    try
    {
      Directory.CreateDirectory(Path.Combine(root, "[ur]", "directives"));
      File.WriteAllText(Path.Combine(root, "[ur]", "directives", "X.ur.md"), @"---
id: 1
type: directive
title: x
content_version: 1
schema_id: directive
schema_version: 2
status: enacted
date: 2026-01-24T00:00:00Z
steward: s
---
# x
");
      var result = VerifyDirectivesLogic.Run(root);

      await Assert.That(result.Ok).IsFalse();
      await Assert.That(result.Errors.Count).IsGreaterThan(0);
    }
    finally
    {
      if (Directory.Exists(root))
        Directory.Delete(root, recursive: true);
    }
  }

  [Test]
  public async Task Detects_non_consecutive_positive_ids()
  {
    var root = Path.Combine(Path.GetTempPath(), "urtool-tests", Guid.NewGuid().ToString("n"));
    Directory.CreateDirectory(root);

    try
    {
      var d = Path.Combine(root, "[ur]", "directives");
      Directory.CreateDirectory(d);

      // Founding set
      File.WriteAllText(Path.Combine(d, "L.ur.md"), @"---
id: -3
type: directive
title: L
content_version: 1
schema_id: directive
schema_version: 2
status: enacted
date: 2026-01-24T00:00:00Z
steward: s
---
# L
");
      File.WriteAllText(Path.Combine(d, "U.ur.md"), @"---
id: -2
type: directive
title: U
content_version: 1
schema_id: directive
schema_version: 2
status: enacted
date: 2026-01-24T00:00:00Z
steward: s
---
# U
");
      File.WriteAllText(Path.Combine(d, "P.ur.md"), @"---
id: -1
type: directive
title: P
content_version: 1
schema_id: directive
schema_version: 2
status: enacted
date: 2026-01-24T00:00:00Z
steward: s
---
# P
");
      File.WriteAllText(Path.Combine(d, "F.ur.md"), @"---
id: 0
type: directive
title: F
content_version: 1
schema_id: directive
schema_version: 2
status: enacted
date: 2026-01-24T00:00:00Z
steward: s
---
# F
");

      // Gap: 1 then 3
      File.WriteAllText(Path.Combine(d, "D1.ur.md"), @"---
id: 1
type: directive
title: D1
content_version: 1
schema_id: directive
schema_version: 2
status: enacted
date: 2026-01-24T00:00:00Z
steward: s
---
# D1
");
      File.WriteAllText(Path.Combine(d, "D3.ur.md"), @"---
id: 3
type: directive
title: D3
content_version: 1
schema_id: directive
schema_version: 2
status: enacted
date: 2026-01-24T00:00:00Z
steward: s
---
# D3
");
      var result = VerifyDirectivesLogic.Run(root);

      await Assert.That(result.Ok).IsFalse();
      await Assert.That(string.Join("\n", result.Errors)).Contains("consecutive");
    }
    finally
    {
      if (Directory.Exists(root))
        Directory.Delete(root, recursive: true);
    }
  }
}
