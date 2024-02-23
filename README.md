# FetchingDataTimerFunction

This GitHub repository provides a .NET Core 6 Azure Function App designed for automated data fetching and logging. Leveraging Azure Functions, it periodically (every minute) retrieves data from a public API (https://api.publicapis.org/random?auth=null) and logs the attempt's success or failure in Azure Table Storage. Additionally, it stores the full payload in Azure Blob Storage. The app includes two GET API endpoints: one for listing all log entries within a specified time frame and another for retrieving specific payloads from the blob storage based on log entry identifiers. This solution demonstrates a cloud-native application pattern, utilizing Azure's serverless computing and storage services for efficient data processing and management.

The function uses Azure's existing storage and to access it runs the function locally using these links:

A GET API call to list all logs for the specific time period (from/to)
  - {localhost}/api/logs?from={time}&to={time} | Time example format is: 2024-02-22T12:39:19.163Z or 12:39

A GET API call to fetch a payload from blob for the specific log entry
  - {localhost}/api/payload/{logid}
