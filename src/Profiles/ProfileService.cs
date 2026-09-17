using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using Inject0rHUD.Config;
using Inject0rHUD.Localization;

namespace Inject0rHUD.Profiles
{
    internal sealed class ProfileService
    {
        private readonly ModConfig _config;
        private readonly List<HudProfile> _profiles = new List<HudProfile>();
        private readonly string _path;
        private int _index;

        internal string LastMessage { get; private set; }

        internal ProfileService(ModConfig config)
        {
            _config = config;
            _path = Path.Combine(Paths.ConfigPath, "Inject0rHUD.profiles");
            Load();
            SelectByName(config.ActiveProfile.Value, false);
        }

        internal HudProfile Current
        {
            get
            {
                if (_profiles.Count == 0)
                    return null;

                if (_index < 0) _index = 0;
                if (_index >= _profiles.Count) _index = _profiles.Count - 1;
                return _profiles[_index];
            }
        }

        internal int Count => _profiles.Count;
        internal int Index => _index;

        internal void Next()
        {
            if (_profiles.Count == 0) return;
            _index = (_index + 1) % _profiles.Count;
            ApplyCurrent();
        }

        internal void Previous()
        {
            if (_profiles.Count == 0) return;
            _index = (_index - 1 + _profiles.Count) % _profiles.Count;
            ApplyCurrent();
        }

        internal void ApplyCurrent()
        {
            HudProfile p = Current;
            if (p == null) return;

            p.Apply(_config);
            LastMessage = ModLocalization.T("profile.msg.applied", p.Name);
        }

        internal void SaveCurrent()
        {
            HudProfile p = Current;
            if (p == null) return;

            if (p.BuiltIn)
            {
                LastMessage = ModLocalization.T("profile.msg.readonly");
                return;
            }

            HudProfile captured = HudProfile.Capture(p.Name, false, _config);
            _profiles[_index] = captured;
            SaveCustomProfiles();
            LastMessage = ModLocalization.T("profile.msg.saved", captured.Name);
        }

        internal void CreateCustom(string requestedName)
        {
            string name = CleanName(requestedName);
            if (string.IsNullOrEmpty(name))
                name = "Custom";

            name = MakeUniqueName(name);

            HudProfile profile = HudProfile.Capture(name, false, _config);
            _profiles.Add(profile);
            _index = _profiles.Count - 1;
            profile.Apply(_config);
            SaveCustomProfiles();

            LastMessage = ModLocalization.T("profile.msg.created", name);
        }

        internal void RenameCurrent(string requestedName)
        {
            HudProfile p = Current;
            if (p == null || p.BuiltIn)
            {
                LastMessage = ModLocalization.T("profile.msg.rename_blocked");
                return;
            }

            string name = CleanName(requestedName);
            if (string.IsNullOrEmpty(name))
                return;

            p.Name = MakeUniqueName(name, p);
            _config.ActiveProfile.Value = p.Name;
            _config.Save();
            SaveCustomProfiles();
            LastMessage = ModLocalization.T("profile.msg.renamed", p.Name);
        }

        internal void DeleteCurrent()
        {
            HudProfile p = Current;
            if (p == null || p.BuiltIn)
            {
                LastMessage = ModLocalization.T("profile.msg.delete_blocked");
                return;
            }

            string name = p.Name;
            _profiles.RemoveAt(_index);
            if (_index >= _profiles.Count) _index = _profiles.Count - 1;
            if (_index < 0) _index = 0;

            ApplyCurrent();
            SaveCustomProfiles();
            LastMessage = ModLocalization.T("profile.msg.deleted", name);
        }

        internal string ExportCurrent()
        {
            HudProfile captured = HudProfile.Capture(
                Current != null ? Current.Name : "Exported",
                false,
                _config);

            string code = captured.ExportCode();
            LastMessage = ModLocalization.T("profile.msg.clipboard");
            return code;
        }

        internal void ImportCode(string code)
        {
            HudProfile imported = HudProfile.ImportCode(code, false);
            imported.Name = MakeUniqueName(
                string.IsNullOrEmpty(imported.Name) ? "Imported" : imported.Name);

            _profiles.Add(imported);
            _index = _profiles.Count - 1;
            imported.Apply(_config);
            SaveCustomProfiles();

            LastMessage = ModLocalization.T("profile.msg.imported", imported.Name);
        }

        internal void SelectByName(string name, bool apply)
        {
            int found = _profiles.FindIndex(
                p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));

            _index = found >= 0 ? found : Math.Min(1, Math.Max(0, _profiles.Count - 1));

            if (apply)
                ApplyCurrent();
        }

        private void Load()
        {
            _profiles.Clear();
            AddBuiltIns();

            if (!File.Exists(_path))
                return;

            try
            {
                string[] lines = File.ReadAllLines(_path);
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line))
                        continue;

                    try
                    {
                        HudProfile p = HudProfile.ImportCode(line, false);
                        p.Name = MakeUniqueName(p.Name);
                        _profiles.Add(p);
                    }
                    catch
                    {
                        // Ignore one broken custom profile instead of breaking the mod.
                    }
                }
            }
            catch
            {
            }
        }

        private void SaveCustomProfiles()
        {
            try
            {
                string[] lines = _profiles
                    .Where(p => !p.BuiltIn)
                    .Select(p => p.ExportCode())
                    .ToArray();

                File.WriteAllLines(_path, lines);
            }
            catch
            {
            }
        }

        private void AddBuiltIns()
        {
            var minimal = HudProfile.Capture("Minimal", true, _config);
            minimal.Scale = 0.78f;
            minimal.FontSize = 11;
            minimal.PanelWidth = 235;
            minimal.BackgroundOpacity = 0.42f;
            minimal.SectionHeaders = false;
            minimal.ShowRested = true;
            minimal.ShowPower = false;
            minimal.ShowDurability = true;
            minimal.ShowIcons = false;
            minimal.SmartMode = SmartDurabilityMode.BelowThreshold;
            minimal.SmartThreshold = 35;
            minimal.ShowFps = false;
            minimal.ShowPing = false;
            _profiles.Add(minimal);

            var compact = HudProfile.Capture("Compact", true, _config);
            compact.Scale = 0.90f;
            compact.FontSize = 12;
            compact.PanelWidth = 280;
            compact.BackgroundOpacity = 0.60f;
            compact.SectionHeaders = true;
            compact.ShowIcons = true;
            compact.SmartMode = SmartDurabilityMode.Off;
            _profiles.Add(compact);

            var full = HudProfile.Capture("Full", true, _config);
            full.Scale = 1.0f;
            full.FontSize = 13;
            full.PanelWidth = 320;
            full.BackgroundOpacity = 0.72f;
            full.SectionHeaders = true;
            full.ShowIcons = true;
            full.ShowFps = true;
            full.ShowPing = true;
            _profiles.Add(full);
        }

        private string MakeUniqueName(string desired, HudProfile ignore = null)
        {
            string baseName = CleanName(desired);
            if (string.IsNullOrEmpty(baseName))
                baseName = "Custom";

            string candidate = baseName;
            int suffix = 2;

            while (_profiles.Any(p =>
                !ReferenceEquals(p, ignore) &&
                string.Equals(p.Name, candidate, StringComparison.OrdinalIgnoreCase)))
            {
                candidate = baseName + " " + suffix++;
            }

            return candidate;
        }

        private static string CleanName(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            string s = value.Trim();
            if (s.Length > 32)
                s = s.Substring(0, 32);

            return s.Replace("\t", " ").Replace("\r", " ").Replace("\n", " ");
        }
    }
}
