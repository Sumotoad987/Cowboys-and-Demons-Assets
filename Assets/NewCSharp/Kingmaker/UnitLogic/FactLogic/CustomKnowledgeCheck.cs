using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem;

namespace MyOwlcatModification
{
    [AllowedOn(typeof(BlueprintFeature), false)]
    [TypeId("f5ede7836ff34126adcd26e23e2def34")]
    public class CustomKnowledgeCheck : Ex.Kingmaker.UnitLogic.FactLogic.CustomKnowledgeCheck
    {
    }
}
