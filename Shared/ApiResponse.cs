using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Dynamic;

namespace ApiGMPKlik.Shared
{
    // Tambahkan di file ApiResponse.cs
    public static class ApiResponseExtensions
    {
        public static async Task WriteAsJsonAsync(this HttpResponse response, ApiResponse<object> apiResponse)
        {
            response.ContentType = "application/json";
            response.StatusCode = apiResponse.StatusCode;
            await response.WriteAsync(apiResponse.ToJson());
        }
    }
    /// <summary>
    /// Response Types untuk berbagai kebutuhan
    /// </summary>
    public enum ResponseType
    {
        Success = 1,
        Info = 2,
        Warning = 3,
        Error = 4,
        Danger = 5,
        ValidationError = 6,
        Unauthorized = 7,
        Forbidden = 8,
        NotFound = 9
    }

    /// <summary>
    /// Unified API Response dengan support Tree Structure & Pagination
    /// </summary>
    public class ApiResponse<T>
    {
        [JsonProperty("success")]
        public bool IsSuccess { get; set; }

        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ResponseType Type { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;

        [JsonProperty("title")]
        public string? Title { get; set; } // Untuk toast/alert title

        [JsonProperty("data")]
        public T? Data { get; set; }

        [JsonProperty("errors")]
        public List<ErrorDetail>? Errors { get; set; }

        [JsonProperty("warnings")]
        public List<string>? Warnings { get; set; }

        [JsonProperty("metadata")]
        public ResponseMetadata? Metadata { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [JsonProperty("statusCode")]
        public int StatusCode { get; set; }

        [JsonProperty("requestId")]
        public string RequestId { get; set; } = Guid.NewGuid().ToString("N")[..8];

        [JsonProperty("treeInfo")]
        public TreeMetadata? TreeInfo { get; set; } // Untuk response tree structure

        // ==================== SUCCESS METHODS ====================

        public static ApiResponse<T> Success(T data, string message = "Success", string? title = null)
        {
            return new ApiResponse<T>
            {
                IsSuccess = true,
                Type = ResponseType.Success,
                Message = message,
                Title = title ?? "Berhasil",
                Data = data,
                StatusCode = 200,
                Errors = null,
                Warnings = null
            };
        }

        public static ApiResponse<T> Created(T data, string message = "Created successfully")
        {
            return new ApiResponse<T>
            {
                IsSuccess = true,
                Type = ResponseType.Success,
                Message = message,
                Title = "Created",
                Data = data,
                StatusCode = 201
            };
        }

        // ==================== INFO METHODS ====================

        public static ApiResponse<T> Info(string message, T? data = default, string? title = null)
        {
            return new ApiResponse<T>
            {
                IsSuccess = true,
                Type = ResponseType.Info,
                Message = message,
                Title = title ?? "Informasi",
                Data = data,
                StatusCode = 200
            };
        }

        // ==================== WARNING METHODS ====================

        public static ApiResponse<T> Warning(string message, List<string>? warnings = null, T? data = default)
        {
            return new ApiResponse<T>
            {
                IsSuccess = true, // Warning tetap success=true tapi dengan warning messages
                Type = ResponseType.Warning,
                Message = message,
                Title = "Peringatan",
                Data = data,
                Warnings = warnings,
                StatusCode = 200
            };
        }

        // ==================== ERROR METHODS ====================

        public static ApiResponse<T> Error(string message, string error, int statusCode = 400)
        {
            return new ApiResponse<T>
            {
                IsSuccess = false,
                Type = ResponseType.Error,
                Message = message,
                Title = "Error",
                StatusCode = statusCode,
                Errors = new List<ErrorDetail> { new ErrorDetail { Message = error } }
            };
        }

        public static ApiResponse<T> Error(string message, List<ErrorDetail> errors, int statusCode = 400)
        {
            return new ApiResponse<T>
            {
                IsSuccess = false,
                Type = ResponseType.Error,
                Message = message,
                Title = "Error",
                StatusCode = statusCode,
                Errors = errors
            };
        }

        public static ApiResponse<T> Error(string message, List<string> errors, int statusCode = 400)
        {
            var errorDetails = errors.Select(e => new ErrorDetail { Message = e }).ToList();
            return Error(message, errorDetails, statusCode);
        }

        public static ApiResponse<T> ValidationError(List<ErrorDetail> errors, string message = "Validation failed")
        {
            return new ApiResponse<T>
            {
                IsSuccess = false,
                Type = ResponseType.ValidationError,
                Message = message,
                Title = "Validasi Gagal",
                StatusCode = 400,
                Errors = errors
            };
        }

        // ==================== DANGER METHODS (Critical Errors) ====================

        public static ApiResponse<T> Danger(string message, string error, int statusCode = 500)
        {
            return new ApiResponse<T>
            {
                IsSuccess = false,
                Type = ResponseType.Danger,
                Message = message,
                Title = "Kesalahan Sistem",
                StatusCode = statusCode,
                Errors = new List<ErrorDetail> { new ErrorDetail { Message = error, Severity = ErrorSeverity.Critical } }
            };
        }

        public static ApiResponse<T> Danger(string message, Exception ex)
        {
            return new ApiResponse<T>
            {
                IsSuccess = false,
                Type = ResponseType.Danger,
                Message = message,
                Title = "Kesalahan Sistem",
                StatusCode = 500,
                Errors = new List<ErrorDetail>
                {
                    new ErrorDetail
                    {
                        Message = ex.Message,
                        Detail = ex.InnerException?.Message,
                        Severity = ErrorSeverity.Critical
                    }
                }
            };
        }

        // ==================== HTTP STATUS METHODS ====================

        public static ApiResponse<T> NotFound(string resource = "Resource")
        {
            return new ApiResponse<T>
            {
                IsSuccess = false,
                Type = ResponseType.NotFound,
                Message = $"{resource} tidak ditemukan",
                Title = "Not Found",
                StatusCode = 404
            };
        }

        public static ApiResponse<T> Unauthorized(string message = "Unauthorized access")
        {
            return new ApiResponse<T>
            {
                IsSuccess = false,
                Type = ResponseType.Unauthorized,
                Message = message,
                Title = "Akses Ditolak",
                StatusCode = 401
            };
        }

        public static ApiResponse<T> Forbidden(string message = "Forbidden access")
        {
            return new ApiResponse<T>
            {
                IsSuccess = false,
                Type = ResponseType.Forbidden,
                Message = message,
                Title = "Forbidden",
                StatusCode = 403
            };
        }

        // ==================== TREE STRUCTURE SUPPORT ====================

        public static ApiResponse<T> TreeResponse(T data, TreeMetadata treeInfo, string message = "Success")
        {
            return new ApiResponse<T>
            {
                IsSuccess = true,
                Type = ResponseType.Success,
                Message = message,
                Data = data,
                TreeInfo = treeInfo,
                StatusCode = 200
            };
        }

        // ==================== PAGINATION SUPPORT ====================

        public static ApiResponse<T> Paginated(T data, int page, int pageSize, int totalCount, string message = "Success")
        {
            return new ApiResponse<T>
            {
                IsSuccess = true,
                Type = ResponseType.Success,
                Message = message,
                Data = data,
                Metadata = new ResponseMetadata
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                    HasNext = page * pageSize < totalCount,
                    HasPrevious = page > 1
                },
                StatusCode = 200
            };
        }

        // ==================== SERIALIZATION ====================

        public string ToJson()
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new List<JsonConverter> { new StringEnumConverter() },
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.None,
                DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffZ"
            };

