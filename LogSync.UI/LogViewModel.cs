using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogSync.UI
{
    public class LogViewModel
    {
        // This view's window was scrolled to a new position
        public void OnScroll(int position)
        {

        }

        // A Synchronized view's window was scrolled - scroll this view to match
        public void MatchScroll(int position)
        {

        }

        // A line in this view was selected
        public void OnSelect(DateTime timestamp, int chunkLineIndex = 0)
        {

        }

        // A line in a Synchronized view was selected - select same line in this view
        public void MatchSelect(DateTime timestamp, int chunkLineIndex = 0)
        {

        }
    }
}
