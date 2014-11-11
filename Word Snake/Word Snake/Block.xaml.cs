using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Word_Snake
{
    public sealed partial class Block : UserControl
    {
        private String _text;

        public Block()
        {
            this.InitializeComponent();
        }

        public String Text
        {
            get
            {
                return _text;
            }

            set
            {
                _text = value;
                text_block.Text = _text.ToUpper();
            }
        }

        public Brush Fill
        {
            get
            {
                return box_rectangle.Fill;
            }

            set
            {
                box_rectangle.Fill = value;
            }
        }

        public Brush Stroke
        {
            get
            {
                return box_rectangle.Stroke;
            }

            set
            {
                box_rectangle.Stroke = value;
            }
        }

        public Brush TextColor
        {
            get
            {
                return text_block.Foreground;
            }

            set
            {
                text_block.Foreground = value;
            }
        }
    }
}
