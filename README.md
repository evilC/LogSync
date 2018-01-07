# LogSync
A tool to view multiple logs, synchronized in time and with synced scrollbars

## About
LogSync is aimed at scenarios where you have multiple logs (Say from a client-server application) and wish to view them together 
to analyze what was going on at any given point in time.  
Whilst LogSync provides an altered view of the logs, it does not alter the original files in any way.

# How it works
Each timestamped line in each log will have a corresponding line in all log views.  
For example, if you have 5 logs, and two which contain the timestamp `2018-06-01 01:22:33.444`, 
then those two log views will contain the text from their logs for that timestamp, and all other 3 logs will contain a blank line at that point.  

Using this technique, all log views are of *equal length*, therefore if displayed side by side in scrolling gui elements, the scrollbars will be identical.  
Then we just rig the code so that when one view is scrolled by the user, the other log views are scrolled to match. 

## Usage
### GUI
1. Double-click LogSync.exe
2. Click `Load Logs...`
3. Select the log(s) you wish to view.  
You can multi-select using shift, ctrl etc

### Command Line
The following command-line parameters are available
* -LoadLogs  
Loads the specified logs
Syntax: `-LoadLogs <logname> <logname> [...]`  
Example: `LogSync.exe -LoadLogs "server logs\log1.log" "client logs\log2.txt" NoSpaces\NeedsNoQuotes.log`
