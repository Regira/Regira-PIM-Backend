# Fleet Manager

## EF Core

### Migrations

Add-Migration [MigrationName] -context [DbContext] -project '[ProjectName]'
Update-Database -context [DbContext]

```
Add-Migration InitialMigration -context WebshopContext -project 'Webshop.Data'
Update-Database -context WebshopContext
```


#### Reverting

Update-Database [MigrationName] -context [DbContext]
Remove-Migration -context [DbContext] -project '[ProjectName]'

```
Update-Database InitialMigration -context WebshopContext
Remove-Migration -context [DbContext] -project 'Webshop.Data'
```
