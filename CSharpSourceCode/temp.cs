using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using TaleWorlds.Library;

namespace ProperSkillDistributor
{
    internal static class templatexporter
    {
        [CommandLineFunctionality.CommandLineArgumentFunction("export_templates", "skillpreset")]
        public static string exportroleslots(List<string> slotargs)
        {
            SkillPresetBehavior presetstore = SkillPresetBehavior.Current;
            if (presetstore == null)
            {
                return "no presetstore";
            }

            List<SkillPreset> roleslots = new List<SkillPreset>();
            if (slotargs == null || slotargs.Count == 0)
            {
                foreach (SkillPreset saveslot in presetstore.GetPresets())
                {
                    if (saveslot != null && saveslot.IsConfigured && !saveslot.IsMimicPreset)
                    {
                        roleslots.Add(saveslot);
                    }
                }
            }
            else
            {
                foreach (string typedslot in slotargs)
                {
                    int slotnumber;
                    if (!int.TryParse(typedslot, out slotnumber))
                    {
                        continue;
                    }

                    SkillPreset saveslot = presetstore.GetPreset(slotnumber);
                    if (saveslot != null && saveslot.IsConfigured && !saveslot.IsMimicPreset)
                    {
                        roleslots.Add(saveslot);
                    }
                }
            }
            if (roleslots.Count == 0)
            {
                return "unfound";
            }

            string modulefolder = Path.GetFullPath(Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..",".."));

            string dumpfolder = Path.Combine(modulefolder, "PresetExports");
            Directory.CreateDirectory(dumpfolder);
            string dumpfile = Path.Combine(dumpfolder, "roleslots" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt");

            StringBuilder note = new StringBuilder();

            foreach (SkillPreset roleslot in roleslots)
            {
                note.AppendLine("slot " + roleslot.SlotIndex); note.AppendLine("name = " + roleslot.Name); note.AppendLine(""); note.AppendLine("attributes");
                foreach (KeyValuePair<string, int> target in roleslot.AttributeTargets)
                {
                    note.AppendLine(target.Key + " = " + target.Value);
                }
                note.AppendLine("");

                note.AppendLine("focus");
                foreach (KeyValuePair<string, int> target in roleslot.SkillFocusTargets)
                {
                    note.AppendLine(target.Key + " = " + target.Value);
                }
                note.AppendLine("");

                note.AppendLine("perks");
                foreach (string perkid in roleslot.SelectedPerkIds)
                {
                    note.AppendLine(perkid);
                }
                note.AppendLine("");
                note.AppendLine("----");
                note.AppendLine("");
            }

            File.WriteAllText(dumpfile, note.ToString(), Encoding.UTF8);
            return dumpfile;
        }
    }
}