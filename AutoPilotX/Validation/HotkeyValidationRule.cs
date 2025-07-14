using System.Globalization;
using System.Windows.Controls;
using System.Windows.Input;

namespace AutoPilotX.Validation
{
    public class HotkeyValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is string str)
            {
                try
                {
                    var converter = new KeyGestureConverter();
                    converter.ConvertFromString(str);
                    return ValidationResult.ValidResult;
                }
                catch (System.Exception)
                {
                    return new ValidationResult(false, "Invalid hotkey");
                }
            }
            return new ValidationResult(false, "Invalid hotkey");
        }
    }
}
