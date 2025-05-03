using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DeviceInterfaceManager.ViewModels.Dialogs;

public class AskTextBoxDialogModel : ObservableValidator
{
    public string? Title { get; set; }

    [Required]
    [CustomValidation(typeof(AskTextBoxDialogModel), nameof(ValidateFileName))]
    public string? Text { get; set; }

    public static ValidationResult? ValidateFileName(string name, ValidationContext validationContext)
    {
        char[] invalidChars = Path.GetInvalidFileNameChars();
        char[] invalidCharsInName = name.Where(ch => invalidChars.Contains(ch)).Distinct().ToArray();

        if (invalidCharsInName.Length == 0)
        {
            return ValidationResult.Success;
        }

        string invalidCharsString = new(invalidCharsInName);
        return new ValidationResult($"The file name contains invalid characters: {invalidCharsString}");
    }

    public bool CheckForErrors()
    {
        ValidateAllProperties();
        return HasErrors;
    }
}