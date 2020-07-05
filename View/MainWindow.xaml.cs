using Levitate.Core;
using Levitate.Model;
using Levitate.Properties;
using Levitate.ViewModel;
using MahApps.Metro.Controls;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Levitate.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        #region Properties Declaration
        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;
        #endregion


        public MainWindow()
        {
            InitializeComponent();
            SetLanguage(Settings.Default.Lang);
        }

        

        #region Setting Languages
        // Property
        private string _culture;
        public string CultureName
        {
            get { return _culture = Properties.Settings.Default.Lang; }
            set
            {
                if (value != _culture)
                {
                    _culture = Properties.Settings.Default.Lang = value;
                    Properties.Settings.Default.Save();
                }
            }
        }
        /// <summary>
        /// Setting languege
        /// </summary>
        /// <param name="_cultureName"></param>
        private void SetLanguage(string _cultureName)
        {
            if (_cultureName != null)
            {
                ResourceDictionary dict1 = new ResourceDictionary { Source = new Uri("/Resources/Languages/Lang_vi-vn.xaml", UriKind.Relative) };
                ResourceDictionary dict2 = new ResourceDictionary { Source = new Uri("/Resources/Languages/Lang_jp-jp.xaml", UriKind.Relative) };
                ResourceDictionary dict3 = new ResourceDictionary { Source = new Uri("/Resources/Languages/Lang_en-us.xaml", UriKind.Relative) };

                Application.Current.Resources.MergedDictionaries.Remove(dict1);
                Application.Current.Resources.MergedDictionaries.Remove(dict2);
                Application.Current.Resources.MergedDictionaries.Remove(dict3);

                switch (_cultureName)
                {
                    case "vi":
                        Resources.MergedDictionaries.Add(dict1);
                        radioButton_vn.IsChecked = true;
                        break;
                    case "jp":
                        Resources.MergedDictionaries.Add(dict2);
                        radioButton_jp.IsChecked = true;
                        break;
                    case "en":
                    default:
                        Resources.MergedDictionaries.Add(dict3);
                        radioButton_en.IsChecked = true;
                        break;
                }

                Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(_cultureName);
                Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(_cultureName);

                CultureName = _cultureName;
            }
        }


        // Radio buttons' events handler
        private void radioButton_en_Checked(object sender, RoutedEventArgs e)
        {
            CultureName = "en";
            SetLanguage(CultureName);
        }
        private void radioButton_vn_Checked(object sender, RoutedEventArgs e)
        {
            CultureName = "vi";
            SetLanguage(CultureName);
        }
        private void radioButton_jp_Checked(object sender, RoutedEventArgs e)
        {
            CultureName = "jp";
            SetLanguage(CultureName);
        }
        #endregion


        #region Handling Grid's events
        // Catching dragging event => move window along to cursor
        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }


        // Catching onclosing event => save all settings from user
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();

            var instance = this.DataContext as MainWindowViewModel;
            instance.MusicManager.Dispose();

            base.OnClosing(e);
        }

        /*
        // Handling Drag drop files event
        private async void Grid_Drop(object sender, System.Windows.DragEventArgs e)
        {
            // Get all dropped files/folders
            string[] _files = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop, false);
            List<string> _directories = new List<string>();

            Parallel.ForEach(_files, f =>
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
            await Core.Utils.TrackProcess(this, _directories);

            MainWindowViewModel.Instance.MusicManager.LoadApplicationPlaylist();
        }

        // Handling Effect when it has drag enter event
        private void Grid_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            e.Effects = System.Windows.DragDropEffects.Move;
        }
        */
        #endregion


        // Handling ToggleButton is clicked event
        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            //ToggleButton _heart = (ToggleButton)sender;
            var instance = this.DataContext as MainWindowViewModel;

            // Push value down to model
            Model.Track _track = ((ToggleButton)sender).DataContext as Model.Track;
            Updater.UpdateOnlyFavourite2DB(_track.ID, _track.IsFavourite);

            if (instance.MusicManager.SelectedPlaylist.Prio == Playlist.Priority.User)
            {
                instance.MusicManager.AllSongs.ListTrack.FirstOrDefault(item => item.ID == _track.ID).IsFavourite = _track.IsFavourite;

                for (int i = 0; i < instance.MusicManager.Playlists.Count; i++)
                {
                    if (instance.MusicManager.Playlists[i].Name != instance.MusicManager.SelectedPlaylist.Name)
                    {
                        for (int j = 0; j < instance.MusicManager.Playlists[i].ListTrack.Count; j++)
                        {
                            if(instance.MusicManager.Playlists[i].ListTrack[i].ID == _track.ID)
                            instance.MusicManager.Playlists[i].ListTrack[j].IsFavourite = _track.IsFavourite;
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < instance.MusicManager.Playlists.Count; i++)
                {
                    if (instance.MusicManager.Playlists[i].Name != instance.MusicManager.SelectedPlaylist.Name)
                    {
                        for (int j = 0; j < instance.MusicManager.Playlists[i].ListTrack.Count; j++)
                        {
                            if (instance.MusicManager.Playlists[i].ListTrack[i].ID == _track.ID)
                                instance.MusicManager.Playlists[i].ListTrack[j].IsFavourite = _track.IsFavourite;
                        }
                    }
                }
            }
            
        }

        // Handling listView is double clicked event
        private void listView_View_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var instance = this.DataContext as MainWindowViewModel;
            instance.PlaySelectedTrack.Execute(null);

            if(instance.MusicManager.State == Manager.PlayerState.Playing)
            {
                button_play.Visibility = Visibility.Hidden;
                button_pause.Visibility = Visibility.Visible;
            }
            else
            {
                button_play.Visibility = Visibility.Visible;
                button_pause.Visibility = Visibility.Hidden;
            }
        }
        
        // Handling file drag-drop-ed
        private void listView_View_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
        }

        // Handling file drag-drop-ed
        private void listView_View_Drop(object sender, DragEventArgs e)
        {
            if (e.Effects == DragDropEffects.None)
                return;

            var instance = this.DataContext as MainWindowViewModel;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                instance.DragDropFiles((string[])e.Data.GetData(DataFormats.FileDrop));
            }

            instance.MusicManager.LoadApplicationPlaylist();
        }
        
        // Handling timeline valueChanged
        private void slider_Timeline_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //var instance = this.DataContext as MainWindowViewModel;
            if (((MainWindowViewModel)this.DataContext).MusicManager.PositionIsUpdate)
            {
                ((MainWindowViewModel)this.DataContext).MusicManager.PositionIsUpdate = false;
            }
            else
            {
                ((MainWindowViewModel)this.DataContext).MusicManager.Position = (int)slider_Timeline.Value;
                ((MainWindowViewModel)this.DataContext).MusicManager.Player.Position = TimeSpan.FromSeconds(slider_Timeline.Value);
            }
        }


        #region classical PlayPause clicked event
        private void button_play_Click(object sender, RoutedEventArgs e)
        {
            var instance = this.DataContext as MainWindowViewModel;
            instance.TogglePlayPause.Execute(null);

            button_play.Visibility = System.Windows.Visibility.Hidden;
            button_pause.Visibility = System.Windows.Visibility.Visible;
        }

        private void button_pause_Click(object sender, RoutedEventArgs e)
        {
            var instance = this.DataContext as MainWindowViewModel;
            instance.TogglePlayPause.Execute(null);

            button_play.Visibility = System.Windows.Visibility.Visible;
            button_pause.Visibility = System.Windows.Visibility.Hidden;
        }

        #endregion


        #region Sort Zone
        private void ColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            var instance = this.DataContext as MainWindowViewModel;

            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            string sortBy = column.Tag.ToString();
            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                instance.MusicManager.ViewSource.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortCol = column;

            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            instance.MusicManager.ViewSource.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }
        #endregion


        #region Mouse move over header Image
        private void header_coverArt_MouseEnter(object sender, MouseEventArgs e)
        {
            header_button_miniPlayerChanger.Visibility = Visibility.Visible;
        }
        private void header_coverArt_MouseLeave(object sender, MouseEventArgs e)
        {
            header_button_miniPlayerChanger.Visibility = Visibility.Hidden;
        }
        #endregion
    }


    // Sort Adorner
    public class SortAdorner : Adorner
    {
        private static Geometry ascGeometry =
                Geometry.Parse("M 0 4 L 3.5 0 L 7 4 Z");

        private static Geometry descGeometry =
                Geometry.Parse("M 0 0 L 3.5 4 L 7 0 Z");

        public ListSortDirection Direction { get; private set; }

        public SortAdorner(UIElement element, ListSortDirection dir)
                : base(element)
        {
            this.Direction = dir;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (AdornedElement.RenderSize.Width < 20)
                return;

            TranslateTransform transform = new TranslateTransform
                    (
                            AdornedElement.RenderSize.Width - 15,
                            (AdornedElement.RenderSize.Height - 5) / 2
                    );
            drawingContext.PushTransform(transform);

            Geometry geometry = ascGeometry;
            if (this.Direction == ListSortDirection.Descending)
                geometry = descGeometry;
            drawingContext.DrawGeometry(Brushes.LightGray, null, geometry);

            drawingContext.Pop();
        }
    }
}
