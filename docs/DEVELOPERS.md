# Developers

This document is a more techinal in-depth overview of the EventXperience repo. This document will go over useful tips and advice when developing EventXperience.

For Architecture, refer to [ARCHITECTURE.md](\ARCHITECTURE.md)
For Configuration, refer to [CONFIGURATION.md](\CONFIGURATION.md)
For Setup, refer to [SETUP.md](\SETUP.md)
For Testing, refer to [TESTING.md](\TESTING.md)
For Deplyoment, refer to [DEPLOYMENT.md](\DEPLOYMENT.md)
For APIs, refer to [API.md](\API.md)

## Languages and Frameworks

EventXperience is developed using [TypeScript](https://www.typescriptlang.org/) for the frontend and [C#](https://dotnet.microsoft.com/en-us/languages/csharp) for the backend. Although [.NET Core](https://dotnet.microsoft.com/en-us/) uses many languages, it is mainly known for C#. We leverage C# for its built in support for `asynchronous` operations, alongside some other useful features provided by [ASP.NET Core](https://dotnet.microsoft.com/en-us/apps/aspnet). `ASP.NET Core` abstracts many features that we would have to make ourselves such as HTTP request and validation, middleware and database drivers.

## Pull/Merge Request

All pull request generally must be peer-reviewed by at least one other core developer. It should following this format:

- `Summary`: fixes, problems addressed, new feature
- `Issues`: what issue or bug did your PR address
- `Test`: how to verify it works
- `Extra notes`: anything else future developers need to know regarding your change

## Git Strategy

EventXperience will use the [GitLab Flow](https://about.gitlab.com/topics/version-control/what-is-gitlab-flow/) strategy throughout the development.

## Basic Architecture

EventXperience follows a basic client-server architecture. Currently at this time, the client is a web browser while the backend is a modular monolothic design. The backend is designed to be a MVC (Model-View-Controller) to improve maintainability.

Future work after initial completetion of the prototype would be:

- Introduce SSR and SEO via [Angular SSR](https://angular.dev/guide/ssr)
- Microservice design via [NATS](https://nats.io/)
- Mobile App via [React Native](https://reactnative.dev/)
