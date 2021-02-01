# RoutinizeService
## Backend API for Routinize app. Written in C# with .NET Core, and using T-SQL.

###### Introduction

RoutinizeService contains **RoutinizeCore**, which is the main API that communicates with client.
**RoutinizeCore** makes use of other *services* and *libraries* to separate the logics into smaller manageable sub-services.

###### AssistantLibrary

This library provides the following services:

- Google Recaptcha Verification
- Email Sender
- Google Two Factor Authentication
- Assistant Service: supports cryptography producer and verifier

###### HelperLibrary

This library provides the following services and assets:

- Common-used Constants
- Common-used Enums
- Helpers: static methods to process and produce arbitrary data on demand

###### NotifierLibrary

This library provides services that implement ***Google Firebase Cloud Message*** for the following activities:

- To notify a user upon new collaboration request, mentioning, task assignment, etc...
- To notify all users about app updates and announcements (i.e. promotions, events, scheduled activities...)

###### SignalLibrary

This library provides services that implement ***SignalR*** framework for group and collaborator discussions vis chat feature.

###### MongoLibrary

This library maintains a connection to ***MongoDB*** server for the following services:

- Add logs for the API to facilitate debugging activities
- Add audits and history logs on pending changes in user data that may be reverted
- Add logs and usage statistics for client app to assist debugging
- Add user feedbacks, suggestions, bug reports that help improve the API and mobile app

###### MediaLibrary

This library is developed using a combination of ***CakePHP*** and .NET frameworks, therefore, it will consist of 2 smaller projects.

- CakePHP project: runs in Apache server; provides services to process photos, videos and audios; saves them to local server storage and update data to MySQL server.
- .NET project: uses MySQL connector to access MySQL server; provides services to RoutinizeCore; communicates with the CakePHP project.

