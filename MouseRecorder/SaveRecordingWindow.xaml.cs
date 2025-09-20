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

namespace MouseRecorder
{
    /// <summary>
    /// Interaction logic for SaveRecordingWindow.xaml
    /// </summary>
    public partial class SaveRecordingWindow : Window
    {
        public string RecordingName { get; private set; }
        public SaveRecordingWindow()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            RecordingName = tbRecordingName.Text.Trim();        // To avoid whitespaces, etc.

            if (!string.IsNullOrWhiteSpace(RecordingName))
            {
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Name cannot be empty!");
            }
        }
    }
}
