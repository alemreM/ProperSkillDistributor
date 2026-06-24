using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web.Script.Serialization;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace ProperSkillDistributor
{
    internal static class PredefinedTemplateCatalog
    {
        private static List<TemplatePreset> _templatesForThisLoadOrder;
        private static string _templatesProblem;

        public static string LoadError
        {
            get { return _templatesProblem; }
        }

        public static List<TemplatePreset> GetTemplates()
        {
            if (_templatesForThisLoadOrder == null)
            {
                _templatesForThisLoadOrder = ReadShippedBuildsForThisCampaignSetup();
            }

            return new List<TemplatePreset>(_templatesForThisLoadOrder);
        }

        private static List<TemplatePreset> ReadShippedBuildsForThisCampaignSetup()
        {
            _templatesProblem = null;

            var moduleRoot = Path.GetFullPath(Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "..",
                ".."));

            var moduleDataPath = Path.Combine(moduleRoot, "ModuleData");
            var shippedRows = ReadPresetRows(moduleDataPath);
            var loadingProblem = _templatesProblem;

            if (shippedRows.Count == 0)
            {
                return new List<TemplatePreset>();
            }

            var knownAttributes = new HashSet<string>();
            var knownSkills = new HashSet<string>();
            var knownPerks = new HashSet<string>();

            foreach (var attribute in Attributes.All)
            {
                knownAttributes.Add(attribute.StringId);
            }

            foreach (var skill in Skills.All)
            {
                knownSkills.Add(skill.StringId);
            }

            foreach (var perk in PerkObject.All)
            {
                if (perk.Skill != null && !string.IsNullOrEmpty(perk.StringId))
                {
                    knownPerks.Add(perk.StringId);
                }
            }

            var builds = new List<TemplatePreset>();
            var missingFromThisInstall = new HashSet<string>();
            var skippedRows = 0;

            foreach (var shippedBuild in shippedRows)
            {
                if (shippedBuild == null || string.IsNullOrEmpty(shippedBuild.id) || string.IsNullOrEmpty(shippedBuild.name))
                {
                    skippedRows++;
                    continue;
                }

                var attributes = new Dictionary<string, int>();
                var focus = new Dictionary<string, int>();
                var perks = new List<string>();
                var perksAlreadyAdded = new HashSet<string>();

                if (shippedBuild.attributes != null)
                {
                    foreach (var target in shippedBuild.attributes)
                    {
                        if (knownAttributes.Contains(target.Key) && target.Value > 0)
                        {
                            attributes[target.Key] = target.Value;
                        }
                        else if (!string.IsNullOrEmpty(target.Key))
                        {
                            missingFromThisInstall.Add(target.Key);
                        }
                    }
                }

                if (shippedBuild.focus != null)
                {
                    foreach (var target in shippedBuild.focus)
                    {
                        if (knownSkills.Contains(target.Key) && target.Value > 0)
                        {
                            focus[target.Key] = target.Value;
                        }
                        else if (!string.IsNullOrEmpty(target.Key))
                        {
                            missingFromThisInstall.Add(target.Key);
                        }
                    }
                }

                if (shippedBuild.perks != null)
                {
                    foreach (var perkId in shippedBuild.perks)
                    {
                        if (string.IsNullOrEmpty(perkId) || !perksAlreadyAdded.Add(perkId))
                        {
                            continue;
                        }

                        // templates are shipped along with naval dlc perks. for native compability ensure perk id exist, maybe for tor too sometime
                        if (knownPerks.Contains(perkId))
                        {
                            perks.Add(perkId);
                        }
                        else
                        {
                            missingFromThisInstall.Add(perkId);
                        }
                    }
                }

                if (attributes.Count == 0 && focus.Count == 0 && perks.Count == 0)
                {
                    skippedRows++;
                    continue;
                }

                builds.Add(new TemplatePreset(
                    shippedBuild.id,
                    LocalizeText(shippedBuild.name),
                    LocalizeText(shippedBuild.description),
                    attributes,
                    focus,
                    perks));
            }

            var problemNotes = new List<string>();

            if (!string.IsNullOrEmpty(loadingProblem))
            {
                problemNotes.Add(loadingProblem);
            }

            if (builds.Count == 0)
            {
                problemNotes.Add(new TextObject("{=no_shipped_templates}No shipped template matches. Validate your game files").ToString());
            }
            else if (missingFromThisInstall.Count > 0 || skippedRows > 0)
            {
                var cutMessage = new TextObject("{=templates_loaded_with_cuts}Templates loaded with cuts. Missing optional ids: {MISSING_COUNT}, skipped rows: {SKIPPED_COUNT}.");
                cutMessage.SetTextVariable("MISSING_COUNT", missingFromThisInstall.Count);
                cutMessage.SetTextVariable("SKIPPED_COUNT", skippedRows);
                problemNotes.Add(cutMessage.ToString());
            }

            _templatesProblem = problemNotes.Count > 0 ? string.Join(" ", problemNotes.ToArray()) : null;

            return builds;
        }

        private static List<ShippedPresetRow> ReadPresetRows(string moduleDataPath)
        {
            var presetFiles = new List<string>();
            presetFiles.Add("skill_presets.json");

            if (TorCompatibility.IsLoaded)
            {
                presetFiles.Add("skill_presets_tor.json");
            }

            if (File.Exists(Path.Combine(moduleDataPath, "your_presets.json")))
            {
                presetFiles.Add("your_presets.json");
            }

            var rows = new List<ShippedPresetRow>();
            var readProblems = new List<string>();
            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = 8 * 1024 * 1024;

            foreach (var presetFile in presetFiles)
            {
                var presetJson = Path.Combine(moduleDataPath, presetFile);

                if (!File.Exists(presetJson))
                {
                    if (presetFile == "skill_presets.json")
                    {
                        var missingTemplateFile = new TextObject("{=template_file_missing}ModuleData/{FILE} is missing.");
                        missingTemplateFile.SetTextVariable("FILE", "skill_presets.json");
                        _templatesProblem = missingTemplateFile.ToString();
                        return new List<ShippedPresetRow>();
                    }

                    var missingOptionalFile = new TextObject("{=template_file_missing}ModuleData/{FILE} is missing.");
                    missingOptionalFile.SetTextVariable("FILE", presetFile);
                    readProblems.Add(missingOptionalFile.ToString());
                    continue;
                }

                ShippedPresetSheet sheet;

                try
                {
                    sheet = serializer.Deserialize<ShippedPresetSheet>(File.ReadAllText(presetJson));
                }
                catch (Exception exception)
                {
                    if (presetFile == "skill_presets.json")
                    {
                        var unreadableTemplateFile = new TextObject("{=cant_read_predefined_templates}Cant read predefined preset templates. Check {FILE}. {REASON}");
                        unreadableTemplateFile.SetTextVariable("FILE", "skill_presets.json");
                        unreadableTemplateFile.SetTextVariable("REASON", exception.Message);
                        _templatesProblem = unreadableTemplateFile.ToString();
                        return new List<ShippedPresetRow>();
                    }

                    var unreadableOptionalFile = new TextObject("{=cant_read_optional_templates}Cant read optional preset templates. Check {FILE}. {REASON}");
                    unreadableOptionalFile.SetTextVariable("FILE", presetFile);
                    unreadableOptionalFile.SetTextVariable("REASON", exception.Message);
                    readProblems.Add(unreadableOptionalFile.ToString());
                    continue;
                }

                if (sheet == null || sheet.presets == null || sheet.presets.Count == 0)
                {
                    if (presetFile == "skill_presets.json")
                    {
                        var emptyTemplateFile = new TextObject("{=template_file_no_rows}{FILE} exists but has no preset rows.");
                        emptyTemplateFile.SetTextVariable("FILE", "skill_presets.json");
                        _templatesProblem = emptyTemplateFile.ToString();
                        return new List<ShippedPresetRow>();
                    }

                    var emptyOptionalFile = new TextObject("{=template_file_no_rows}{FILE} exists but has no preset rows.");
                    emptyOptionalFile.SetTextVariable("FILE", presetFile);
                    readProblems.Add(emptyOptionalFile.ToString());
                    continue;
                }

                rows.AddRange(sheet.presets);
            }

            _templatesProblem = readProblems.Count > 0 ? string.Join(" ", readProblems.ToArray()) : null;

            return rows;
        }


        private static string LocalizeText(string rawText)
        {
            return string.IsNullOrEmpty(rawText) ? string.Empty : new TextObject(rawText).ToString();
        }

        private sealed class ShippedPresetSheet
        {
            public List<ShippedPresetRow> presets { get; set; }
        }

        private sealed class ShippedPresetRow
        {
            public string id { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public Dictionary<string, int> attributes { get; set; }
            public Dictionary<string, int> focus { get; set; }
            public List<string> perks { get; set; }
        }
    }
}