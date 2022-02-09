using System.ComponentModel.DataAnnotations;
using System.IO;

namespace CtActivator
{
	internal class FileExistsAttribute : ValidationAttribute
	{
		protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
		{
			if (value is string path && File.Exists(path))
			{
				return ValidationResult.Success;
			}
			return new ValidationResult($"The file '{value}' has not been found.");
		}
	}
}

	

