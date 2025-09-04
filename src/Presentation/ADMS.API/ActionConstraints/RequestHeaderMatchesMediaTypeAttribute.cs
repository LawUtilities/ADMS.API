using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace ADMS.API.ActionConstraints;

/// <summary>
///     Validates request header
/// </summary>
[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
public class RequestHeaderMatchesMediaTypeAttribute : Attribute, IActionConstraint
{
    private readonly MediaTypeCollection _mediaTypes = [];
    private readonly string _requestHeaderToMatch;

    /// <summary>
    ///     RequestHeaderMatchesMediaTypeAttribute constructor
    /// </summary>
    /// <param name="requestHeaderToMatch">Request Header to Match</param>
    /// <param name="mediaType">media Type</param>
    /// <param name="otherMediaTypes">other Media Types</param>
    /// <exception cref="ArgumentNullException">requestHeaderToMatch is null</exception>
    /// <exception cref="ArgumentException">media type or other media types are null</exception>
    public RequestHeaderMatchesMediaTypeAttribute(string requestHeaderToMatch,
        string mediaType, params string[] otherMediaTypes)
    {
        _requestHeaderToMatch = requestHeaderToMatch
                                ?? throw new ArgumentNullException(nameof(requestHeaderToMatch));

        // check if the inputted media types are valid media types
        // and add them to the _mediaTypes collection                     

        if (MediaTypeHeaderValue.TryParse(mediaType,
                out var parsedMediaType))
            _mediaTypes.Add(parsedMediaType);
        else
            throw new ArgumentException(null, nameof(mediaType));

        foreach (var otherMediaType in otherMediaTypes)
            if (MediaTypeHeaderValue.TryParse(otherMediaType,
                    out var parsedOtherMediaType))
                _mediaTypes.Add(parsedOtherMediaType);
            else
                throw new ArgumentException(null, nameof(otherMediaTypes));
    }

    /// <summary>
    ///     Search Order
    /// </summary>
    public int Order { get; }

    /// <summary>
    ///     Verifies match
    /// </summary>
    /// <param name="context">context to match</param>
    /// <returns>true if matched, false otherwise</returns>
    public bool Accept(ActionConstraintContext context)
    {
        var requestHeaders = context.RouteContext.HttpContext.Request.Headers;
        if (!requestHeaders.TryGetValue(_requestHeaderToMatch, out var value)) return false;

#pragma warning disable CS8604 // Possible null reference argument.
        MediaType parsedRequestMediaType = new(value);
#pragma warning restore CS8604 // Possible null reference argument.

        // if one of the media types matches, return true
        return _mediaTypes.Select(mediaType => new MediaType(mediaType)).Contains(parsedRequestMediaType);
    }
}