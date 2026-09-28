// Requires: dotnet add package Bogus
//
// Seeds, in dependency order:
//   LeaveTypes -> LeaveAccrualRules -> LeaveBalances (per user x per type,
//   current year) -> LeaveRequests (per user, drawn against balances) ->
//   PublicHolidays -> LeaveBlackoutPeriods -> LeaveDelegations
//
// USER REFERENCES: as with the Assets seeder, UserId/DelegatorUserId/etc are
// random Guids unless you pass real ones in via ExistingUserIds.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Alphabet.Domain.Entities;

namespace Alphabet.Infrastructure.Data.Seed
{
    public class LeaveSeedOptions
    {
        public int UserCount { get; set; } = 20; // used only if ExistingUserIds is empty
        public int RequestsPerUser { get; set; } = 3;
        public int PublicHolidayCount { get; set; } = 10;
        public int BlackoutPeriodCount { get; set; } = 2;
        public int DelegationCount { get; set; } = 5;
        public bool WipeExisting { get; set; } = false;
        public IReadOnlyList<Guid> ExistingUserIds { get; set; } = Array.Empty<Guid>();
    }

    public class LeaveSeedResult
    {
        public int LeaveTypes { get; set; }
        public int AccrualRules { get; set; }
        public int Balances { get; set; }
        public int Requests { get; set; }
        public int PublicHolidays { get; set; }
        public int BlackoutPeriods { get; set; }
        public int Delegations { get; set; }
    }

    public interface ILeaveSeeder
    {
        Task<LeaveSeedResult> SeedAsync(LeaveSeedOptions options, CancellationToken ct = default);
    }

    public class LeaveSeeder : ILeaveSeeder
    {
        private readonly DbContext _context; // replace with your actual AppDbContext type

        // A fixed, realistic set of leave types rather than fully random ones —
        // leave types are few and meaningfully named in real systems.
        private static readonly (string Name, string Code, bool IsPaid, double DaysPerYear, string Color)[] TypeDefs =
        {
            ("Annual Leave",    "ANNUAL",  true,  21, "#4C8BF5"),
            ("Sick Leave",      "SICK",    true,  10, "#EF5350"),
            ("Casual Leave",    "CASUAL",  true,  7,  "#66BB6A"),
            ("Maternity Leave", "MAT",     true,  90, "#AB47BC"),
            ("Paternity Leave", "PAT",     true,  14, "#42A5F5"),
            ("Unpaid Leave",    "UNPAID",  false, 0,  "#9E9E9E"),
            ("Bereavement Leave","BEREAVE",true,  5,  "#8D6E63"),
        };

        public LeaveSeeder(DbContext context)
        {
            _context = context;
        }

