using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.View;
using QuickGraph.Algorithms.Search;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Kingmaker.Visual.Mounts
{
    [ExecuteInEditMode]
    public class MountOffsets : Ex.Kingmaker.Visual.Mounts.MountOffsets
    {
        public MountOffsets() : base()
        {

        }

#if UNITY_EDITOR

        public bool AutoUpdate = true;
        int wait = 0;

        private void Update()
        {
            if (!AutoUpdate)
            {
                wait = 0;
                return;
            }
            if (wait < 15)
            {
                wait++;
                return;
            }

            if (OffsetsConfig == null)
                return;

            var race = gameObject.GetComponent<UnitEntityView>()?.Data.Get<UnitPartSaddled>()?.Rider?.Blueprint.Race;
            if (!race)
                return;
            var offsetData = OffsetsConfig.offsets.FirstOrDefault(data => data.Races.Contains(race));
            if (offsetData == null)
                return;
            if (Root != null)
                offsetData.RootPosition = Root.localPosition;
            if (RootBattle != null)
                offsetData.RootBattlePosition = RootBattle.localPosition;
            if (PelvisIkTarget != null)
            {
                offsetData.PelvisPosition = PelvisIkTarget.localPosition;
                offsetData.PelvisRotation = Quat(PelvisIkTarget.localRotation);
            }
            if (Hands != null)
                offsetData.HandsPosition = Hands.localPosition;
            if (RightKneeIkTarget != null)
                offsetData.RightKneePosition = RightKneeIkTarget.localPosition;
            if (LeftKneeIkTarget != null)
                offsetData.LeftKneePosition = LeftKneeIkTarget.localPosition;
            if (RightFootIkTarget != null)
            {
                offsetData.RightFootPosition = RightFootIkTarget.localPosition;
                offsetData.RightFootRotation = Quat(RightFootIkTarget.localRotation);
            }
            if (LeftFootIkTarget != null)
            {
                offsetData.LeftFootPosition = LeftFootIkTarget.localPosition;
                offsetData.LeftFootRotation = Quat(LeftFootIkTarget.localRotation);
            }
            EditorUtility.SetDirty(OffsetsConfig);
        }
        Vector4 Quat(Quaternion vec)
            => new Vector4(vec.x, vec.y, vec.z, vec.w);
#endif

    }
}
