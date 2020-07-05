using Levitate.Core;
using Levitate.ViewModel;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Levitate.View
{
    /// <summary>
    /// Interaction logic for MiniPlayer.xaml
    /// </summary>
    public partial class MiniPlayer : MetroWindow
    {
        public MiniPlayer(MainWindowViewModel mainWVM)
        {
            InitializeComponent();
            this.DataContext = mainWVM;
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {

            if (sizeInfo.WidthChanged) this.Width = sizeInfo.NewSize.Height * 1;
            else this.Height = sizeInfo.NewSize.Width / 1;
        }
        
        private void MetroWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            ((MainWindowViewModel)this.DataContext)._window.Visibility = Visibility.Visible;

            base.OnClosing(e);
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




        // Handling event ToggleButton is checked
        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            Updater.UpdateOnlyFavourite2DB(((MainWindowViewModel)this.DataContext).MusicManager.CurrentTrack.ID,
                ((MainWindowViewModel)this.DataContext).MusicManager.CurrentTrack.IsFavourite);
        }

        // Handling event ToggleButton is unchecked
        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            Updater.UpdateOnlyFavourite2DB(((MainWindowViewModel)this.DataContext).MusicManager.CurrentTrack.ID,
                ((MainWindowViewModel)this.DataContext).MusicManager.CurrentTrack.IsFavourite);
        }

        // Handling timeline valueChanged
        private void slider_Timeline_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var instance = this.DataContext as MainWindowViewModel;

            if (instance.MusicManager.PositionIsUpdate)
            {
                instance.MusicManager.PositionIsUpdate = false;
            }
            else
            {
                instance.MusicManager.Position = (int)slider_Timeline.Value;
                instance.MusicManager.Player.Position = TimeSpan.FromSeconds(slider_Timeline.Value);
            }
        }
        
    }
}
