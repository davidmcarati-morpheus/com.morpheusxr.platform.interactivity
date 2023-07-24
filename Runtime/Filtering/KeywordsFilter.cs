using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class KeywordsFilter
{
    [SerializeField] string CheckExpression;

    public bool Check(GameObject go)
    {
        var keywordsSource = go.GetComponentInParent<IKeywordsSource>();
        if (keywordsSource != null)
        {
            if (string.IsNullOrEmpty(CheckExpression))
                return true;

            if (keywordsSource.Keywords != null && keywordsSource.Keywords.Count > 0)
            {
                foreach (var keyword in keywordsSource.Keywords)
                {
                    if (CheckExpression.Contains(keyword))
                        return true;
                }
            }
        }

        return false;
    }
}
