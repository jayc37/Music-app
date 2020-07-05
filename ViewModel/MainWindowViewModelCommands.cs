using Levitate.Core;
using Levitate.Model;
using Levitate.Properties;
using Levitate.View;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;

namespace Levitate.ViewModel
{
    public partial class MainWindowViewModel
    {
        private RelayCommand _addFile;
        public RelayCommand AddFile
        {
            get
            {
                return _addFile ?? (_addFile = new RelayCommand(async parameter =>
                {
                    // Using OpenfileDialog
                    using (var _openDialog = new OpenFileDialog())
                    {
                        // Filter is m4a and mp3 extensions, allow multiselection, default initialDirectory is system Music folder
                        _openDialog.Filter = "Music (*.mp3, *.m4a)|*.mp3;*.m4a|MP3 (*.mp3)|*.mp3|M4A (*.m4a)|*.m4a";
                        _openDialog.Multiselect = true;
                        _openDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                        DialogResult result = _openDialog.ShowDialog();

                        string[] filespaths = null;

                        // If dialog return OK pressed, collect paths
                        if (result == DialogResult.OK)
                            filespaths = _openDialog.FileNames;
                        else
                            return;

                        // If paths is not null, handle files
                        if (filespaths.Length > 0)
                        {
                            await Core.Utils.TrackProcess((MetroWindow)System.Windows.Application.Current.MainWindow, filespaths);
                        }
                    }

                    MusicManager.LoadApplicationPlaylist();
                }));
            }
        }


        private RelayCommand _addFolder;
        public RelayCommand AddFolder
        {
            get
            {
                return _addFolder ?? (_addFolder = new RelayCommand(async parameter =>
                {
                    // Using FolderBrowserDialog
                    using (var _openFolderDialog = new FolderBrowserDialog())
                    {
                        DialogResult result = _openFolderDialog.ShowDialog();

                        // If folder is selected
                        if (!string.IsNullOrWhiteSpace(_openFolderDialog.SelectedPath))
                        {
                            // Get all directories from selected folder (include subDirectories in it)
                            string[] filespaths = Directory.GetFiles(_openFolderDialog.SelectedPath, "*.*", SearchOption.AllDirectories);

                            // Handle files
                            if (filespaths != null)
                            {
                                await Core.Utils.TrackProcess((MetroWindow)System.Windows.Application.Current.MainWindow, filespaths);
                            }
                        }
                    }

                    MusicManager.LoadApplicationPlaylist();
                }));
            }
        }


        private RelayCommand _addFileToPlaylist;
        public RelayCommand AddFileToPlaylist
        {
            get
            {
                return _addFileToPlaylist ?? (_addFileToPlaylist = new RelayCommand(parameter =>
                {

                }));
            }
        }


        private RelayCommand _copy2Clipboard;
        public RelayCommand Copy2Clipboard
        {
            get
            {
                return _copy2Clipboard ?? (_copy2Clipboard = new RelayCommand(parameter =>
                {
                    // If has no selected item => return
                    if (parameter == null)
                        return;

                    // Copy to clipboard:

                    // Delaration
                    StringCollection _paths = new StringCollection();
                    System.Windows.Clipboard.Clear(); // remove old data in clipboard

                    // Get paths from tracks
                    foreach (var _track in ((IList)parameter).Cast<Model.Track>().ToList())
                    {
                        if (_track.IsExist)
                            _paths.Add(_track.Location);
                    }

                    // Save to clipboard
                    System.Windows.Clipboard.SetFileDropList(_paths);
                }));
            }
        }
        

        private RelayCommand _deleteSelectedTracks;
        public RelayCommand DeleteSelectedTracks
        {
            get
            {
                return _deleteSelectedTracks ?? (_deleteSelectedTracks = new RelayCommand(parameter =>
                {
                    var tracks = ((IList)parameter).Cast<Model.Track>().ToList();

                    if (tracks == null || tracks.Count == 0) return;

                    foreach(var track in tracks)
                    {
                        MusicManager.DeleteOneTrack(track);
                    }
                }));
            }
        }
        

