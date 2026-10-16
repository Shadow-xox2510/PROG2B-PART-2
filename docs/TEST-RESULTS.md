\# RaceDay API Test Results



\## Test Execution



The solution was tested using the .NET CLI:



`dotnet test RaceDayAPI.slnx`



\## Test Coverage



The automated tests cover:



\- User registration

\- Login and session information

\- Rejection of unauthenticated profile requests

\- Event creation by organisers

\- Rejection of event creation by participants

\- Participant enrolment

\- Recording event results

\- Viewing a participant's own results

\- Rejection of registration with an invalid role



\## Result



The latest verified test run completed with \*\*9 tests passed and 0 failed\*\*.



\## Notes



The build reported package vulnerability warnings. These should be reviewed and resolved before a production deployment.

