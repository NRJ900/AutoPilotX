using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace AutoPilotX.Utils
{
    public static class ControlExtensions
    {
        public static void InvokeIfRequired(this ISynchronizeInvoke control, Action action)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(action, null);
            }
            else
            {
                action();
            }
        }
    }
}