        public async Task<LeaveSeedResult> SeedAsync(LeaveSeedOptions options, CancellationToken ct = default)
        {
            if (options.WipeExisting)
                await WipeExistingAsync(ct);

            var random = new Random();
            var currentYear = DateTime.UtcNow.Year;

            var userIds = options.ExistingUserIds.Count > 0
                ? options.ExistingUserIds
                : Enumerable.Range(0, options.UserCount).Select(_ => Guid.NewGuid()).ToList();

            // ---- Leave Types ----
            var leaveTypes = TypeDefs.Select(t => new LeaveType
            {
                Id = Guid.NewGuid(),
                Name = t.Name,
                Code = t.Code,
                Description = $"{t.Name} as defined by company policy.",
                Color = t.Color,
                Icon = "calendar",
                IsPaid = t.IsPaid,
                DefaultDaysPerYear = t.DaysPerYear,
                MaxConsecutiveDays = t.Code == "UNPAID" ? null : (int)Math.Max(5, t.DaysPerYear),
                MinDaysPerRequest = t.Code == "UNPAID" ? 1 : 0.5,
                MaxDaysPerRequest = t.DaysPerYear > 0 ? t.DaysPerYear : null,
                RequiresApproval = true,
                ApprovalChainId = null, // see scope note in LeaveEntities.cs
                CarryForwardEnabled = t.Code is "ANNUAL" or "CASUAL",
                MaxCarryForwardDays = t.Code is "ANNUAL" or "CASUAL" ? Math.Round(t.DaysPerYear * 0.25, 1) : 0,
                CarryForwardExpiryMonths = 3,
                EncashmentEnabled = t.Code == "ANNUAL",
                EncashmentRate = t.Code == "ANNUAL" ? 1.0 : null,
                ProrationEnabled = t.Code != "UNPAID",
                RequiresAttachment = t.Code is "SICK" or "MAT" or "PAT" or "BEREAVE",
                IsActive = true
            }).ToList();

            await _context.Set<LeaveType>().AddRangeAsync(leaveTypes, ct);
            await _context.SaveChangesAsync(ct);

            // ---- Accrual Rules (one per leave type, skip Unpaid) ----
            var accrualFaker = new Faker();
            var accrualRules = leaveTypes
                .Where(t => t.Code != "UNPAID" && t.DefaultDaysPerYear > 0)
                .Select(t => new LeaveAccrualRule
                {
                    Id = Guid.NewGuid(),
                    LeaveTypeId = t.Id,
                    AccrualMethod = LeaveAccrualMethod.Monthly,
                    AccrualRate = Math.Round(t.DefaultDaysPerYear / 12.0, 2),
                    MaxAccrual = t.DefaultDaysPerYear
                }).ToList();

            await _context.Set<LeaveAccrualRule>().AddRangeAsync(accrualRules, ct);
            await _context.SaveChangesAsync(ct);

            // ---- Leave Balances (every user x every leave type, current year) ----
            var balances = new List<LeaveBalance>();
            var balanceLookup = new Dictionary<(Guid UserId, Guid TypeId), LeaveBalance>();

            foreach (var userId in userIds)
            {
                foreach (var type in leaveTypes)
                {
                    var carryForward = type.CarryForwardEnabled
                        ? Math.Round(random.NextDouble() * type.MaxCarryForwardDays, 1)
                        : 0;
                    var allocated = type.DefaultDaysPerYear;
                    var used = allocated > 0 ? Math.Round(random.NextDouble() * allocated * 0.6, 1) : 0;

                    var balance = new LeaveBalance
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        LeaveTypeId = type.Id,
                        Year = currentYear,
                        Allocated = allocated,
                        Remaining = Math.Max(0, Math.Round(allocated + carryForward - used, 1)),
                        CarryForward = carryForward
                    };
                    balances.Add(balance);
                    balanceLookup[(userId, type.Id)] = balance;
                }
            }
            await _context.Set<LeaveBalance>().AddRangeAsync(balances, ct);
            await _context.SaveChangesAsync(ct);

            // ---- Leave Requests ----
            var requests = new List<LeaveRequest>();
            var reasonFaker = new Faker();
            var payableTypes = leaveTypes.Where(t => t.DefaultDaysPerYear > 0).ToList();

            foreach (var userId in userIds)
            {
                for (int i = 0; i < options.RequestsPerUser; i++)
                {
                    var type = payableTypes[random.Next(payableTypes.Count)];
                    var start = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(random.Next(-180, 60)));
                    var span = random.Next(1, Math.Min(5, Math.Max(2, type.MaxConsecutiveDays ?? 5)));
                    var end = start.AddDays(span - 1);

                    var status = start < DateOnly.FromDateTime(DateTime.UtcNow)
                        ? (LeaveRequestStatus)random.Next(2, 5) // Approved/Rejected/Cancelled for past requests
                        : LeaveRequestStatus.Pending;

                    var request = new LeaveRequest
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        LeaveTypeId = type.Id,
                        StartDate = start,
                        EndDate = end,
                        StartDatePart = LeaveDayPart.FullDay,
                        EndDatePart = LeaveDayPart.FullDay,
                        TotalDays = span,
                        Reason = reasonFaker.Lorem.Sentence(6),
                        ContactNumber = reasonFaker.Phone.PhoneNumber(),
                        AlternateArrangements = random.NextDouble() < 0.3 ? reasonFaker.Lorem.Sentence() : null,
                        Status = status,
                        SubmittedAt = start.ToDateTime(TimeOnly.MinValue).AddDays(-random.Next(1, 14)),
                    };

                    if (status != LeaveRequestStatus.Pending)
                    {
                        request.DecidedAt = request.SubmittedAt.AddDays(random.Next(1, 5));
                        request.DecidedByUserId = userIds[random.Next(userIds.Count)];
                        request.DecisionComment = status == LeaveRequestStatus.Rejected
                            ? reasonFaker.Lorem.Sentence()
                            : null;
                    }

