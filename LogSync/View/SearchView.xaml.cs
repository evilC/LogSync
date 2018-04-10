using LogSync.ViewModel;
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

namespace LogSync.View
{
    /// <summary>
    /// Interaction logic for SearchView.xaml
    /// </summary>
    public partial class SearchView : Window
    {
        SyncedViewModel logSync;
        private string focusedLog;

        public SearchView(SyncedViewModel _logSync)
        {
            logSync = _logSync;
            //focusedLog = logSync.GetFocusedLog();
            InitializeComponent();
        }

        public void SetLog()
        {
            focusedLog = logSync.GetFocusedLog();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //var startPos = lastSearchResultIndex == -1 ? 0 : lastSearchResultIndex;
            if (focusedLog == null)
            {
                return;
            }
            logSync.Search(SearchTerm.Text, focusedLog);
            Hide();
        }
    }
}
