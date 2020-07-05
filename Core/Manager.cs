using Levitate.Model;
using Levitate.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Threading;

namespace Levitate.Core
{
    public class Manager : PropertyChangedBase, IDisposable
    {
        #region Declarations
        /*
        private Track _selectedTrack;
        public Track SelectedTrack
        {
            get { return _selectedTrack; }
            set { SetProperty(value, ref _selectedTrack); }
        }
        */

        #region InstanceZone

        private static Manager _managerInstance;
        public static Manager ManagerInstance
        {
            get { return _managerInstance ?? (_managerInstance = new Manager()); }
        }

        #endregion

        private Track _currentTrack;
        public Track CurrentTrack
        {
            get { return _currentTrack; }
            set
            {
                if (_currentTrack != value)
                {
                    SetProperty(value, ref _currentTrack);
                    OnPropertyChanged("CurrentTrack");
                }
            }
        }


        private Track _selectedTrack;
        public Track SelectedTrack
        {
            get { return _selectedTrack; }
            set
            {
                if (_selectedTrack != value)
                {
                    SetProperty(value, ref _selectedTrack);
                    OnPropertyChanged("SelectedTrack");
                }
            }
        }




        private bool _isLoopOne;
        public bool IsLoopOne
        {
            get { return _isLoopOne; }
            set
            {
                if (value != _isLoopOne)
                {
                    SetProperty(value, ref _isLoopOne);
                    OnPropertyChanged("IsLoopOne");
                }
            }
        }


        private bool _isShuffle;
        public bool IsShuffle
        {
            get { return _isShuffle; }
            set
            {
                if (SetProperty(value, ref _isShuffle) && value)
                {
                    OnPropertyChanged("IsShuffle");
                }
            }
        }
        



        private Playlist _selectedPlaylist;
        public Playlist SelectedPlaylist
        {
            get { return _selectedPlaylist; }
            set
            {
                if(SetProperty(value, ref _selectedPlaylist))
                {
                    OnPropertyChanged("SelectedPlaylist");
                    ViewSource = (CollectionView)CollectionViewSource.GetDefaultView(SelectedPlaylist.ListTrack);
                    ViewSource.Refresh();
                }
            }
        }


        private Playlist _currentPlaylist;
        public Playlist CurrentPlaylist
        {
            get { return _currentPlaylist; }
            set
            {
                SetProperty(value, ref _currentPlaylist);
                OnPropertyChanged("CurrentPlaylist");
            }
        }


        private Playlist _allSongs;                         //ok
        public Playlist AllSongs
        {
            get { return _allSongs; }
            set
            {
                SetProperty(value, ref _allSongs);
                OnPropertyChanged("AllSongs");
            }
        }
        
        public Playlist RecentlyAdded
        {
            get { return Reader.GetRecentlyAdded(AllSongs); }
        }
        
        public Playlist Favourites
        {
            get { return Reader.GetFavourites(AllSongs); }
        }


