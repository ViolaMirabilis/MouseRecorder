using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;


namespace MouseRecorder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // where the mouse recordings are saved
        const string filePath = @"C:\Users\zajac\Desktop\C# Projects\MouseRecorder\MouseRecorder\Temporary\coordinates.txt";
        const string savedRecordingsPath = @"C:\Users\zajac\Desktop\C# Projects\MouseRecorder\MouseRecorder\SavedRecordings";

        private Enums.ApplicationState currentState = Enums.ApplicationState.Idle;

        private CustomTimer recordingTimer;     // records mouse movement
        private CustomTimer playbackTimer;      // playus mouse movement

        #region ComboBox
        private List<string> recordedMovement = new List<string>();      // temporarily holds position and action before writing to a file
        private List<string> playbackMovement = new List<string>();     // holds the coordinates read from a text file
        private int playbackLineIndex = 0;
        public ObservableCollection<string> ComboBoxEntries { get; } = new ObservableCollection<string>();      // populates the combobox list
        public string CurrentlySelectedRecording { get; set; } = string.Empty;
        #endregion

        #region DLL Import Mouse Information Region
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        static extern void mouse_event (uint dwFlags, uint x, uint y, uint dwData, IntPtr dwExtraInfo);     // performs mouse clicks

        [DllImport("user32.dll")]       // @see https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getasynckeystate
        static extern short GetAsyncKeyState(int vKey);      // checks what button is currently pressed. Returns short to check whether the button is pressed or not

        

        #endregion

        #region hotkeys
        // keys variables
        const int STARTRECORDING = 0x74;     // F5 to start/stop recording
        const int STOPREPLAY = 0x75;        // F6 to start replaying the "recording"

        // Needed to send mouse events with mouse_event
        const uint LEFTDOWN = 0x02;     // left mouse button down
        const uint LEFTUP = 0x04;       // right mouse button up
        const uint RIGHTDOWN = 0x0008;
        const uint RIGHTUP = 0x0010;

        const int VK_LBUTTON = 0x01;        // left mouse button
        const int VK_RBUTTON = 0x02;        // right mouse button

        // Needed to detect what key has been pressed

        bool mouseRecording = false;
        bool mouseReplaying = false;

        #endregion

        public struct POINT     // holds X, Y of the mouse
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            FillComboBoxEntriesOnInit();

            recordingTimer = new CustomTimer(8, WritePoint);
            //playbackTimer = new CustomTimer(8, ReadPoint);
            
        }

        private void FillComboBoxEntriesOnInit()
        {
            string[] fileEntries = Directory.GetFiles(savedRecordingsPath);
            foreach (var fileName in fileEntries)
            {
                ComboBoxEntries.Add(Path.GetFileNameWithoutExtension(fileName));
            }   
        }

        private void ProcessMouseRecordingFile(string currentlySelectedRecording)
        {
            string file = $@"{savedRecordingsPath}\{CurrentlySelectedRecording}.txt";

            playbackMovement = File.ReadAllLines(file).ToList();        // converts the .txt into a list
            playbackLineIndex = 0;
        }

        // writing coordinates to the screen

        private async Task TestWritePointAsync()
        {
            POINT p;
            while (true)
            {
                if (GetCursorPos(out p))
                {
                    bool leftDown = (GetAsyncKeyState(VK_LBUTTON) & 0x8000) != 0;
                    bool rightDown = (GetAsyncKeyState(VK_RBUTTON) & 0x8000) != 0;

                    string currentAction = "NULL";

                    if (leftDown) currentAction = "LClick";
                    if (rightDown) currentAction = "RClick";

                    string mouseCoordinates = $"{p.X.ToString()} {p.Y.ToString()} {currentAction}";
                    recordedMovement.Add(mouseCoordinates);
                        
                    if ((GetAsyncKeyState(STOPREPLAY) & 0x8000) != 0)       // F6 to stop
                    {
                        break;
                    }

                    await Task.Delay(8);
                }
            }
            
        }

        private void WritePoint()       // EventArgs instead of RoutedEventArgs. Idk why, but it works.
        {
            POINT p;        // a field of POINT type
            if (GetCursorPos(out p))
            {
                //Xpos.Text = p.X.ToString();
                //Ypos.Text = p.Y.ToString();

                bool leftDown = (GetAsyncKeyState(VK_LBUTTON) & 0x8000) != 0;       // checks if left is down
                bool rightDown = (GetAsyncKeyState(VK_RBUTTON) & 0x8000) != 0;       // checks if right is down

                string currentAction = "NULL";

                if (leftDown) currentAction = "LClick";
                if (rightDown) currentAction = "RClick";

                //string mouseCoordinates = $"{p.X.ToString()}, {p.Y.ToString()}, {DateTime.Now}, {currentAction}";      // date not needed ig
                string mouseCoordinates = $"{p.X.ToString()} {p.Y.ToString()} {currentAction}";
                recordedMovement.Add(mouseCoordinates);     // adding to a temporary list
                //File.AppendAllText(filePath, coords + Environment.NewLine);
                
            }
        }

        private async Task ReadPointAsync()        // maybe it should be async so it can both read and move
        {
            mouseReplaying = true;
            int x, y;       // mouse coordinates

            while (mouseReplaying && playbackLineIndex < playbackMovement.Count)
            {
                string file = playbackMovement[playbackLineIndex].ToString();       // gets an index of a line in the list
                playbackLineIndex++;

                string[] splitLine = file.Split(' ');      // Splits by space
                x = Convert.ToInt32(splitLine[0]);      // X coord
                y = Convert.ToInt32(splitLine[1]);      // Y coord

                SetCursorPos(x, y);

                string action = splitLine[2];
                if (action == "LClick") LeftMouseClick();
                if (action == "RClick") RightMouseClick();

                // STOPS the playback on F6
                if ((GetAsyncKeyState(STOPREPLAY) & 0x8000) != 0)
                {
                    playbackTimer.Stop();
                }

                await Task.Delay(8);        // 8ms
            } 
        }

        private void LeftMouseClick()
        {
            mouse_event(LEFTDOWN, 0, 0, 0, IntPtr.Zero);

            mouse_event(LEFTUP, 0, 0, 0, IntPtr.Zero);
        }

        private void RightMouseClick()
        {
            mouse_event(RIGHTDOWN, 0, 0, 0, IntPtr.Zero);

            mouse_event(RIGHTUP, 0, 0, 0, IntPtr.Zero);
        }

        private string SetCurrentStateLabel(Enums.ApplicationState currentState)
        {
            return currentState switch
            {
                Enums.ApplicationState.Idle => "Idle",
                Enums.ApplicationState.Recording => "Recording...",
                Enums.ApplicationState.Playing => "Playing...",
                Enums.ApplicationState.Paused => "Paused"
            };
        }

        #region Buttons Logic
        private async void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessMouseRecordingFile(CurrentlySelectedRecording);

            if (currentState == Enums.ApplicationState.Idle || currentState == Enums.ApplicationState.Paused || currentState == Enums.ApplicationState.Playing)
            {

                if (ComboBoxEntries.Count <= 0) return;     // If list is empty, returns

                mouseRecording = !mouseRecording;     // quick toggle
                await ReadPointAsync();

                if (mouseRecording == false)
                {
                    currentState = Enums.ApplicationState.Paused;
                    PlayPauseButtonImage.Source = new BitmapImage(new Uri(@"/Assets/PlayButton.png", UriKind.Relative));
                }
                else
                {
                    currentState = Enums.ApplicationState.Playing;
                    PlayPauseButtonImage.Source = new BitmapImage(new Uri(@"/Assets/PauseButton.png", UriKind.Relative));
                }

                lblStatus.Content = SetCurrentStateLabel(currentState).ToString();
            }
        }
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            // Cannot be clicked if idle
            if (currentState == Enums.ApplicationState.Idle) return;        
            
            currentState = Enums.ApplicationState.Idle;
            lblStatus.Content = SetCurrentStateLabel(currentState).ToString();
            mouseRecording = false;
            recordingTimer.Stop();
            //playbackTimer.Stop();
            RecordButton.IsEnabled = true;

            var popup = new SaveRecordingWindow();
            if (popup.ShowDialog() == true)
            {
                string filename = popup.RecordingName;     // gets name from the popup textbox. It's using the SaveRecordingWindow property
                string newMouseRecording = @"C:\Users\zajac\Desktop\C# Projects\MouseRecorder\MouseRecorder\SavedRecordings\" + filename + ".txt";

                File.WriteAllLines(newMouseRecording, recordedMovement);        // writes recorded Movement to the savedFilePath
                ComboBoxEntries.Add(filename);
            }
        }

        private async void RecordButton_Click(object sender, RoutedEventArgs e)
        {
            // Can start recording ONLY when idle
            if (currentState == Enums.ApplicationState.Idle)
            {
                //recordingTimer.Start();

                RecordButton.IsEnabled = false;
                currentState = Enums.ApplicationState.Recording;
                lblStatus.Content = SetCurrentStateLabel(currentState).ToString();

                await TestWritePointAsync();
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // open settings window
            // play once
            //
        }
        #endregion
    }
}