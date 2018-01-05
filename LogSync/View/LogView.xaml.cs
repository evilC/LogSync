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

namespace LogSync.View
{
    /// <summary>
    /// Interaction logic for LogView.xaml
    /// </summary>
    public partial class LogView : UserControl
    {
        private MainWindow mainWindow;  // A referece to the main window, so we can send events back to it
        private int id;                 // The id by which the main window knows this view

        public LogView(MainWindow mw, int _id)
        {
            InitializeComponent();
            mainWindow = mw;
            id = _id;
        }

        #region Responding to events from this View
        /// <summary>
        /// Called when this view is scrolled by the user.
        /// Notifies the main window, so that it can scroll other views
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnScrollChange(object sender, ScrollChangedEventArgs e)
        {
            // Filter out things which do not alter the scrollbars
            if (e.VerticalChange == 0 ) { return; }

            // Notify the main window of the scroll, so it can scroll the other views to match
            mainWindow.OnScrollChanged(id, e);
        }
        #endregion

        #region Simulating events from other Views
        /// <summary>
        /// Scroll this view. Called on all other views when one of the views is scrolled
        /// </summary>
        /// <param name="e">The ScrollChangedEventArgs of the original scroll</param>
        public void DoScroll(ScrollChangedEventArgs e)
        {
            var sv = GetScrollViewer(LogListView) as ScrollViewer;
            sv.ScrollToVerticalOffset(e.VerticalOffset);
        }
        #endregion

        //https://stackoverflow.com/questions/1009036/how-can-i-programmatically-scroll-a-wpf-listview
        public static DependencyObject GetScrollViewer(DependencyObject o)
        {
            // Return the DependencyObject if it is a ScrollViewer
            if (o is ScrollViewer)
            { return o; }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(o); i++)
            {
                var child = VisualTreeHelper.GetChild(o, i);

                var result = GetScrollViewer(child);
                if (result == null)
                {
                    continue;
                }
                else
                {
                    return result;
                }
            }
            return null;
        }
    }
}