        private ICollectionView _viewSource;
        public ICollectionView ViewSource
        {
            get { return _viewSource; }
            set
            {
                SetProperty(value, ref _viewSource);
            }
        }




        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                if (SetProperty(value, ref _searchText))
                {
                    ViewSource.Filter = item => string.IsNullOrWhiteSpace(SearchText) || item.ToString().ToUpper().Contains(SearchText.ToUpper());

                    ViewSource.Refresh();
                }
            }
        }




        private ObservableCollection<Playlist> _playlists;  //ok
        public ObservableCollection<Playlist> Playlists
        {
            get { return _playlists; }
            set
            {
                SetProperty(value, ref _playlists);
                OnPropertyChanged("Playlists");
            }
        }
        



        private byte[] _currentTrackCoverArt;
        public byte[] CurrentTrackCoverArt
        {
            get { return _currentTrackCoverArt; }
            set
            {
                if (value != _currentTrackCoverArt)
                {
                    SetProperty(value, ref _currentTrackCoverArt);
                    OnPropertyChanged("CurrentTrackCoverArt");
                }
            }
        }




        public enum PlayerState
        {
            Playing,
            Pausing,
            Stopping
        }


        private PlayerState _state = PlayerState.Stopping;
        public PlayerState State
        {
            get { return _state; }
            set
            {
                SetProperty(value, ref _state);
                OnPropertyChanged("State");
            }
        }


        public DispatcherTimer Timer { get; set; }



        public bool positionIsUpdate = true;
        public bool PositionIsUpdate
        {
            get { return positionIsUpdate; }
            set
            {
                if(value!= positionIsUpdate)
                {
                    SetProperty(value, ref positionIsUpdate);
                    OnPropertyChanged("PositionIsUpdate");
                }
            }
        }



        private int _position = 0;
        public int Position
        {
            get { return _position; }
            set
            {
                if(value!=_position)
                {
                    SetProperty(value, ref _position);
                    OnPropertyChanged("Position");
                    TimeGone = Player.Position;
                }
            }
        }



        private int _maxPosition = 1;
        public int MaxPosition
        {
            get { return _maxPosition; }
            set
            {
                if (value != _maxPosition)
                {
                    SetProperty(value, ref _maxPosition);
                    OnPropertyChanged("MaxPosition");
                }
            }
        }



        private TimeSpan _timeGone = TimeSpan.Zero;
        public TimeSpan TimeGone
        {
            get { return _timeGone; }
            set
            {
                if (SetProperty(value, ref _timeGone))
                {
                    OnPropertyChanged("TimeGone");
                    TimeRemain = CurrentTrack.Duration - value;
                }
            }
        }



        private TimeSpan _timeRemain = TimeSpan.Zero;
        public TimeSpan TimeRemain
        {
            get { return _timeRemain; }
            set
            {
                _timeRemain = value;
                OnPropertyChanged("TimeRemain");
            }
        }



        //public WMPLib.WindowsMediaPlayer Player { get; protected set; }
        public System.Windows.Media.MediaPlayer Player { get; set; }
        //public System.Windows.Media.MediaClock Clock { get; set; }
        //public System.Windows.Media.MediaTimeline Timeline { get; set; }
        public ManagerCommands Commands { get; protected set; }
        
        #endregion



        #region Ctor, Loading and Events
        /// <summary>
        /// Ctor
        /// </summary>
        public Manager()
        {
            Playlists = new ObservableCollection<Playlist>();

            Load();
            Commands = new ManagerCommands(this);
        }

        /// <summary>
        /// Load method
        /// </summary>
        void Load()
        {
            // Load Player state
            IsLoopOne = Properties.Settings.Default.IsLoopOne;
            IsShuffle = Properties.Settings.Default.IsShuffle;

            // Load playlist
            LoadPlaylists();
            
            // Load engine
            LoadEngine();
        }

        /// <summary>
        /// Load media player engine
        /// </summary>
        void LoadEngine()
        {
            //Player = new WMPLib.WindowsMediaPlayer();
            //Player.settings.setMode("autoRewind", IsLoopOne);
            
            Player = new System.Windows.Media.MediaPlayer();
            //Player.Volume = Properties.Settings.Default.volume;
            Player.MediaEnded += Player_MediaEnded;
            
            Timer = new DispatcherTimer();
            Timer.Interval = new TimeSpan(0, 0, 1);
            Timer.Tick += new EventHandler(Timer_Tick);
        }

        /// <summary>
        /// Load Playlists
        /// </summary>
        void LoadPlaylists()
        {
            // Load user's playlists
            Task t1 = Task.Factory.StartNew(() => Playlists = PlaylistsSetting.LoadPlaylists());

            // Load application playlist
            Task t2 = Task.Factory.StartNew(() => LoadApplicationPlaylist());

            Task.WaitAll();
        }

        /// <summary>
        /// Load application type playlist and pump data to this fields
        /// </summary>
        public void LoadApplicationPlaylist()
        {
            // Load library
            AllSongs = Reader.GetSongs();

            CurrentPlaylist = SelectedPlaylist = AllSongs;

            if (CurrentTrack == null)
            {
                CurrentTrack = CurrentPlaylist.ListTrack.Count > 0 ? CurrentPlaylist.ListTrack.First() : null;
                CurrentTrackCoverArt = CurrentTrack != null ? Reader.GetCoverArt(CurrentTrack) : null;
                MaxPosition = (int)CurrentTrack.Duration.TotalSeconds;

                if (CurrentTrack != null)
                    TimeRemain = CurrentTrack.Duration;
            }
        }




        /// <summary>
        /// Handling tick event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, EventArgs e)
        {
            PositionIsUpdate = true;
            Position++;
        }

        /// <summary>
        /// Handling complete a song event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Player_MediaEnded(object sender, EventArgs e)
        {
            CurrentTrack.PlayingState = null;
            Timer.Stop();
            Updater.UpdatePlayCount(CurrentTrack);
            PlayForward();
        }

        #endregion



        #region Play control
        /// <summary>
        /// Stop playing
        /// </summary>
        public void Stop()
        {
            State = PlayerState.Stopping;
            Player.Stop();
            Timer.Stop();

            Position = 0;
            TimeGone = new TimeSpan();
            TimeRemain = new TimeSpan();

            MaxPosition = 0;

            if (CurrentTrack != null)
                CurrentTrack.PlayingState = null;
        }

        /// <summary>
        ///  Pause playing
        /// </summary>
        public void Pause()
        {
            if (State == PlayerState.Stopping) return;
            
            State = PlayerState.Pausing;
            CurrentTrack.PlayingState = Utils.ConvertBitmapSourceToByteArray(new Uri(@"pack://application:,,,/Resources/Images/pause.png"));
            Player.Pause();
            Timer.Stop();
        }

        /// <summary>
        /// Begin play
        /// </summary>
        public void BeginPlay()
        {
            if (!CurrentTrack.IsExist) PlayForward();

            // Open track
            Player.Open(new Uri(CurrentTrack.Location));

            // Change icon state
            CurrentTrack.PlayingState = Utils.ConvertBitmapSourceToByteArray(new Uri(@"pack://application:,,,/Resources/Images/play.png"));

            // Call cover art
            CurrentTrackCoverArt = Reader.GetCoverArt(CurrentTrack);

            // Update last time play
            Updater.UpdateLastTimePlay(CurrentTrack);

            // Get MaxPosition
            MaxPosition = (int)CurrentTrack.Duration.TotalSeconds;
            Position = 0;

            // Change state of engine
            State = PlayerState.Playing;

            // Play and start timer
            Player.Play();
            Timer.Start();
        }

        /// <summary>
        /// Check current track and play it
        /// </summary>
        public void Play()
        {
            if (State == PlayerState.Playing)
                return;

            if (State == PlayerState.Pausing)
            {
                State = PlayerState.Playing;
                CurrentTrack.PlayingState = Utils.ConvertBitmapSourceToByteArray(new Uri(@"pack://application:,,,/Resources/Images/play.png"));

                Player.Play();
                Timer.Start();
                return;
            }

            if(State == PlayerState.Stopping)
            {
                if (CurrentTrack != null)
                {
                    BeginPlay();
                    return;
                }
                else if (CurrentPlaylist != null && CurrentPlaylist.ListTrack.Count > 0)
                {
                    CurrentTrack = CurrentPlaylist.ListTrack.First();
                    BeginPlay();
                    return;
                }
                else if (SelectedPlaylist != null && SelectedPlaylist.ListTrack.Count > 0)
                {
                    CurrentPlaylist = SelectedPlaylist;
                    CurrentTrack = CurrentPlaylist.ListTrack.First();
                    BeginPlay();
                    return;
                }
            }
        }
        


        /// <summary>
        /// Play Selected track in selected playlist
        /// </summary>
        public void PlaySelectedTrack()
        {
            Stop();

            if (SelectedTrack != null)
            {
                // Change current playlist and track
                CurrentPlaylist = SelectedPlaylist;
                CurrentTrack = SelectedTrack;

                BeginPlay();
            }
        }
        
        /// <summary>
        /// Go forward
        /// </summary>
        public void PlayForward()
        {
            // Stop playing
            //Stop();
            CurrentTrack.PlayingState = null;

            // If current playlist null => return
            if (CurrentPlaylist == null || CurrentPlaylist.ListTrack.Count == 0)
            {
                return;
            }

            // If loop one => back to Zero
            if (IsLoopOne)
            {
                Timer.Start();
                Position = 0;
                Player.Position = TimeSpan.Zero;
                return;
            }

            // Call next track and assign to CurrentTrack
            if(!IsShuffle)
                CurrentTrack = Playlist.NextTrack(CurrentPlaylist, CurrentTrack);
            else
                CurrentTrack = Playlist.RandomTrack(CurrentPlaylist);

            // Load CoverArt
            CurrentTrackCoverArt = Reader.GetCoverArt(CurrentTrack);

            // Checking
            if (CurrentTrack.IsExist)
            {
                if (State == PlayerState.Playing)
                {
                    BeginPlay();
                    return;
                }
                return;
            }
            else
                PlayForward();
        }

        /// <summary>
        /// Go backward
        /// </summary>
        public void PlayBackward()
        {
            // Stop playing
            //Stop();
            CurrentTrack.PlayingState = null;

            // If current playlist null => return
            if (CurrentPlaylist == null || CurrentPlaylist.ListTrack.Count == 0)
            {
                return;
            }

            // If loop one => back to Zero
            if (IsLoopOne)
            {
                Player.Position = TimeSpan.Zero;
                return;
            }

            // Call next track and assign to CurrentTrack
            if (!IsShuffle)
                CurrentTrack = Playlist.PreviousTrack(CurrentPlaylist, CurrentTrack);
            else
                CurrentTrack = Playlist.RandomTrack(CurrentPlaylist);

            // Load CoverArt
            CurrentTrackCoverArt = Reader.GetCoverArt(CurrentTrack);

            // Checking
            if (CurrentTrack.IsExist)
            {
                if (State == PlayerState.Playing)
                {
                    BeginPlay();
                    return;
                }
                return;
            }
            else
                PlayBackward();
        }
        #endregion



        #region Essential Methods

        /// <summary>
        /// Delete one given playlist
        /// </summary>
        /// <param name="playlist"></param>
        public void DeleteOnePlaylist(Playlist playlist)
        {
            if (playlist.Prio == Playlist.Priority.User && playlist.CanEdit)
            {
                // Check if engine is play a song in a user playlist => stop and change current playlist to AllSongs
                if (CurrentPlaylist == playlist)
                {
                    Stop();
                    CurrentPlaylist = AllSongs;
                }

                Playlists.Remove(playlist);
                ViewSource.Refresh();
            }
        }

        /// <summary>
        /// Delete all user playlist
        /// </summary>
        public void DeleteAllUserPlaylist()
        {
            // Check if engine is play a song in a user playlist => stop and change current playlist to recently added
            if (Playlists != null)
            {
                if (Playlists.Contains(CurrentPlaylist))
                {
                    Stop();
                    CurrentPlaylist = AllSongs;
                }

                // Delete all user playlists
                for (int i = 0; i < Playlists.Count; i++)
                {
                    if (Playlists[i].Prio == Playlist.Priority.User)
                    {
                        if (Playlists[i].Name == SelectedPlaylist.Name)
                            SelectedPlaylist = AllSongs;
                        Playlists.Remove(Playlists[i]);
                    }
                }

                ViewSource.Refresh();
            }
        }

        /// <summary>
        /// Delete one given track
        /// </summary>
        /// <param name="track"></param>
        public void DeleteOneTrack(Track track)
        {
            // Kick out current track
            if (track == CurrentTrack)
            {
                Stop();
                CurrentTrack = Playlist.NextTrack(CurrentPlaylist, CurrentTrack);
            }

            // If playlist is application and can not edit => return
            if (!SelectedPlaylist.CanEdit && SelectedPlaylist.Prio == Playlist.Priority.Application)
                return;
            // In case selected playlist is current playlist
            else if (CurrentPlaylist.DateCreated == SelectedPlaylist.DateCreated)
            {
                // In case that current playlist is All Songs
                if (SelectedPlaylist.CanEdit && SelectedPlaylist.Prio == Playlist.Priority.Application)
                {
                    RemoveSelecurrent(track);
                    Updater.DeleteOneTrack(track);
                }
                RemoveSelecurrent(track);
            }
            // In case All Songs playlist
            else if (SelectedPlaylist.CanEdit && SelectedPlaylist.Prio == Playlist.Priority.Application)
            {
                RemoveInAllSongs(track);
            }
            // In case User's playlist
            else if (SelectedPlaylist.Prio == Playlist.Priority.User)
            {
                RemoveSelecPL(track);
            }

            ViewSource.Refresh();
        }


        #region Essential for deleting one track
        void RemoveSelecurrent(Track track)
        {
            if (CurrentTrack == track)
            {
                Stop();
                if (!IsLoopOne)
                    PlayForward();
            }
            CurrentPlaylist.RemoveTrack(track);
            SelectedPlaylist.RemoveTrack(track);
        }
        void RemoveSelecPL(Track track)
        {
            SelectedPlaylist.RemoveTrack(track);
        }
        void RemoveInAllSongs(Track track)
        {
            if (CurrentTrack == track)
            {
                Stop();
                if (!IsLoopOne)
                    PlayForward();
            }
            CurrentPlaylist.RemoveTrack(track);
            SelectedPlaylist.RemoveTrack(track);
            Updater.DeleteOneTrack(track);
        }
        #endregion


        /// <summary>
        /// Save settings
        /// </summary>
        public void Save()
        {
            using (var setting = new PlaylistsSetting())
            {
                setting.Playlists = this.Playlists;
                setting.SavePlaylist();
            }

            Properties.Settings.Default.volume = Player.Volume;
            Properties.Settings.Default.IsLoopOne = IsLoopOne;
            Properties.Settings.Default.IsShuffle = IsShuffle;

            Properties.Settings.Default.Save();
        }

        #endregion



        #region Disposable Implement
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                Save();

                if (disposing)
                {
                    Stop();

                    //CurrentPlaylist = null;
                    //CurrentTrack = null;
                    Player.Close();
                    //Playlists = null;
                    //SelectedPlaylist = null;
                    //SelectedTrack = null;
                    //CurrentTrackCoverArt = null;
                    //Position = 0;
                    //MaxPosition = 0;
                }

                _disposed = true;
            }
        }

        ~Manager()
        {
            Dispose(false);
        }
        private bool _disposed = false;
        #endregion

    }
}
