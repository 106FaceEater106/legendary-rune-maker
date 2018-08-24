﻿using LCU.NET;
using Legendary_Rune_Maker.Data;
using Onova;
using Onova.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Legendary_Rune_Maker
{
    /// <summary>
    /// Interaction logic for LoadingWindow.xaml
    /// </summary>
    public partial class LoadingWindow : Window
    {
        private bool Shown;
        private CancellationTokenSource CancelSource = new CancellationTokenSource();

        public LoadingWindow()
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            
            InitializeComponent();
        }

        private void ShowMainWindow()
        {
            if (Shown)
                return;
            Shown = true;

            new MainWindow();
            this.Close();
        }

        private async void Window_Initialized(object sender, EventArgs e)
        {
            var manager = new UpdateManager(
                new WebPackageResolver("https://pipe0481.heliohost.org/plrm.man"),
                new ZipPackageExtractor());
            var update = await manager.CheckForUpdatesAsync();

            if (update.CanUpdate)
            {
                Status.Text = "Updating...";
                Hint.Visibility = Visibility.Hidden;
                Cancel.Visibility = Visibility.Visible;
                Progress.IsIndeterminate = false;
                
                var progress = new Progress<double>(o => Progress.Value = o);

                try
                {
                    await manager.PrepareUpdateAsync(update.LastVersion, progress, CancelSource.Token);
                }
                catch (TaskCanceledException)
                {
                }

                if (manager.IsUpdatePrepared(update.LastVersion))
                {
                    manager.LaunchUpdater(update.LastVersion);
                    Environment.Exit(0);

                    return; //Just in case
                }
            }

            Dispatcher.Invoke(() =>
            {
                Progress.Value = 0;
                Status.Text = "Loading...";
            });

            await Riot.CacheAll(o => Dispatcher.Invoke(() => Progress.Value = o));
            
            Dispatcher.Invoke(ShowMainWindow);
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            CancelSource.Cancel();
            Cancel.IsEnabled = false;
        }
    }
}