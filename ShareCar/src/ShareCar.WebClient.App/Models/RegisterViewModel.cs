using System.ComponentModel.DataAnnotations;

namespace ShareCar.WebClient.App.Models;

public class RegisterViewModel
{
  [Required(ErrorMessage = "Username is required.")]
  [StringLength(100, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 100 characters.")]
  public string Username { get; set; } = string.Empty;

  [Required(ErrorMessage = "Email is required.")]
  [EmailAddress(ErrorMessage = "Invalid email address.")]
  public string Email { get; set; } = string.Empty;

  [Required(ErrorMessage = "Password is required.")]
  [DataType(DataType.Password)]
  [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
  public string Password { get; set; } = string.Empty;

  [Required(ErrorMessage = "Please confirm your password.")]
  [DataType(DataType.Password)]
  [Compare("Password", ErrorMessage = "Passwords do not match.")]
  public string ConfirmPassword { get; set; } = string.Empty;
}
