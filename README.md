# FetchingDataTimerFunction

This GitHub repository provides a .NET Core 6 Azure Function App designed for automated data fetching and logging. Leveraging Azure Functions, it periodically (every minute) retrieves data from a public API (https://api.publicapis.org/random?auth=null) and logs the attempt's success or failure in an Azure Table Storage. Additionally, it stores the full payload in Azure Blob Storage. The app includes two GET API endpoints: one for listing all log entries within a specified time frame and another for retrieving specific payloads from the blob storage based on log entry identifiers. This solution demonstrates a cloud-native application pattern, utilizing Azure's serverless computing and storage services for efficient data processing and management.

Function uses Azure exisitng storage and to access it run function locally using this links:
A GET API call to list all logs for the specific time period (from/to)
  - {localhost}/api/logs?from={time}&to={time} | Time example foramt is: 14:00
A GET API call to fetch a payload from blob for the specific log entry
  - {localhost}/api/payload/{logid}
