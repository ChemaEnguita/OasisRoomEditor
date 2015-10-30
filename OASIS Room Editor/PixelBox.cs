using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace OASIS_Room_Editor
{
    /// <summary>
    /// A PictureBox with configurable interpolation mode.
    /// </summary>
    public class PixelBox : PictureBox
    {
        #region Initialization
        /// <summary>
        /// Initializes a new instance of the <see cref="PixelBox"> class.
        /// </see></summary>
        public PixelBox()
        {
            // Set default.
            InterpolationMode = InterpolationMode.Default;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the interpolation mode.
        /// </summary>
        /// <value>The interpolation mode.</value>
        [Category("Behavior")]
        [DefaultValue(InterpolationMode.Default)]
        public InterpolationMode InterpolationMode { get; set; }
        #endregion

        #region Overrides of PictureBox
        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Paint"> event.
        /// </see></summary>
        /// <param name="pe" />A <see cref="T:System.Windows.Forms.PaintEventArgs"> that contains the event data. 
        protected override void OnPaint(PaintEventArgs pe)
        {
            pe.Graphics.InterpolationMode = InterpolationMode;
            pe.Graphics.PixelOffsetMode= PixelOffsetMode.Half;
            base.OnPaint(pe);
        }
        #endregion
    }
}
