namespace OASIS_Room_Editor
{
    partial class AICHelperDialog
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
            this.comboBoxAttr1 = new System.Windows.Forms.ComboBox();
            this.comboBoxAttr2 = new System.Windows.Forms.ComboBox();
            this.Cancel = new System.Windows.Forms.Button();
            this.OK = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.boxColumn = new System.Windows.Forms.MaskedTextBox();
            this.boxRowStart = new System.Windows.Forms.MaskedTextBox();
            this.boxRowEnd = new System.Windows.Forms.MaskedTextBox();
            this.checkInverse1 = new System.Windows.Forms.CheckBox();
            this.checkInverse2 = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // comboBoxAttr1
            // 
            this.comboBoxAttr1.FormattingEnabled = true;
            this.comboBoxAttr1.Items.AddRange(new object[] {
            "Black ink",
            "Red ink",
            "Green ink",
            "Yellow ink",
            "Blue ink",
            "Magenta ink",
            "Cyan ink",
            "White ink",
            "Black paper",
            "Red paper",
            "Green paper",
            "Yellow paper",
            "Blue paper",
            "Magenta paper",
            "Cyan paper",
            "White paper"});
            this.comboBoxAttr1.Location = new System.Drawing.Point(15, 50);
            this.comboBoxAttr1.Name = "comboBoxAttr1";
            this.comboBoxAttr1.Size = new System.Drawing.Size(121, 21);
            this.comboBoxAttr1.TabIndex = 0;
            // 
            // comboBoxAttr2
            // 
            this.comboBoxAttr2.FormattingEnabled = true;
            this.comboBoxAttr2.Items.AddRange(new object[] {
            "Black ink",
            "Red ink",
            "Green ink",
            "Yellow ink",
            "Blue ink",
            "Magenta ink",
            "Cyan ink",
            "White ink",
            "Black paper",
            "Red paper",
            "Green paper",
            "Yellow paper",
            "Blue paper",
            "Magenta paper",
            "Cyan paper",
            "White paper"});
            this.comboBoxAttr2.Location = new System.Drawing.Point(142, 50);
            this.comboBoxAttr2.Name = "comboBoxAttr2";
            this.comboBoxAttr2.Size = new System.Drawing.Size(121, 21);
            this.comboBoxAttr2.TabIndex = 1;
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(197, 221);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 23);
            this.Cancel.TabIndex = 8;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // OK
            // 
            this.OK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OK.Location = new System.Drawing.Point(116, 221);
            this.OK.Name = "OK";
            this.OK.Size = new System.Drawing.Size(75, 23);
            this.OK.TabIndex = 7;
            this.OK.Text = "OK";
            this.OK.UseVisualStyleBackColor = true;
            this.OK.Click += new System.EventHandler(this.OK_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(20, 126);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(98, 13);
            this.label2.TabIndex = 12;
            this.label2.Text = "In column (0 to 39):";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(160, 13);
            this.label1.TabIndex = 13;
            this.label1.Text = "Alternate the following attributes:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(20, 152);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(101, 13);
            this.label3.TabIndex = 14;
            this.label3.Text = "From row (0 to 199):";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(20, 176);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(91, 13);
            this.label4.TabIndex = 15;
            this.label4.Text = "To row (0 to 199):";
            // 
            // boxColumn
            // 
            this.boxColumn.Location = new System.Drawing.Point(141, 123);
            this.boxColumn.Mask = "99";
            this.boxColumn.Name = "boxColumn";
            this.boxColumn.Size = new System.Drawing.Size(36, 20);
            this.boxColumn.TabIndex = 4;
            // 
            // boxRowStart
            // 
            this.boxRowStart.Location = new System.Drawing.Point(141, 149);
            this.boxRowStart.Mask = "999";
            this.boxRowStart.Name = "boxRowStart";
            this.boxRowStart.Size = new System.Drawing.Size(36, 20);
            this.boxRowStart.TabIndex = 5;
            // 
            // boxRowEnd
            // 
            this.boxRowEnd.Location = new System.Drawing.Point(141, 173);
            this.boxRowEnd.Mask = "999";
            this.boxRowEnd.Name = "boxRowEnd";
            this.boxRowEnd.Size = new System.Drawing.Size(36, 20);
            this.boxRowEnd.TabIndex = 6;
            // 
            // checkInverse1
            // 
            this.checkInverse1.AutoSize = true;
            this.checkInverse1.Location = new System.Drawing.Point(23, 78);
            this.checkInverse1.Name = "checkInverse1";
            this.checkInverse1.Size = new System.Drawing.Size(81, 17);
            this.checkInverse1.TabIndex = 2;
            this.checkInverse1.Text = "Inverse flag";
            this.checkInverse1.UseVisualStyleBackColor = true;
            // 
            // checkInverse2
            // 
            this.checkInverse2.AutoSize = true;
            this.checkInverse2.Location = new System.Drawing.Point(153, 78);
            this.checkInverse2.Name = "checkInverse2";
            this.checkInverse2.Size = new System.Drawing.Size(81, 17);
            this.checkInverse2.TabIndex = 3;
            this.checkInverse2.Text = "Inverse flag";
            this.checkInverse2.UseVisualStyleBackColor = true;
            // 
            // AICHelperDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(295, 256);
            this.Controls.Add(this.checkInverse2);
            this.Controls.Add(this.checkInverse1);
            this.Controls.Add(this.boxRowEnd);
            this.Controls.Add(this.boxRowStart);
            this.Controls.Add(this.boxColumn);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.OK);
            this.Controls.Add(this.comboBoxAttr2);
            this.Controls.Add(this.comboBoxAttr1);
            this.Name = "AICHelperDialog";
            this.Text = "AIC helper";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxAttr1;
        private System.Windows.Forms.ComboBox comboBoxAttr2;
        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Button OK;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.MaskedTextBox boxColumn;
        private System.Windows.Forms.MaskedTextBox boxRowStart;
        private System.Windows.Forms.MaskedTextBox boxRowEnd;
        private System.Windows.Forms.CheckBox checkInverse1;
        private System.Windows.Forms.CheckBox checkInverse2;
    }
}