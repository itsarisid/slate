DECLARE @now datetimeoffset = SYSUTCDATETIME();
DECLARE @annual uniqueidentifier = NEWID();
DECLARE @sick uniqueidentifier = NEWID();
DECLARE @unpaid uniqueidentifier = NEWID();

IF NOT EXISTS (SELECT 1 FROM dbo.LeaveTypes WHERE Code = 'ANNUAL')
BEGIN
    INSERT INTO dbo.LeaveTypes
    (Id, Name, Code, Description, Color, Icon, IsPaid, DefaultDaysPerYear, MaxConsecutiveDays, MinDaysPerRequest, MaxDaysPerRequest, RequiresApproval, ApprovalChainId, CarryForwardEnabled, MaxCarryForwardDays, CarryForwardExpiryMonths, EncashmentEnabled, EncashmentRate, ProrationEnabled, EligibilityRulesJson, BlackoutDatesJson, RequiresAttachment, AllowedAttachmentTypesJson, AutoApproveRulesJson, IsActive, CreatedAt, UpdatedAt)
    VALUES
    (@annual, 'Annual Leave', 'ANNUAL', 'Paid annual leave entitlement.', '#2F80ED', NULL, 1, 21, 30, 0.5, NULL, 1, NULL, 1, 5, 3, 0, NULL, 1, '{"minEmploymentDays":0,"probationPassed":false,"applicableRoles":[],"excludedRoles":[]}', '[]', 0, '[]', '{"enabled":false,"maxDays":0,"leadTimeDays":0}', 1, @now, @now);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.LeaveTypes WHERE Code = 'SICK')
BEGIN
    INSERT INTO dbo.LeaveTypes
    (Id, Name, Code, Description, Color, Icon, IsPaid, DefaultDaysPerYear, MaxConsecutiveDays, MinDaysPerRequest, MaxDaysPerRequest, RequiresApproval, ApprovalChainId, CarryForwardEnabled, MaxCarryForwardDays, CarryForwardExpiryMonths, EncashmentEnabled, EncashmentRate, ProrationEnabled, EligibilityRulesJson, BlackoutDatesJson, RequiresAttachment, AllowedAttachmentTypesJson, AutoApproveRulesJson, IsActive, CreatedAt, UpdatedAt)
    VALUES
    (@sick, 'Sick Leave', 'SICK', 'Medical leave with attachment support.', '#27AE60', NULL, 1, 10, 10, 0.5, NULL, 1, NULL, 0, 0, 0, 0, NULL, 0, '{"minEmploymentDays":0,"probationPassed":false,"applicableRoles":[],"excludedRoles":[]}', '[]', 1, '["pdf","jpg","png"]', '{"enabled":false,"maxDays":0,"leadTimeDays":0}', 1, @now, @now);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.LeaveTypes WHERE Code = 'UNPAID')
BEGIN
    INSERT INTO dbo.LeaveTypes
    (Id, Name, Code, Description, Color, Icon, IsPaid, DefaultDaysPerYear, MaxConsecutiveDays, MinDaysPerRequest, MaxDaysPerRequest, RequiresApproval, ApprovalChainId, CarryForwardEnabled, MaxCarryForwardDays, CarryForwardExpiryMonths, EncashmentEnabled, EncashmentRate, ProrationEnabled, EligibilityRulesJson, BlackoutDatesJson, RequiresAttachment, AllowedAttachmentTypesJson, AutoApproveRulesJson, IsActive, CreatedAt, UpdatedAt)
    VALUES
    (@unpaid, 'Unpaid Leave', 'UNPAID', 'Unpaid discretionary leave.', '#8E44AD', NULL, 0, 0, 30, 0.5, NULL, 1, NULL, 0, 0, 0, 0, NULL, 0, '{"minEmploymentDays":0,"probationPassed":false,"applicableRoles":[],"excludedRoles":[]}', '[]', 0, '[]', '{"enabled":false,"maxDays":0,"leadTimeDays":0}', 1, @now, @now);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.LeaveApprovalChains WHERE Code = 'DEFAULT-LEAVE')
BEGIN
    INSERT INTO dbo.LeaveApprovalChains
    (Id, Name, Code, Description, ApplicableToJson, ApprovalLevelsJson, FinalApprovalLevel, AllowSkipLevels, ParallelApproval, IsActive, CreatedAt, UpdatedAt)
    VALUES
    (NEWID(), 'Default Leave Approval', 'DEFAULT-LEAVE', 'Manager then HR approval for standard leave requests.',
     '{"leaveTypeIds":[],"departmentIds":[],"employeeLevels":[],"geographyIds":[],"minDays":0,"maxDays":null}',
     '[{"level":1,"name":"Manager Approval","approverType":"RoleBased","approverValue":"Manager","requiredApprovers":1,"timeoutHours":48,"escalationEnabled":true,"escalationAfterHours":48,"escalationToRole":"HR","autoApproveOnTimeout":false,"canDelegate":true,"conditions":{}},{"level":2,"name":"HR Approval","approverType":"Hr","approverValue":"HR","requiredApprovers":1,"timeoutHours":48,"escalationEnabled":true,"escalationAfterHours":48,"escalationToRole":"Admin","autoApproveOnTimeout":false,"canDelegate":true,"conditions":{}}]',
     2, 0, 0, 1, @now, @now);
END;
