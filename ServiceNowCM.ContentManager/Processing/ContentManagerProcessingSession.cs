using ServiceNowCM.Application.DTOs;
using ServiceNowCM.Application.Interfaces;
using ServiceNowCM.Domain.Entities;
using ServiceNowCM.Domain.Enums;
using System.Diagnostics;
using System.Text.Json;
using TRIM.SDK;

namespace ServiceNowCM.ContentManager.Processing
{
    internal sealed class ContentManagerProcessingSession
        : IContentManagerProcessingSession
    {
        // -------------------------------------------------------------
        // IMPORTANT
        //
        // We DO NOT store:
        //
        // Database
        // RecordType
        // FieldDefinition
        //
        // TRIM SDK Database objects are thread-affine.
        //
        // The ASP.NET request may continue on another thread after an
        // await, so keeping those objects between batches is unsafe.
        //
        // We only store normal .NET configuration data here.
        // -------------------------------------------------------------

        private readonly ContentManagerConnection _connection;
        private readonly string? _password;
        private readonly long _recordTypeUri;
        private readonly IReadOnlyList<IntegrationField> _mappings;

        public ContentManagerProcessingSession(
            ContentManagerConnection connection,
            string? password,
            long recordTypeUri,
            IEnumerable<IntegrationField> mappings)
        {
            _connection =
                connection ??
                throw new ArgumentNullException(nameof(connection));

            if (recordTypeUri <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(recordTypeUri),
                    "Content Manager Record Type URI must be greater than zero.");
            }

            _password = password;

            _recordTypeUri = recordTypeUri;

            _mappings =
                mappings
                    .OrderBy(x => x.DisplayOrder)
                    .ToList();
        }

        public Task<IReadOnlyList<ContentManagerRecordCreateResult>>
            ProcessRecordsAsync(
                IEnumerable<IReadOnlyDictionary<string, JsonElement>>
                    sourceRecords,
                CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var results =
                new List<ContentManagerRecordCreateResult>();

            // ---------------------------------------------------------
            // CRITICAL THREAD-SAFETY RULE
            //
            // Create the CM Database HERE.
            //
            // Everything below this point is synchronous.
            // There is NO await while the Database is being used.
            //
            // Therefore:
            //
            // Database created on Thread X
            //      ↓
            // RecordType loaded on Thread X
            //      ↓
            // Fields loaded on Thread X
            //      ↓
            // Records created on Thread X
            //
            // This avoids:
            //
            // "This database was created by another thread..."
            // ---------------------------------------------------------

            Database? db = null;

            try
            {
                // -----------------------------------------------------
                // 1. Create Database on current thread
                // -----------------------------------------------------
                db =
                    CreateDatabase(
                        _connection,
                        _password);

                // -----------------------------------------------------
                // 2. Connect on same thread
                // -----------------------------------------------------
                db.Connect();

                // -----------------------------------------------------
                // 3. Load Record Type on same thread
                // -----------------------------------------------------
                var recordType =
                    new RecordType(
                        db,
                        _recordTypeUri);

                // -----------------------------------------------------
                // 4. Resolve Additional Fields ONCE for this batch
                // -----------------------------------------------------
                var additionalFields =
                    new Dictionary<long, FieldDefinition>();

                foreach (var mapping in _mappings)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (!string.Equals(
                            mapping.TargetFieldType,
                            "AdditionalField",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (!mapping.TargetFieldUri.HasValue)
                    {
                        throw new InvalidOperationException(
                            $"Additional Field URI is missing for " +
                            $"'{mapping.TargetFieldName}'.");
                    }

                    var uri =
                        mapping.TargetFieldUri.Value;

                    if (!additionalFields.ContainsKey(uri))
                    {
                        additionalFields.Add(
                            uri,
                            new FieldDefinition(
                                db,
                                uri));
                    }
                }

                // -----------------------------------------------------
                // 5. Process every source record
                // -----------------------------------------------------
                foreach (var sourceRecord in sourceRecords)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    string? sourceSysId = null;

                    if (sourceRecord.TryGetValue(
                            "sys_id",
                            out var sysIdElement))
                    {
                        sourceSysId =
                            GetJsonValueAsString(
                                sysIdElement);
                    }

                    try
                    {
                        // -------------------------------------------------
                        // Create CM Record on SAME thread as Database
                        // -------------------------------------------------
                        var record =
                            new Record(
                                db,
                                recordType);

                        // -------------------------------------------------
                        // Apply configured mappings
                        // -------------------------------------------------
                        foreach (var mapping in _mappings)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            if (string.IsNullOrWhiteSpace(
                                    mapping.TargetFieldName) ||
                                string.IsNullOrWhiteSpace(
                                    mapping.TargetFieldType))
                            {
                                continue;
                            }

                            if (!sourceRecord.TryGetValue(
                                    mapping.SourceFieldName,
                                    out var sourceValue))
                            {
                                continue;
                            }

                            var value =
                                GetJsonValueAsString(
                                    sourceValue);

                            if (string.IsNullOrWhiteSpace(value))
                            {
                                continue;
                            }

                            // ---------------------------------------------
                            // Built-in Content Manager property
                            // ---------------------------------------------
                            if (string.Equals(
                                    mapping.TargetFieldType,
                                    "Property",
                                    StringComparison.OrdinalIgnoreCase))
                            {
                                ApplyBuiltInProperty(
                                    record,
                                    mapping,
                                    value);

                                continue;
                            }

                            // ---------------------------------------------
                            // Content Manager Additional Field
                            // ---------------------------------------------
                            if (string.Equals(
                                    mapping.TargetFieldType,
                                    "AdditionalField",
                                    StringComparison.OrdinalIgnoreCase))
                            {
                                ApplyAdditionalField(
                                    record,
                                    mapping,
                                    value,
                                    additionalFields);

                                continue;
                            }

                            throw new InvalidOperationException(
                                $"Unsupported Content Manager target field type " +
                                $"'{mapping.TargetFieldType}'.");
                        }

                        // -------------------------------------------------
                        // Save CM record
                        // -------------------------------------------------
                        record.Save();

                        results.Add(
                            new ContentManagerRecordCreateResult
                            {
                                SourceSysId = sourceSysId,
                                Success = true,
                                RecordUri = record.Uri,
                                RecordNumber = record.Number,
                                Message =
                                    "Content Manager record created successfully."
                            });
                    }
                    catch (Exception ex)
                    {
                        // -------------------------------------------------
                        // One bad record must NOT terminate whole batch.
                        // -------------------------------------------------
                        Debug.WriteLine(
                            $"CM SDK ERROR | " +
                            $"SourceSysId: {sourceSysId} | " +
                            $"ExceptionType: {ex.GetType().FullName} | " +
                            $"Message: {ex.Message} | " +
                            $"Details: {ex}");

                        results.Add(
                            new ContentManagerRecordCreateResult
                            {
                                SourceSysId = sourceSysId,
                                Success = false,
                                Message = ex.Message
                            });
                    }
                }
            }
            catch (Exception ex)
            {
                // -----------------------------------------------------
                // Batch-level failure.
                //
                // Examples:
                // - CM connection failed
                // - RecordType could not be loaded
                // - Additional Field definition could not be loaded
                //
                // We rethrow because this means the entire batch cannot
                // safely be processed.
                // -----------------------------------------------------
                Debug.WriteLine(
                    $"CM BATCH ERROR | " +
                    $"ExceptionType: {ex.GetType().FullName} | " +
                    $"Message: {ex.Message} | " +
                    $"Details: {ex}");

                throw;
            }

