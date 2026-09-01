using UnityEngine;

namespace GRstory.Combat
{
    public class DamageContext
    {
        public GameObject Attacker { get; set; }

        public float Damage { get; set; }

        public EDamageType Type { get; set; }
    }
}