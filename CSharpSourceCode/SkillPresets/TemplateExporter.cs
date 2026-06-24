using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web.Script.Serialization;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace ProperSkillDistributor
{
    internal static class TemplateExporter
    {
        [CommandLineFunctionality.CommandLineArgumentFunction("export_templates", "skillpreset")]
        public static string ExportPresetSlots(List<string> slotArgs)
        {
            var presetStore = SkillPresetBehavior.Current;

            if (presetStore == null)
            {
                return new TextObject("{=no_preset_store}No preset store").ToString();
            }

            var exportSlots = new List<SkillPreset>();

            if (slotArgs == null || slotArgs.Count == 0)
            {
                foreach (var slot in presetStore.GetPresets())
                {
                    if (CanWriteSlot(slot))
                    {
                        exportSlots.Add(slot);
                    }
                }
            }
            else
            {
                foreach (var typedSlot in slotArgs)
                {
                    int slotIndex;

                    if (!int.TryParse(typedSlot, out slotIndex))
                    {
                        continue;
                    }

                    var slot = presetStore.GetPreset(slotIndex);

                    if (CanWriteSlot(slot))
                    {
                        exportSlots.Add(slot);
                    }
                }
            }

            if (exportSlots.Count == 0)
            {
                return new TextObject("{=no_exportable_slots}No exportable preset slots").ToString();
            }

            string jsonPath;
            string failReason;

            return WriteUserPresetJson(exportSlots, null, out jsonPath, out failReason)
                ? jsonPath
                : failReason;
        }

        public static bool TryExportPreset(SkillPreset slot, string popupName, out string message)
        {
            if (slot == null || !slot.IsConfigured)
            {
                message = new TextObject("{=only_configured_export}Only configured presets can be exported.").ToString();
                return false;
            }

            if (slot.IsMimicPreset && MimicSource(slot) == null)
            {
                message = new TextObject("{=mimic_source_missing}Mimic source hero was not found, cant export snapshot.").ToString();
                return false;
            }

            string jsonPath;

            if (!WriteUserPresetJson(new List<SkillPreset> { slot }, popupName, out jsonPath, out message))
            {
                return false;
            }

            var exportedMessage = new TextObject("{=exported_to_user_file}Exported to ModuleData/{FILE}.");
            exportedMessage.SetTextVariable("FILE", "your_presets.json");
            message = exportedMessage.ToString();
            return true;
        }

        private static bool WriteUserPresetJson(List<SkillPreset> exportSlots, string popupName, out string jsonPath, out string failReason)
        {
            jsonPath = UserPresetJsonPath();
            failReason = null;

            var json = new JavaScriptSerializer();
            json.MaxJsonLength = 8 * 1024 * 1024;

            var rows = new List<UserPresetRow>();

            if (File.Exists(jsonPath))
            {
                try
                {
                    var oldFile = json.Deserialize<UserPresetFile>(File.ReadAllText(jsonPath));

                    if (oldFile != null && oldFile.presets != null)
                    {
                        foreach (var row in oldFile.presets)
                        {
                            if (row != null && !string.IsNullOrEmpty(row.id))
                            {
                                rows.Add(row);
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    var cantReadUserFile = new TextObject("{=cant_read_user_file}Cant read ModuleData/{FILE}. Fix or delete it first. Launching the game as admin may help. {REASON}");
                    cantReadUserFile.SetTextVariable("FILE", "your_presets.json");
                    cantReadUserFile.SetTextVariable("REASON", exception.Message);
                    failReason = cantReadUserFile.ToString();
                    return false;
                }
            }

            foreach (var slot in exportSlots)
            {
                var row = new UserPresetRow();
                row.id = NextUserPresetId(rows);
                row.name = exportSlots.Count == 1 && !string.IsNullOrWhiteSpace(popupName)
                    ? popupName.Trim()
                    : slot.Name;
                var exportedFromSlot = new TextObject("{=exported_from_slot}Exported from preset slot {SLOT_INDEX}.");
                exportedFromSlot.SetTextVariable("SLOT_INDEX", slot.SlotIndex);
                row.description = exportedFromSlot.ToString();
                row.attributes = new Dictionary<string, int>();
                row.focus = new Dictionary<string, int>();
                row.perks = new List<string>();

                slot.RebuildAfterLoad();

                var srcHero = slot.IsMimicPreset ? MimicSource(slot) : null;

                if (srcHero != null)
                {
                    // exported presets are portable snapshots of the heros current skill set
                    foreach (var attribute in Attributes.All)
                    {
                        int value = srcHero.GetAttributeValue(attribute);

                        if (value > 0)
                        {
                            row.attributes[attribute.StringId] = value;
                        }
                    }

                    foreach (var skill in Skills.All)
                    {
                        int value = srcHero.HeroDeveloper.GetFocus(skill);

                        if (value > 0)
                        {
                            row.focus[skill.StringId] = value;
                        }
                    }

                    foreach (var perk in PerkObject.All)
                    {
                        if (perk.Skill != null && srcHero.GetPerkValue(perk))
                        {
                            row.perks.Add(perk.StringId);
                        }
                    }

                    var mimicSnapshotDescription = new TextObject("{=snapshot_from_mimic_slot}Snapshot exported from mimic preset slot {SLOT_INDEX}.");
                    mimicSnapshotDescription.SetTextVariable("SLOT_INDEX", slot.SlotIndex);
                    row.description = mimicSnapshotDescription.ToString();
                }
                else
                {
                    foreach (var target in slot.AttributeTargets)
                    {
                        row.attributes[target.Key] = target.Value;
                    }

                    foreach (var target in slot.SkillFocusTargets)
                    {
                        row.focus[target.Key] = target.Value;
                    }

                    foreach (var perkId in slot.SelectedPerkIds)
                    {
                        row.perks.Add(perkId);
                    }
                }
                rows.Add(row);
            }

            var file = new StringBuilder();
            file.AppendLine("{");
            file.AppendLine("  \"presets\": [");

            for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                var row = rows[rowIndex];
                var attributeIds = row.attributes == null
                    ? new List<string>()
                    : new List<string>(row.attributes.Keys);
                var skillIds = row.focus == null
                    ? new List<string>()
                    : new List<string>(row.focus.Keys);

                attributeIds.Sort(StringComparer.Ordinal);
                skillIds.Sort(StringComparer.Ordinal);

                file.AppendLine("    {");
                file.AppendLine("      \"id\": " + json.Serialize(row.id) + ",");
                file.AppendLine("      \"name\": " + json.Serialize(row.name ?? string.Empty) + ",");
                file.AppendLine("      \"description\": " + json.Serialize(row.description ?? string.Empty) + ",");
                file.AppendLine("      \"attributes\": {");

                for (int i = 0; i < attributeIds.Count; i++)
                {
                    string attributeId = attributeIds[i];
                    file.Append("        " + json.Serialize(attributeId) + ": " + row.attributes[attributeId]);

                    if (i + 1 < attributeIds.Count)
                    {
                        file.Append(",");
                    }

                    file.AppendLine();
                }

                file.AppendLine("      },");
                file.AppendLine("      \"focus\": {");

                for (int i = 0; i < skillIds.Count; i++)
                {
                    string skillId = skillIds[i];
                    file.Append("        " + json.Serialize(skillId) + ": " + row.focus[skillId]);

                    if (i + 1 < skillIds.Count)
                    {
                        file.Append(",");
                    }

                    file.AppendLine();
                }

                file.AppendLine("      },");
                file.AppendLine("      \"perks\": [");

                if (row.perks != null)
                {
                    for (int i = 0; i < row.perks.Count; i++)
                    {
                        file.Append("        " + json.Serialize(row.perks[i]));

                        if (i + 1 < row.perks.Count)
                        {
                            file.Append(",");
                        }

                        file.AppendLine();
                    }
                }

                file.AppendLine("      ]");
                file.Append("    }");

                if (rowIndex + 1 < rows.Count)
                {
                    file.Append(",");
                }

                file.AppendLine();
            }

            file.AppendLine("  ]");
            file.AppendLine("}");

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(jsonPath));
                File.WriteAllText(jsonPath, file.ToString(), Encoding.UTF8);
                return true;
            }
            catch (Exception exception)
            {
                var cantWriteUserFile = new TextObject("{=cant_write_user_file}Cant write ModuleData/{FILE}. Maybe the file is read-only or open somewhere. {REASON}");
                cantWriteUserFile.SetTextVariable("FILE", "your_presets.json");
                cantWriteUserFile.SetTextVariable("REASON", exception.Message);
                failReason = cantWriteUserFile.ToString();
                return false;
            }
        }

        private static bool CanWriteSlot(SkillPreset slot)
        {
            if (slot == null || !slot.IsConfigured)
            {
                return false;
            }

            return !slot.IsMimicPreset || MimicSource(slot) != null;
        }

        private static string NextUserPresetId(List<UserPresetRow> rows)
        {
            int number = 1;

            while (true)
            {
                string id = "your_preset_" + number;
                bool taken = false;

                foreach (var row in rows)
                {
                    if (row != null && row.id == id)
                    {
                        taken = true;
                        break;
                    }
                }

                if (!taken)
                {
                    return id;
                }

                number++;
            }
        }

        private static Hero MimicSource(SkillPreset slot)
        {
            var presetStore = SkillPresetBehavior.Current;
            return presetStore == null ? null : presetStore.GetMimicSourceHero(slot);
        }

        private static string UserPresetJsonPath()
        {
            string moduleFolder = Path.GetFullPath(Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "..",
                ".."));

            return Path.Combine(moduleFolder, "ModuleData", "your_presets.json");
        }

        private sealed class UserPresetFile
        {
            public List<UserPresetRow> presets { get; set; }
        }

        private sealed class UserPresetRow
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