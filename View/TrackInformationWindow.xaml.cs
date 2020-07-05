using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Levitate.Core;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Levitate.Model;
using Levitate.ViewModel;

namespace Levitate.View
{
    /// <summary>
    /// Interaction logic for TrackInformationWindow.xaml
    /// </summary>
    public partial class TrackInformationWindow : MetroWindow, IDisposable
    {
        public Track Track { get; set; }

        public TrackInformationWindow(ref Track _track, string _cultureName)
        {
            Reader.GetDetailsOfTrack(ref _track);

            InitializeComponent();

            SetLanguage(_cultureName);

            Track = _track;
            DataContext = this;
        }

        // Move window when drag anywhere on surface
        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        // Switch on Edit mode
        private void toggleButton_EditMode_Checked(object sender, RoutedEventArgs e)
        {
            label_Editable.Visibility = Visibility.Visible;
            label_Uneditable.Visibility = Visibility.Hidden;
            button_OK.IsEnabled = true;

            textBox_Title.IsReadOnly = false; textBox_Title.ToolTip = textBlock_Title.Text;
            textBox_Artist.IsReadOnly = false; textBox_Artist.ToolTip = textBlock_Title.Text;
            textBox_Album.IsReadOnly = false; textBox_Album.ToolTip = textBlock_Title.Text;
            textBox_Album_artist.IsReadOnly = false; textBox_Album_artist.ToolTip = textBlock_Title.Text;
            textBox_Composer.IsReadOnly = false; textBox_Composer.ToolTip = textBlock_Title.Text;
            textBox_Genre.IsReadOnly = false; textBox_Genre.ToolTip = textBlock_Title.Text;
            textBox_Year.IsReadOnly = false; textBox_Year.ToolTip = textBlock_Title.Text;
            textBox_lyrics.IsReadOnly = false;
        }

        // Switch off Edit mode
        private void toggleButton_EditMode_Unchecked(object sender, RoutedEventArgs e)
        {
            label_Editable.Visibility = Visibility.Hidden;
            label_Uneditable.Visibility = Visibility.Visible;
            button_OK.IsEnabled = false;

            textBox_Title.IsReadOnly = true; textBox_Title.ToolTip = null;
            textBox_Artist.IsReadOnly = true; textBox_Artist.ToolTip = null;
            textBox_Album.IsReadOnly = true; textBox_Album.ToolTip = null;
            textBox_Album_artist.IsReadOnly = true; textBox_Album_artist.ToolTip = null;
            textBox_Composer.IsReadOnly = true; textBox_Composer.ToolTip = null;
            textBox_Genre.IsReadOnly = true; textBox_Genre.ToolTip = null;
            textBox_Year.IsReadOnly = true; textBox_Year.ToolTip = null;
            textBox_lyrics.IsReadOnly = true;
        }

        // Handling event when cancel button is clicked
        private async void button_X_Click(object sender, RoutedEventArgs e)
        {
            if (toggleButton_EditMode.IsChecked == true)
            {
                MessageDialogResult result = await this.ShowMessageAsync
                    (this.FindResource("attention").ToString(), this.FindResource("doYouWantToExit").ToString(), MessageDialogStyle.AffirmativeAndNegative, null);
                if (result == MessageDialogResult.Affirmative)
                {
                    this.Close();
                }
            }
            else
            {
                this.Close();
            }

        }

        // Handling event when x button is clicked 
        private async void OnCloseExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (toggleButton_EditMode.IsChecked == true)
            {
                MessageDialogResult result = await this.ShowMessageAsync
                    ((string)Application.Current.Resources["attention"], (string)Application.Current.Resources["doYouWantToExit"], MessageDialogStyle.AffirmativeAndNegative, null);
                if (result == MessageDialogResult.Affirmative)
                {
                    this.Close();
                }
            }
            else
            {
                this.Close();
            }
        }

        // Setting language
        private void SetLanguage(string _cultureName = null)
        {
            if (_cultureName != null)
            {
                ResourceDictionary dict = new ResourceDictionary();

                switch (_cultureName)
                {
                    case "vi":
                        dict.Source = new Uri("/Resources/Languages/Lang_vi-vn.xaml", UriKind.Relative);
                        break;
                    case "jp":
                        dict.Source = new Uri("/Resources/Languages/Lang_jp-jp.xaml", UriKind.Relative);
                        break;
                    case "en":
                    default:
                        dict.Source = new Uri("/Resources/Languages/Lang_en-us.xaml", UriKind.Relative);
                        break;
                }

                this.Resources.MergedDictionaries.Add(dict);

                Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(_cultureName);
                Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(_cultureName);
            }
        }

        // Open file explorer if Location textbox is doubleclicked
        private void textBox_Location_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Track.OpenTrackLocation();
        }


        // Handling event okButton is clicked
        private void button_OK_Click(object sender, RoutedEventArgs e)
        {
            Updater.UpdateDetails2DB(this.Track);
            Updater.UpdateDetails2File(this.Track);
            this.Dispose();
            this.Close();
        }

        // Handling event ToggleButton is checked
        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            Updater.UpdateOnlyFavourite2DB(Track.ID, Track.IsFavourite);
        }

        // Handling event ToggleButton is unchecked
        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            Updater.UpdateOnlyFavourite2DB(Track.ID, Track.IsFavourite);
        }

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
                if (disposing)
                {
                    Track.DisposeDetails();
                }
                _disposed = true;
            }
        }

        ~TrackInformationWindow()
        {
            Dispose(false);
        }
        private bool _disposed = false;
        #endregion

    }
}
