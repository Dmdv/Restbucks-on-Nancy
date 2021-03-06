﻿namespace RestBucks.Infrastructure
{
  using Nancy.Responses.Negotiation;
  using System;
  using System.Collections.Generic;
  using Nancy;
  using System.Linq;
  using Nancy.Responses;

  public class XmlResponseProcessor : IResponseProcessor
  {
    private readonly ISerializer xmlSerializer;

    public XmlResponseProcessor(IEnumerable<ISerializer> serializers)
    {
      xmlSerializer = serializers.First(s => s.GetType() == typeof(DefaultXmlSerializer));
    }

    public ProcessorMatch CanProcess(MediaRange requestedMediaRange, dynamic model, NancyContext context)
    {
      if (IsSomeXmlType(requestedMediaRange) || IsTextHhtml(requestedMediaRange))
        return new ProcessorMatch {RequestedContentTypeResult = MatchResult.ExactMatch, ModelResult = MatchResult.DontCare};
      else
        return new ProcessorMatch {ModelResult = MatchResult.NoMatch, RequestedContentTypeResult = MatchResult.NoMatch};
    }

    private static bool IsSomeXmlType(MediaRange requestedMediaRange)
    {
      return IsWildCardOrApplicationType(requestedMediaRange.Type) && IsWildCardOrXmlSubType(requestedMediaRange.Subtype);
    }

    private bool IsTextHhtml(MediaRange requestedMediaRange)
    {
      return requestedMediaRange.Type.ToString().Equals("text", StringComparison.InvariantCultureIgnoreCase) &&
             requestedMediaRange.Subtype.ToString().Equals("html", StringComparison.InvariantCultureIgnoreCase);
    }

    private static bool IsWildCardOrXmlSubType(MediaType subtype)
    {
      return (subtype.IsWildcard ||
              subtype.ToString().Equals("xml", StringComparison.InvariantCultureIgnoreCase) ||
              subtype.ToString().EndsWith("+xml", StringComparison.InvariantCultureIgnoreCase));
    }

    private static bool IsWildCardOrApplicationType(MediaType superType)
    {
      return (superType.IsWildcard || superType.ToString().ToLower().Equals("application"));
    }

    public Response Process(MediaRange requestedMediaRange, dynamic model, NancyContext context)
    {
      var xmlResponseType = typeof (XmlResponse<>).MakeGenericType(model.GetType());
      return Activator.CreateInstance(xmlResponseType, model, "application/vnd.restbucks+xml", xmlSerializer);
    }

    public IEnumerable<Tuple<string, MediaRange>> ExtensionMappings
    {
      get
      {
        yield return new Tuple<string, MediaRange>("xml", "application/vnd.restbucks+xml");
      }
    }
  }
}