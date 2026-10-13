\# RaceDay Part 2 – API



\## Project Overview



RaceDay is an API project developed as part of the PROG2B Part 2 assignment. It provides a backend structure for managing race-day resources through API endpoints.



\## Technologies Used



\* C# and ASP.NET Core Web API

\* .NET 10

\* Entity Framework Core

\* Microsoft SQL Server

\* Swagger / OpenAPI

\* GitHub and GitHub Actions



\## Project Structure



\* `RaceDayAPI/` – Main API application.

\* `RaceDayAPI.Test/` – Unit test project.

\* `RaceDayAPI.slnx` – Solution file.

\* `.github/workflows/` – GitHub Actions CI workflow.

\* `screenshots/` – Submission evidence, including the successful CI build.



\## Prerequisites



Before running the project, install:



\* .NET 10 SDK

\* Visual Studio or another compatible C# IDE

\* SQL Server, if required by the configured database connection



\## Setup Instructions



1\. Clone or download this repository.

2\. Open `RaceDayAPI.slnx` in a compatible IDE.

3\. Check the database connection settings in `RaceDayAPI/appsettings.json`.

4\. Ensure the required database is available and configured.

5\. Restore the project dependencies.

6\. Build the solution.

7\. Run the API project.



\## Running the Project



From a terminal in the repository folder, run:



```bash

dotnet restore RaceDayAPI.slnx

dotnet build RaceDayAPI.slnx

dotnet run --project RaceDayAPI/RaceDayAPI.csproj

```



Use the local URL shown in the terminal to access the API.



\## Swagger API Documentation



When the application is running and Swagger is enabled, open the Swagger URL configured for the application to view and test the available API endpoints.



\## Authentication and Role-Based Access



The API is intended to support registration, login, and role-based access control. Use the implemented authentication endpoints and test the permitted and restricted operations.



\## Testing



Run the automated tests with:



```bash

dotnet test RaceDayAPI.slnx

```



\## Continuous Integration



The repository uses GitHub Actions to restore dependencies, build the solution, and run the automated tests. The workflow status and results can be viewed under the repository's \*\*Actions\*\* tab.



\## Submission Evidence



\* Successful GitHub Actions build screenshot: `screenshots/part2-ci-green-check.png`

\* Video presentation: \*\*Add your video link here\*\*



\## Repository



GitHub: https://github.com/Shadow-xox2510/PROG2B-PART-2



