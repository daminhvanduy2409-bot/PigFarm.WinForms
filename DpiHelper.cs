using System.Windows.Forms;

namespace PigFarm.WinForms
{
    /// <summary>
    /// Tiện ích scale kích thước theo DPI thực tế của form.
    /// Dùng: DpiHelper.Scale(form, 200) → trả về pixel đúng với DPI hiện tại.
    /// Baseline = 96 DPI (100%).
    /// </summary>
    internal static class DpiHelper
    {
        private const float BaseDpi = 96f;

        /// <summary>Scale một giá trị int theo DPI của form.</summary>
        public static int Scale(Form form, int value)
        {
            float factor = form.DeviceDpi / BaseDpi;
            return (int)(value * factor);
        }

        /// <summary>Scale float → int.</summary>
        public static int Scale(Form form, float value)
        {
            float factor = form.DeviceDpi / BaseDpi;
            return (int)(value * factor);
        }

        /// <summary>Scale System.Drawing.Size.</summary>
        public static System.Drawing.Size Scale(Form form, System.Drawing.Size sz)
            => new(Scale(form, sz.Width), Scale(form, sz.Height));

        /// <summary>Scale font size theo DPI (dùng khi tạo Font thủ công).</summary>
        public static float ScaleFont(Form form, float basePt)
        {
            // WinForms tự scale font nếu AutoScaleMode = Dpi,
            // nhưng helper này hữu ích cho owner-draw controls.
            float factor = form.DeviceDpi / BaseDpi;
            return basePt * factor;
        }

        /// <summary>Trả về factor nhân (1.0 = 96dpi, 1.25 = 120dpi, 1.5 = 144dpi).</summary>
        public static float Factor(Form form) => form.DeviceDpi / BaseDpi;
    }
}