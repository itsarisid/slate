// swagger.json doesn't expose explicit status enums for workflow instances or
// steps (they're mutated via a free-text WorkflowStepActionRequest.Action
// string, e.g. "Approve"/"Reject"). The enums below are added for seeding
// purposes — adjust to match however your domain models workflow state.

namespace Alphabet.Domain.Entities
{
    public enum WorkflowInstanceStatus
    {
        InProgress = 1,
        Completed = 2,
        Rejected = 3,
        Cancelled = 4
    }

    public enum WorkflowStepInstanceStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3,
        Skipped = 4,
        TimedOut = 5
    }
}
