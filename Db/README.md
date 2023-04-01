dotnet tool install --global dotnet-ef

dotnet ef database drop -v
dotnet ef database update -v

dotnet ef migrations add InitialCreate
dotnet ef database update
