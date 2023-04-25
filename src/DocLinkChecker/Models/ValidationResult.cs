namespace DocLinkChecker.Models
{
    using System.Net;

    /// <summary>
    /// Validation result.
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationResult"/> class.
        /// </summary>
        /// <param name="success">Value indicating whether validation was successful.</param>
        /// <param name="statuscode">HTTP Status code.</param>
        /// <param name="errormsg">Error message.</param>
        /// <param name="document">Document.</param>
        public ValidationResult(bool success, HttpStatusCode? statuscode, string errormsg, Hyperlink document)
        {
            Success = success;
            StatusCode = statuscode;
            ErrorMessage = errormsg;
            Document = document;
        }

        /// <summary>
        /// Gets or sets a value indicating whether validation was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the status code.
        /// </summary>
        public HttpStatusCode? StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the document.
        /// </summary>
        public Hyperlink Document { get; set; }

        /// <summary>
        /// Get the result as a string.
        /// </summary>
        /// <returns>String with result.</returns>
        public string ToReport()
        {
            var code = StatusCode == null ? "-1" : ((int)StatusCode).ToString();
            return $"{code}; {ErrorMessage}; {Document.LineNum}; {Document.Url}; {Document.FullPathName}";
        }
    }
}
