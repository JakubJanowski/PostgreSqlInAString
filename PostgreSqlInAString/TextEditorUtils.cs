using System.Windows.Media;

namespace PostgreSqlInAString {
    // Current text editor colors can't be read due to access violation error while reading protected memory. It succeeds in some cases but some tokens spontaneously change to light themed colors (e.g. black operators/punctuation) while in dark mode.
    // System.AccessViolationException: 'Attempted to read or write protected memory. This is often an indication that other memory is corrupt.'
    internal static class TextEditorUtils {
        internal static readonly Color DefaultStringColor = Color.FromRgb(214, 157, 133);
        internal static readonly Color DefaultStringEscapeColor = Color.FromRgb(255, 214, 143); 
        
        internal static Color MixWithStringColor(Color color) {
            return CombineColors(DefaultStringColor, color);
        }
        
        internal static Color MixWithStringEscapeColor(Color color) {
            return CombineColors(DefaultStringEscapeColor, color);
        }

        internal static Color CombineColors(Color baseColor, Color overlayColor) {
            return Color.FromArgb(baseColor.A, (byte)((baseColor.R + overlayColor.R) / 2), (byte)((baseColor.G + overlayColor.G) / 2), (byte)((baseColor.B + overlayColor.B) / 2));
        }
    }
}
