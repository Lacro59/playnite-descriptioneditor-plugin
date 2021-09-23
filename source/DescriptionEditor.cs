﻿using DescriptionEditor.Views;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using CommonPluginsShared;
using CommonPluginsShared.PlayniteExtended;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Automation;
using Playnite.SDK.Events;

// TODO Integrate control HtmlTextView
namespace DescriptionEditor
{
    public class DescriptionEditor : PluginExtended<DescriptionEditorSettingsViewModel>
    {
        public override Guid Id { get; } = Guid.Parse("7600a469-4616-4547-94b8-0c330db02b8f");

        private TextBox TextDescription;
        private Button BtDescriptionEditor;


        public DescriptionEditor(IPlayniteAPI api) : base(api)
        {
            Properties = new GenericPluginProperties { HasSettings = true };

            // Add Event for WindowBase for get the "WindowGameEdit".
            EventManager.RegisterClassHandler(typeof(Window), Window.LoadedEvent, new RoutedEventHandler(WindowBase_LoadedEvent));
        }


        #region Custom event
        private void WindowBase_LoadedEvent(object sender, System.EventArgs e)
        {
            string WinIdProperty = string.Empty;
            string WinName = string.Empty;
            try
            {
                WinIdProperty = ((Window)sender).GetValue(AutomationProperties.AutomationIdProperty).ToString();
                WinName = ((Window)sender).Name;

                if (WinIdProperty == "WindowGameEdit")
                {
                    Window WindowGameEdit = (Window)sender;
                    DockPanel ElementParent = (DockPanel)((Button)WindowGameEdit.FindName("ButtonDownload")).Parent;
                    TabControl tabControl = (TabControl)WindowGameEdit.FindName("TabControlMain");
                    tabControl.SelectionChanged += TabControl_SelectionChanged;

                    if (ElementParent != null)
                    {
                        // Game Description
                        TextDescription = (TextBox)WindowGameEdit.FindName("TextDescription");

                        // Add new button
                        BtDescriptionEditor = new Button();
                        BtDescriptionEditor.Content = resources.GetString("LOCDescriptionEditorButton");
                        Style style = Application.Current.FindResource("BottomButton") as Style;
                        BtDescriptionEditor.Style = style;
                        BtDescriptionEditor.Click += OnButtonClick;

                        ElementParent.Children.Add(BtDescriptionEditor);
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, $"Error on WindowBase_LoadedEvent for {WinName} & {WinIdProperty}");
            }
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            var windowCreationOptions = new WindowCreationOptions
            {
                ShowMinimizeButton = false,
                ShowMaximizeButton = true,
                ShowCloseButton = true
            };

            var ViewExtension = new DescriptionEditorView(PlayniteApi, TextDescription);
            Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(PlayniteApi, resources.GetString("LOCDescriptionEditor"), ViewExtension, windowCreationOptions);
            windowExtension.ResizeMode = ResizeMode.CanResize;
            windowExtension.ShowDialog();
        }

        private void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            string ControlName = string.Empty;

            try
            {
                ControlName = ((TabControl)sender).Name;

                if (ControlName == "TabControlMain")
                {
                    if (BtDescriptionEditor != null)
                    {
                        BtDescriptionEditor.Visibility = Visibility.Collapsed;
                        TabItem tabItem = (TabItem)((TabControl)sender).SelectedItem;

                        if (tabItem != null)
                        {
                            foreach (TextBlock textBlock in UI.FindVisualChildren<TextBlock>((DependencyObject)tabItem.Content))
                            {
                                if (textBlock.Text == resources.GetString("LOCGameDescriptionTitle"))
                                {
                                    BtDescriptionEditor.Visibility = Visibility.Visible;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, $"Error on TabControl_SelectionChanged for {ControlName}");
            }
        }
        #endregion


        #region Game event
        public override void OnGameSelected(OnGameSelectedEventArgs args)
        {

        }

        // Add code to be executed when game is started running.
        public override void OnGameStarted(OnGameStartedEventArgs args)
        {
            
        }

        // Add code to be executed when game is preparing to be started.
        public override void OnGameStarting(OnGameStartingEventArgs args)
        {
            
        }

        // Add code to be executed when game is preparing to be started.
        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
            
        }

        // Add code to be executed when game is finished installing.
        public override void OnGameInstalled(OnGameInstalledEventArgs args)
        {

        }

        // Add code to be executed when game is uninstalled.
        public override void OnGameUninstalled(OnGameUninstalledEventArgs args)
        {
            
        }
        #endregion


        #region Application event
        // Add code to be executed when Playnite is initialized.
        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            
        }

        // Add code to be executed when Playnite is shutting down.
        public override void OnApplicationStopped(OnApplicationStoppedEventArgs args)
        {
            
        }
        #endregion


        // Add code to be executed when library is updated.
        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
            
        }


        #region Settings
        public override ISettings GetSettings(bool firstRunSettings)
        {
            return PluginSettings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new DescriptionEditorSettingsView();
        }
        #endregion
    }
}