namespace OASIS_Room_Editor
{
    partial class EditorMain
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.panel1 = new System.Windows.Forms.Panel();
            this.HiresPictureBox = new OASIS_Room_Editor.PixelBox();
            this.contextMenuAttributes = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.setPaperToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.blackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.redToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.greenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.yellowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.blueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.magentaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cyanToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.whiteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setInkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.blackToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.redToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.greenToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.yellowToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.blueToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.magentaToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.cyanToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.whiteToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.removeAttributeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toggleInverseFlagToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.flipAllBitsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.HiresPictureBox)).BeginInit();
            this.contextMenuAttributes.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(783, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(37, 20);
            this.toolStripMenuItem1.Text = "&File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(92, 22);
            this.exitToolStripMenuItem.Text = "&Exit";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 470);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(783, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Controls.Add(this.HiresPictureBox);
            this.panel1.Location = new System.Drawing.Point(96, 44);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(554, 294);
            this.panel1.TabIndex = 3;
            // 
            // HiresPictureBox
            // 
            this.HiresPictureBox.BackColor = System.Drawing.SystemColors.Control;
            this.HiresPictureBox.ErrorImage = null;
            this.HiresPictureBox.InitialImage = null;
            this.HiresPictureBox.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            this.HiresPictureBox.Location = new System.Drawing.Point(0, 0);
            this.HiresPictureBox.Margin = new System.Windows.Forms.Padding(0);
            this.HiresPictureBox.Name = "HiresPictureBox";
            this.HiresPictureBox.Size = new System.Drawing.Size(1536, 272);
            this.HiresPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.HiresPictureBox.TabIndex = 2;
            this.HiresPictureBox.TabStop = false;
            this.HiresPictureBox.Click += new System.EventHandler(this.HiresPictureBox_Click);
            this.HiresPictureBox.Paint += new System.Windows.Forms.PaintEventHandler(this.HiresPictureBox_Paint);
            this.HiresPictureBox.MouseHover += new System.EventHandler(this.HiresPictureBox_MouseHover);
            // 
            // contextMenuAttributes
            // 
            this.contextMenuAttributes.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.setPaperToolStripMenuItem,
            this.setInkToolStripMenuItem,
            this.toolStripSeparator1,
            this.removeAttributeToolStripMenuItem,
            this.toggleInverseFlagToolStripMenuItem,
            this.flipAllBitsToolStripMenuItem});
            this.contextMenuAttributes.Name = "contextMenuAttributes";
            this.contextMenuAttributes.Size = new System.Drawing.Size(176, 142);
            // 
            // setPaperToolStripMenuItem
            // 
            this.setPaperToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.blackToolStripMenuItem,
            this.redToolStripMenuItem,
            this.greenToolStripMenuItem,
            this.yellowToolStripMenuItem,
            this.blueToolStripMenuItem,
            this.magentaToolStripMenuItem,
            this.cyanToolStripMenuItem,
            this.whiteToolStripMenuItem});
            this.setPaperToolStripMenuItem.Name = "setPaperToolStripMenuItem";
            this.setPaperToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.setPaperToolStripMenuItem.Text = "Set Paper";
            // 
            // blackToolStripMenuItem
            // 
            this.blackToolStripMenuItem.Name = "blackToolStripMenuItem";
            this.blackToolStripMenuItem.Size = new System.Drawing.Size(121, 22);
            this.blackToolStripMenuItem.Text = "Black";
            // 
            // redToolStripMenuItem
            // 
            this.redToolStripMenuItem.Name = "redToolStripMenuItem";
            this.redToolStripMenuItem.Size = new System.Drawing.Size(121, 22);
            this.redToolStripMenuItem.Text = "Red";
            // 
            // greenToolStripMenuItem
            // 
            this.greenToolStripMenuItem.Name = "greenToolStripMenuItem";
            this.greenToolStripMenuItem.Size = new System.Drawing.Size(121, 22);
            this.greenToolStripMenuItem.Text = "Green";
            // 
            // yellowToolStripMenuItem
            // 
            this.yellowToolStripMenuItem.Name = "yellowToolStripMenuItem";
            this.yellowToolStripMenuItem.Size = new System.Drawing.Size(121, 22);
            this.yellowToolStripMenuItem.Text = "Yellow";
            // 
            // blueToolStripMenuItem
            // 
            this.blueToolStripMenuItem.Name = "blueToolStripMenuItem";
            this.blueToolStripMenuItem.Size = new System.Drawing.Size(121, 22);
            this.blueToolStripMenuItem.Text = "Blue";
            // 
            // magentaToolStripMenuItem
            // 
            this.magentaToolStripMenuItem.Name = "magentaToolStripMenuItem";
            this.magentaToolStripMenuItem.Size = new System.Drawing.Size(121, 22);
            this.magentaToolStripMenuItem.Text = "Magenta";
            // 
            // cyanToolStripMenuItem
            // 
            this.cyanToolStripMenuItem.Name = "cyanToolStripMenuItem";
            this.cyanToolStripMenuItem.Size = new System.Drawing.Size(121, 22);
            this.cyanToolStripMenuItem.Text = "Cyan";
            // 
            // whiteToolStripMenuItem
            // 
            this.whiteToolStripMenuItem.Name = "whiteToolStripMenuItem";
            this.whiteToolStripMenuItem.Size = new System.Drawing.Size(121, 22);
            this.whiteToolStripMenuItem.Text = "White";
            // 
            // setInkToolStripMenuItem
            // 
            this.setInkToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.blackToolStripMenuItem1,
            this.redToolStripMenuItem1,
            this.greenToolStripMenuItem1,
            this.yellowToolStripMenuItem1,
            this.blueToolStripMenuItem1,
            this.magentaToolStripMenuItem1,
            this.cyanToolStripMenuItem1,
            this.whiteToolStripMenuItem1});
            this.setInkToolStripMenuItem.Name = "setInkToolStripMenuItem";
            this.setInkToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.setInkToolStripMenuItem.Text = "Set Ink";
            // 
            // blackToolStripMenuItem1
            // 
            this.blackToolStripMenuItem1.Name = "blackToolStripMenuItem1";
            this.blackToolStripMenuItem1.Size = new System.Drawing.Size(121, 22);
            this.blackToolStripMenuItem1.Text = "Black";
            // 
            // redToolStripMenuItem1
            // 
            this.redToolStripMenuItem1.Name = "redToolStripMenuItem1";
            this.redToolStripMenuItem1.Size = new System.Drawing.Size(121, 22);
            this.redToolStripMenuItem1.Text = "Red";
            // 
            // greenToolStripMenuItem1
            // 
            this.greenToolStripMenuItem1.Name = "greenToolStripMenuItem1";
            this.greenToolStripMenuItem1.Size = new System.Drawing.Size(121, 22);
            this.greenToolStripMenuItem1.Text = "Green";
            // 
            // yellowToolStripMenuItem1
            // 
            this.yellowToolStripMenuItem1.Name = "yellowToolStripMenuItem1";
            this.yellowToolStripMenuItem1.Size = new System.Drawing.Size(121, 22);
            this.yellowToolStripMenuItem1.Text = "Yellow";
            // 
            // blueToolStripMenuItem1
            // 
            this.blueToolStripMenuItem1.Name = "blueToolStripMenuItem1";
            this.blueToolStripMenuItem1.Size = new System.Drawing.Size(121, 22);
            this.blueToolStripMenuItem1.Text = "Blue";
            // 
            // magentaToolStripMenuItem1
            // 
            this.magentaToolStripMenuItem1.Name = "magentaToolStripMenuItem1";
            this.magentaToolStripMenuItem1.Size = new System.Drawing.Size(121, 22);
            this.magentaToolStripMenuItem1.Text = "Magenta";
            // 
            // cyanToolStripMenuItem1
            // 
            this.cyanToolStripMenuItem1.Name = "cyanToolStripMenuItem1";
            this.cyanToolStripMenuItem1.Size = new System.Drawing.Size(121, 22);
            this.cyanToolStripMenuItem1.Text = "Cyan";
            // 
            // whiteToolStripMenuItem1
            // 
            this.whiteToolStripMenuItem1.Name = "whiteToolStripMenuItem1";
            this.whiteToolStripMenuItem1.Size = new System.Drawing.Size(121, 22);
            this.whiteToolStripMenuItem1.Text = "White";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(172, 6);
            // 
            // removeAttributeToolStripMenuItem
            // 
            this.removeAttributeToolStripMenuItem.Name = "removeAttributeToolStripMenuItem";
            this.removeAttributeToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.removeAttributeToolStripMenuItem.Text = "Remove Attribute";
            // 
            // toggleInverseFlagToolStripMenuItem
            // 
            this.toggleInverseFlagToolStripMenuItem.Name = "toggleInverseFlagToolStripMenuItem";
            this.toggleInverseFlagToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.toggleInverseFlagToolStripMenuItem.Text = "Toggle Inverse Flag";
            this.toggleInverseFlagToolStripMenuItem.Click += new System.EventHandler(this.toggleInverseFlagToolStripMenuItem_Click);
            // 
            // flipAllBitsToolStripMenuItem
            // 
            this.flipAllBitsToolStripMenuItem.Name = "flipAllBitsToolStripMenuItem";
            this.flipAllBitsToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.flipAllBitsToolStripMenuItem.Text = "Flip all bits";
            // 
            // EditorMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(783, 492);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "EditorMain";
            this.Text = "OASIS Room Editor";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.HiresPictureBox)).EndInit();
            this.contextMenuAttributes.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.Panel panel1;
        //private System.Windows.Forms.PictureBox HiresPictureBox;
        private PixelBox HiresPictureBox;
        private System.Windows.Forms.ContextMenuStrip contextMenuAttributes;
        private System.Windows.Forms.ToolStripMenuItem setPaperToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem blackToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem redToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem greenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem yellowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem blueToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem magentaToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cyanToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem whiteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setInkToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem blackToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem redToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem greenToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem yellowToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem blueToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem magentaToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem cyanToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem whiteToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem removeAttributeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toggleInverseFlagToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem flipAllBitsToolStripMenuItem;
    }
}

