using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CloudCam
{
    class OutlinedTextBlock : Panel
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                "Text", typeof(string),
                typeof(OutlinedTextBlock),
                new PropertyMetadata(TextChanged)
            );

        private static void TextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((OutlinedTextBlock)d).InvalidateVisual();
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        
        protected override void OnRender(DrawingContext drawingContext)
        {
            if (String.IsNullOrWhiteSpace(Text))
            {
                return;
            }

            // Create the FormattedText.
            const double fontsize = 70;
            Typeface typeface = new Typeface("Segoe");
            FormattedText formatted_text =
                new FormattedText(Text,
                    new CultureInfo("en-us"), FlowDirection.LeftToRight,
                    typeface, fontsize, Brushes.Black);
            formatted_text.SetFontWeight(FontWeights.Bold);

            // Center horizontally.
            formatted_text.TextAlignment = TextAlignment.Center;

            // Pick an origin to center the text.
            Point origin = new Point(
                this.ActualWidth / 2,
                (this.ActualHeight - formatted_text.Height) / 2);

            // Convert the text into geometry.
            Geometry geometry = formatted_text.BuildGeometry(origin);

            // Draw the geometry.
            Pen pen = new Pen(Brushes.Red, 2);
            drawingContext.DrawGeometry(Brushes.Yellow, pen, geometry);
        }
    }
}
