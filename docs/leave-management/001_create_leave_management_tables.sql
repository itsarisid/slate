IF OBJECT_ID('dbo.LeaveActivityLogs', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.LeaveActivityLogs (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        UserId uniqueidentifier NULL,
        LeaveRequestId uniqueidentifier NULL,
        Action nvarchar(100) NOT NULL,
        OldValueJson nvarchar(max) NULL,
        NewValueJson nvarchar(max) NULL,
        Timestamp datetimeoffset NOT NULL,
        IpAddress nvarchar(100) NULL,
        UserAgent nvarchar(500) NULL,
        DetailsJson nvarchar(max) NULL,
        CreatedAt datetimeoffset NOT NULL,
        UpdatedAt datetimeoffset NOT NULL
    );
END;

IF OBJECT_ID('dbo.LeaveTypes', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.LeaveTypes (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        Name nvarchar(150) NOT NULL,
        Code nvarchar(40) NOT NULL UNIQUE,
        Description nvarchar(1000) NOT NULL,
        Color nvarchar(20) NOT NULL,
        Icon nvarchar(100) NULL,
        IsPaid bit NOT NULL,
        DefaultDaysPerYear decimal(10,2) NOT NULL,
        MaxConsecutiveDays int NULL,
        MinDaysPerRequest decimal(10,2) NOT NULL,
        MaxDaysPerRequest decimal(10,2) NULL,
        RequiresApproval bit NOT NULL,
        ApprovalChainId uniqueidentifier NULL,
        CarryForwardEnabled bit NOT NULL,
        MaxCarryForwardDays decimal(10,2) NOT NULL,
        CarryForwardExpiryMonths int NOT NULL,
        EncashmentEnabled bit NOT NULL,
        EncashmentRate decimal(10,2) NULL,
        ProrationEnabled bit NOT NULL,
        EligibilityRulesJson nvarchar(max) NOT NULL,
        BlackoutDatesJson nvarchar(max) NOT NULL,
        RequiresAttachment bit NOT NULL,
        AllowedAttachmentTypesJson nvarchar(max) NOT NULL,
        AutoApproveRulesJson nvarchar(max) NOT NULL,
        IsActive bit NOT NULL,
        CreatedAt datetimeoffset NOT NULL,
        UpdatedAt datetimeoffset NOT NULL
    );
END;

IF OBJECT_ID('dbo.LeaveBalances', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.LeaveBalances (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        UserId uniqueidentifier NOT NULL,
        LeaveTypeId uniqueidentifier NOT NULL,
        [Year] int NOT NULL,
        Allocated decimal(10,2) NOT NULL,
        Taken decimal(10,2) NOT NULL,
        Pending decimal(10,2) NOT NULL,
        Approved decimal(10,2) NOT NULL,
        Remaining decimal(10,2) NOT NULL,
        CarryForward decimal(10,2) NOT NULL,
        CreatedAt datetimeoffset NOT NULL,
        UpdatedAt datetimeoffset NOT NULL,
        CONSTRAINT UX_LeaveBalances_User_Type_Year UNIQUE (UserId, LeaveTypeId, [Year])
    );
END;

IF OBJECT_ID('dbo.LeaveRequests', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.LeaveRequests (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        LeaveTypeId uniqueidentifier NOT NULL,
        UserId uniqueidentifier NOT NULL,
        StartDate date NOT NULL,
        EndDate date NOT NULL,
        PartialDaysJson nvarchar(max) NOT NULL,
        TotalDays decimal(10,2) NOT NULL,
        Reason nvarchar(2000) NOT NULL,
        Status nvarchar(40) NOT NULL,
        CurrentApprovalLevel int NOT NULL,
        AppliedAt datetimeoffset NOT NULL,
        CancelledAt datetimeoffset NULL,
        CancelledReason nvarchar(2000) NULL,
        AttachmentIdsJson nvarchar(max) NOT NULL,
        ContactNumber nvarchar(50) NULL,
        AlternateArrangements nvarchar(2000) NULL,
        ApplyToAllDays bit NOT NULL,
        IsHalfDay bit NOT NULL,
        CreatedAt datetimeoffset NOT NULL,
        UpdatedAt datetimeoffset NOT NULL
    );
END;

IF OBJECT_ID('dbo.LeaveApprovalChains', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.LeaveApprovalChains (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        Name nvarchar(150) NOT NULL,
        Code nvarchar(60) NOT NULL UNIQUE,
        Description nvarchar(1000) NOT NULL,
        ApplicableToJson nvarchar(max) NOT NULL,
        ApprovalLevelsJson nvarchar(max) NOT NULL,
        FinalApprovalLevel int NOT NULL,
        AllowSkipLevels bit NOT NULL,
        ParallelApproval bit NOT NULL,
        IsActive bit NOT NULL,
        CreatedAt datetimeoffset NOT NULL,
        UpdatedAt datetimeoffset NOT NULL
    );
END;

IF OBJECT_ID('dbo.LeaveApprovalWorkflows', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.LeaveApprovalWorkflows (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        LeaveRequestId uniqueidentifier NOT NULL,
        ApprovalChainId uniqueidentifier NOT NULL,
        Status nvarchar(40) NOT NULL,
        CurrentLevel int NOT NULL,
        InitiatedAt datetimeoffset NOT NULL,
        CompletedAt datetimeoffset NULL,
        CreatedAt datetimeoffset NOT NULL,
        UpdatedAt datetimeoffset NOT NULL
    );
END;

IF OBJECT_ID('dbo.LeaveWorkflowSteps', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.LeaveWorkflowSteps (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        WorkflowId uniqueidentifier NOT NULL,
        [Level] int NOT NULL,
        ApproverUserId uniqueidentifier NULL,
        ApproverType nvarchar(50) NOT NULL,
        ApproverValue nvarchar(250) NOT NULL,
        Status nvarchar(40) NOT NULL,
        Action nvarchar(60) NULL,
        Comment nvarchar(2000) NULL,
        AttachmentsJson nvarchar(max) NOT NULL,
        AssignedAt datetimeoffset NOT NULL,
        RespondedAt datetimeoffset NULL,
        TimeoutHours int NOT NULL,
        IsEscalated bit NOT NULL,
        CreatedAt datetimeoffset NOT NULL,
        UpdatedAt datetimeoffset NOT NULL
    );
END;

IF OBJECT_ID('dbo.LeaveDelegations', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.LeaveDelegations (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        DelegatorUserId uniqueidentifier NOT NULL,
        DelegateToUserId uniqueidentifier NOT NULL,
        DelegationType nvarchar(40) NOT NULL,
        Permission nvarchar(40) NOT NULL,
        ApplicableLeaveTypesJson nvarchar(max) NOT NULL,
        ApplicableApprovalLevelsJson nvarchar(max) NOT NULL,
        ApplicableDepartmentsJson nvarchar(max) NOT NULL,
        ApplicableEmployeesJson nvarchar(max) NOT NULL,
        StartDate date NOT NULL,
        EndDate date NULL,
        Reason nvarchar(2000) NOT NULL,
        IsActive bit NOT NULL,
        RevokedAt datetimeoffset NULL,
        CreatedAt datetimeoffset NOT NULL,
        UpdatedAt datetimeoffset NOT NULL
    );
END;

IF OBJECT_ID('dbo.LeavePublicHolidays', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.LeavePublicHolidays (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        Name nvarchar(150) NOT NULL,
        Date date NOT NULL,
        Country nvarchar(10) NOT NULL,
        State nvarchar(50) NULL,
        IsPaid bit NOT NULL,
        Recurring bit NOT NULL,
        CreatedAt datetimeoffset NOT NULL,
        UpdatedAt datetimeoffset NOT NULL
    );
END;

IF OBJECT_ID('dbo.LeaveBlackoutPeriods', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.LeaveBlackoutPeriods (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        StartDate date NOT NULL,
        EndDate date NOT NULL,
        Reason nvarchar(1000) NOT NULL,
        ApplicableToJson nvarchar(max) NOT NULL,
        IsActive bit NOT NULL,
        CreatedAt datetimeoffset NOT NULL,
        UpdatedAt datetimeoffset NOT NULL
    );
END;

IF OBJECT_ID('dbo.LeaveAccrualRules', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.LeaveAccrualRules (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        LeaveTypeId uniqueidentifier NOT NULL,
        AccrualMethod nvarchar(40) NOT NULL,
        AccrualRate decimal(10,2) NOT NULL,
        MaxAccrual decimal(10,2) NOT NULL,
        TenureRulesJson nvarchar(max) NOT NULL,
        IsActive bit NOT NULL,
        CreatedAt datetimeoffset NOT NULL,
        UpdatedAt datetimeoffset NOT NULL
    );
END;
