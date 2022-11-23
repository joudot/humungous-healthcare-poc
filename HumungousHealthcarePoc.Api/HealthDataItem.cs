namespace HumungousHealthcarePoc.Api;

public record HealthDataItem
{
    public string PartitionKey => PatientId;

    public string? Id { get; init; }

    public required string PatientId { get; init; }

    public ICollection<string> Symptoms { get; init; } = new List<string>();

    public string HealthStatus { get; init; } = "I feel well";

    public DateTimeOffset? SubmissionDate { get; init; } = DateTimeOffset.UtcNow;
}

public class HealthDataItemValidator : AbstractValidator<HealthDataItem>
{
}