                    requests.Add(request);
                }
            }
            await _context.Set<LeaveRequest>().AddRangeAsync(requests, ct);
            await _context.SaveChangesAsync(ct);

            // ---- Public Holidays (current year) ----
            var holidayFaker = new Faker<PublicHoliday>()
                .RuleFor(h => h.Id, _ => Guid.NewGuid())
                .RuleFor(h => h.Name, f => f.PickRandom(
                    "New Year's Day", "National Day", "Labour Day", "Eid al-Fitr",
                    "Eid al-Adha", "Independence Day", "Christmas Day", "Founding Day",
                    "Sports Day", "Martyrs' Day"))
                .RuleFor(h => h.Date, f => DateOnly.FromDateTime(f.Date.Between(
                    new DateTime(currentYear, 1, 1), new DateTime(currentYear, 12, 31))))
                .RuleFor(h => h.Country, f => f.Address.CountryCode())
                .RuleFor(h => h.State, f => null)
                .RuleFor(h => h.IsPaid, _ => true)
                .RuleFor(h => h.Recurring, f => f.Random.Bool(0.7f));

            var holidays = holidayFaker.Generate(options.PublicHolidayCount);
            await _context.Set<PublicHoliday>().AddRangeAsync(holidays, ct);
            await _context.SaveChangesAsync(ct);

            // ---- Blackout Periods ----
            var blackoutFaker = new Faker<LeaveBlackoutPeriod>()
                .RuleFor(b => b.Id, _ => Guid.NewGuid())
                .RuleFor(b => b.Reason, f => f.PickRandom(
                    "Year-end close", "Peak enrollment period", "Exam season", "Audit period"))
                .RuleFor(b => b.ApplicableTo, f => f.Random.Bool(0.5f)
                    ? new List<string> { "*" }
                    : new List<string> { f.Commerce.Department() });

            var blackoutPeriods = blackoutFaker.Generate(options.BlackoutPeriodCount);
            foreach (var period in blackoutPeriods)
            {
                var start = DateOnly.FromDateTime(new Faker().Date.Between(
                    new DateTime(currentYear, 1, 1), new DateTime(currentYear, 11, 1)));
                period.StartDate = start;
                period.EndDate = start.AddDays(random.Next(3, 14));
            }
            await _context.Set<LeaveBlackoutPeriod>().AddRangeAsync(blackoutPeriods, ct);
            await _context.SaveChangesAsync(ct);

            // ---- Delegations ----
            var delegations = new List<LeaveDelegation>();
            for (int i = 0; i < options.DelegationCount && userIds.Count >= 2; i++)
            {
                var delegator = userIds[random.Next(userIds.Count)];
                Guid delegate_;
                do { delegate_ = userIds[random.Next(userIds.Count)]; } while (delegate_ == delegator);

                var start = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-random.Next(0, 60)));
                delegations.Add(new LeaveDelegation
                {
                    Id = Guid.NewGuid(),
                    DelegatorUserId = delegator,
                    DelegateToUserId = delegate_,
                    DelegationType = (LeaveDelegationType)random.Next(1, 4),
                    Permission = (LeaveDelegationPermission)random.Next(1, 4),
                    ApplicableLeaveTypes = random.NextDouble() < 0.5
                        ? leaveTypes.OrderBy(_ => random.Next()).Take(random.Next(1, 3)).Select(t => t.Id).ToList()
                        : new List<Guid>(), // empty = applies to all types
                    StartDate = start,
                    EndDate = random.NextDouble() < 0.6 ? start.AddDays(random.Next(7, 90)) : null,
                    Reason = new Faker().Lorem.Sentence(5),
                    IsActive = true
                });
            }
            await _context.Set<LeaveDelegation>().AddRangeAsync(delegations, ct);
            await _context.SaveChangesAsync(ct);

            return new LeaveSeedResult
            {
                LeaveTypes = leaveTypes.Count,
                AccrualRules = accrualRules.Count,
                Balances = balances.Count,
                Requests = requests.Count,
                PublicHolidays = holidays.Count,
                BlackoutPeriods = blackoutPeriods.Count,
                Delegations = delegations.Count
            };
        }

        private async Task WipeExistingAsync(CancellationToken ct)
        {
            _context.Set<LeaveDelegation>().RemoveRange(_context.Set<LeaveDelegation>());
            _context.Set<LeaveBlackoutPeriod>().RemoveRange(_context.Set<LeaveBlackoutPeriod>());
            _context.Set<PublicHoliday>().RemoveRange(_context.Set<PublicHoliday>());
            _context.Set<LeaveRequest>().RemoveRange(_context.Set<LeaveRequest>());
            _context.Set<LeaveBalance>().RemoveRange(_context.Set<LeaveBalance>());
            _context.Set<LeaveAccrualRule>().RemoveRange(_context.Set<LeaveAccrualRule>());
            _context.Set<LeaveType>().RemoveRange(_context.Set<LeaveType>());
            await _context.SaveChangesAsync(ct);
        }
    }
}
