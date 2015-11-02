using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OASIS_Room_Editor
{
    public partial class DialogSizeHires : Form
    {
        public DialogSizeHires()
        {
            InitializeComponent();
            maskedTextBoxWidth.Text = "240";
            maskedTextBoxHeight.Text = "200";
        }

        public int picWidth { get; set; }
        public int picHeight { get; set; }

        private void OK_Click(object sender, EventArgs e)
        {
            picWidth = Int32.Parse(maskedTextBoxWidth.Text);
            picHeight = Int32.Parse(maskedTextBoxHeight.Text);
        }
    }
}
