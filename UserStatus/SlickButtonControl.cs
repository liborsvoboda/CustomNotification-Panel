using System.Windows.Controls.Primitives;

namespace SensioUserStatus
{
    public class SlickToggleButton : ToggleButton
    {
        // Property to hold a Corner Radius (button's dont have this property)
        private string cornerRadius;
        public string CornerRadius
        {
            get { return cornerRadius; }
            set { cornerRadius = value; }
        }

        // Property to hold the colour for the background highlighting (on mouse over)
        private string highlightBackground;
        public string HighlightBackground
        {
            get { return highlightBackground; }
            set { highlightBackground = value; }
        }

        // Property to hold the background colour applied when the button is in the pressed state
        private string pressedBackground;
        public string PressedBackground
        {
            get { return pressedBackground; }
            set { pressedBackground = value; }
        }
    }
}
