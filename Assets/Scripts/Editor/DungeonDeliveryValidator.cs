using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class DungeonDeliveryValidator
{
    private static readonly string[] RequiredDocs =
    {
        "README.md",
        "WORKLOG.md",
        "MEMORIA_BORRADOR.md",
        "MEMORIA_ENTREGA_BORRADOR.md",
        "Docs/REQUIREMENTS_AUDIT.md",
        "Docs/PLAYTEST_CHECKLIST.md",
        "Docs/DEMO_SCRIPT.md",
        "Docs/MERGE_CHECKLIST.md",
        "Docs/PR_DESCRIPTION_DRAFT.md",
        "Docs/HANDOFF_NEXT_STEPS.md",
        "Docs/RUBRIC_SCORECARD.md",
        "Docs/Captures/README.md"
    };

    private static readonly string[] RequiredTools =
    {
        "Tools/run_validation.sh"
    };

    private static readonly string[] RequiredCaptures =
    {
        "Docs/Captures/01_level1_start.png",
        "Docs/Captures/02_level6_traps.png",
        "Docs/Captures/03_level9_falling_floor.png",
        "Docs/Captures/04_boss_room.png"
    };

    public static void Run()
    {
        try
        {
            ValidateDocsExist();
            ValidateToolsExist();
            ValidateUnityMetaFiles();
            ValidateCapturesExist();
            ValidateCurrentReferences();
            Debug.Log("[DungeonDeliveryValidator] OK");
            EditorApplication.Exit(0);
        }
        catch (Exception exception)
        {
            Debug.LogError("[DungeonDeliveryValidator] FAILED\n" + exception);
            EditorApplication.Exit(1);
        }
    }

    private static void ValidateToolsExist()
    {
        foreach (string path in RequiredTools)
        {
            if (!File.Exists(path))
            {
                throw new InvalidOperationException("Missing delivery tool: " + path);
            }

            string text = File.ReadAllText(path);
            RequireContains(text, "DungeonBatchValidator.Run", "Validation script should run the batch validator.");
            RequireContains(text, "DungeonBuildSmoke.Run", "Validation script should run the build smoke.");
            RequireContains(text, "DungeonDeliveryValidator.Run", "Validation script should run delivery validation.");
            RequireContains(text, "LOG_ERROR_PATTERN", "Validation script should define an error log pattern.");
            RequireContains(text, "check_log", "Validation script should scan Unity logs for errors.");
            RequireContains(text, "MissingComponentException", "Validation script should fail on missing component errors.");
            RequireContains(text, "NullReferenceException", "Validation script should fail on null reference errors.");
            RequireContains(text, "error CS", "Validation script should fail on C# compiler errors.");
        }
    }

    private static void ValidateDocsExist()
    {
        foreach (string path in RequiredDocs)
        {
            if (!File.Exists(path))
            {
                throw new InvalidOperationException("Missing delivery doc: " + path);
            }

            if (new FileInfo(path).Length < 120)
            {
                throw new InvalidOperationException("Delivery doc looks too small: " + path);
            }
        }
    }

    private static void ValidateUnityMetaFiles()
    {
        string scriptsRoot = "Assets/Scripts";
        if (!Directory.Exists(scriptsRoot))
        {
            throw new InvalidOperationException("Missing scripts directory: " + scriptsRoot);
        }

        foreach (string directory in Directory.GetDirectories(scriptsRoot, "*", SearchOption.AllDirectories))
        {
            RequireMetaFile(directory);
        }

        foreach (string script in Directory.GetFiles(scriptsRoot, "*.cs", SearchOption.AllDirectories))
        {
            RequireMetaFile(script);
        }
    }

    private static void RequireMetaFile(string assetPath)
    {
        string metaPath = assetPath + ".meta";
        if (!File.Exists(metaPath))
        {
            throw new InvalidOperationException("Missing Unity meta file: " + metaPath);
        }
    }

    private static void ValidateCapturesExist()
    {
        foreach (string path in RequiredCaptures)
        {
            if (!File.Exists(path))
            {
                throw new InvalidOperationException("Missing capture: " + path);
            }

            if (new FileInfo(path).Length < 4096)
            {
                throw new InvalidOperationException("Capture looks empty or corrupted: " + path);
            }
        }
    }

    private static void ValidateCurrentReferences()
    {
        string readme = File.ReadAllText("README.md");
        RequireContains(readme, "Docs/HANDOFF_NEXT_STEPS.md", "README should link the handoff doc.");
        RequireContains(readme, "/tmp/vj-pre-merge-build.log", "README should reference the latest build log.");

        string audit = File.ReadAllText("Docs/REQUIREMENTS_AUDIT.md");
        RequireContains(audit, "/tmp/vj-pre-merge-requirements.log", "Audit should reference the latest requirements log.");
        RequireContains(audit, "Cubierto", "Audit should summarize covered requirements.");

        string memory = File.ReadAllText("MEMORIA_ENTREGA_BORRADOR.md");
        RequireContains(memory, "/tmp/vj-pre-merge-all-levels.log", "Memory should reference the latest all-levels smoke.");
        RequireContains(memory, "Docs/Captures/04_boss_room.png", "Memory should embed boss capture.");

        string handoff = File.ReadAllText("Docs/HANDOFF_NEXT_STEPS.md");
        RequireContains(handoff, "No se ha hecho commit ni push", "Handoff should preserve git/push status.");
        RequireContains(handoff, "Revision manual pendiente", "Handoff should list manual review tasks.");
        RequireContains(handoff, "Docs/RUBRIC_SCORECARD.md", "Handoff should link the rubric scorecard.");

        string scorecard = File.ReadAllText("Docs/RUBRIC_SCORECARD.md");
        RequireContains(scorecard, "Base - 4 puntos", "Scorecard should cover base score.");
        RequireContains(scorecard, "Game Feel y Arte - 4 puntos", "Scorecard should cover game-feel score.");
        RequireContains(scorecard, "Memoria - 2 puntos", "Scorecard should cover memory score.");

        string demo = File.ReadAllText("Docs/DEMO_SCRIPT.md");
        RequireContains(demo, "Boss", "Demo script should include boss coverage.");
        RequireContains(demo, "Sala 6", "Demo script should include trap coverage.");

        string merge = File.ReadAllText("Docs/MERGE_CHECKLIST.md");
        RequireContains(merge, "git diff --check", "Merge checklist should include whitespace validation.");
        RequireContains(merge, "DungeonBuildSmoke", "Merge checklist should include build validation.");

        string prDraft = File.ReadAllText("Docs/PR_DESCRIPTION_DRAFT.md");
        RequireContains(prDraft, "Tools/run_validation.sh", "PR draft should include the validation command.");
        RequireContains(prDraft, "Manual Checklist Before Merge", "PR draft should include manual merge checklist.");
        RequireContains(prDraft, "No se ha hecho commit ni push", "PR draft should preserve git/push status.");
    }

    private static void RequireContains(string text, string expected, string message)
    {
        if (!text.Contains(expected, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(message + " Missing: " + expected);
        }
    }
}