        private RelayCommand _getTrackInfo;
        public RelayCommand GetTrackInfo
        {
            get
            {
                return _getTrackInfo ?? (_getTrackInfo = new RelayCommand(parameter =>
                {
                    if (MusicManager.SelectedTrack == null || !MusicManager.SelectedTrack.IsExist) return;
                    var track = MusicManager.SelectedTrack;
                    using (var infoWindow = new TrackInformationWindow(ref track, Settings.Default.Lang))
                    {
                        infoWindow.ShowDialog();
                    }
                }));
            }
        }


        private RelayCommand _playCommand;
        public RelayCommand PlayCommand
        {
            get
            {
                return _playCommand ?? (_playCommand = new RelayCommand(parameter =>
                {
                    MusicManager.Play();
                }));
            }
        }


        private RelayCommand _platSelectedTrack;
        public RelayCommand PlaySelectedTrack
        {
            get
            {
                return _platSelectedTrack ?? (_platSelectedTrack = new RelayCommand(parameter =>
                {
                    MusicManager.PlaySelectedTrack();
                }));
            }
        }


        private RelayCommand _togglePlayPause;
        public RelayCommand TogglePlayPause
        {
            get
            {
                return _togglePlayPause ?? (_togglePlayPause = new RelayCommand(parameter =>
                {
                    if (MusicManager.State == Manager.PlayerState.Playing)
                        MusicManager.Pause();
                    else
                        MusicManager.Play(); ;
                }));
            }
        }


        private RelayCommand _playForward;
        public RelayCommand PlayForward
        {
            get
            {
                return _playForward ?? (_playForward = new RelayCommand(parameter =>
                {
                    MusicManager.PlayForward();

                    ChangeButtonVisibility();
                }));
            }
        }


        private RelayCommand _playBackward;
        public RelayCommand PlayBackward
        {
            get
            {
                return _playBackward ?? (_playBackward = new RelayCommand(parameter =>
                {
                    MusicManager.PlayBackward();

                    ChangeButtonVisibility();
                }));
            }
        }



        private RelayCommand _cleanUpLibrary;
        public RelayCommand CleanUpLibrary
        {
            get
            {
                return _cleanUpLibrary ?? (_cleanUpLibrary = new RelayCommand(async parameter =>
                {
                    // Show comfirm message dialog
                    MessageDialogResult result = await ((MetroWindow)System.Windows.Application.Current.MainWindow).ShowMessageAsync
                         (System.Windows.Application.Current.MainWindow.FindResource("attention").ToString(),
                          System.Windows.Application.Current.MainWindow.FindResource("doYouWantToClearAll").ToString(), MessageDialogStyle.AffirmativeAndNegative, null);

                    // If return affirmative then clean db
                    if (result == MessageDialogResult.Affirmative)
                    {
                        MusicManager.Stop();

                        if (MusicManager.Playlists != null)
                            MusicManager.Playlists.Clear();

                        Updater.CleanDatabase();
                        MusicManager.Player.Stop();
                        MusicManager.CurrentTrack = null;
                        MusicManager.LoadApplicationPlaylist();
                    }
                }));
            }
        }


        private RelayCommand _newPlaylist;
        public RelayCommand NewPlaylist
        {
            get
            {
                return _newPlaylist ?? (_newPlaylist = new RelayCommand(async parameter =>
                {
                    // Input Dialog setting declarations
                    MetroDialogSettings setting = new MetroDialogSettings();
                    setting.CustomResourceDictionary = System.Windows.Application.Current.MainWindow.Resources;
                    setting.AffirmativeButtonText = System.Windows.Application.Current.FindResource("ok").ToString();
                    setting.NegativeButtonText = System.Windows.Application.Current.FindResource("cancel").ToString();

                    // Call Input Dialog
                    string newPlaylistName = await ((MetroWindow)System.Windows.Application.Current.MainWindow).ShowInputAsync(
                        System.Windows.Application.Current.MainWindow.FindResource("newPlaylist").ToString(),
                        System.Windows.Application.Current.MainWindow.FindResource("playlistName").ToString(),
                        setting);

                    if (!string.IsNullOrWhiteSpace(newPlaylistName))
                    {
                        Playlist newPlaylist = new Playlist();
                        newPlaylist.Name = newPlaylistName;

                        MusicManager.Playlists.Add(newPlaylist);
                    }
                }));
            }
        }


