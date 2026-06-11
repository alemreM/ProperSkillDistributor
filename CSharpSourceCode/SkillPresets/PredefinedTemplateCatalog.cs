using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web.Script.Serialization;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.Core;
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

            var presetJson = Path.Combine(moduleRoot, "ModuleData", "skill_presets.json");

            if (!File.Exists(presetJson))
            {
                _templatesProblem = "ModuleData/skill_presets.json is missing.";
                return new List<TemplatePreset>();
            }

            ShippedPresetSheet sheet;

            try
            {
                var serializer = new JavaScriptSerializer();
                serializer.MaxJsonLength = 4 * 1024 * 1024;
                sheet = serializer.Deserialize<ShippedPresetSheet>(File.ReadAllText(presetJson));
            }
            catch (Exception exception)
            {
                _templatesProblem = "Cant read predefined preset templates. Check skill_presets.json. " + exception.Message;
                return new List<TemplatePreset>();
            }

            if (sheet == null || sheet.presets == null || sheet.presets.Count == 0)
            {
                _templatesProblem = "skill_presets.json exists but has no preset rows.";
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

            foreach (var shippedBuild in sheet.presets)
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
                    shippedBuild.name,
                    shippedBuild.description ?? string.Empty,
                    attributes,
                    focus,
                    perks));
            }

            if (builds.Count == 0)
            {
                _templatesProblem = "No shipped template matches. Validate your game files";
            }
            else if (missingFromThisInstall.Count > 0 || skippedRows > 0)
            {
                _templatesProblem = "Templates loaded with cuts. Missing optional ids: "
                    + missingFromThisInstall.Count
                    + ", skipped rows: "
                    + skippedRows
                    + ".";
            }

            return builds;
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