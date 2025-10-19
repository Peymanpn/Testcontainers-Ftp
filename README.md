# FTP Server Integration Test with TestContainer

This project demonstrates FTP server integration testing using TestContainers with support for passive ports randomly assigned per container. It provides a simple web API for testing FTP connections and includes comprehensive integration tests that spin up isolated FTP server containers.

## Features

- Simple ASP.NET Core web API for FTP connection testing
- Integration tests using TestContainers with pure-ftpd Docker image
- Support for FTP protocol (non-SSL)
- Automatic port management for passive FTP mode
- Clean test isolation with containerized FTP servers

## Prerequisites

- .NET 10.0 SDK
- Docker (for running TestContainers)


## Usage
 
### API Endpoints

- `POST /ftp/test` - Test FTP connection

#### Testing FTP Connection

Send a POST request to `/ftp/test` with the following JSON payload:

```json
{
  "host": "ftp.example.com",
  "port": 21,
  "username": "your-username",
  "password": "your-password",
  "useSsl": false,
  "basePath": "/",
  "timeoutSeconds": 30
}
```

### Running Tests

To run the integration tests:

```bash
cd TestProject
dotnet test
```

The tests will automatically start FTP server containers using TestContainers, run the tests, and clean up the containers afterward.

## Project Structure

- `FtpPlayground/` - Main ASP.NET Core web API project
  - `Program.cs` - Application entry point and API endpoints
  - `FtpService.cs` - FTP service implementation
  - `IFtpService.cs` - FTP service interface
  - `FluentFtpClientAdapter.cs` - FTP client adapter using FluentFTP
  - `FtpConnectionDetails.cs` - Model for FTP connection details
- `TestProject/` - Integration tests
  - `FtpTests.cs` - FTP connection tests
  - `WebAppFixture.cs` - Test fixture with TestContainers setup
  - `BaseIntegrationTest.cs` - Base test class

## Dependencies

- **FluentFTP** - FTP client library
- **TestContainers** - Container management for testing
- **xUnit** - Testing framework

## Limitations

- Currently supports only plain FTP protocol
- FTPS (FTP over SSL/TLS) is not yet implemented
- Designed for demonstration and testing purposes

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Run tests to ensure everything works
6. Submit a pull request

## License

This project is provided as-is for demonstration purposes.