            return JsonConvert.SerializeObject(this, settings);
        }

        public static ApiResponse<T> FromJson(string json)
        {
            return JsonConvert.DeserializeObject<ApiResponse<T>>(json)!;
        }
    }

    /// <summary>
    /// Non-generic version untuk response tanpa data
    /// </summary>
    public static class ApiResponse
    {
        public static ApiResponse<object> Success(string message = "Success")
            => ApiResponse<object>.Success(null!, message);

        public static ApiResponse<object> Created(string message = "Created successfully")
            => ApiResponse<object>.Created(null!, message);

        public static ApiResponse<object> Error(string message, string error, int statusCode = 400)
            => ApiResponse<object>.Error(message, error, statusCode);

        public static ApiResponse<object> Error(string message, List<string> errors, int statusCode = 400)
            => ApiResponse<object>.Error(message, errors, statusCode);

        public static ApiResponse<object> Warning(string message, List<string>? warnings = null)
            => ApiResponse<object>.Warning(message, warnings);

        public static ApiResponse<object> Danger(string message, string error)
            => ApiResponse<object>.Danger(message, error);

        public static ApiResponse<object> NotFound(string resource = "Resource")
            => ApiResponse<object>.NotFound(resource);

        public static ApiResponse<object> ValidationError(List<ErrorDetail> errors)
            => ApiResponse<object>.ValidationError(errors);
    }

    // ==================== SUPPORTING CLASSES ====================

    public class ErrorDetail
    {
        [JsonProperty("field")]
        public string? Field { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;

        [JsonProperty("code")]
        public string? Code { get; set; }

        [JsonProperty("detail")]
        public string? Detail { get; set; }

        [JsonProperty("severity")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ErrorSeverity Severity { get; set; } = ErrorSeverity.Error;
    }

    public enum ErrorSeverity
    {
        Low = 1,
        Medium = 2,
        Error = 3,
        High = 3,
        Critical = 4
    }

    public class ResponseMetadata
    {
        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("totalCount")]
        public int TotalCount { get; set; }

        [JsonProperty("totalPages")]
        public int TotalPages { get; set; }

        [JsonProperty("hasNext")]
        public bool HasNext { get; set; }

        [JsonProperty("hasPrevious")]
        public bool HasPrevious { get; set; }

        [JsonProperty("executionTimeMs")]
        public long? ExecutionTimeMs { get; set; }
    }

    public class TreeMetadata
    {
        [JsonProperty("totalNodes")]
        public int TotalNodes { get; set; }

        [JsonProperty("maxDepth")]
        public int MaxDepth { get; set; }

        [JsonProperty("rootId")]
        public string? RootId { get; set; }

        [JsonProperty("levels")]
        public List<LevelInfo>? Levels { get; set; }

        [JsonProperty("isComplete")]
        public bool IsComplete { get; set; } // Apakah tree lengkap atau terpotong
    }

    public class LevelInfo
    {
        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }
    }

    /// <summary>
    /// DTO untuk Tree Node (bisa digunakan untuk ReferralTree atau struktur hierarki lain)
    /// </summary>
    public class TreeNodeDto<T>
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("data")]
        public T? Data { get; set; }

        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("parentId")]
        public string? ParentId { get; set; }

        [JsonProperty("children")]
        public List<TreeNodeDto<T>> Children { get; set; } = new List<TreeNodeDto<T>>();

        [JsonProperty("hasChildren")]
        public bool HasChildren => Children.Any();

        [JsonProperty("metadata")]
        public Dictionary<string, object>? Metadata { get; set; }
    }
}