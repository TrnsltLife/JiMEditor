using JiME.Common;
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

namespace JiME.Views
{
    /// <summary>
    /// Interaction logic for MagWizWindow.xaml
    /// </summary>
    public partial class MagWizWindow : Window
    {
        public MagWizWindow()
        {
            InitializeComponent();
            WindowExtension.SetCloseButtonVisibility(this, CloseButtonVisibility.CloseDisabled);
        }

        public void SearchTextChanged(object sender, EventArgs e)
        {
            Console.WriteLine("Search for: " + ((TextBox)sender).Text);
        }

        public void BorderDoubleClick(object sender, EventArgs e)
        {
            Console.WriteLine("BorderDoubleClick");
            WindowState = WindowState.Minimized;
        }
    }
}
