\# RaceDay – PROG2B Part 3



RaceDay is a web-based race event management application developed using ASP.NET Core MVC, ASP.NET Core Web API, Entity Framework Core, SQL Server, Docker, and Azure Blob Storage.



The application allows organisers to manage race events, monitor participant enrolments, and capture race results. Participants can register for events, manage their profiles, and view their race achievements.



\## Technologies Used



\- ASP.NET Core 10

\- ASP.NET Core MVC

\- ASP.NET Core Web API

\- C#

\- Entity Framework Core

\- Microsoft SQL Server 2022

\- Azure Blob Storage

\- Docker and Docker Compose

\- GitHub Actions

\- HTML, CSS, JavaScript, and Bootstrap



\## Application Features



\### Participant Features



\- Register and log in

\- View available race events

\- Enrol in race events

\- View registered events

\- View published race results

\- Update personal profile information

\- Upload a profile picture



\### Organiser Features



\- Log in to the organiser dashboard

\- Create race events

\- Update event information

\- Manage event details

\- Upload event banners

\- View registered participants

\- Capture participant finishing times and positions

\- Publish race results



\## Project Structure



The solution contains three main projects.



\### RaceDayAPI



The ASP.NET Core Web API handles:



\- Application business logic

\- Database operations

\- Event management

\- Participant enrolments

\- Profile management

\- Race results

\- Azure Blob Storage integration



\### RaceDayMVC



The ASP.NET Core MVC application provides the user interface.



It contains:



\- Controllers

\- Models

\- Razor Views

\- CSS and JavaScript

\- Participant and organiser dashboards



The MVC application communicates with RaceDayAPI using HTTP requests.



\### RaceDayAPI.Test



This project contains automated tests used to verify application behaviour.



\## Application Architecture



The application uses a multi-project architecture.



1\. Users interact with the RaceDay MVC website.

2\. The MVC application sends HTTP requests to RaceDayAPI.

3\. RaceDayAPI processes requests and accesses SQL Server.

4\. Azure Blob Storage stores uploaded profile pictures and event banners.

5\. The API returns responses to the MVC application.



\## Docker Containerisation



The RaceDay application is containerised using Docker.



Docker Compose manages three services:



| Service | Description | Port |

|---|---|---|

| racedaymvc | ASP.NET Core MVC frontend | 5000 |

| racedayapi | ASP.NET Core REST API | 7079 |

| sqlserver | SQL Server 2022 database | 1433 |



The services communicate through a shared Docker network.



SQL Server data is stored in a persistent Docker volume.



\## Prerequisites



Before running the application, install:



\- Docker Desktop

\- Git



An Azure Storage account and valid connection string are required for Azure Blob Storage functionality.



Docker Desktop must be running before executing the Docker Compose commands.



\## Running RaceDay Using Docker



\### Step 1: Clone the Repository



```bash

git clone --branch part-3 https://github.com/Shadow-xox2510/PROG2B-PART-2.git

```



Navigate to the project directory:



```bash

cd PROG2B-PART-2

```



\### Step 2: Configure Environment Variables



Create a `.env` file in the root directory, next to `docker-compose.yml`.



Add the following environment variables:



```dotenv

MSSQL\_SA\_PASSWORD=YOUR\_STRONG\_SQL\_PASSWORD

AZURE\_BLOB\_CONNECTION\_STRING=YOUR\_AZURE\_BLOB\_CONNECTION\_STRING

```



Replace the example values with valid credentials.



For an existing SQL Server database volume, use its current SA password.



\*\*Security:\*\* Never commit the `.env` file or expose database passwords and Azure storage credentials in GitHub.



\### Step 3: Validate Docker Compose



Run:



```bash

docker compose config --quiet

```



This checks whether the Docker Compose configuration is valid.



\### Step 4: Build and Start the Containers



Run:



```bash

docker compose up -d --build

```



Docker will build the MVC and API images and start the SQL Server container.



The API and MVC services are configured to wait for their dependencies as defined in Docker Compose.



\### Step 5: Verify the Containers



Run:



```bash

docker compose ps

```



The following containers should be running:



\- raceday-mvc

\- raceday-api

\- raceday-sql



The SQL Server container should report a healthy status.



\### Step 6: Access the Application



Open the following URLs:



\*\*RaceDay MVC Website\*\*



http://localhost:5000



\*\*RaceDay API\*\*



http://localhost:7079



Use the MVC website to register or log in and access the available features.



\## Database Configuration



RaceDay uses Microsoft SQL Server 2022.



The database name is:



`RaceDayDB2`



Entity Framework Core is used for database access.



Database migration files are stored in:



`RaceDayAPI/Migrations`



A new database must have the application's schema applied before database-dependent features can be used.



The Docker SQL Server volume preserves database data between normal container restarts.



\## Azure Blob Storage Integration



Azure Blob Storage is used to store uploaded images.



The application uses two containers:



\- `profilepictures` – stores participant profile images

\- `eventbanners` – stores race event banner images



The Azure connection string is supplied through an environment variable.



The application accesses Azure Blob Storage through the API.



\## GitHub Actions CI/CD



The project uses GitHub Actions for continuous integration.



The workflow is located at:



`.github/workflows/part3-ci.yml`



The workflow:



1\. Checks out the repository.

2\. Installs the .NET 10 SDK.

3\. Restores project dependencies.

4\. Builds RaceDayAPI.

5\. Builds RaceDayMVC.

6\. Runs automated tests.



The workflow runs when changes are pushed to the configured Git branches.



\## Running Automated Tests



To run tests locally using the .NET SDK, execute:



```bash

dotnet test RaceDayAPI.Test/RaceDayAPI.Test.csproj --configuration Release

```



\## Useful Docker Commands



Check running containers:



```bash

docker compose ps

```



View API logs:



```bash

docker compose logs --tail 50 racedayapi

```



View MVC logs:



```bash

docker compose logs --tail 50 racedaymvc

```



Stop the application:



```bash

docker compose down

```



This stops the containers without deleting the named SQL Server data volume.



\## Demonstration Video



The unlisted YouTube demonstration covers:



\- The fully running RaceDay application

\- Participant registration and login

\- Organiser event management

\- Participant enrolments

\- Race result capture

\- MVC project structure

\- Azure Blob Storage integration

\- Docker containerisation

\- Running the application through Docker Compose



\## GitHub Repository



Repository:



https://github.com/Shadow-xox2510/PROG2B-PART-2



Part 3 branch:



https://github.com/Shadow-xox2510/PROG2B-PART-2/tree/part-3



\## Conclusion



RaceDay demonstrates the integration of ASP.NET Core MVC, REST APIs, SQL Server, Azure Blob Storage, and Docker.



The applications supports race event management, participant enrolments, profile management, and race result recording.



Docker Compose provides a consistent environment for running the application, while GitHub Actions supports automated builds and testing.



\---



\*\*Project:\*\* PROG2B – Part 3



\*\*Application:\*\* RaceDay