            return Task.FromResult<
                IReadOnlyList<ContentManagerRecordCreateResult>>(
                    results);
        }

        private static void ApplyBuiltInProperty(
            Record record,
            IntegrationField mapping,
            string value)
        {
            if (string.Equals(
                    mapping.TargetFieldName,
                    "Title",
                    StringComparison.OrdinalIgnoreCase))
            {
                record.Title = value;

                return;
            }

            throw new InvalidOperationException(
                $"Unsupported Content Manager built-in property " +
                $"'{mapping.TargetFieldName}'.");
        }

        private static void ApplyAdditionalField(
            Record record,
            IntegrationField mapping,
            string value,
            IReadOnlyDictionary<long, FieldDefinition> additionalFields)
        {
            if (!mapping.TargetFieldUri.HasValue)
            {
                throw new InvalidOperationException(
                    $"Content Manager Additional Field URI is missing " +
                    $"for '{mapping.TargetFieldName}'.");
            }

            var uri =
                mapping.TargetFieldUri.Value;

            if (!additionalFields.TryGetValue(
                    uri,
                    out var fieldDefinition))
            {
                throw new InvalidOperationException(
                    $"Content Manager Additional Field URI '{uri}' " +
                    $"was not loaded for this batch.");
            }

            var propertyOrFieldValue =
                new PropertyOrFieldValue(
                    fieldDefinition);

            propertyOrFieldValue.SetValueFromString(
                value);

            var userFieldValue =
                propertyOrFieldValue.GetCurrentValue();

            record.SetFieldValue(
                fieldDefinition,
                userFieldValue);
        }

        private static string? GetJsonValueAsString(
            JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String =>
                    element.GetString(),

                JsonValueKind.Number =>
                    element.ToString(),

                JsonValueKind.True =>
                    "true",

                JsonValueKind.False =>
                    "false",

                JsonValueKind.Null =>
                    null,

                JsonValueKind.Undefined =>
                    null,

                _ =>
                    element.ToString()
            };
        }

        private static Database CreateDatabase(
            ContentManagerConnection connection,
            string? password)
        {
            var db =
                new Database
                {
                    WorkgroupServerName =
                        connection.WorkgroupServerName,

                    WorkgroupServerPort =
                        connection.WorkgroupServerPort,

                    Id =
                        connection.DatasetId
                };

            if (connection.AuthenticationType ==
                ContentManagerAuthenticationType.IntegratedWindows)
            {
                db.AuthenticationMethod =
                    ClientAuthenticationMechanism.IntegratedWindows;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(
                        connection.Username))
                {
                    throw new InvalidOperationException(
                        "Username is required for explicit authentication.");
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                    throw new InvalidOperationException(
                        "Password could not be retrieved.");
                }

                db.SetAuthenticationCredentials(
                    connection.Username,
                    password);
            }

            return db;
        }
    }
}