using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace Jint.Ex.WinForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            AsyncronousEngine.EmbedScriptAssemblies.Add(Assembly.GetExecutingAssembly());
            // Expose to the JavaScript the function setUserMessage which add a string 
            // in the Listbox
            AsyncronousEngine.Engine.SetValue("setUserMessage", new Action<string>(__setUserMessage__));
        }

        private void __setUserMessage__(string s)
        {
            try
            {
                // When the method is called by the JavaScript engine it will be called from
                // different thread
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action<string>(__setUserMessage__), s);
                }
                else
                {
                    this.lbOut.Items.Add(s);
                    this.lbOut.SelectedIndex = this.lbOut.Items.Count - 1;
                }
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                Debugger.Break();
            }
        }

        private void butSynchronousExecution_Click(object sender, EventArgs e)
        {
            AsyncronousEngine.RequestFileExecution("SynchronousExecution.js");
        }

        private void butASynchronousExecution_Click(object sender, EventArgs e)
        {
            AsyncronousEngine.RequestFileExecution("AsynchronousExecution.js");
        }

        private void butTimer_Click(object sender, EventArgs e)
        {
            AsyncronousEngine.RequestFileExecution("Timer.js");
        }

        private void butMultipleTimer_Click(object sender, EventArgs e)
        {
            AsyncronousEngine.RequestFileExecution("MultipleTimers.js");
        }

        private void butClearListBox_Click(object sender, EventArgs e)
        {
            this.lbOut.Items.Clear();
        }

        private void butClearEventQueue_Click(object sender, EventArgs e)
        {
            butClearListBox_Click(sender, e);
            AsyncronousEngine.RequestClearQueue();
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // How to correctly request stopping the AsyncronousEngine event loop
            AsyncronousEngine.Stop(() =>
            {
                Thread.Sleep(100);
                Application.DoEvents();
            });
        }
    }
}
