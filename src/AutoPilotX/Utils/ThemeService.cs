using System;
using System.Drawing;
using System.Windows.Forms;

namespace AutoPilotX.Utils
{
    public static class ThemeService
    {
        public static void ApplyTheme(Form form)
        {
            // 1. DWM Dark Mode for Title Bar
            ThemeUtils.UseImmersiveDarkMode(form.Handle, true);

            // 2. Form Background
            form.BackColor = ThemeColor.Background;
            form.ForeColor = ThemeColor.TextPrimary;

            // 3. Recursive control styling
            ApplyToControls(form.Controls);
        }

        private static void ApplyToControls(Control.ControlCollection controls)
        {
            foreach (Control c in controls)
            {
                StyleControl(c);
                if (c.HasChildren)
                {
                    ApplyToControls(c.Controls);
                }
            }
        }

        private static void StyleControl(Control c)
        {
            // General reset
            c.ForeColor = ThemeColor.TextPrimary;

            switch (c)
            {
                case Button btn:
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;
                    btn.BackColor = ThemeColor.Accent;
                    btn.ForeColor = Color.White;
                    btn.Cursor = Cursors.Hand;
                    
                    // Simple hover effect tweak (WinForms doesn't support easy styling for hover color without paint override, 
                    // but FlatAppearance handles some of it)
                    btn.FlatAppearance.MouseOverBackColor = ThemeColor.AccentHover;
                    btn.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(ThemeColor.Accent);
                    
                    if (!btn.Enabled)
                    {
                        btn.BackColor = ThemeColor.Surface;
                        btn.ForeColor = ThemeColor.TextSecondary;
                    }
                    
                    // Handle enable/disable changes manually if needed, or rely on paint
                    btn.EnabledChanged += (s, e) => 
                    {
                        var b = (Button)s!;
                        b.BackColor = b.Enabled ? ThemeColor.Accent : ThemeColor.Surface;
                        b.ForeColor = b.Enabled ? Color.White : ThemeColor.TextSecondary;
                    };
                    break;

                case Label lbl:
                    lbl.BackColor = Color.Transparent; 
                    // Assume parents handle background. 
                    break;

                case TextBox txt:
                    txt.BackColor = ThemeColor.InputBackground;
                    txt.ForeColor = ThemeColor.TextPrimary;
                    txt.BorderStyle = BorderStyle.FixedSingle;
                    break;
                
                case NumericUpDown num:
                    num.BackColor = ThemeColor.InputBackground;
                    num.ForeColor = ThemeColor.TextPrimary;
                    break;

                case ComboBox cmb:
                    cmb.BackColor = ThemeColor.InputBackground;
                    cmb.ForeColor = ThemeColor.TextPrimary;
                    cmb.FlatStyle = FlatStyle.Flat; 
                    break;

                case ListBox lb:
                    lb.BackColor = ThemeColor.InputBackground;
                    lb.ForeColor = ThemeColor.TextPrimary;
                    lb.BorderStyle = BorderStyle.FixedSingle;
                    break;
                
                case DataGridView dgv:
                    dgv.BackgroundColor = ThemeColor.Surface;
                    dgv.DefaultCellStyle.BackColor = ThemeColor.InputBackground;
                    dgv.DefaultCellStyle.ForeColor = ThemeColor.TextPrimary;
                    dgv.DefaultCellStyle.SelectionBackColor = ThemeColor.Accent;
                    dgv.ColumnHeadersDefaultCellStyle.BackColor = ThemeColor.Surface;
                    dgv.ColumnHeadersDefaultCellStyle.ForeColor = ThemeColor.TextPrimary;
                    dgv.EnableHeadersVisualStyles = false;
                    dgv.BorderStyle = BorderStyle.None;
                    dgv.GridColor = ThemeColor.ControlBorder;
                    break;

                case TabControl tc:
                    // TabControl is notoriously hard to style without OwnerDraw.
                    // For now, let's keep it simple or maybe set it later.
                    // We'll leave standard but maybe fix the inner pages.
                    break;

                case TabPage tp:
                    tp.BackColor = ThemeColor.Background;
                    tp.ForeColor = ThemeColor.TextPrimary;
                    break;
            }
        }
    }
}
