using ServiceNowCM.Application.DTOs;
using ServiceNowCM.Application.Interfaces;
using ServiceNowCM.Domain.Entities;
using ServiceNowCM.Domain.Enums;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ServiceNowCM.Infrastructure.ServiceNow
{
    public class ServiceNowClient : IServiceNowClient
    {
        private readonly HttpClient _httpClient;

        public ServiceNowClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ConnectionTestResult> TestConnectionAsync(
            ServiceNowConnection connection,
            string secret,
            CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                if (connection.AuthenticationType == AuthenticationType.Basic)
                {
                    return await TestBasicAuthenticationAsync(
                        connection,
                        secret,
                        stopwatch,
                        cancellationToken);
                }

                return new ConnectionTestResult
                {
                    Success = false,
                    Message = "OAuth2 connection testing is not implemented yet.",
                    DurationMilliseconds = stopwatch.ElapsedMilliseconds
                };
            }
            catch (OperationCanceledException)
            {
                return new ConnectionTestResult
                {
                    Success = false,
                    Message = "ServiceNow connection test timed out or was cancelled.",
                    DurationMilliseconds = stopwatch.ElapsedMilliseconds
                };
            }
            catch (HttpRequestException ex)
            {
                return new ConnectionTestResult
                {
                    Success = false,
                    Message = $"Unable to connect to ServiceNow: {ex.Message}",
                    DurationMilliseconds = stopwatch.ElapsedMilliseconds
                };
            }
        }

        private async Task<ConnectionTestResult> TestBasicAuthenticationAsync(
    ServiceNowConnection connection,
    string password,
    Stopwatch stopwatch,
    CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(connection.Username))
            {
                return new ConnectionTestResult
                {
                    Success = false,
                    Message = "Username is required for Basic authentication.",
                    DurationMilliseconds = stopwatch.ElapsedMilliseconds
                };
            }

            var credentials =
                $"{connection.Username}:{password}";

            var encodedCredentials =
                Convert.ToBase64String(
                    Encoding.UTF8.GetBytes(credentials));

            using var request =
                new HttpRequestMessage(
                    HttpMethod.Get,
                    $"{connection.InstanceUrl.TrimEnd('/')}/api/now/table/sys_user?sysparm_limit=1");

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Basic",
                    encodedCredentials);

            request.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue(
                    "application/json"));

            using var response =
                await _httpClient.SendAsync(
                    request,
                    cancellationToken);

            stopwatch.Stop();

            if (response.IsSuccessStatusCode)
            {
                return new ConnectionTestResult
                {
                    Success = true,
                    Message = "ServiceNow connection successful.",
                    StatusCode = (int)response.StatusCode,
                    DurationMilliseconds = stopwatch.ElapsedMilliseconds
                };
            }

            return new ConnectionTestResult
            {
                Success = false,
                Message = $"ServiceNow returned HTTP {(int)response.StatusCode} ({response.StatusCode}).",
                StatusCode = (int)response.StatusCode,
                DurationMilliseconds = stopwatch.ElapsedMilliseconds
            };
        }

        public async Task<IReadOnlyList<ServiceNowTableInfo>> GetTablesAsync(
    ServiceNowConnection connection,
    string secret,
    CancellationToken cancellationToken = default)
        {
            var requestUrl =
                $"{connection.InstanceUrl.TrimEnd('/')}/api/now/table/sys_db_object" +
                "?sysparm_fields=name,label" +
                "&sysparm_query=active=true" +
                "&sysparm_limit=1000";

            using var request =
                new HttpRequestMessage(
                    HttpMethod.Get,
                    requestUrl);

            ApplyAuthentication(
                request,
                connection,
                secret);

            request.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue(
                    "application/json"));

            using var response =
                await _httpClient.SendAsync(
                    request,
                    cancellationToken);

            response.EnsureSuccessStatusCode();

            var json =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            using var document =
                JsonDocument.Parse(json);

            var result =
                document.RootElement.GetProperty("result");

            var tables =
                new List<ServiceNowTableInfo>();

            foreach (var item in result.EnumerateArray())
            {
                tables.Add(
                    new ServiceNowTableInfo
                    {
                        Name =
                            item.GetProperty("name").GetString()
                            ?? string.Empty,

                        Label =
                            item.GetProperty("label").GetString()
                            ?? string.Empty
                    });
            }

            return tables;
        }

        private static void ApplyAuthentication(
            HttpRequestMessage request,
            ServiceNowConnection connection,
            string secret)
        {
            if (connection.AuthenticationType == AuthenticationType.Basic)
            {
                if (string.IsNullOrWhiteSpace(connection.Username))
                {
                    throw new InvalidOperationException(
                        "Username is required for Basic authentication.");
                }

                var credentials = $"{connection.Username}:{secret}";

                var encodedCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));

                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", encodedCredentials);

                return;
            }

            throw new NotSupportedException(
                $"Authentication type '{connection.AuthenticationType}' is not supported yet.");
        }

        public async Task<IReadOnlyList<ServiceNowFieldInfo>> GetFieldsAsync(
            ServiceNowConnection connection,
            string secret,
            string tableName,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentException(
                    "Table name is required.",
                    nameof(tableName));
            }

            var query =
                $"name={Uri.EscapeDataString(tableName)}";

            var requestUrl =
                $"{connection.InstanceUrl.TrimEnd('/')}/api/now/table/sys_dictionary" +
                "?sysparm_fields=element,column_label,internal_type,reference" +
                $"&sysparm_query={query}" +
                "&sysparm_limit=2000";

            using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

            ApplyAuthentication(
                request,
                connection,
                secret);

            request.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue(
                    "application/json"));

            using var response =
                await _httpClient.SendAsync(request, cancellationToken);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            using var document = JsonDocument.Parse(json);

            var result = document.RootElement.GetProperty("result");

            var fields = new List<ServiceNowFieldInfo>();

            foreach (var item in result.EnumerateArray())
            {
                string? referenceTable = null;

                if (item.TryGetProperty(
                    "reference",
                    out var reference))
                {
                    referenceTable =
                        GetElementValue(reference);
                }

                string? dataType = null;

                if (item.TryGetProperty(
                    "internal_type",
                    out var internalType))
                {
                    dataType =
                        GetElementValue(internalType);
                }

                fields.Add(
                    new ServiceNowFieldInfo
                    {
                        Name =
                            item.GetProperty("element").GetString()
                            ?? string.Empty,

                        Label =
                            item.GetProperty("column_label").GetString()
                            ?? string.Empty,

                        DataType =
                            dataType ?? string.Empty,

                        IsReference =
                            !string.IsNullOrWhiteSpace(
                                referenceTable),

                        ReferenceTable =
                            referenceTable
                    });
            }

            return fields;
        }

        public async Task<ServiceNowPageResult> FetchPageAsync(
            ServiceNowConnection connection,
            string secret,
            ServiceNowQueryOptions options,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(options.TableName))
            {
                throw new ArgumentException(
                    "ServiceNow table name is required.",
                    nameof(options));
            }

            if (options.PageSize <= 0 || options.PageSize > 1000)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(options.PageSize),
                    "Page size must be between 1 and 1000.");
            }

            if (options.Offset < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(options.Offset),
                    "Offset cannot be negative.");
            }

            var parameters = new List<string>();

            if (options.Fields.Count > 0)
            {
                var fields = string.Join(",", options.Fields);

                parameters.Add(
                    $"sysparm_fields={Uri.EscapeDataString(fields)}");
            }

            if (!string.IsNullOrWhiteSpace(options.Query))
            {
                parameters.Add(
                    $"sysparm_query={Uri.EscapeDataString(options.Query)}");
            }

            parameters.Add(
                $"sysparm_display_value={options.DisplayValues.ToString().ToLowerInvariant()}");

            parameters.Add(
                $"sysparm_exclude_reference_link={options.ExcludeReferenceLinks.ToString().ToLowerInvariant()}");

            parameters.Add(
                $"sysparm_limit={options.PageSize}");

            parameters.Add(
                $"sysparm_offset={options.Offset}");

            var requestUrl =
                $"{connection.InstanceUrl.TrimEnd('/')}" +
                $"/api/now/table/{Uri.EscapeDataString(options.TableName)}" +
                $"?{string.Join("&", parameters)}";

            using var request =
                new HttpRequestMessage(
                    HttpMethod.Get,
                    requestUrl);

            ApplyAuthentication(
                request,
                connection,
                secret);

            request.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue(
                    "application/json"));

            using var response =
                await _httpClient.SendAsync(
                    request,
                    cancellationToken);

            response.EnsureSuccessStatusCode();

            var json =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            using var document =
                JsonDocument.Parse(json);

            var records =
                new List<Dictionary<string, JsonElement>>();

            if (document.RootElement.TryGetProperty(
                "result",
                out var resultElement))
            {
                foreach (var item in resultElement.EnumerateArray())
                {
                    var record =
                        new Dictionary<string, JsonElement>();

                    foreach (var property in item.EnumerateObject())
                    {
                        record[property.Name] =
                            property.Value.Clone();
                    }

                    records.Add(record);
                }
            }

            return new ServiceNowPageResult
            {
                Records = records,
                Offset = options.Offset,
                PageSize = options.PageSize,
                ReturnedCount = records.Count,

                // If ServiceNow returned a full page,
                // another page MAY exist.
                HasMore = records.Count == options.PageSize
            };
        }

        private static string? GetElementValue(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.String)
            {
                return element.GetString();
            }

            if (element.ValueKind == JsonValueKind.Object)
            {
                if (element.TryGetProperty("value", out var value))
                {
                    return value.GetString();
                }

                if (element.TryGetProperty("display_value", out var displayValue))
                {
                    return displayValue.GetString();
                }
            }

            return null;
        }


    }

    
    } 