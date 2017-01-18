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
  public sealed class HttpRequestHeaders : HttpHeaders
  {
    private static readonly Dictionary<string, HttpHeaderParser> s_parserStore = CreateParserStore();
    private static readonly HashSet<string> s_invalidHeaders = CreateInvalidHeaders();

    private HttpGeneralHeaders _generalHeaders;
    private HttpHeaderValueCollection<MediaTypeWithQualityHeaderValue> _accept;
    private HttpHeaderValueCollection<NameValueWithParametersHeaderValue> _expect;
    private bool _expectContinueSet;
    private HttpHeaderValueCollection<EntityTagHeaderValue> _ifMatch;
    private HttpHeaderValueCollection<EntityTagHeaderValue> _ifNoneMatch;
    private HttpHeaderValueCollection<TransferCodingWithQualityHeaderValue> _te;
    private HttpHeaderValueCollection<ProductInfoHeaderValue> _userAgent;
    private HttpHeaderValueCollection<StringWithQualityHeaderValue> _acceptCharset;
    private HttpHeaderValueCollection<StringWithQualityHeaderValue> _acceptEncoding;
    private HttpHeaderValueCollection<StringWithQualityHeaderValue> _acceptLanguage;

    #region Request Headers

    public HttpHeaderValueCollection<MediaTypeWithQualityHeaderValue> Accept
    {
      get
      {
        if (_accept == null)
        {
          _accept = new HttpHeaderValueCollection<MediaTypeWithQualityHeaderValue>(
              KnownHeaderNames.Accept, this);
        }
        return _accept;
      }
    }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Charset",
        Justification = "The HTTP header name is 'Accept-Charset'.")]
    public HttpHeaderValueCollection<StringWithQualityHeaderValue> AcceptCharset
    {
      get
      {
        if (_acceptCharset == null)
        {
          _acceptCharset = new HttpHeaderValueCollection<StringWithQualityHeaderValue>(
              KnownHeaderNames.AcceptCharset, this);
        }
        return _acceptCharset;
      }
    }

    public HttpHeaderValueCollection<StringWithQualityHeaderValue> AcceptEncoding
    {
      get
      {
        if (_acceptEncoding == null)
        {
          _acceptEncoding = new HttpHeaderValueCollection<StringWithQualityHeaderValue>(
              KnownHeaderNames.AcceptEncoding, this);
        }
        return _acceptEncoding;
      }
    }

    public HttpHeaderValueCollection<StringWithQualityHeaderValue> AcceptLanguage
    {
      get
      {
        if (_acceptLanguage == null)
        {
          _acceptLanguage = new HttpHeaderValueCollection<StringWithQualityHeaderValue>(
              KnownHeaderNames.AcceptLanguage, this);
        }
        return _acceptLanguage;
      }
    }

    public AuthenticationHeaderValue Authorization
    {
      get { return (AuthenticationHeaderValue)GetParsedValues(KnownHeaderNames.Authorization); }
      set { SetOrRemoveParsedValue(KnownHeaderNames.Authorization, value); }
    }

    public HttpHeaderValueCollection<NameValueWithParametersHeaderValue> Expect
    {
      get { return ExpectCore; }
    }

    public bool? ExpectContinue
    {
      get
      {
        if (ExpectCore.IsSpecialValueSet)
        {
          return true;
        }
        if (_expectContinueSet)
        {
          return false;
        }
        return null;
      }
      set
      {
        if (value == true)
        {
          _expectContinueSet = true;
          ExpectCore.SetSpecialValue();
        }
        else
        {
          _expectContinueSet = value != null;
          ExpectCore.RemoveSpecialValue();
        }
      }
    }

    public string From
    {
      get { return (string)GetParsedValues(KnownHeaderNames.From); }
      set
      {
        // Null and empty string are equivalent. In this case it means, remove the From header value (if any).
        if (value == string.Empty)
        {
          value = null;
        }

        if ((value != null) && !HeaderUtilities.IsValidEmailAddress(value))
        {
          throw new FormatException("The specified value is not a valid 'From' header string.");
        }
        SetOrRemoveParsedValue(KnownHeaderNames.From, value);
      }
    }

    public string Host
    {
      get { return (string)GetParsedValues(KnownHeaderNames.Host); }
      set
      {
        // Null and empty string are equivalent. In this case it means, remove the Host header value (if any).
        if (value == string.Empty)
        {
          value = null;
        }

        string host = null;
        if ((value != null) && (HttpRuleParser.GetHostLength(value, 0, false, out host) != value.Length))
        {
          throw new FormatException("The specified value is not a valid 'Host' header string.");
        }
        SetOrRemoveParsedValue(KnownHeaderNames.Host, value);
      }
    }

    public HttpHeaderValueCollection<EntityTagHeaderValue> IfMatch
    {
      get
      {
        if (_ifMatch == null)
        {
          _ifMatch = new HttpHeaderValueCollection<EntityTagHeaderValue>(
              KnownHeaderNames.IfMatch, this);
        }
        return _ifMatch;
      }
    }

    public DateTimeOffset? IfModifiedSince
    {
      get { return HeaderUtilities.GetDateTimeOffsetValue(KnownHeaderNames.IfModifiedSince, this); }
      set { SetOrRemoveParsedValue(KnownHeaderNames.IfModifiedSince, value); }
    }

    public HttpHeaderValueCollection<EntityTagHeaderValue> IfNoneMatch
    {
      get
      {
        if (_ifNoneMatch == null)
        {
          _ifNoneMatch = new HttpHeaderValueCollection<EntityTagHeaderValue>(
              KnownHeaderNames.IfNoneMatch, this);
        }
        return _ifNoneMatch;
      }
    }

    public RangeConditionHeaderValue IfRange
    {
      get { return (RangeConditionHeaderValue)GetParsedValues(KnownHeaderNames.IfRange); }
      set { SetOrRemoveParsedValue(KnownHeaderNames.IfRange, value); }
    }

    public DateTimeOffset? IfUnmodifiedSince
    {
      get { return HeaderUtilities.GetDateTimeOffsetValue(KnownHeaderNames.IfUnmodifiedSince, this); }
      set { SetOrRemoveParsedValue(KnownHeaderNames.IfUnmodifiedSince, value); }
    }

    public int? MaxForwards
    {
      get
      {
        object storedValue = GetParsedValues(KnownHeaderNames.MaxForwards);
        if (storedValue != null)
        {
          return (int)storedValue;
        }
        return null;
      }
      set { SetOrRemoveParsedValue(KnownHeaderNames.MaxForwards, value); }
    }


    public AuthenticationHeaderValue ProxyAuthorization
    {
      get { return (AuthenticationHeaderValue)GetParsedValues(KnownHeaderNames.ProxyAuthorization); }
      set { SetOrRemoveParsedValue(KnownHeaderNames.ProxyAuthorization, value); }
    }

    public RangeHeaderValue Range
    {
      get { return (RangeHeaderValue)GetParsedValues(KnownHeaderNames.Range); }
      set { SetOrRemoveParsedValue(KnownHeaderNames.Range, value); }
    }

    public Uri Referrer
    {
      get { return (Uri)GetParsedValues(KnownHeaderNames.Referer); }
      set { SetOrRemoveParsedValue(KnownHeaderNames.Referer, value); }
    }

    public HttpHeaderValueCollection<TransferCodingWithQualityHeaderValue> TE
    {
      get
      {
        if (_te == null)
        {
          _te = new HttpHeaderValueCollection<TransferCodingWithQualityHeaderValue>(
              KnownHeaderNames.TE, this);
        }
        return _te;
      }
    }

    public HttpHeaderValueCollection<ProductInfoHeaderValue> UserAgent
    {
      get
      {
        if (_userAgent == null)
        {
          _userAgent = new HttpHeaderValueCollection<ProductInfoHeaderValue>(KnownHeaderNames.UserAgent,
              this);
        }
        return _userAgent;
      }
    }

    private HttpHeaderValueCollection<NameValueWithParametersHeaderValue> ExpectCore
    {
      get
      {
        if (_expect == null)
        {
          _expect = new HttpHeaderValueCollection<NameValueWithParametersHeaderValue>(
              KnownHeaderNames.Expect, this, HeaderUtilities.ExpectContinue);
        }
        return _expect;
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

    internal HttpRequestHeaders()
    {
      _generalHeaders = new HttpGeneralHeaders(this);

      base.SetConfiguration(s_parserStore, s_invalidHeaders);
    }

    private static Dictionary<string, HttpHeaderParser> CreateParserStore()
    {
      var parserStore = new Dictionary<string, HttpHeaderParser>(StringComparer.OrdinalIgnoreCase);

      parserStore.Add(KnownHeaderNames.Accept, MediaTypeHeaderParser.MultipleValuesParser);
      parserStore.Add(KnownHeaderNames.AcceptCharset, GenericHeaderParser.MultipleValueStringWithQualityParser);
      parserStore.Add(KnownHeaderNames.AcceptEncoding, GenericHeaderParser.MultipleValueStringWithQualityParser);
      parserStore.Add(KnownHeaderNames.AcceptLanguage, GenericHeaderParser.MultipleValueStringWithQualityParser);
      parserStore.Add(KnownHeaderNames.Authorization, GenericHeaderParser.SingleValueAuthenticationParser);
      parserStore.Add(KnownHeaderNames.Expect, GenericHeaderParser.MultipleValueNameValueWithParametersParser);
      parserStore.Add(KnownHeaderNames.From, GenericHeaderParser.MailAddressParser);
      parserStore.Add(KnownHeaderNames.Host, GenericHeaderParser.HostParser);
      parserStore.Add(KnownHeaderNames.IfMatch, GenericHeaderParser.MultipleValueEntityTagParser);
      parserStore.Add(KnownHeaderNames.IfModifiedSince, DateHeaderParser.Parser);
      parserStore.Add(KnownHeaderNames.IfNoneMatch, GenericHeaderParser.MultipleValueEntityTagParser);
      parserStore.Add(KnownHeaderNames.IfRange, GenericHeaderParser.RangeConditionParser);
      parserStore.Add(KnownHeaderNames.IfUnmodifiedSince, DateHeaderParser.Parser);
      parserStore.Add(KnownHeaderNames.MaxForwards, Int32NumberHeaderParser.Parser);
      parserStore.Add(KnownHeaderNames.ProxyAuthorization, GenericHeaderParser.SingleValueAuthenticationParser);
      parserStore.Add(KnownHeaderNames.Range, GenericHeaderParser.RangeParser);
      parserStore.Add(KnownHeaderNames.Referer, UriHeaderParser.RelativeOrAbsoluteUriParser);
      parserStore.Add(KnownHeaderNames.TE, TransferCodingHeaderParser.MultipleValueWithQualityParser);
      parserStore.Add(KnownHeaderNames.UserAgent, ProductInfoHeaderParser.MultipleValueParser);

      HttpGeneralHeaders.AddParsers(parserStore);

      return parserStore;
    }

    private static HashSet<string> CreateInvalidHeaders()
    {
      var invalidHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      HttpContentHeaders.AddKnownHeaders(invalidHeaders);
      return invalidHeaders;

      // Note: Reserved response header names are allowed as custom request header names.  Reserved response
      // headers have no defined meaning or format when used on a request.  This enables a server to accept
      // any headers sent from the client as either content headers or request headers.
    }

    internal static void AddKnownHeaders(HashSet<string> headerSet)
    {
      Debug.Assert(headerSet != null);

      headerSet.Add(KnownHeaderNames.Accept);
      headerSet.Add(KnownHeaderNames.AcceptCharset);
      headerSet.Add(KnownHeaderNames.AcceptEncoding);
      headerSet.Add(KnownHeaderNames.AcceptLanguage);
      headerSet.Add(KnownHeaderNames.Authorization);
      headerSet.Add(KnownHeaderNames.Expect);
      headerSet.Add(KnownHeaderNames.From);
      headerSet.Add(KnownHeaderNames.Host);
      headerSet.Add(KnownHeaderNames.IfMatch);
      headerSet.Add(KnownHeaderNames.IfModifiedSince);
      headerSet.Add(KnownHeaderNames.IfNoneMatch);
      headerSet.Add(KnownHeaderNames.IfRange);
      headerSet.Add(KnownHeaderNames.IfUnmodifiedSince);
      headerSet.Add(KnownHeaderNames.MaxForwards);
      headerSet.Add(KnownHeaderNames.ProxyAuthorization);
      headerSet.Add(KnownHeaderNames.Range);
      headerSet.Add(KnownHeaderNames.Referer);
      headerSet.Add(KnownHeaderNames.TE);
      headerSet.Add(KnownHeaderNames.UserAgent);
    }

    internal override void AddHeaders(HttpHeaders sourceHeaders)
    {
      base.AddHeaders(sourceHeaders);
      HttpRequestHeaders sourceRequestHeaders = sourceHeaders as HttpRequestHeaders;
      Debug.Assert(sourceRequestHeaders != null);

      // Copy special values but do not overwrite.
      _generalHeaders.AddSpecialsFrom(sourceRequestHeaders._generalHeaders);

      bool? expectContinue = ExpectContinue;
      if (!expectContinue.HasValue)
      {
        ExpectContinue = sourceRequestHeaders.ExpectContinue;
      }
    }
  }
}