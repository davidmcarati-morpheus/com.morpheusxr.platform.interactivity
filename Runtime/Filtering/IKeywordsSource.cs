using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IKeywordsSource
{
    IReadOnlyCollection<string> Keywords { get; }
}
