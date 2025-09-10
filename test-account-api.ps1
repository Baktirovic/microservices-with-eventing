# Test script for the Account API with User and Person endpoints
Write-Host "Testing Account API with User and Person endpoints..." -ForegroundColor Green

# Wait a moment for service to start
Start-Sleep -Seconds 3

# Test User API
Write-Host "`nTesting User API..." -ForegroundColor Yellow
try {
    # Test GET users
    $usersResponse = Invoke-RestMethod -Uri "http://localhost:5001/api/users" -Method GET
    Write-Host "GET /api/users - Found $($usersResponse.Count) users" -ForegroundColor Green

    # Test POST user
    $newUser = @{
        Username = "testuser"
        Email = "test@example.com"
        PasswordHash = "hashedpassword123"
        Salt = "randomsalt"
    } | ConvertTo-Json

    $createUserResponse = Invoke-RestMethod -Uri "http://localhost:5001/api/users" -Method POST -Body $newUser -ContentType "application/json"
    Write-Host "POST /api/users - Created user with ID: $($createUserResponse.Id)" -ForegroundColor Green

    $userId = $createUserResponse.Id

    # Test GET user by ID
    $userResponse = Invoke-RestMethod -Uri "http://localhost:5001/api/users/$userId" -Method GET
    Write-Host "GET /api/users/$userId - Retrieved user: $($userResponse.Username)" -ForegroundColor Green

    # Test Person API
    Write-Host "`nTesting Person API..." -ForegroundColor Yellow
    
    # Test POST person
    $newPerson = @{
        FirstName = "John"
        LastName = "Doe"
        MiddleName = "Michael"
        PhoneNumber = "123-456-7890"
        Address = "123 Main St"
        City = "Anytown"
        State = "CA"
        PostalCode = "12345"
        Country = "USA"
        DateOfBirth = "1990-01-01T00:00:00Z"
        Gender = "Male"
        Notes = "Test person"
        UserId = $userId
    } | ConvertTo-Json

    $createPersonResponse = Invoke-RestMethod -Uri "http://localhost:5001/api/persons" -Method POST -Body $newPerson -ContentType "application/json"
    Write-Host "POST /api/persons - Created person with ID: $($createPersonResponse.Id)" -ForegroundColor Green

    $personId = $createPersonResponse.Id

    # Test GET person by ID
    $personResponse = Invoke-RestMethod -Uri "http://localhost:5001/api/persons/$personId" -Method GET
    Write-Host "GET /api/persons/$personId - Retrieved person: $($personResponse.FirstName) $($personResponse.LastName)" -ForegroundColor Green

    # Test GET person by user ID
    $personByUserResponse = Invoke-RestMethod -Uri "http://localhost:5001/api/persons/user/$userId" -Method GET
    Write-Host "GET /api/persons/user/$userId - Retrieved person for user: $($personByUserResponse.FirstName) $($personByUserResponse.LastName)" -ForegroundColor Green

    # Test GET all persons
    $personsResponse = Invoke-RestMethod -Uri "http://localhost:5001/api/persons" -Method GET
    Write-Host "GET /api/persons - Found $($personsResponse.Count) persons" -ForegroundColor Green

} catch {
    Write-Host "API test failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Make sure the Account API is running on port 5001" -ForegroundColor Yellow
}

Write-Host "`nAccount API Testing Complete!" -ForegroundColor Green
Write-Host "You can access Swagger UI at: https://localhost:5001/swagger" -ForegroundColor Cyan
