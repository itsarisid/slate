// Entities inferred from CreateLeaveTypeRequest/UpdateLeaveTypeRequest,
// SubmitLeaveRequest/ModifyLeaveRequest/CancelLeaveRequest/RejectLeaveRequest,
// InitializeLeaveBalanceRequest(Item)/AdjustLeaveBalanceRequest,
// CreateAccrualRuleRequest, CreatePublicHolidayRequest,
// CreateBlackoutPeriodRequest, and CreateDelegationRequest in swagger.json.
//
// SCOPE NOTE: LeaveType.ApprovalChainId references the generic
// ApprovalChain/ApprovalLevelDefinition system (also used elsewhere, e.g.
// Privileges/Workflows). That's cross-cutting infra rather than part of the
// Leave module itself, so I've left it as a nullable Guid here (either null
// or a random placeholder) instead of modeling the full approval chain.
// Say the word if you want a dedicated Approval Chains module/seeder.

using System;
using System.Collections.Generic;

namespace Alphabet.Domain.Entities
{
    public class LeaveType
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = default!;
        public string Code { get; set; } = default!;
        public string? Description { get; set; }
        public string? Color { get; set; }
        public string? Icon { get; set; }
        public bool IsPaid { get; set; }
        public double DefaultDaysPerYear { get; set; }
        public int? MaxConsecutiveDays { get; set; }
        public double MinDaysPerRequest { get; set; }
        public double? MaxDaysPerRequest { get; set; }
        public bool RequiresApproval { get; set; }
        public Guid? ApprovalChainId { get; set; }
        public bool CarryForwardEnabled { get; set; }
        public double MaxCarryForwardDays { get; set; }
        public int CarryForwardExpiryMonths { get; set; }
        public bool EncashmentEnabled { get; set; }
        public double? EncashmentRate { get; set; }
        public bool ProrationEnabled { get; set; }
        public bool RequiresAttachment { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class LeaveAccrualRule
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid LeaveTypeId { get; set; }
        public LeaveAccrualMethod AccrualMethod { get; set; }
        public double AccrualRate { get; set; }
        public double MaxAccrual { get; set; }
    }

    public class LeaveBalance
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public Guid LeaveTypeId { get; set; }
        public int Year { get; set; }
        public double Allocated { get; set; }
        public double Remaining { get; set; }
        public double CarryForward { get; set; }
    }

    public class LeaveRequest
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public Guid LeaveTypeId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public LeaveDayPart StartDatePart { get; set; }
        public LeaveDayPart EndDatePart { get; set; }
        public double TotalDays { get; set; }
        public string? Reason { get; set; }
        public string? ContactNumber { get; set; }
        public string? AlternateArrangements { get; set; }
        public LeaveRequestStatus Status { get; set; }
        public DateTime SubmittedAt { get; set; }
        public DateTime? DecidedAt { get; set; }
        public Guid? DecidedByUserId { get; set; }
        public string? DecisionComment { get; set; }
    }

    public class PublicHoliday
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = default!;
        public DateOnly Date { get; set; }
        public string? Country { get; set; }
        public string? State { get; set; }
        public bool IsPaid { get; set; } = true;
        public bool Recurring { get; set; }
    }

    public class LeaveBlackoutPeriod
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string? Reason { get; set; }
        public List<string> ApplicableTo { get; set; } = new(); // e.g. department names or "*"
    }

    public class LeaveDelegation
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid DelegatorUserId { get; set; }
        public Guid DelegateToUserId { get; set; }
        public LeaveDelegationType DelegationType { get; set; }
        public LeaveDelegationPermission Permission { get; set; }
        public List<Guid> ApplicableLeaveTypes { get; set; } = new();
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? Reason { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
