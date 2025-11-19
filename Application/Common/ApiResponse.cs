using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common
{
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }         /// Indicates if the API call was successful.
        public T Data { get; set; }                /// The data payload (e.g., an AdminDto, a List<VendorDto>).
        public string Message { get; set; }       /// A simple message for success or a general error.
        public List<string> Errors { get; set; }  /// A list of validation errors (e.g., "Email is required").




        public ApiResponse(bool isSuccess, string message = null, T data = default, List<string> errors = null)
        {
            IsSuccess = isSuccess;
            Message = message;
            Data = data;
            Errors = errors;
        }



        //  Static Helper Methods 

        public static ApiResponse<T> Success(T data, string message = "Success")

        {
            return new ApiResponse<T>(true, message, data);
        }

        public static ApiResponse<T> Failure(string errorMessage, List<string> errors = null)

        {
            return new ApiResponse<T>(false, errorMessage, default, errors);
        }

    }

}
