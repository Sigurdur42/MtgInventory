﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MtgBinder.Avalonia.ViewModels;

namespace MtgBinder.Avalonia.Views
{
    public class MainWindow : Window
    {
        private readonly MainWindowViewModel _viewModel;

        public MainWindow()
            : this(null)
        {

        }

        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;

#if DEBUG
            this.AttachDevTools();
#endif

            // Wire up view models until we know better
            this.FindControl<CardLookupView>("cardLookupView")?.SetViewModel(viewModel.CardLookup);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
