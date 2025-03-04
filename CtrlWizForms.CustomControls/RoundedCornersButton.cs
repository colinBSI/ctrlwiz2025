using CtrlWizForms.CustomControls.Utility;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CtrlWizForms.CustomControls
{
    public class RoundedCornersButton : Button
    {
        private Color _borderColor = Color.Silver;
        private Color _onHoverBorderColor = Color.Gray;
        private Color _buttonColor = Color.Red;
        private Color _onHoverButtonColor = Color.Yellow;
        private Color _textColor = Color.White;
        private Color _onHoverTextColor = Color.Gray;
        private bool _isHovering;

        public RoundedCornersButton()
        {
            DoubleBuffered = true;

            MouseEnter += (sender, e) =>
            {
                _isHovering = true;

                Invalidate();
            };
            MouseLeave += (sender, e) =>
            {
                _isHovering = false;

                Invalidate();
            };
        }

        public Color BorderColor
        {
            get => _borderColor;
            set
            {
                _borderColor = value;

                Invalidate();
            }
        }
        public Color OnHoverBorderColor
        {
            get => _onHoverBorderColor;
            set
            {
                _onHoverBorderColor = value;

                Invalidate();
            }
        }
        public Color ButtonColor
        {
            get => _buttonColor;
            set
            {
                _buttonColor = value;

                Invalidate();
            }
        }
        public Color OnHoverButtonColor
        {
            get => _onHoverButtonColor;
            set
            {
                _onHoverButtonColor = value;

                Invalidate();
            }
        }
        public Color TextColor
        {
            get => _textColor;
            set
            {
                _textColor = value;

                Invalidate();
            }
        }
        public Color OnHoverTextColor
        {
            get => _onHoverTextColor;
            set
            {
                _onHoverTextColor = value;

                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            Brush brush = new SolidBrush(_isHovering ? _onHoverBorderColor : _borderColor);

            g.SmoothingMode = SmoothingMode.AntiAlias;

            //Inner part. Button itself.
            g.FillRoundedRectangle(brush, new Rectangle(1, 1, Width - 2, Height - 2), 7);

            brush.Dispose();
            brush = new SolidBrush(_isHovering ? _onHoverTextColor : _textColor);

            //Button Text.
            SizeF stringSize = g.MeasureString(Text, Font);

            g.DrawString(Text, Font, brush, (Width - stringSize.Width) / 2, (Height - stringSize.Height) / 2);
        }
    }
}