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
  public sealed class HttpResponseHeaders : HttpHeaders
  {
    private static readonly Dictionary<string, HttpHeaderParser> s_parserStore = CreateParserStore();
    private static readonly HashSet<string> s_invalidHeaders = CreateInvalidHeaders();

    private HttpGeneralHeaders _generalHeaders;
    private HttpHeaderValueCollection<string> _acceptRanges;
    private HttpHeaderValueCollection<AuthenticationHeaderValue> _wwwAuthenticate;
    private HttpHeaderValueCollection<AuthenticationHeaderValue> _proxyAuthenticate;
    private HttpHeaderValueCollection<ProductInfoHeaderValue> _server;
    private HttpHeaderValueCollection<string> _vary;

    #region Response Headers

    public HttpHeaderValueCollection<string> AcceptRanges
    {
      get
      {
        if (_acceptRanges == null)
        {
          _acceptRanges = new HttpHeaderValueCollection<string>(KnownHeaderNames.AcceptRanges,
              this, HeaderUtilities.TokenValidator);
        }
        return _acceptRanges;
      }
    }

    public TimeSpan? Age
    {
      get { return HeaderUtilities.GetTimeSpanValue(KnownHeaderNames.Age, this); }
      set { SetOrRemoveParsedValue(KnownHeaderNames.Age, value); }
    }

    public EntityTagHeaderValue ETag
    {
      get { return (EntityTagHeaderValue)GetParsedValues(KnownHeaderNames.ETag); }
      set { SetOrRemoveParsedValue(KnownHeaderNames.ETag, value); }
    }

    public Uri Location
    {
      get { return (Uri)GetParsedValues(KnownHeaderNames.Location); }
      set { SetOrRemoveParsedValue(KnownHeaderNames.Location, value); }
    }

    public HttpHeaderValueCollection<AuthenticationHeaderValue> ProxyAuthenticate
    {
      get
      {
        if (_proxyAuthenticate == null)
        {
          _proxyAuthenticate = new HttpHeaderValueCollection<AuthenticationHeaderValue>(
              KnownHeaderNames.ProxyAuthenticate, this);
        }
        return _proxyAuthenticate;
      }
    }

    public RetryConditionHeaderValue RetryAfter
    {
      get { return (RetryConditionHeaderValue)GetParsedValues(KnownHeaderNames.RetryAfter); }
      set { SetOrRemoveParsedValue(KnownHeaderNames.RetryAfter, value); }
    }

    public HttpHeaderValueCollection<ProductInfoHeaderValue> Server
    {
      get
      {
        if (_server == null)
        {
          _server = new HttpHeaderValueCollection<ProductInfoHeaderValue>(KnownHeaderNames.Server, this);
        }
        return _server;
      }
    }

    public HttpHeaderValueCollection<string> Vary
    {
      get
      {
        if (_vary == null)
        {
          _vary = new HttpHeaderValueCollection<string>(KnownHeaderNames.Vary,
              this, HeaderUtilities.TokenValidator);
        }
        return _vary;
      }
    }

    public HttpHeaderValueCollection<AuthenticationHeaderValue> WwwAuthenticate
    {
      get
      {
        if (_wwwAuthenticate == null)
        {
          _wwwAuthenticate = new HttpHeaderValueCollection<AuthenticationHeaderValue>(
              KnownHeaderNames.WWWAuthenticate, this);
        }
        return _wwwAuthenticate;
      }
    }

    #endregion

    #region General Headers

    public CacheControlHeaderValue CacheControl
    {
      get { return _generalHeaders.CacheControl; }
      set { _generalHeaders.CacheControl = value; }
    }

    public HttpHeaderValueCollection<string> Connection
    {
      get { return _generalHeaders.Connection; }
    }

    public bool? ConnectionClose
    {
      get { return _generalHeaders.ConnectionClose; }
      set { _generalHeaders.ConnectionClose = value; }
    }

    public DateTimeOffset? Date
    {
      get { return _generalHeaders.Date; }
      set { _generalHeaders.Date = value; }
    }

    public HttpHeaderValueCollection<NameValueHeaderValue> Pragma
    {
      get { return _generalHeaders.Pragma; }
    }

    public HttpHeaderValueCollection<string> Trailer
    {
      get { return _generalHeaders.Trailer; }
    }

    public HttpHeaderValueCollection<TransferCodingHeaderValue> TransferEncoding
    {
      get { return _generalHeaders.TransferEncoding; }
    }

    public bool? TransferEncodingChunked
    {
      get { return _generalHeaders.TransferEncodingChunked; }
      set { _generalHeaders.TransferEncodingChunked = value; }
    }

    public HttpHeaderValueCollection<ProductHeaderValue> Upgrade
    {
      get { return _generalHeaders.Upgrade; }
    }

    public HttpHeaderValueCollection<ViaHeaderValue> Via
    {
      get { return _generalHeaders.Via; }
    }

    public HttpHeaderValueCollection<WarningHeaderValue> Warning
    {
      get { return _generalHeaders.Warning; }
    }

    #endregion

    internal HttpResponseHeaders()
    {
      _generalHeaders = new HttpGeneralHeaders(this);

      base.SetConfiguration(s_parserStore, s_invalidHeaders);
    }

    private static Dictionary<string, HttpHeaderParser> CreateParserStore()
    {
      var parserStore = new Dictionary<string, HttpHeaderParser>(StringComparer.OrdinalIgnoreCase);

      parserStore.Add(KnownHeaderNames.AcceptRanges, GenericHeaderParser.TokenListParser);
      parserStore.Add(KnownHeaderNames.Age, TimeSpanHeaderParser.Parser);
      parserStore.Add(KnownHeaderNames.ETag, GenericHeaderParser.SingleValueEntityTagParser);
      parserStore.Add(KnownHeaderNames.Location, UriHeaderParser.RelativeOrAbsoluteUriParser);
      parserStore.Add(KnownHeaderNames.ProxyAuthenticate, GenericHeaderParser.MultipleValueAuthenticationParser);
      parserStore.Add(KnownHeaderNames.RetryAfter, GenericHeaderParser.RetryConditionParser);
      parserStore.Add(KnownHeaderNames.Server, ProductInfoHeaderParser.MultipleValueParser);
      parserStore.Add(KnownHeaderNames.Vary, GenericHeaderParser.TokenListParser);
      parserStore.Add(KnownHeaderNames.WWWAuthenticate, GenericHeaderParser.MultipleValueAuthenticationParser);

      HttpGeneralHeaders.AddParsers(parserStore);

      return parserStore;
    }

    private static HashSet<string> CreateInvalidHeaders()
    {
      var invalidHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      HttpContentHeaders.AddKnownHeaders(invalidHeaders);
      return invalidHeaders;

      // Note: Reserved request header names are allowed as custom response header names.  Reserved request
      // headers have no defined meaning or format when used on a response. This enables a client to accept
      // any headers sent from the server as either content headers or response headers.
    }

    internal static void AddKnownHeaders(HashSet<string> headerSet)
    {
      Debug.Assert(headerSet != null);

      headerSet.Add(KnownHeaderNames.AcceptRanges);
      headerSet.Add(KnownHeaderNames.Age);
      headerSet.Add(KnownHeaderNames.ETag);
      headerSet.Add(KnownHeaderNames.Location);
      headerSet.Add(KnownHeaderNames.ProxyAuthenticate);
      headerSet.Add(KnownHeaderNames.RetryAfter);
      headerSet.Add(KnownHeaderNames.Server);
      headerSet.Add(KnownHeaderNames.Vary);
      headerSet.Add(KnownHeaderNames.WWWAuthenticate);
    }

    internal override void AddHeaders(HttpHeaders sourceHeaders)
    {
      base.AddHeaders(sourceHeaders);
      HttpResponseHeaders sourceResponseHeaders = sourceHeaders as HttpResponseHeaders;
      Debug.Assert(sourceResponseHeaders != null);

      // Copy special values, but do not overwrite
      _generalHeaders.AddSpecialsFrom(sourceResponseHeaders._generalHeaders);
    }
  }
}