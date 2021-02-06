﻿using System;
using System.Collections.Generic;
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

namespace MtgInventoryWpf
{
    /// <summary>
    /// Interaction logic for CardListView.xaml
    /// </summary>
    public partial class CardListView : UserControl
    {
        public CardListView()
        {
            InitializeComponent();
        }

        private async void OnPasteFromClipboard(object sender, RoutedEventArgs e)
        {
            if (DataContext is CardListViewModel viewModel)
            {
                await viewModel.CopyFromClipboard();
            }
        }
    }
}