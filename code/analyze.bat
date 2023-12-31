@echo off
for /d /r %%i in (*TestResults) do (
    if exist "%%i" (
        echo Lösche den Ordner "%%i"
        rmdir /s /q "%%i"
    )
)

dotnet test --collect:"XPlat Code Coverage"
reportgenerator "-reports:*/TestResults/*/coverage.cobertura.xml" "-targetdir:.\CoverageReport" -reporttypes:HtmlInline
