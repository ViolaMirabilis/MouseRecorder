using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MouseRecorder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // where the mouse recordings are saved
        const string filePath = @"C:\Users\zajac\Desktop\C# Projects\MouseRecorder\MouseRecorder\Temporary\coordinates.txt";

        private Enums.ApplicationState currentState = Enums.ApplicationState.Idle;

        private CustomTimer recordingTimer;     // records mouse movement
        private CustomTimer playbackTimer;      // playus mouse movement

        #region Mouse information region
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        static extern void mouse_event (uint dwFlags, uint x, uint y, uint dwData, IntPtr dwExtraInfo);     // performs mouse clicks

        [DllImport("user32.dll")]       // @see https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getasynckeystate
        static extern short GetAsyncKeyState(int vKey);      // checks what button is currently pressed. Returns short to check whether the button is pressed or not

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

            recordingTimer = new CustomTimer(8, WritePoint);
            playbackTimer = new CustomTimer(8, ReadPoint);
            
        }

        // writing coordinates to the screen
        private void WritePoint()       // EventArgs instead of RoutedEventArgs. Idk why, but it works.
        {
            POINT p;        // a field of POINT type
            if (GetCursorPos(out p))
            {
                Xpos.Text = p.X.ToString();
                Ypos.Text = p.Y.ToString();

                bool leftDown = (GetAsyncKeyState(VK_LBUTTON) & 0x8000) != 0;       // checks if left is down
                bool rightDown = (GetAsyncKeyState(VK_RBUTTON) & 0x8000) != 0;       // checks if right is down

                string currentAction = "";

                if (leftDown) currentAction += "Left click";
                if (rightDown) currentAction += "Right click";

                string coords = $"{p.X.ToString()}, {p.Y.ToString()}, {DateTime.Now}, {currentAction}";      // date not needed ig
                File.AppendAllText(filePath, coords + Environment.NewLine);
                
            }
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

        // buttons logic

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentState == Enums.ApplicationState.Idle || currentState == Enums.ApplicationState.Paused)
            {
                mouseRecording = !mouseRecording;     // quick toggle

                if (mouseRecording == false)
                {
                    PlayPauseButtonImage.Source = new BitmapImage(new Uri(@"/Assets/PlayButton.png", UriKind.Relative));
                }
                else
                {
                    PlayPauseButtonImage.Source = new BitmapImage(new Uri(@"/Assets/PauseButton.png", UriKind.Relative));
                }
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            // can be clicked whenever
            recordingTimer.Stop();
            playbackTimer.Stop();
        }

        private void RecordButton_Click(object sender, RoutedEventArgs e)
        {
            // can start recording ONLY when idle
            if (currentState == Enums.ApplicationState.Idle)
            {
                // record
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // open settings window
        }
    }
}