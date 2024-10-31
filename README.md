# Student Attendance Management System (SAMS) - .NET Web API Project
This is a part of Capstone Project, the goal of this project is to create a Web API Server which is responsible to manage resources, handle requests and execute business logic.

## Contributer
* Le Dang Khoa  - <a href="https://github.com/khoaLe12">GitHub</a>
* Tran Thai Quoc - <a href="https://github.com/Toannd832">GitHub</a>

## Tech Stack
* Framework: ASP.NET Core API, EF Core
* Database: MS SQL Server
* Deploy: Docker, Google Cloud Platform
* Others: SMTP, Hangfire, OAuth2, Firebase, Websocket, Google Cloud Vision API

## Description
The server plays a crucial role in managing tasks, interacting with other platforms, and distributing work. In this system, the data such as fingerprint templates are stored in a centralized database through the server, enabling any IoT device to download fingerprint data collected by other modules for attendance purposes. Other critical data, such as calendar events, schedules, class information, and attendance reports, are also stored in the server for retrieval and processing. Below are two examples of the server's responsibilities:

1. Fingerprint Collection: To collect a student's fingerprint, we use a web client to send a request to the server to initiate the process. The server then triggers the fingerprint scanning module. As each fingerprint is collected, the module uploads it to the server, ensuring successful uploads and tracking the progress. Once the process is complete, the server notifies the client.

2. Data Preparation for Fingerprint Attendance: To use a module for fingerprint-based attendance, the user must initiate a data preparation process. Users can view their available modules and connection statuses through the web client. The server monitors module connections, determining whether they are online or offline. Any change in a module's connection status is communicated to the client. The user can select one from the list of online modules and request the server to begin preparation. The server then creates a session to track the progress and results of the preparation and sends a command to the module to perform the required action. During this process, the server continuously updates the completion percentage and sends it to the client for display. Upon completion, the system provides feedback on the success or failure of the preparation and reports how many fingerprint templates were successfully downloaded on the module side.

## Diagram
### Context Diagram
![File Structure](https://github.com/khoaLe12/Public-Image/blob/main/Context_Diagram.png)


### System Overview
![File Structure](https://github.com/khoaLe12/Public-Image/blob/main/System_Overview.png)



### System Architecture
![File Structure](https://github.com/khoaLe12/Public-Image/blob/main/System_Architecture.png)

