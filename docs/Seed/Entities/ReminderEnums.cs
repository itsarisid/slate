// ReminderStatus(0-4) and ReminderType(0-5) are bare integer enums in
// swagger.json — names below are best-guess, zero-based to match.
//
// ReminderType: given CreateReminderRequest already has a separate
// `notificationMethods` array for delivery channel(s), ReminderType more
// likely categorizes *what kind* of reminder this is (standalone vs. linked
// to another entity) rather than duplicating the channel — that's the
// assumption baked into the names below. Reconcile against your real enum
// if you have one.

namespace Alphabet.Domain.Entities
{
    public enum ReminderStatus
    {
        Pending = 0,
        Triggered = 1,
        Snoozed = 2,
        Dismissed = 3,
        Cancelled = 4
    }

    public enum ReminderType
    {
        Standalone = 0,
        Todo = 1,
        Event = 2,
        Task = 3,
        LeaveRequest = 4,
        Custom = 5
    }
}
