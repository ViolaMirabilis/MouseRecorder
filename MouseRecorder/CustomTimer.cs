using System.Windows.Controls;
using System.Windows.Threading;

namespace MouseRecorder;

public class CustomTimer
{
    private DispatcherTimer timer;
    private Action tickAction;      // what happens every tick

    public bool IsRunning => timer.IsEnabled;

    public CustomTimer(int msInterval, Action tickAction)
    {
        timer = new DispatcherTimer();
        timer.Interval = TimeSpan.FromMilliseconds(msInterval);
        this.tickAction = tickAction;
        timer.Tick += (s, e) => tickAction();     // attaching event handler using lambda expression

        #region future reference to the event handler expression
        // WITHOUT LAMBDA

        /*
        button.Click += new EventHandler(delegate (Object s, EventArgs e) {
            //some code
        })
        */

        // WITH LAMBDA

        /*
         button.Click += (s,e) => {
            //some code
        };

        // OR

        //button.Click += (o,r) => {};
         
         
         */
        #endregion
    }

    public void Start() => timer.Start();
    public void Stop() => timer.Stop();
}
