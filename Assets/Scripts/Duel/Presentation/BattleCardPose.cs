using UnityEngine;

namespace AChen.Duel.Presentation
{
    public readonly struct BattleCardPose
    {
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
        public Quaternion PlaneRotation { get; }
        public Vector3 PivotPosition { get; }
        public Vector3 Scale { get; }
        public Vector3 OffsetPosition { get; }
        public Quaternion OffsetRotation { get; }
        public Quaternion TurnRotation { get; }
        public bool Hand { get; }
        public BattleCardPose(Vector3 position,Quaternion rotation,Quaternion plane,Vector3 pivot,Vector3 scale,
            Vector3 offset,Quaternion offsetRotation,Quaternion turn,bool hand)
        {Position=position;Rotation=rotation;PlaneRotation=plane;PivotPosition=pivot;Scale=scale;
         OffsetPosition=offset;OffsetRotation=offsetRotation;TurnRotation=turn;Hand=hand;}
        public BattleCardPose WithAppeal(bool selected) => new BattleCardPose(Position,Rotation,PlaneRotation,
            PivotPosition+(selected?new Vector3(0,2,3):Vector3.zero),Scale,Vector3.zero,
            selected?Quaternion.identity:OffsetRotation,TurnRotation,Hand);
        public static BattleCardPose Lerp(BattleCardPose a,BattleCardPose b,float t,float arc=0)
            => new BattleCardPose(Vector3.Lerp(a.Position,b.Position,t)+Vector3.up*Mathf.Sin(t*Mathf.PI)*arc,
                Quaternion.Slerp(a.Rotation,b.Rotation,t),Quaternion.Slerp(a.PlaneRotation,b.PlaneRotation,t),
                Vector3.Lerp(a.PivotPosition,b.PivotPosition,t),Vector3.Lerp(a.Scale,b.Scale,t),
                Vector3.Lerp(a.OffsetPosition,b.OffsetPosition,t),Quaternion.Slerp(a.OffsetRotation,b.OffsetRotation,t),
                Quaternion.Slerp(a.TurnRotation,b.TurnRotation,t),b.Hand);
    }
}
