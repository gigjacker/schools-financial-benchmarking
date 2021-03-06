﻿using System;
using System.Collections.Generic;

namespace SFB.Web.Domain.Models
{
    public class QueryResultsModel
    {
        public QueryResultsModel(int numberOfResults, dynamic facets, IEnumerable<IDictionary<string, object>> results,
            int taken,
            int skipped)
        {
            Facets = facets;
            NumberOfResults = numberOfResults;
            Taken = taken;
            Skipped = skipped;
            Results = results;
        }

        public string ErrorMessage { get; set; }
        public int NumberOfResults { get; set; }
        public IEnumerable<IDictionary<string, object>> Results { get; set; }
        public int Taken { get; private set; }
        public int Skipped { get; private set; }
        public dynamic Facets { get; set; }
        public string QueryLat { get; set; }
        public string QueryLong { get; set; }
        public bool Disambiguate { get; set; }
    }

    public class SuggestionQueryResult
    {
        public IList<Disambiguation> Matches { get; set; }

        public SuggestionQueryResult(IList<Disambiguation> matches)
        {
            Matches = matches;
        }

        public SuggestionQueryResult()
        {

        }
    }

    public class Disambiguation : IEquatable<Disambiguation>
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public string Lat { get; set; }
        public string Long { get; set; }

        public bool Equals(Disambiguation other) => other?.Id == Id;

        public override int GetHashCode() => Id.GetHashCode();

        public override bool Equals(object obj) => (obj as Disambiguation)?.Id == Id;
    }
}
