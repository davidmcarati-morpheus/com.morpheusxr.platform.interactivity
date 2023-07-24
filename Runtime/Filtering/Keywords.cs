using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Keywords : MonoBehaviour, IKeywordsSource
{
    [SerializeField] string[] keywords;
    IReadOnlyCollection<string> IKeywordsSource.Keywords => cachedKeywords;

    HashSet<string> cachedKeywords;

    private void Awake()
    {
        CacheKeywords(keywords);
    }

    public void AddKeyword(string keyword)
    {
        cachedKeywords.Add(keyword);
        keywords = new string[cachedKeywords.Count];
        cachedKeywords.CopyTo(keywords);
    }

    public void CacheKeywords(string[] keywordsList)
    {
        if (keywordsList == null)
            return;

        keywords = keywordsList;

        cachedKeywords = new HashSet<string>(keywords);
    }
}
