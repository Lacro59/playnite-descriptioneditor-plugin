﻿using CommonPluginsShared.Plugins;
using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DescriptionEditor
{
    public class DescriptionEditorSettings : PluginSettings
    {
        #region Settings variables

        #endregion

        // Playnite serializes settings object to a JSON object and saves it as text file.
        // If you want to exclude some property from being saved then use `JsonDontSerialize` ignore attribute.
        #region Variables exposed

        #endregion  
    }


    public class DescriptionEditorSettingsViewModel : ObservableObject, ISettings
    {
        private readonly DescriptionEditor Plugin;
        private DescriptionEditorSettings EditingClone { get; set; }

        private DescriptionEditorSettings _settings;
        public DescriptionEditorSettings Settings { get => _settings; set => SetValue(ref _settings, value); }


        public DescriptionEditorSettingsViewModel(DescriptionEditor plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            Plugin = plugin;

            // Load saved settings.
            DescriptionEditorSettings savedSettings = plugin.LoadPluginSettings<DescriptionEditorSettings>();

            // LoadPluginSettings returns null if not saved data is available.
            Settings = savedSettings ?? new DescriptionEditorSettings();
        }

        // Code executed when settings view is opened and user starts editing values.
        public void BeginEdit()
        {
            EditingClone = Serialization.GetClone(Settings);
        }

        // Code executed when user decides to cancel any changes made since BeginEdit was called.
        // This method should revert any changes made to Option1 and Option2.
        public void CancelEdit()
        {
            Settings = EditingClone;
        }

        // Code executed when user decides to confirm changes made since BeginEdit was called.
        // This method should save settings made to Option1 and Option2.
        public void EndEdit()
        {
            Plugin.SavePluginSettings(Settings);
        }

        // Code execute when user decides to confirm changes made since BeginEdit was called.
        // Executed before EndEdit is called and EndEdit is not called if false is returned.
        // List of errors is presented to user if verification fails.
        public bool VerifySettings(out List<string> errors)
        {
            errors = new List<string>();
            return true;
        }
    }
}
