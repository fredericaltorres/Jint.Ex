namespace Jint.Ex.WinForm
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.butSynchronousExecution = new System.Windows.Forms.Button();
            this.lbOut = new System.Windows.Forms.ListBox();
            this.butASynchronousExecution = new System.Windows.Forms.Button();
            this.butTimer = new System.Windows.Forms.Button();
            this.butClearListBox = new System.Windows.Forms.Button();
            this.butClearEventQueue = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.butMultipleTimer = new System.Windows.Forms.Button();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // butSynchronousExecution
            // 
            this.butSynchronousExecution.Location = new System.Drawing.Point(3, 418);
            this.butSynchronousExecution.Name = "butSynchronousExecution";
            this.butSynchronousExecution.Size = new System.Drawing.Size(348, 23);
            this.butSynchronousExecution.TabIndex = 0;
            this.butSynchronousExecution.Text = "Synchronous Execution";
            this.butSynchronousExecution.UseVisualStyleBackColor = true;
            this.butSynchronousExecution.Click += new System.EventHandler(this.butSynchronousExecution_Click);
            // 
            // lbOut
            // 
            this.lbOut.FormattingEnabled = true;
            this.lbOut.Location = new System.Drawing.Point(0, 32);
            this.lbOut.Name = "lbOut";
            this.lbOut.Size = new System.Drawing.Size(348, 381);
            this.lbOut.TabIndex = 1;
            // 
            // butASynchronousExecution
            // 
            this.butASynchronousExecution.Location = new System.Drawing.Point(3, 447);
            this.butASynchronousExecution.Name = "butASynchronousExecution";
            this.butASynchronousExecution.Size = new System.Drawing.Size(348, 23);
            this.butASynchronousExecution.TabIndex = 2;
            this.butASynchronousExecution.Text = "Asynchronous Execution";
            this.butASynchronousExecution.UseVisualStyleBackColor = true;
            this.butASynchronousExecution.Click += new System.EventHandler(this.butASynchronousExecution_Click);
            // 
            // butTimer
            // 
            this.butTimer.Location = new System.Drawing.Point(3, 476);
            this.butTimer.Name = "butTimer";
            this.butTimer.Size = new System.Drawing.Size(348, 23);
            this.butTimer.TabIndex = 3;
            this.butTimer.Text = "Timer";
            this.butTimer.UseVisualStyleBackColor = true;
            this.butTimer.Click += new System.EventHandler(this.butTimer_Click);
            // 
            // butClearListBox
            // 
            this.butClearListBox.Location = new System.Drawing.Point(3, 541);
            this.butClearListBox.Name = "butClearListBox";
            this.butClearListBox.Size = new System.Drawing.Size(348, 23);
            this.butClearListBox.TabIndex = 4;
            this.butClearListBox.Text = "Clear Listbox";
            this.butClearListBox.UseVisualStyleBackColor = true;
            this.butClearListBox.Click += new System.EventHandler(this.butClearListBox_Click);
            // 
            // butClearEventQueue
            // 
            this.butClearEventQueue.Location = new System.Drawing.Point(3, 570);
            this.butClearEventQueue.Name = "butClearEventQueue";
            this.butClearEventQueue.Size = new System.Drawing.Size(348, 23);
            this.butClearEventQueue.TabIndex = 5;
            this.butClearEventQueue.Text = "Clear Event Queue";
            this.butClearEventQueue.UseVisualStyleBackColor = true;
            this.butClearEventQueue.Click += new System.EventHandler(this.butClearEventQueue_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(350, 27);
            this.menuStrip1.TabIndex = 6;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.quitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(45, 23);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // quitToolStripMenuItem
            // 
            this.quitToolStripMenuItem.Name = "quitToolStripMenuItem";
            this.quitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Q)));
            this.quitToolStripMenuItem.Size = new System.Drawing.Size(166, 24);
            this.quitToolStripMenuItem.Text = "Quit";
            this.quitToolStripMenuItem.Click += new System.EventHandler(this.quitToolStripMenuItem_Click);
            // 
            // butMultipleTimer
            // 
            this.butMultipleTimer.Location = new System.Drawing.Point(3, 505);
            this.butMultipleTimer.Name = "butMultipleTimer";
            this.butMultipleTimer.Size = new System.Drawing.Size(348, 23);
            this.butMultipleTimer.TabIndex = 7;
            this.butMultipleTimer.Text = "Multiple Timers";
            this.butMultipleTimer.UseVisualStyleBackColor = true;
            this.butMultipleTimer.Click += new System.EventHandler(this.butMultipleTimer_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(350, 606);
            this.Controls.Add(this.butMultipleTimer);
            this.Controls.Add(this.butClearEventQueue);
            this.Controls.Add(this.butClearListBox);
            this.Controls.Add(this.butTimer);
            this.Controls.Add(this.butASynchronousExecution);
            this.Controls.Add(this.lbOut);
            this.Controls.Add(this.butSynchronousExecution);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Jint.Ex WinForm Demo";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button butSynchronousExecution;
        private System.Windows.Forms.ListBox lbOut;
        private System.Windows.Forms.Button butASynchronousExecution;
        private System.Windows.Forms.Button butTimer;
        private System.Windows.Forms.Button butClearListBox;
        private System.Windows.Forms.Button butClearEventQueue;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem;
        private System.Windows.Forms.Button butMultipleTimer;
    }
}

