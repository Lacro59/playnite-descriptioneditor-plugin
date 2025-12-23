using CommonPluginsShared;
using CommonPluginsShared.Extensions;
using CommonPluginsShared.PlayniteExtended;
using DescriptionEditor.Views;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;

// TODO Integrate control HtmlTextView
namespace DescriptionEditor
{
    public class DescriptionEditor : PluginExtended<DescriptionEditorSettingsViewModel>
    {
        public override Guid Id { get; } = Guid.Parse("7600a469-4616-4547-94b8-0c330db02b8f");

        private TextBox TextDescription { get; set; }
        private Button BtDescriptionEditor { get; set; }


        public DescriptionEditor(IPlayniteAPI api) : base(api)
        {
            Properties = new GenericPluginProperties { HasSettings = false };

            // Add Event for WindowBase for get the "WindowGameEdit".
            EventManager.RegisterClassHandler(typeof(Window), Window.LoadedEvent, new RoutedEventHandler(WindowBase_LoadedEvent));
        }


        #region Custom event

        private void WindowBase_LoadedEvent(object sender, EventArgs e)
        {
            string winIdProperty = string.Empty;
            string winName = string.Empty;
            try
            {
                winIdProperty = ((Window)sender).GetValue(AutomationProperties.AutomationIdProperty).ToString();
                winName = ((Window)sender).Name;

                if (winIdProperty == "WindowGameEdit")
                {
                    Window windowGameEdit = (Window)sender;
                    DockPanel elementParent = (DockPanel)((Button)windowGameEdit.FindName("ButtonDownload")).Parent;
                    TabControl tabControl = (TabControl)windowGameEdit.FindName("TabControlMain");
                    tabControl.SelectionChanged += TabControl_SelectionChanged;

                    if (elementParent != null)
                    {
                        // Game Description
                        TextDescription = (TextBox)windowGameEdit.FindName("TextDescription");

                        // Add new button
                        Style style = Application.Current.FindResource("BottomButton") as Style;
                        BtDescriptionEditor = new Button
                        {
                            Content = ResourceProvider.GetString("LOCDescriptionEditorButton"),
                            Style = style
                        };
                        BtDescriptionEditor.Click += OnButtonClick;
                        _ = elementParent.Children.Add(BtDescriptionEditor);
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, $"Error on WindowBase_LoadedEvent for {winName} & {winIdProperty}");
            }
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            WindowOptions windowOptions = new WindowOptions
            {
                ShowMinimizeButton = false,
                ShowMaximizeButton = true,
                ShowCloseButton = true,
                CanBeResizable = true,
                Height = 700,
                Width = 1200
            };

            DescriptionEditorView ViewExtension = new DescriptionEditorView(TextDescription);
            Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(ResourceProvider.GetString("LOCDescriptionEditor"), ViewExtension, windowOptions);
            _ = windowExtension.ShowDialog();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string controlName = string.Empty;
            try
            {
                controlName = ((TabControl)sender).Name;

                if (controlName == "TabControlMain" && BtDescriptionEditor != null)
                {
                    BtDescriptionEditor.Visibility = Visibility.Collapsed;
                    TabItem tabItem = (TabItem)((TabControl)sender).SelectedItem;

                    if (tabItem != null)
                    {
                        foreach (TextBlock textBlock in UI.FindVisualChildren<TextBlock>((DependencyObject)tabItem.Content))
                        {
                            if (textBlock.Text.IsEqual(ResourceProvider.GetString("LOCGameDescriptionTitle")))
                            {
                                BtDescriptionEditor.Visibility = Visibility.Visible;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, $"Error on TabControl_SelectionChanged for {controlName}");
            }
        }

        #endregion

        #region Menus

        // To add new game menu items override GetGameMenuItems
        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            Game gameMenu = args.Games.First();
            List<Guid> ids = args.Games.Select(x => x.Id).ToList();
            List<GameMenuItem> gameMenuItems = new List<GameMenuItem>();

            gameMenuItems.Add(new GameMenuItem
            {
                MenuSection = $"{ResourceProvider.GetString("LOCDescriptionEditor")}|{ResourceProvider.GetString("LOCDescriptionEditorButtonImageFormatter")}",
                Description = ResourceProvider.GetString("LOCDescriptionEditorButtonRemoveImg"),
                Action = (mainMenuItem) =>
                {
                    MessageBoxResult response = PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCConfirmationAskGeneric"), ResourceProvider.GetString("LOCDescriptionEditor"), MessageBoxButton.YesNo);
                    if (response == MessageBoxResult.Yes)
                    {
                        foreach (Guid id in ids)
                        {
                            Game game = PlayniteApi.Database.Games.Get(id);
                            if (game != null)
                            {
                                game.Description = HtmlHelper.RemoveTag(game.Description, "img");
                                PlayniteApi.Database.Games.Update(game);
                            }
                        }
                    }
                }
            });

            gameMenuItems.Add(new GameMenuItem
            {
                MenuSection = $"{ResourceProvider.GetString("LOCDescriptionEditor")}|{ResourceProvider.GetString("LOCDescriptionEditorButtonImageFormatter")}",
                Description = ResourceProvider.GetString("LOCDescriptionEditorButtonAdd100PercentImg"),
                Action = (mainMenuItem) =>
                {
                    MessageBoxResult response = PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCConfirmationAskGeneric"), ResourceProvider.GetString("LOCDescriptionEditor"), MessageBoxButton.YesNo);
                    if (response == MessageBoxResult.Yes)
                    {
                        foreach (Guid id in ids)
                        {
                            Game game = PlayniteApi.Database.Games.Get(id);
                            if (game != null)
                            {
                                game.Description = HtmlHelper.Add100PercentStyle(game.Description);
                                PlayniteApi.Database.Games.Update(game);
                            }
                        }
                    }
                }
            });

            gameMenuItems.Add(new GameMenuItem
            {
                MenuSection = $"{ResourceProvider.GetString("LOCDescriptionEditor")}|{ResourceProvider.GetString("LOCDescriptionEditorButtonImageFormatter")}",
                Description = ResourceProvider.GetString("LOCDescriptionEditorButtonRemoveStyleImg"),
                Action = (mainMenuItem) =>
                {
                    MessageBoxResult response = PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCConfirmationAskGeneric"), ResourceProvider.GetString("LOCDescriptionEditor"), MessageBoxButton.YesNo);
                    if (response == MessageBoxResult.Yes)
                    {
                        foreach (Guid id in ids)
                        {
                            Game game = PlayniteApi.Database.Games.Get(id);
                            if (game != null)
                            {
                                game.Description = HtmlHelper.RemoveSizeStyle(game.Description);
                                PlayniteApi.Database.Games.Update(game);
                            }
                        }
                    }
                }
            });

            gameMenuItems.Add(new GameMenuItem
            {
                MenuSection = $"{ResourceProvider.GetString("LOCDescriptionEditor")}|{ResourceProvider.GetString("LOCDescriptionEditorButtonImageFormatter")}",
                Description = ResourceProvider.GetString("LOCDescriptionEditorButtonCenterImg"),
                Action = (mainMenuItem) =>
                {
                    MessageBoxResult response = PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCConfirmationAskGeneric"), ResourceProvider.GetString("LOCDescriptionEditor"), MessageBoxButton.YesNo);
                    if (response == MessageBoxResult.Yes)
                    {
                        foreach (Guid id in ids)
                        {
                            Game game = PlayniteApi.Database.Games.Get(id);
                            if (game != null)
                            {
                                game.Description = HtmlHelper.CenterImage(game.Description);
                                PlayniteApi.Database.Games.Update(game);
                            }
                        }
                    }
                }
            });


            gameMenuItems.Add(new GameMenuItem
            {
                MenuSection = $"{ResourceProvider.GetString("LOCDescriptionEditor")}|{ResourceProvider.GetString("LOCDescriptionEditorButtonHtmlFormatSteam")}",
                Description = ResourceProvider.GetString("LOCDescriptionEditorRemoveAboutGame"),
                Action = (mainMenuItem) =>
                {
                    MessageBoxResult response = PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCConfirmationAskGeneric"), ResourceProvider.GetString("LOCDescriptionEditor"), MessageBoxButton.YesNo);
                    if (response == MessageBoxResult.Yes)
                    {
                        foreach (Guid id in ids)
                        {
                            Game game = PlayniteApi.Database.Games.Get(id);
                            if (game != null)
                            {
                                game.Description = HtmlHelper.SteamRemoveAbout(game.Description);
                                PlayniteApi.Database.Games.Update(game);
                            }
                        }
                    }
                }
            });


            gameMenuItems.Add(new GameMenuItem
            {
                MenuSection = $"{ResourceProvider.GetString("LOCDescriptionEditor")}",
                Description = ResourceProvider.GetString("LOCDescriptionEditorButtonMarkdownToHtml"),
                Action = (mainMenuItem) =>
                {
                    MessageBoxResult response = PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCConfirmationAskGeneric"), ResourceProvider.GetString("LOCDescriptionEditor"), MessageBoxButton.YesNo);
                    if (response == MessageBoxResult.Yes)
                    {
                        foreach (Guid id in ids)
                        {
                            Game game = PlayniteApi.Database.Games.Get(id);
                            if (game != null)
                            {
                                game.Description = HtmlHelper.MarkdownToHtml(game.Description);
                                PlayniteApi.Database.Games.Update(game);
                            }
                        }
                    }
                }
            });


            gameMenuItems.Add(new GameMenuItem
            {
                MenuSection = $"{ResourceProvider.GetString("LOCDescriptionEditor")}|{ResourceProvider.GetString("LOCDescriptionEditorButtonHtmlFormater")}",
                Description = ResourceProvider.GetString("LOCDescriptionEditorButtonHeaderToBold"),
                Action = (mainMenuItem) =>
                {
                    MessageBoxResult response = PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCConfirmationAskGeneric"), ResourceProvider.GetString("LOCDescriptionEditor"), MessageBoxButton.YesNo);
                    if (response == MessageBoxResult.Yes)
                    {
                        foreach (Guid id in ids)
                        {
                            Game game = PlayniteApi.Database.Games.Get(id);
                            if (game != null)
                            {
                                game.Description = HtmlHelper.HeaderToBold(game.Description);
                                PlayniteApi.Database.Games.Update(game);
                            }
                        }
                    }
                }
            });

            gameMenuItems.Add(new GameMenuItem
            {
                MenuSection = $"{ResourceProvider.GetString("LOCDescriptionEditor")}|{ResourceProvider.GetString("LOCDescriptionEditorButtonHtmlFormater")}",
                Description = "-"
            });

            gameMenuItems.Add(new GameMenuItem
            {
                MenuSection = $"{ResourceProvider.GetString("LOCDescriptionEditor")}|{ResourceProvider.GetString("LOCDescriptionEditorButtonHtmlFormater")}",
                Description = "<p>...</p> => ...<br><br>",
                Action = (mainMenuItem) =>
                {
                    MessageBoxResult response = PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCConfirmationAskGeneric"), ResourceProvider.GetString("LOCDescriptionEditor"), MessageBoxButton.YesNo);
                    if (response == MessageBoxResult.Yes)
                    {
                        foreach (Guid id in ids)
                        {
                            Game game = PlayniteApi.Database.Games.Get(id);
                            if (game != null)
                            {
                                game.Description = HtmlHelper.ParagraphRemove(game.Description);
                                PlayniteApi.Database.Games.Update(game);
                            }
                        }
                    }
                }
            });

            gameMenuItems.Add(new GameMenuItem
            {
                MenuSection = $"{ResourceProvider.GetString("LOCDescriptionEditor")}|{ResourceProvider.GetString("LOCDescriptionEditorButtonHtmlFormater")}",
                Description = "...<br><br> => <p>...</p>",
                Action = (mainMenuItem) =>
                {
                    MessageBoxResult response = PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCConfirmationAskGeneric"), ResourceProvider.GetString("LOCDescriptionEditor"), MessageBoxButton.YesNo);
                    if (response == MessageBoxResult.Yes)
                    {
                        foreach (Guid id in ids)
                        {
                            Game game = PlayniteApi.Database.Games.Get(id);
                            if (game != null)
                            {
                                game.Description = HtmlHelper.BrBrToP(game.Description);
                                PlayniteApi.Database.Games.Update(game);
                            }
                        }
                    }
                }
            });

            gameMenuItems.Add(new GameMenuItem
            {
                MenuSection = $"{ResourceProvider.GetString("LOCDescriptionEditor")}|{ResourceProvider.GetString("LOCDescriptionEditorButtonHtmlFormater")}",
                Description = "-"
            });

            gameMenuItems.Add(new GameMenuItem
            {
                MenuSection = $"{ResourceProvider.GetString("LOCDescriptionEditor")}|{ResourceProvider.GetString("LOCDescriptionEditorButtonHtmlFormater")}",
                Description = "<br><br> => <br>",
                Action = (mainMenuItem) =>
                {
                    MessageBoxResult response = PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCConfirmationAskGeneric"), ResourceProvider.GetString("LOCDescriptionEditor"), MessageBoxButton.YesNo);
                    if (response == MessageBoxResult.Yes)
                    {
                        foreach (Guid id in ids)
                        {
                            Game game = PlayniteApi.Database.Games.Get(id);
                            if (game != null)
                            {
                                game.Description = HtmlHelper.BrRemove(game.Description, 2, 1);
                                PlayniteApi.Database.Games.Update(game);
                            }
                        }
                    }
                }
            });

            gameMenuItems.Add(new GameMenuItem
            {
                MenuSection = $"{ResourceProvider.GetString("LOCDescriptionEditor")}|{ResourceProvider.GetString("LOCDescriptionEditorButtonHtmlFormater")}",
                Description = "<br><br>br> => <br>",
                Action = (mainMenuItem) =>
                {
                    MessageBoxResult response = PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCConfirmationAskGeneric"), ResourceProvider.GetString("LOCDescriptionEditor"), MessageBoxButton.YesNo);
                    if (response == MessageBoxResult.Yes)
                    {
                        foreach (Guid id in ids)
                        {
                            Game game = PlayniteApi.Database.Games.Get(id);
                            if (game != null)
                            {
                                game.Description = HtmlHelper.BrRemove(game.Description, 3, 1);
                                PlayniteApi.Database.Games.Update(game);
                            }
                        }
                    }
                }
            });

            gameMenuItems.Add(new GameMenuItem
            {
                MenuSection = $"{ResourceProvider.GetString("LOCDescriptionEditor")}|{ResourceProvider.GetString("LOCDescriptionEditorButtonHtmlFormater")}",
                Description = "<br><br> => <br>br>",
                Action = (mainMenuItem) =>
                {
                    MessageBoxResult response = PlayniteApi.Dialogs.ShowMessage(ResourceProvider.GetString("LOCConfirmationAskGeneric"), ResourceProvider.GetString("LOCDescriptionEditor"), MessageBoxButton.YesNo);
                    if (response == MessageBoxResult.Yes)
                    {
                        foreach (Guid id in ids)
                        {
                            Game game = PlayniteApi.Database.Games.Get(id);
                            if (game != null)
                            {
                                game.Description = HtmlHelper.BrRemove(game.Description, 3, 2);
                                PlayniteApi.Database.Games.Update(game);
                            }
                        }
                    }
                }
            });


#if DEBUG
            gameMenuItems.Add(new GameMenuItem
            {
                MenuSection = ResourceProvider.GetString("LOCDescriptionEditor"),
                Description = "-"
            });
            gameMenuItems.Add(new GameMenuItem
            {
                MenuSection = ResourceProvider.GetString("LOCDescriptionEditor"),
                Description = "Test",
                Action = (mainMenuItem) =>
                {

                }
            });
#endif

            return gameMenuItems;
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