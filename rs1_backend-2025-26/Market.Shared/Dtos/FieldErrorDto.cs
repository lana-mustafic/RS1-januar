namespace Market.Shared.Dtos;

/// <summary>
/// Represents a single field-level validation error.
/// </summary>
public sealed class FieldErrorDto
{
    /// <summary>
    /// The name of the field that the error refers to.
    /// </summary>
    public string Field { get; set; } = default!;

    /// <summary>
    /// The validation error message.
    /// </summary>
    public string Message { get; set; } = default!;

    /// <summary>
    /// The error code defined in the validator. (optional)
    /// </summary>
    public string? ErrorCode { get; set; }
}
