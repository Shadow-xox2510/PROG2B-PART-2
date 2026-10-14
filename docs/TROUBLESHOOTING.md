\# RaceDay API Troubleshooting



\## Restore Problems



Run the following command from the repository root:



`dotnet restore RaceDayAPI.slnx`



Check that the .NET SDK required by the project is installed.



\## Build Problems



Run:



`dotnet build RaceDayAPI.slnx --configuration Release`



Read the error messages and check the affected project or source file.



\## Test Problems



Run:



`dotnet test RaceDayAPI.slnx`



Review failed test names and their error messages before making changes.



\## GitHub Actions Problems



Open the repository's Actions tab and select the failed workflow run. Read the step logs to identify whether dependency restoration, building or testing failed.



\## Git Problems



Use `git status` to check modified files and `git log -5 --oneline` to review recent commits.

