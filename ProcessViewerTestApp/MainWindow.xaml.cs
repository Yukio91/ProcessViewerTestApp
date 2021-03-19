using ProcessViewerTestApp.ViewModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProcessViewerTestApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        internal MainWindowViemModel Model => (MainWindowViemModel)DataContext;
        public ProcessListViewModel ProcessListViewModel => Model.ProcessListViewModel;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViemModel();
        }
    }
}