        private RelayCommand _deleteAllPlaylists;
        public RelayCommand DeleteAllPlaylists
        {
            get
            {
                return _deleteAllPlaylists ?? (_deleteAllPlaylists = new RelayCommand(async parameter =>
                {
                    // Message Dialog setting declarations
                    MetroDialogSettings setting = new MetroDialogSettings();
                    setting.CustomResourceDictionary = System.Windows.Application.Current.MainWindow.Resources;
                    setting.AffirmativeButtonText = System.Windows.Application.Current.Resources["ok"].ToString();
                    setting.NegativeButtonText = System.Windows.Application.Current.FindResource("cancel").ToString();

                    // Show comfirm message dialog
                    MessageDialogResult result = await((MetroWindow)System.Windows.Application.Current.MainWindow).ShowMessageAsync
                         (System.Windows.Application.Current.MainWindow.FindResource("attention").ToString(),
                          System.Windows.Application.Current.MainWindow.FindResource("doYouWantToDeletePlaylists").ToString(), MessageDialogStyle.AffirmativeAndNegative,
                          setting);

                    // If return affirmative then clean db
                    if (result == MessageDialogResult.Affirmative)
                    {
                        MusicManager.DeleteAllUserPlaylist();
                    }
                }));
            }
        }


        private RelayCommand _deleteSelectedPlaylist;
        public RelayCommand DeleteSelectedPlaylists
        {
            get
            {
                return _deleteSelectedPlaylist ?? (_deleteSelectedPlaylist = new RelayCommand(async parameter =>
                {
                    // Message Dialog setting declarations
                    MetroDialogSettings setting = new MetroDialogSettings();
                    setting.CustomResourceDictionary = System.Windows.Application.Current.MainWindow.Resources;
                    setting.AffirmativeButtonText = System.Windows.Application.Current.FindResource("ok").ToString();
                    setting.NegativeButtonText = System.Windows.Application.Current.FindResource("cancel").ToString();

                    // Show comfirm message dialog
                    MessageDialogResult result = await ((MetroWindow)System.Windows.Application.Current.MainWindow).ShowMessageAsync
                         (System.Windows.Application.Current.MainWindow.FindResource("attention").ToString(),
                          System.Windows.Application.Current.MainWindow.FindResource("doYouWantToDeleteSelectedPlaylists").ToString(), MessageDialogStyle.AffirmativeAndNegative,
                          setting);

                    // If return affirmative then clean db
                    if (result == MessageDialogResult.Affirmative)
                    {
                        var playlists = ((IList)parameter).Cast<Playlist>().ToList();

                        foreach(var playlist in playlists)
                        {
                            MusicManager.DeleteOnePlaylist(playlist);
                        }
                    }
                }));
            }
        }


        private RelayCommand _renameSelectedPlaylist;
        public RelayCommand RenameSelectedPlaylist
        {
            get
            {
                return _renameSelectedPlaylist ?? (_renameSelectedPlaylist = new RelayCommand(async parameter =>
                {
                    // Input Dialog setting declarations
                    MetroDialogSettings setting = new MetroDialogSettings();
                    setting.CustomResourceDictionary = System.Windows.Application.Current.MainWindow.Resources;
                    setting.DefaultText = MusicManager.SelectedPlaylist.Name;
                    setting.AffirmativeButtonText = System.Windows.Application.Current.FindResource("ok").ToString();
                    setting.NegativeButtonText = System.Windows.Application.Current.FindResource("cancel").ToString();

                    // Call Input Dialog
                    string newName = await((MetroWindow)System.Windows.Application.Current.MainWindow).ShowInputAsync(
                        System.Windows.Application.Current.MainWindow.FindResource("renamePlaylist").ToString(),
                        System.Windows.Application.Current.MainWindow.FindResource("newName").ToString(),
                        setting);

                    if(!string.IsNullOrWhiteSpace(newName))
                        MusicManager.SelectedPlaylist.Name = newName;
                }));
            }
        }

        
        private RelayCommand _showRecentlyAdded;
        public RelayCommand ShowRecentlyAdded
        {
            get
            {
                return _showRecentlyAdded ?? (_showRecentlyAdded = new RelayCommand(parameter =>
                {
                    MusicManager.SelectedPlaylist = MusicManager.RecentlyAdded;
                }));
            }
        }


