
namespace KQC
{
	partial class MainForm
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.NotifyIcon notifyIcon1;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
		    this.components = new System.ComponentModel.Container();
		    System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
		    this.button1 = new System.Windows.Forms.Button();
		    this.label1 = new System.Windows.Forms.Label();
		    this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
		    this.SuspendLayout();
		    // 
		    // button1
		    // 
		    this.button1.FlatAppearance.BorderSize = 0;
		    this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		    this.button1.Font = new System.Drawing.Font("MS UI Gothic", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
		    this.button1.ForeColor = System.Drawing.Color.Black;
		    this.button1.Location = new System.Drawing.Point(1, 0);
		    this.button1.Name = "button1";
		    this.button1.Size = new System.Drawing.Size(284, 90);
		    this.button1.TabIndex = 0;
		    this.button1.Text = "Sample PlayerName";
		    this.button1.UseVisualStyleBackColor = true;
		    this.button1.Click += new System.EventHandler(this.Button1Click);
		    // 
		    // label1
		    // 
		    this.label1.Location = new System.Drawing.Point(49, 93);
		    this.label1.Name = "label1";
		    this.label1.Size = new System.Drawing.Size(188, 11);
		    this.label1.TabIndex = 1;
		    this.label1.Text = "↑Ctrl-C the name and then click!";
		    // 
		    // notifyIcon1
		    // 
		    this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
		    this.notifyIcon1.Text = "KQC: Click to check \'Sample PlayerName\'";
		    this.notifyIcon1.Visible = true;
		    this.notifyIcon1.Click += new System.EventHandler(this.Button1Click);
		    // 
		    // MainForm
		    // 
		    this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
		    this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		    this.ClientSize = new System.Drawing.Size(284, 113);
		    this.Controls.Add(this.label1);
		    this.Controls.Add(this.button1);
		    this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		    this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
		    this.MaximizeBox = false;
		    this.Name = "MainForm";
		    this.Text = "KOS Quick Checker";
		    this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainFormFormClosing);
		    this.ResumeLayout(false);

		}
	}
}
