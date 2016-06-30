using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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

namespace Sat_Apps_Mission_Control
{
    public sealed partial class SatKitLive : UserControl
    {


        public SatKitLive()
        {
            this.InitializeComponent();
            this.DataContext = this;

        }

        
        public SIKDataViewModel SIKdata
        {
            get { return (SIKDataViewModel)GetValue(SIKdataProperty); }
            set { SetValue(SIKdataProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SIKdataProperty =
            DependencyProperty.Register("SIKdata", typeof(SIKDataViewModel), typeof(SatKitLive), null);// new PropertyMetadata(0));


        public event PropertyChangedEventHandler PropertyChanged;
        void SetValueDp(DependencyProperty property, object value,
            [System.Runtime.CompilerServices.CallerMemberName] String p = null)
        {
            SetValue(property, value);
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(p));
        }
    }
}