        private RelayCommand _showAllSongs;
        public RelayCommand ShowAllSongs
        {
            get
            {
                return _showAllSongs ?? (_showAllSongs = new RelayCommand(parameter =>
                {
                    MusicManager.SelectedPlaylist = MusicManager.AllSongs;
                }));
            }
        }


        private RelayCommand _showFavourites;
        public RelayCommand ShowFavourites
        {
            get
            {
                return _showFavourites ?? (_showFavourites = new RelayCommand(parameter =>
                {
                    MusicManager.SelectedPlaylist = MusicManager.Favourites;
                }));
            }
        }


        private RelayCommand _toggleFavourite;
        public RelayCommand ToggleFavourite
        {
            get
            {
                return _toggleFavourite ?? (_toggleFavourite = new RelayCommand(parameter =>
                {
                    Model.Track _track = ((ToggleButton)parameter).DataContext as Model.Track;
                    Updater.UpdateOnlyFavourite2DB(_track.ID, _track.IsFavourite);

                    // Change value in ViewModel.AllSongs

                    if (MusicManager.SelectedPlaylist.Prio == Model.Playlist.Priority.Application)
                    {
                        Parallel.ForEach(MusicManager.Playlists, playlist =>
                        {
                            if (playlist.Name != MusicManager.SelectedPlaylist.Name)
                            {
                                playlist.ListTrack.FirstOrDefault(item => item.ID == _track.ID).IsFavourite = _track.IsFavourite;
                            }
                        });
                    }
                    else
                    {
                        MusicManager.AllSongs.ListTrack.First(item => item.ID == _track.ID).IsFavourite = _track.IsFavourite;

                        Parallel.ForEach(MusicManager.Playlists, playlist =>
                        {
                            if (playlist.Name != MusicManager.SelectedPlaylist.Name)
                            {
                                playlist.ListTrack.FirstOrDefault(item => item.ID == _track.ID).IsFavourite = _track.IsFavourite;
                            }
                        });
                    }
                }));
            }
        }


        private RelayCommand _playSelectedPlaylist;
        public RelayCommand PlaySelectedPlaylist
        {
            get
            {
                return _playSelectedPlaylist ?? (_playSelectedPlaylist = new RelayCommand(parameter =>
                {
                    MusicManager.Stop();
                    MusicManager.CurrentPlaylist = MusicManager.SelectedPlaylist.ListTrack.Count > 0 ? MusicManager.SelectedPlaylist : MusicManager.CurrentPlaylist;
                    MusicManager.CurrentTrack = MusicManager.SelectedPlaylist.ListTrack.Count > 0 ? MusicManager.SelectedPlaylist.ListTrack.First() : null;

                    if (MusicManager.CurrentTrack != null && MusicManager.CurrentTrack.IsExist)
                        MusicManager.Play();
                }));
            }
        }


        private RelayCommand _openMiniPlayer;
        public RelayCommand OpenMiniPlayer
        {
            get
            {
                return _openMiniPlayer ?? (_openMiniPlayer = new RelayCommand(parameter =>
                {
                    var _miniPlayer = new MiniPlayer(_window.DataContext as MainWindowViewModel);
                    _window.Visibility = System.Windows.Visibility.Hidden;
                    _miniPlayer.Show();
                }));
            }
        }

        private RelayCommand _openAbout;

        public RelayCommand OpenAbout
        {
            get
            {
                return _openAbout ?? (_openAbout = new RelayCommand(parameter =>
                {
                    var about = new AboutWindow();
                    about.ShowDialog();
                }));
            }
        }



        void ChangeButtonVisibility()
        {
            if (MusicManager.State == Manager.PlayerState.Playing)
            {
                _window.button_play.Visibility = System.Windows.Visibility.Hidden;
                _window.button_pause.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                _window.button_play.Visibility = System.Windows.Visibility.Visible;
                _window.button_pause.Visibility = System.Windows.Visibility.Hidden;
            }
        }
    }
}
