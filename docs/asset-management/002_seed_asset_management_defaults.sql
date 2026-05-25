DECLARE @HeadOfficeId UNIQUEIDENTIFIER = NEWID();
DECLARE @WarehouseId UNIQUEIDENTIFIER = NEWID();
DECLARE @ItCategoryId UNIQUEIDENTIFIER = NEWID();
DECLARE @FacilitiesCategoryId UNIQUEIDENTIFIER = NEWID();
DECLARE @WorkflowId UNIQUEIDENTIFIER = NEWID();

INSERT INTO AssetLocations (
    Id, Name, Code, Type, Street, City, State, PostalCode, Country,
    ParentLocationId, IsActive, Latitude, Longitude, ContactPerson, ContactPhone, CreatedAt, UpdatedAt
)
VALUES
(
    @HeadOfficeId, 'Head Office - IT Depot', 'HO-IT', 'Office', '1 Innovation Drive', 'Doha', 'Doha', '00000', 'Qatar',
    NULL, 1, 25.285400, 51.531000, 'IT Operations', '+97400000000', SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET()
),
(
    @WarehouseId, 'Main Warehouse', 'WH-01', 'Warehouse', '12 Supply Chain Street', 'Doha', 'Doha', '00001', 'Qatar',
    NULL, 1, NULL, NULL, 'Warehouse Manager', '+97400000001', SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET()
);

INSERT INTO AssetCategories (
    Id, Name, Description, ParentCategoryId, CustomFieldsSchemaJson, DepreciationRate, DefaultLocationId, IsActive, CreatedAt, UpdatedAt
)
VALUES
(
    @ItCategoryId, 'IT Equipment', 'Computers, peripherals, and related devices.', NULL,
    N'{"processor":"string","ram":"string"}', 20.00, @HeadOfficeId, 1, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET()
),
(
    @FacilitiesCategoryId, 'Facilities Equipment', 'Office and facility managed equipment.', NULL,
    N'{}', 10.00, @WarehouseId, 1, SYSDATETIMEOFFSET(), SYSDATETIMEOFFSET()
);

INSERT INTO AssetWorkflowDefinitions (
    Id, Name, Description, Version, StepsJson, IsActive, CreatedAt, UpdatedAt
)
VALUES
(
    @WorkflowId,
    'Asset Procurement Workflow',
    'Default procurement and approval workflow for new asset requests.',
    1,
    N'[
        {"Id":"00000000-0000-0000-0000-000000000101","Name":"Request Submission","Order":1,"AssignedToRole":"Requester","RequiredApprovals":0,"TimeoutHours":24,"Actions":["Approve","Reject","RequestChanges"]},
        {"Id":"00000000-0000-0000-0000-000000000102","Name":"Manager Approval","Order":2,"AssignedToRole":"Admin","RequiredApprovals":1,"TimeoutHours":48,"Actions":["Approve","Reject"]},
        {"Id":"00000000-0000-0000-0000-000000000103","Name":"IT Setup","Order":3,"AssignedToRole":"Admin","RequiredApprovals":0,"TimeoutHours":72,"Actions":["Complete","Delegate"]}
    ]',
    1,
    SYSDATETIMEOFFSET(),
    SYSDATETIMEOFFSET()
);
