﻿using Azure.Storage.Blobs.Models;
using Microsoft.Win32;
using RemoteRepo.ViewModels;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RemoteRepo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ItemsViewModel itemViewModel;
        public MainWindow()
        {
            InitializeComponent();
            itemViewModel = new ItemsViewModel();
            Init();
        }

        private void Init()
        {
            cbDirectories.ItemsSource = itemViewModel.Directories;
            lbItems.ItemsSource = itemViewModel.Items;
        }

        private void LbItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataContext = lbItems.SelectedItem as BlobItem;
        }

        private void CbDirectories_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key==Key.Enter)
            {
                itemViewModel.Directory = cbDirectories.Text.Trim();
                cbDirectories.SelectedItem = itemViewModel.Directory;
            }
        }

        private void CbDirectories_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (itemViewModel.Directories.Contains(cbDirectories.Text))
            {
                itemViewModel.Directory = cbDirectories.Text;
                cbDirectories.SelectedItem = itemViewModel.Directory;
            }
        }

        private async void BtnUpload_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            if (dialog.ShowDialog()==true)
            {
                await itemViewModel.UploadAsync(dialog.FileName/*, cbDirectories.Text*/);
            }
            cbDirectories.SelectedItem = itemViewModel.Directory;
        }

        private async void BtnDownload_Click(object sender, RoutedEventArgs e)
        {
            if (lbItems.SelectedItem is not BlobItem item)
            {
                return;
            }
            var dialog = new SaveFileDialog 
            {
                FileName = item.Name[(item.Name.LastIndexOf(ItemsViewModel.ForwardSlash)+1)..],
            };
            if (dialog.ShowDialog() == true)
            {
                await itemViewModel.DownloadAsync(item, dialog.FileName);
            }
            cbDirectories.SelectedItem = itemViewModel.Directory;
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (lbItems.SelectedItem is not BlobItem item)
            {
                return;
            }
            await itemViewModel.DeleteAsync(item);
            cbDirectories.Text = itemViewModel.Directory;

        }
    }
}