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
    public partial class formNewRoom : Form
    {
        public String roomName { get; private set; }
        public int roomID { get; private set; }
        public int roomSize { get; private set; }


        public formNewRoom()
        {
            roomName = "No name";
            roomID = 0;
            roomSize = 36;
            InitializeComponent();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            roomName = textBoxName.Text;
            roomID = Int32.Parse(textBoxID.Text);
            roomSize = Int32.Parse(textBoxSize.Text);

            if ( (roomID > 255)||(roomID<0))
            {
                MessageBox.Show("Room ID should be in the range 0 to 255. Defaulting to 0", "Invalid room ID", MessageBoxButtons.OK, MessageBoxIcon.Error);
                roomID=0;
            }

            if (roomSize>128)
            {
                MessageBox.Show("Room size is too big, defaulting to 128", "Invalid size", MessageBoxButtons.OK, MessageBoxIcon.Error);
                roomSize = 128;
            }

            if (roomSize < 36)
            {
                MessageBox.Show("Room size is too small, defaulting to 36", "Invalid size", MessageBoxButtons.OK, MessageBoxIcon.Error);
                roomSize = 36;
            }
        }
    }
}
