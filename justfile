set windows-shell := ["powershell.exe", "-NoLogo", "-Command"]

proj_name := 'CupheadArchipelago'
test_proj_name := 'CupheadArchipelago.Tests'

build:
    dotnet build {{ proj_name }}

setup:
    dotnet restore

build-all:
    dotnet build

clean:
    dotnet clean

test:
    dotnet run --project {{ test_proj_name }}
