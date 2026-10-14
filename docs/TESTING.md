\# RaceDay API Testing



\## Purpose



Automated tests help verify that important API operations behave as expected and that access restrictions are applied.



\## Existing Test Coverage



The test project includes tests for:



\- User registration

\- Login and session information

\- Rejection of unauthenticated profile requests

\- Event creation by organisers

\- Rejection of event creation by participants

\- Participant enrolment

\- Recording event results

\- Viewing a participant's own results



\## Running the Tests



Open PowerShell in the repository root and run:



```powershell

dotnet restore RaceDayAPI.slnx

dotnet test RaceDayAPI.slnx

```



Review the command output for the number of passed and failed tests.



\## Continuous Integration



The GitHub Actions workflow automatically builds the solution and runs the configured test suite. Check the repository's Actions tab for the latest run and its result.

