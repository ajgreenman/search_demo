using System;
using System.Collections.Generic;

namespace Search.Models
{
    [Serializable]
    public class SuggestionModel
    {
        public ResponseHeaderModel ResponseHeader { get; set; }
        public SpellcheckModel Spellcheck { get; set; }
    }

    [Serializable]
    public class ResponseHeaderModel
    {
        public int Status { get; set; }
        public int QTime { get; set; }
    }

    [Serializable]
    public class SpellcheckModel
    {
        public IEnumerable<object> Suggestions { get; set; }
    }

    [Serializable]
    public class SuggestionDetailModel
    {
        public string SearchText { get; set; }
        public int NumFound { get; set; }
        public IEnumerable<string> Suggestion { get; set; }
    }
}