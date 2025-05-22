# justfile

set windows-shell := ["powershell.exe", "-NoLogo", "-Command"]

proj_name := 'CupheadArchipelago'
test_proj_name := 'CupheadArchipelago.Tests'

default:
    @just --list

setup:
    dotnet restore

build:
    dotnet build {{ proj_name }}

build-tests:
    dotnet build {{ test_proj_name }}

build-all:
    dotnet build

clean:
    dotnet clean

test:
    dotnet run --project {{ test_proj_name }}
