/*
KQC - KOS Quick Checker
Copyright (c) 2016 maybe-eve
This file is part of KQC.
KQC is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.
This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.
You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

namespace KQC
{
    partial class AnalyzeResult
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AnalyzeResult));
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pNameLabel = new System.Windows.Forms.Label();
            this.judgeTextBox = new System.Windows.Forms.TextBox();
            this.listView1 = new System.Windows.Forms.ListView();
            this.nameColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.kosColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.detailTextBox = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.pictureBox1.Location = new System.Drawing.Point(12, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(64, 64);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // pNameLabel
            // 
            this.pNameLabel.AutoSize = true;
            this.pNameLabel.Font = new System.Drawing.Font("MS UI Gothic", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.pNameLabel.Location = new System.Drawing.Point(95, 12);
            this.pNameLabel.Name = "pNameLabel";
            this.pNameLabel.Size = new System.Drawing.Size(107, 13);
            this.pNameLabel.TabIndex = 1;
            this.pNameLabel.Text = "Sample Pilotname";
            this.pNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // judgeTextBox
            // 
            this.judgeTextBox.BackColor = System.Drawing.Color.Blue;
            this.judgeTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.judgeTextBox.Cursor = System.Windows.Forms.Cursors.Default;
            this.judgeTextBox.Font = new System.Drawing.Font("Consolas", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.judgeTextBox.ForeColor = System.Drawing.Color.White;
            this.judgeTextBox.Location = new System.Drawing.Point(98, 47);
            this.judgeTextBox.Name = "judgeTextBox";
            this.judgeTextBox.ReadOnly = true;
            this.judgeTextBox.Size = new System.Drawing.Size(142, 29);
            this.judgeTextBox.TabIndex = 2;
            this.judgeTextBox.TabStop = false;
            this.judgeTextBox.Text = "CAUTION";
            this.judgeTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // listView1
            // 
            this.listView1.AutoArrange = false;
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.nameColumn,
            this.kosColumn});
            this.listView1.Enabled = false;
            this.listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listView1.Location = new System.Drawing.Point(12, 183);
            this.listView1.Name = "listView1";
            this.listView1.Scrollable = false;
            this.listView1.Size = new System.Drawing.Size(228, 127);
            this.listView1.TabIndex = 3;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.ColumnWidthChanging += new System.Windows.Forms.ColumnWidthChangingEventHandler(this.listView1_ColumnWidthChanging);
            // 
            // nameColumn
            // 
            this.nameColumn.Text = "Name (Type)";
            this.nameColumn.Width = 154;
            // 
            // kosColumn
            // 
            this.kosColumn.Text = "KOS Status";
            this.kosColumn.Width = 114;
            // 
            // detailTextBox
            // 
            this.detailTextBox.BackColor = System.Drawing.Color.White;
            this.detailTextBox.Location = new System.Drawing.Point(12, 82);
            this.detailTextBox.Multiline = true;
            this.detailTextBox.Name = "detailTextBox";
            this.detailTextBox.ReadOnly = true;
            this.detailTextBox.Size = new System.Drawing.Size(228, 95);
            this.detailTextBox.TabIndex = 4;
            // 
            // AnalyzeResult
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(252, 322);
            this.Controls.Add(this.detailTextBox);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.judgeTextBox);
            this.Controls.Add(this.pNameLabel);
            this.Controls.Add(this.pictureBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AnalyzeResult";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "AnalyzeResult";
            this.TopMost = true;
            this.Shown += new System.EventHandler(this.AnalyzeResult_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label pNameLabel;
        private System.Windows.Forms.TextBox judgeTextBox;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader nameColumn;
        private System.Windows.Forms.ColumnHeader kosColumn;
        private System.Windows.Forms.TextBox detailTextBox;
    }
}