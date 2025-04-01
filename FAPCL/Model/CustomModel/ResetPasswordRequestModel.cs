using System.ComponentModel.DataAnnotations;

namespace FAPCL.Model.CustomModel;

public class ResetPasswordRequestModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}
