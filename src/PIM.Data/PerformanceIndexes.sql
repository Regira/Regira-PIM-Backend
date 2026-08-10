-- Indexes declared by the PIM model (PimDbContext.OnModelCreating), as an idempotent script.
--
-- New databases get these automatically from EnsureCreated(). An EXISTING database does not:
-- EnsureCreated() is a no-op once the database is there, so run this against databases created
-- before these indexes were added to the model.
--
-- Regenerate with the model rather than editing by hand.

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FacetChildGroup_FacetId' AND object_id = OBJECT_ID('[FacetChildGroup]'))
    CREATE INDEX [IX_FacetChildGroup_FacetId] ON [FacetChildGroup] ([FacetId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FacetLink_ChildId' AND object_id = OBJECT_ID('[FacetLink]'))
    CREATE INDEX [IX_FacetLink_ChildId] ON [FacetLink] ([ChildId]) INCLUDE ([ParentId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FacetParentGroup_FacetId' AND object_id = OBJECT_ID('[FacetParentGroup]'))
    CREATE INDEX [IX_FacetParentGroup_FacetId] ON [FacetParentGroup] ([FacetId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Facets_Code' AND object_id = OBJECT_ID('[Facets]'))
    CREATE INDEX [IX_Facets_Code] ON [Facets] ([Code]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Facets_Title' AND object_id = OBJECT_ID('[Facets]'))
    CREATE INDEX [IX_Facets_Title] ON [Facets] ([Title]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Parties_Code' AND object_id = OBJECT_ID('[Parties]'))
    CREATE INDEX [IX_Parties_Code] ON [Parties] ([Code]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Parties_Created' AND object_id = OBJECT_ID('[Parties]'))
    CREATE INDEX [IX_Parties_Created] ON [Parties] ([Created]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Parties_PartyType_NormalizedTitle' AND object_id = OBJECT_ID('[Parties]'))
    CREATE INDEX [IX_Parties_PartyType_NormalizedTitle] ON [Parties] ([PartyType], [NormalizedTitle]) WHERE [IsArchived] = 0;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PartyAddress_PartyId' AND object_id = OBJECT_ID('[PartyAddress]'))
    CREATE INDEX [IX_PartyAddress_PartyId] ON [PartyAddress] ([PartyId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PartyContactDetails_PartyId' AND object_id = OBJECT_ID('[PartyContactDetails]'))
    CREATE INDEX [IX_PartyContactDetails_PartyId] ON [PartyContactDetails] ([PartyId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PartyRelationship_ChildId' AND object_id = OBJECT_ID('[PartyRelationship]'))
    CREATE INDEX [IX_PartyRelationship_ChildId] ON [PartyRelationship] ([ChildId]) INCLUDE ([ParentId], [RelationshipTypeId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PartyRelationship_RelationshipTypeId' AND object_id = OBJECT_ID('[PartyRelationship]'))
    CREATE INDEX [IX_PartyRelationship_RelationshipTypeId] ON [PartyRelationship] ([RelationshipTypeId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PartyRelationshipContactDetails_PartyRelationshipId' AND object_id = OBJECT_ID('[PartyRelationshipContactDetails]'))
    CREATE INDEX [IX_PartyRelationshipContactDetails_PartyRelationshipId] ON [PartyRelationshipContactDetails] ([PartyRelationshipId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ProductComponent_ComponentId' AND object_id = OBJECT_ID('[ProductComponent]'))
    CREATE INDEX [IX_ProductComponent_ComponentId] ON [ProductComponent] ([ComponentId]) INCLUDE ([AssemblyId], [Quantity], [IsOmittable], [SortOrder]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ProductFacet_FacetId' AND object_id = OBJECT_ID('[ProductFacet]'))
    CREATE INDEX [IX_ProductFacet_FacetId] ON [ProductFacet] ([FacetId]) INCLUDE ([ProductId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Products_Archived' AND object_id = OBJECT_ID('[Products]'))
    CREATE INDEX [IX_Products_Archived] ON [Products] ([Id]) WHERE [IsArchived] = 1;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Products_Created_NotArchived' AND object_id = OBJECT_ID('[Products]'))
    CREATE INDEX [IX_Products_Created_NotArchived] ON [Products] ([Created]) WHERE [IsArchived] = 0;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Products_Title_NotArchived' AND object_id = OBJECT_ID('[Products]'))
    CREATE INDEX [IX_Products_Title_NotArchived] ON [Products] ([Title]) INCLUDE ([Description], [UnitTypeId], [DefaultQuantity], [Created]) WHERE [IsArchived] = 0;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Products_UnitTypeId' AND object_id = OBJECT_ID('[Products]'))
    CREATE INDEX [IX_Products_UnitTypeId] ON [Products] ([UnitTypeId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ProductSupplier_SupplierId_ProductId' AND object_id = OBJECT_ID('[ProductSupplier]'))
    CREATE INDEX [IX_ProductSupplier_SupplierId_ProductId] ON [ProductSupplier] ([SupplierId], [ProductId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FacetChildGroup_FacetGroupId_FacetId' AND object_id = OBJECT_ID('[FacetChildGroup]'))
    CREATE UNIQUE INDEX [IX_FacetChildGroup_FacetGroupId_FacetId] ON [FacetChildGroup] ([FacetGroupId], [FacetId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FacetLink_ParentId_ChildId' AND object_id = OBJECT_ID('[FacetLink]'))
    CREATE UNIQUE INDEX [IX_FacetLink_ParentId_ChildId] ON [FacetLink] ([ParentId], [ChildId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FacetParentGroup_FacetGroupId_FacetId' AND object_id = OBJECT_ID('[FacetParentGroup]'))
    CREATE UNIQUE INDEX [IX_FacetParentGroup_FacetGroupId_FacetId] ON [FacetParentGroup] ([FacetGroupId], [FacetId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PartyRelationship_ParentId_ChildId_RelationshipTypeId' AND object_id = OBJECT_ID('[PartyRelationship]'))
    CREATE UNIQUE INDEX [IX_PartyRelationship_ParentId_ChildId_RelationshipTypeId] ON [PartyRelationship] ([ParentId], [ChildId], [RelationshipTypeId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PartyUsers_PartyId' AND object_id = OBJECT_ID('[PartyUsers]'))
    CREATE UNIQUE INDEX [IX_PartyUsers_PartyId] ON [PartyUsers] ([PartyId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ProductComponent_AssemblyId_ComponentId' AND object_id = OBJECT_ID('[ProductComponent]'))
    CREATE UNIQUE INDEX [IX_ProductComponent_AssemblyId_ComponentId] ON [ProductComponent] ([AssemblyId], [ComponentId]) INCLUDE ([Quantity], [IsOmittable], [SortOrder]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ProductFacet_ProductId_FacetId' AND object_id = OBJECT_ID('[ProductFacet]'))
    CREATE UNIQUE INDEX [IX_ProductFacet_ProductId_FacetId] ON [ProductFacet] ([ProductId], [FacetId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ProductSupplier_ProductId_SupplierId' AND object_id = OBJECT_ID('[ProductSupplier]'))
    CREATE UNIQUE INDEX [IX_ProductSupplier_ProductId_SupplierId] ON [ProductSupplier] ([ProductId], [SupplierId]);
GO

UPDATE STATISTICS Products;
UPDATE STATISTICS ProductComponent;
UPDATE STATISTICS ProductFacet;
UPDATE STATISTICS Facets;
UPDATE STATISTICS Parties;
GO