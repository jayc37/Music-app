using Levitate.Core;
using Levitate.View;
using MahApps.Metro.Controls;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Levitate.ViewModel
{
    public partial class MainWindowViewModel : PropertyChangedBase
    {
        #region InstanceZone

        private static MainWindowViewModel _instance;
        public static MainWindowViewModel Instance
        {
            get { return _instance ?? (_instance = new MainWindowViewModel()); }
        }

        #endregion

        


        private Manager _musicManager;
        public Manager MusicManager
        {
            get { return _musicManager; }
            set
            {
                SetProperty(value, ref _musicManager);
            }
        }

        public MainWindow _window;

        #region Ctor
        public MainWindowViewModel()
        {
            MusicManager = new Manager();
            MusicManager.State = Manager.PlayerState.Stopping;

            Loaded(System.Windows.Application.Current.MainWindow as MainWindow);
        }
        #endregion


        public void Loaded(MainWindow window)
        {
            _window = window;
        }

        /// <summary>
        /// Handling Drag Drop file to window
        /// </summary>
        /// <param name="filepaths"></param>
        public async void DragDropFiles(string[] filepaths)
        {
            List<string> _directories = new List<string>();

            Parallel.ForEach(filepaths, f =>
            {
                if (Directory.Exists(f))
                {
                    // If f is an directory => scan f
                    _directories.AddRange(Core.Utils.ScanFolder(f));
                }
                else
                {
                    // If f is a file => collect
                    _directories.Add(f);
                }
            });

            // Handle tracks
            await Core.Utils.TrackProcess((MetroWindow)System.Windows.Application.Current.MainWindow, _directories);

            MusicManager.LoadApplicationPlaylist();
        }



        #region DragDrop 

        private TrackDropHandler _trackListDropHandler;
        public TrackDropHandler TrackListDropHandler
        {
            get { return _trackListDropHandler ?? (_trackListDropHandler = new TrackDropHandler()); }
        }

        private PlaylistListDropHandler _playlistListDropHandler;
        public PlaylistListDropHandler PlaylistListDropHandler
        {
            get { return _playlistListDropHandler ?? (_playlistListDropHandler = new PlaylistListDropHandler()); }
        }

        #endregion


    }
}
