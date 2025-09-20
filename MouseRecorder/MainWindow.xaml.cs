using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;
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

        private List<string> recordedMovement = new List<string>();      // temporarily holds position and action before writing to a file
        private ObservableCollection<string> comboBoxEntries = new ObservableCollection<string>();      // populates the combobox list
        private string currentlySelectedRecording = "";

        #region DLL Import Mouse Information Region
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        static extern void mouse_event (uint dwFlags, uint x, uint y, uint dwData, IntPtr dwExtraInfo);     // performs mouse clicks

        [DllImport("user32.dll")]       // @see https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getasynckeystate
        static extern short GetAsyncKeyState(int vKey);      // checks what button is currently pressed. Returns short to check whether the button is pressed or not

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        #endregion

        #region hotkeys
        // keys variables
        const uint START_RECORDING = 0x74;     // F5 to start/stop recording
        const uint START_REPLAY = 0x75;        // F6 to start replaying the "recording"

        const int VK_LBUTTON = 0x01;     // left mouse button
        const int VK_RBUTTON = 0x02;       // right mouse button

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

            FillComboBoxEntriesOnInit();

            recordingTimer = new CustomTimer(8, WritePoint);
            playbackTimer = new CustomTimer(8, ReadPoint);
            
        }

        // writing coordinates to the screen
        private void WritePoint()       // EventArgs instead of RoutedEventArgs. Idk why, but it works.
        {
            POINT p;        // a field of POINT type
            if (GetCursorPos(out p))
            {
                //Xpos.Text = p.X.ToString();
                //Ypos.Text = p.Y.ToString();

                bool leftDown = (GetAsyncKeyState(VK_LBUTTON) & 0x8000) != 0;       // checks if left is down
                bool rightDown = (GetAsyncKeyState(VK_RBUTTON) & 0x8000) != 0;       // checks if right is down

                string currentAction = "";

                if (leftDown) currentAction += "Left click";
                if (rightDown) currentAction += "Right click";

                string mouseCoordinates = $"{p.X.ToString()}, {p.Y.ToString()}, {DateTime.Now}, {currentAction}";      // date not needed ig
                recordedMovement.Add(mouseCoordinates);     // adding to a temporary list
                //File.AppendAllText(filePath, coords + Environment.NewLine);
                
            }
        }

        private void FillComboBoxEntriesOnInit()
        {
            string[] fileEntries = Directory.GetFiles(savedRecordingsPath);
            foreach (var fileName in fileEntries)
            {
                comboBoxEntries.Add(Path.GetFileNameWithoutExtension(fileName));
                //comboBoxEntries.Add(fileName);
            }

            ComboBox.ItemsSource = comboBoxEntries;
        }

        private void ReadPoint()
        {
            // TO DO
        }

        // mouse click (up and down)
        private void LeftMouseClick()
        {
            mouse_event(VK_LBUTTON, 0, 0, 0, IntPtr.Zero);

            //mouse_event(LEFTUP, 0, 0, 0, IntPtr.Zero);
        }

        // to do
        private void RightMouseClick()
        {
            mouse_event(VK_RBUTTON, 0, 0, 0, IntPtr.Zero);

            //mouse_event(LEFTUP, 0, 0, 0, IntPtr.Zero);
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

        // Buttons Logic
        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {

            if (currentState == Enums.ApplicationState.Idle || currentState == Enums.ApplicationState.Paused || currentState == Enums.ApplicationState.Playing)
            {

                if (comboBoxEntries.Count <= 0)
                {
                    return;
                }
                currentlySelectedRecording = ComboBox.Items[0].ToString();
                mouseRecording = !mouseRecording;     // quick toggle
                playbackTimer.Start();      // starts the timer

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

            // Can be clicked whenever, no matter the status
            currentState = Enums.ApplicationState.Idle;
            lblStatus.Content = SetCurrentStateLabel(currentState).ToString();
            recordingTimer.Stop();
            playbackTimer.Stop();
            RecordButton.IsEnabled = true;

            var popup = new SaveRecordingWindow();
            if (popup.ShowDialog() == true)
            {
                string filename = popup.RecordingName;     // gets name from the popup textbox. It's using the SaveRecordingWindow property
                string newMouseRecording = @"C:\Users\zajac\Desktop\C# Projects\MouseRecorder\MouseRecorder\SavedRecordings\" + filename + ".txt";

                File.WriteAllLines(newMouseRecording, recordedMovement);        // writes recorded Movement to the savedFilePath
                comboBoxEntries.Add(filename);
            }
        }

        private void RecordButton_Click(object sender, RoutedEventArgs e)
        {
            currentState = Enums.ApplicationState.Recording;
            lblStatus.Content = SetCurrentStateLabel(currentState).ToString();
            // can start recording ONLY when idle
            if (currentState == Enums.ApplicationState.Idle)
            {
                recordingTimer.Start();
                RecordButton.IsEnabled = false;
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // open settings window
            // play once
            //
        }
    }
}