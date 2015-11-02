namespace OASIS_Room_Editor
{
    partial class DialogSizeHires
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
            this.label1 = new System.Windows.Forms.Label();
            this.OK = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.maskedTextBoxWidth = new System.Windows.Forms.MaskedTextBox();
            this.maskedTextBoxHeight = new System.Windows.Forms.MaskedTextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(256, 26);
            this.label1.TabIndex = 0;
            this.label1.Text = "Oric HIRES data files do not contain size information.\r\nPlease enter the size (in" +
    " pixels) below:";
            // 
            // OK
            // 
            this.OK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OK.Location = new System.Drawing.Point(100, 127);
            this.OK.Name = "OK";
            this.OK.Size = new System.Drawing.Size(75, 23);
            this.OK.TabIndex = 1;
            this.OK.Text = "OK";
            this.OK.UseVisualStyleBackColor = true;
            this.OK.Click += new System.EventHandler(this.OK_Click);
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(181, 127);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 23);
            this.Cancel.TabIndex = 2;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(44, 69);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Width";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(137, 69);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Height";
            // 
            // maskedTextBoxWidth
            // 
            this.maskedTextBoxWidth.Location = new System.Drawing.Point(85, 66);
            this.maskedTextBoxWidth.Mask = "999";
            this.maskedTextBoxWidth.Name = "maskedTextBoxWidth";
            this.maskedTextBoxWidth.Size = new System.Drawing.Size(36, 20);
            this.maskedTextBoxWidth.TabIndex = 7;
            // 
            // maskedTextBoxHeight
            // 
            this.maskedTextBoxHeight.Location = new System.Drawing.Point(181, 66);
            this.maskedTextBoxHeight.Mask = "999";
            this.maskedTextBoxHeight.Name = "maskedTextBoxHeight";
            this.maskedTextBoxHeight.Size = new System.Drawing.Size(36, 20);
            this.maskedTextBoxHeight.TabIndex = 8;
            // 
            // DialogSizeHires
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(276, 159);
            this.Controls.Add(this.maskedTextBoxHeight);
            this.Controls.Add(this.maskedTextBoxWidth);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.OK);
            this.Controls.Add(this.label1);
            this.Name = "DialogSizeHires";
            this.Text = "Enter Size Information...";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button OK;
        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.MaskedTextBox maskedTextBoxWidth;
        private System.Windows.Forms.MaskedTextBox maskedTextBoxHeight;
    }
}