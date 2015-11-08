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
    public partial class AICHelperDialog : Form
    {
        public AICHelperDialog()
        {
            InitializeComponent();
            boxColumn.Text = "0";
            boxRowStart.Text = "0";
            boxRowEnd.Text = "199";
            comboBoxAttr1.SelectedIndex = 2;
            comboBoxAttr2.SelectedIndex = 6;
            checkInverse1.Checked = false;
            checkInverse2.Checked = false;
        }

        public int Column { get; set; }
        public int Row1 { get; set; }
        public int Row2 { get; set; }
        public int Attrib1 { get; set; }
        public int Attrib2 { get; set; }
        public bool Inverse1 { get; set; }
        public bool Inverse2 { get; set; }

        private void OK_Click(object sender, EventArgs e)
        {
            Column = Int32.Parse(boxColumn.Text);
            Row1 = Int32.Parse(boxRowStart.Text);
            Row2 = Int32.Parse(boxRowEnd.Text);
            Attrib1 = comboBoxAttr1.SelectedIndex;
            if (Attrib1 > 7)
                Attrib1 = Attrib1 - 8 + 16;
            Attrib2 = comboBoxAttr2.SelectedIndex;
            if (Attrib2 > 7)
                Attrib2 = Attrib2 - 8 + 16;
            Inverse1 = checkInverse1.Checked;
            Inverse2 = checkInverse2.Checked;
        }
    }
}
