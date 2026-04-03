# Fleet Manager

## EF Core

### Migrations

Add-Migration [MigrationName] -context [DbContext] -project '[ProjectName]'
Update-Database -context [DbContext]

```
Add-Migration InitialMigration -context PimDbContext -project 'PIM.Data'
Update-Database -context PimDbContext
```


#### Reverting

Update-Database [MigrationName] -context [DbContext]
Remove-Migration -context [DbContext] -project '[ProjectName]'

```
Update-Database InitialMigration -context PimDbContext
Remove-Migration -context PimDbContext -project 'PIM.Data'
```
