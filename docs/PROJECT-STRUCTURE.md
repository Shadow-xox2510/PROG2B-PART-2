\# RaceDay API Project Structure



\## Overview



The RaceDay API solution contains the main ASP.NET Core Web API project and a separate automated test project.



\## Main Folders and Files



\- `RaceDayAPI/` contains the API application.

\- `RaceDayAPI/Controllers/` contains controllers for authentication, profiles, events, enrolments and results.

\- `RaceDayAPI.Test/` contains automated tests for the API.

\- `RaceDayAPI.slnx` is the solution file.

\- `.github/workflows/` contains the GitHub Actions continuous integration workflow.

\- `screenshots/` stores evidence for the assignment submission.

\- `README.md` provides project information and setup instructions.



\## Testing and Continuous Integration



The test project uses automated tests to check important application behaviours, including registration, login, session handling, event permissions, enrolment and results.



GitHub Actions is configured to restore dependencies, build the solution and run the tests when changes are pushed to the configured branch.

