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
            this.tactButton = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.pictureBox1.Location = new System.Drawing.Point(12, 15);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(64, 64);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // pNameLabel
            // 
            this.pNameLabel.AutoSize = true;
            this.pNameLabel.Font = new System.Drawing.Font("MS UI Gothic", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.pNameLabel.Location = new System.Drawing.Point(95, 19);
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
            this.judgeTextBox.Location = new System.Drawing.Point(98, 50);
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
            this.listView1.BackColor = System.Drawing.Color.White;
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.nameColumn,
            this.kosColumn});
            this.listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listView1.Location = new System.Drawing.Point(12, 198);
            this.listView1.Name = "listView1";
            this.listView1.Scrollable = false;
            this.listView1.Size = new System.Drawing.Size(228, 127);
            this.listView1.TabIndex = 3;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
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
            this.detailTextBox.Location = new System.Drawing.Point(12, 97);
            this.detailTextBox.Multiline = true;
            this.detailTextBox.Name = "detailTextBox";
            this.detailTextBox.ReadOnly = true;
            this.detailTextBox.Size = new System.Drawing.Size(228, 95);
            this.detailTextBox.TabIndex = 4;
            // 
            // tactButton
            // 
            this.tactButton.Enabled = false;
            this.tactButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.tactButton.Location = new System.Drawing.Point(12, 331);
            this.tactButton.Name = "tactButton";
            this.tactButton.Size = new System.Drawing.Size(228, 23);
            this.tactButton.TabIndex = 5;
            this.tactButton.Text = "Start Tactical Analysis";
            this.tactButton.UseVisualStyleBackColor = true;
            this.tactButton.Click += new System.EventHandler(this.tactButton_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.button1);
            this.panel1.Enabled = false;
            this.panel1.Location = new System.Drawing.Point(246, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(306, 351);
            this.panel1.TabIndex = 6;
            // 
            // button1
            // 
            this.button1.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.button1.FlatAppearance.BorderSize = 0;
            this.button1.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.button1.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.ForeColor = System.Drawing.Color.Black;
            this.button1.Image = ((System.Drawing.Image)(resources.GetObject("button1.Image")));
            this.button1.Location = new System.Drawing.Point(286, 2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(18, 14);
            this.button1.TabIndex = 7;
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // AnalyzeResult
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(565, 360);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.tactButton);
            this.Controls.Add(this.detailTextBox);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.judgeTextBox);
            this.Controls.Add(this.pNameLabel);
            this.Controls.Add(this.pictureBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "AnalyzeResult";
            this.Text = "AnalyzeResult";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AnalyzeResult_FormClosing);
            this.Shown += new System.EventHandler(this.AnalyzeResult_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panel1.ResumeLayout(false);
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
        private System.Windows.Forms.Button tactButton;
        internal System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button button1;
    }
}