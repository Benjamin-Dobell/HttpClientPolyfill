// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace System.Net.Http.Headers
{
  [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix",
      Justification = "This is not a collection")]
  public sealed class HttpContentHeaders : HttpHeaders
  {
    private static readonly Dictionary<string, HttpHeaderParser> s_parserStore = CreateParserStore();
    private static readonly HashSet<string> s_invalidHeaders = CreateInvalidHeaders();

    private readonly HttpContent _parent;
    private bool _contentLengthSet;

    private HttpHeaderValueCollection<string> _allow;
    private HttpHeaderValueCollection<string> _contentEncoding;
    private HttpHeaderValueCollection<string> _contentLanguage;

    public ICollection<string> Allow
    {
      get
      {
        if (_allow == null)
        {
          _allow = new HttpHeaderValueCollection<string>(KnownHeaderNames.Allow,
              this, HeaderUtilities.TokenValidator);
        }
        return _allow;
      }
    }

    public ContentDispositionHeaderValue ContentDisposition
    {
      get { return (ContentDispositionHeaderValue)GetParsedValues(KnownHeaderNames.ContentDisposition); }
      set { SetOrRemoveParsedValue(KnownHeaderNames.ContentDisposition, value); }
    }

    // Must be a collection (and not provide properties like "GZip", "Deflate", etc.) since the 
    // order matters!
    public ICollection<string> ContentEncoding
    {
      get
      {
        if (_contentEncoding == null)
        {
          _contentEncoding = new HttpHeaderValueCollection<string>(KnownHeaderNames.ContentEncoding,
              this, HeaderUtilities.TokenValidator);
        }
        return _contentEncoding;
      }
    }

    public ICollection<string> ContentLanguage
    {
      get
      {
        if (_contentLanguage == null)
        {
          _contentLanguage = new HttpHeaderValueCollection<string>(KnownHeaderNames.ContentLanguage,
              this, HeaderUtilities.TokenValidator);
        }
        return _contentLanguage;
      }
    }

    public long? ContentLength
    {
      get
      {
        // 'Content-Length' can only hold one value. So either we get 'null' back or a boxed long value.
        object storedValue = GetParsedValues(KnownHeaderNames.ContentLength);

        // Only try to calculate the length if the user didn't set the value explicitly using the setter.
        if (!_contentLengthSet && (storedValue == null))
        {
          // If we don't have a value for Content-Length in the store, try to let the content calculate
          // it's length. If the content object is able to calculate the length, we'll store it in the
          // store.
          long? calculatedLength = _parent.GetComputedOrBufferLength();

          if (calculatedLength != null)
          {
            SetParsedValue(KnownHeaderNames.ContentLength, (object)calculatedLength.Value);
          }

          return calculatedLength;
        }

        if (storedValue == null)
        {
          return null;
        }
        else
        {
          return (long)storedValue;
        }
      }
      set
      {
        SetOrRemoveParsedValue(KnownHeaderNames.ContentLength, value); // box long value
        _contentLengthSet = true;
      }
    }

    public Uri ContentLocation
    {
      get { return (Uri)GetParsedValues(KnownHeaderNames.ContentLocation); }
      set { SetOrRemoveParsedValue(KnownHeaderNames.ContentLocation, value); }
    }

    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays",
        Justification = "In this case the 'value' is the byte array. I.e. the array is treated as a value.")]
    public byte[] ContentMD5
    {
      get { return (byte[])GetParsedValues(KnownHeaderNames.ContentMD5); }
      set { SetOrRemoveParsedValue(KnownHeaderNames.ContentMD5, value); }
    }

    public ContentRangeHeaderValue ContentRange
    {
      get { return (ContentRangeHeaderValue)GetParsedValues(KnownHeaderNames.ContentRange); }
      set { SetOrRemoveParsedValue(KnownHeaderNames.ContentRange, value); }
    }

    public MediaTypeHeaderValue ContentType
    {
      get { return (MediaTypeHeaderValue)GetParsedValues(KnownHeaderNames.ContentType); }
      set { SetOrRemoveParsedValue(KnownHeaderNames.ContentType, value); }
    }

    public DateTimeOffset? Expires
    {
      get { return HeaderUtilities.GetDateTimeOffsetValue(KnownHeaderNames.Expires, this); }
      set { SetOrRemoveParsedValue(KnownHeaderNames.Expires, value); }
    }

    public DateTimeOffset? LastModified
    {
      get { return HeaderUtilities.GetDateTimeOffsetValue(KnownHeaderNames.LastModified, this); }
      set { SetOrRemoveParsedValue(KnownHeaderNames.LastModified, value); }
    }

    internal HttpContentHeaders(HttpContent parent)
    {
      _parent = parent;

      SetConfiguration(s_parserStore, s_invalidHeaders);
    }

    private static Dictionary<string, HttpHeaderParser> CreateParserStore()
    {
      var parserStore = new Dictionary<string, HttpHeaderParser>(11, StringComparer.OrdinalIgnoreCase);

      parserStore.Add(KnownHeaderNames.Allow, GenericHeaderParser.TokenListParser);
      parserStore.Add(KnownHeaderNames.ContentDisposition, GenericHeaderParser.ContentDispositionParser);
      parserStore.Add(KnownHeaderNames.ContentEncoding, GenericHeaderParser.TokenListParser);
      parserStore.Add(KnownHeaderNames.ContentLanguage, GenericHeaderParser.TokenListParser);
      parserStore.Add(KnownHeaderNames.ContentLength, Int64NumberHeaderParser.Parser);
      parserStore.Add(KnownHeaderNames.ContentLocation, UriHeaderParser.RelativeOrAbsoluteUriParser);
      parserStore.Add(KnownHeaderNames.ContentMD5, ByteArrayHeaderParser.Parser);
      parserStore.Add(KnownHeaderNames.ContentRange, GenericHeaderParser.ContentRangeParser);
      parserStore.Add(KnownHeaderNames.ContentType, MediaTypeHeaderParser.SingleValueParser);
      parserStore.Add(KnownHeaderNames.Expires, DateHeaderParser.Parser);
      parserStore.Add(KnownHeaderNames.LastModified, DateHeaderParser.Parser);

      return parserStore;
    }

    private static HashSet<string> CreateInvalidHeaders()
    {
      var invalidHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

      HttpRequestHeaders.AddKnownHeaders(invalidHeaders);
      HttpResponseHeaders.AddKnownHeaders(invalidHeaders);
      HttpGeneralHeaders.AddKnownHeaders(invalidHeaders);

      return invalidHeaders;
    }

    internal static void AddKnownHeaders(HashSet<string> headerSet)
    {
      Debug.Assert(headerSet != null);

      headerSet.Add(KnownHeaderNames.Allow);
      headerSet.Add(KnownHeaderNames.ContentDisposition);
      headerSet.Add(KnownHeaderNames.ContentEncoding);
      headerSet.Add(KnownHeaderNames.ContentLanguage);
      headerSet.Add(KnownHeaderNames.ContentLength);
      headerSet.Add(KnownHeaderNames.ContentLocation);
      headerSet.Add(KnownHeaderNames.ContentMD5);
      headerSet.Add(KnownHeaderNames.ContentRange);
      headerSet.Add(KnownHeaderNames.ContentType);
      headerSet.Add(KnownHeaderNames.Expires);
      headerSet.Add(KnownHeaderNames.LastModified);
    }
  }
}