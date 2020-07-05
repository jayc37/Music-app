using Levitate.Model;
using Levitate.ViewModel;
using System;

namespace Levitate.Core
{
    public class PlayEngine : PropertyChangedBase, IDisposable
    {
        private Track _currentTrack;
        public Track CurrentTrack
        {
            get { return _currentTrack; }
            set { _currentTrack = value; }
        }
        
        
        private WMPLib.WindowsMediaPlayer _player;
        public WMPLib.WindowsMediaPlayer Player
        {
            get { return _player; }
            set { _player = value; }
        }


        private WMPLib.IWMPPlaylist _playlist;
        public WMPLib.IWMPPlaylist Playlist
        {
            get { return _playlist; }
            set
            {
                if(!value.Equals(_playlist))
                {
                    SetProperty(value, ref _playlist);
                    OnPropertyChanged("Playlist");
                }
            }
        }


        private bool myVar;

        public bool MyProperty
        {
            get { return myVar; }
            set { myVar = value; }
        }



        public PlayEngine()
        {
            Player = new WMPLib.WindowsMediaPlayer();
            CurrentTrack.PlayingState = Utils.ConvertBitmapSourceToByteArray(new Uri(@"pack://application:,,,/Resources/Images/play.png"));
        }

        public void OpenTrack(Track track, Playlist playlist)
        {
            LoadPlaylist(playlist);
            Player.currentPlaylist = Playlist;
        }

        void LoadPlaylist(Playlist playlist)
        {
            foreach(var track in playlist.ListTrack)
            {
                Playlist.appendItem(Player.newMedia(track.Location));
            }
        }
        public void Stop()
        {
            Player.controls.stop();
        }

        public void Play()
        {
            if(CurrentTrack == null)
            {

            }
        }


        #region Dispose zone
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                CurrentTrack.Dispose();
                Player = null;
            }
        }
        #endregion

    }
}
