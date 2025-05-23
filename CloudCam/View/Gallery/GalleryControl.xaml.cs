﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ReactiveUI;

namespace CloudCam.View.Gallery
{
    /// <summary>
    /// Interaction logic for GalleryControl.xaml
    /// </summary>
    public partial class GalleryControl
    {
        private Random _random = new Random();

        public GalleryControl()
        {
            InitializeComponent();

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.CurrentImage, v => v.ImageControl.Source).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.CurrentImage, v => v.ImageControl.LayoutTransform,
                    (_) => new RotateTransform(_random.Next(-20, 20), 0.5, 0.5)).DisposeWith(d);
            });
        }
    }
}
