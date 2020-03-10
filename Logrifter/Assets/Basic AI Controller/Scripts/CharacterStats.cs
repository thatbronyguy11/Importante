using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ViridaxGameStudios
{
    [Serializable]
    public class CharacterStats
    {
        #region Instance Variables
        private int m_Level;
        private float m_StatsMultiplier;
        //Base Stats
        private int m_Strength;
        private int m_Intelligence;
        private int m_Faith;
        //Extension Stats
        private float m_HitPoints;
        private float m_MovementSpeed;
        private float m_AttackDamage;
        private float m_PhysicalDamage;
        private float m_MentalDamage;
        private float m_SpiritualDamage;
        private float m_DetectionRadius;
        private float m_AttackRange;
        private float m_DamageAngle;

        public int Level { get => m_Level; }
        public float StatsMultiplier { get => m_StatsMultiplier; set => m_StatsMultiplier = value; }
        public int Strength { get => m_Strength; }
        public int Intelligence { get => m_Intelligence; }
        public int Faith { get => m_Faith; }
        public float HitPoints { get => m_HitPoints; }
        public float MovementSpeed { get => m_MovementSpeed; }
        public float AttackDamage { get => m_AttackDamage; }
        public float PhysicalDamage { get => m_PhysicalDamage; }
        public float MentalDamage { get => m_MentalDamage; }
        public float SpiritualDamage { get => m_SpiritualDamage; }
        public float DetectionRadius { get => m_DetectionRadius; set => m_DetectionRadius = value; }
        public float AttackRange { get => m_AttackRange; set => m_AttackRange = value; }
        public float DamageAngle { get => m_DamageAngle; set => m_DamageAngle = value; }
        #endregion

        public CharacterStats(float m_StatsMultiplier, int m_Strength, int m_Intelligence, int m_Faith)
        {
            m_Level = 0;
            if (m_StatsMultiplier > 10 || m_StatsMultiplier == 0)
            {
                m_StatsMultiplier = 1.75f;
            }
            this.m_StatsMultiplier = m_StatsMultiplier;
            this.m_Strength = m_Strength;
            this.m_Intelligence = m_Intelligence;
            this.m_Faith = m_Faith;
            m_Level = 1;
            CalculateExtensionStats();
        }

        protected void CalculateExtensionStats()
        {
            //
            //Method Name : void CalculateExtensionStats()
            //Purpose     : This method calculates all of the extension stats using a standard formula.
            //Re-use      : none
            //Input       : none
            //Output      : none
            //
            m_HitPoints = Strength * Intelligence * Faith;
            m_MovementSpeed = 3 + ((Strength / 10) + (Intelligence / 10) + (Faith / 10));
            Debug.Log("Stats Mult: " + StatsMultiplier);
            m_PhysicalDamage = Strength * StatsMultiplier;
            m_MentalDamage = Intelligence * StatsMultiplier;
            m_SpiritualDamage = Faith * StatsMultiplier;
            
            m_AttackDamage = PhysicalDamage + MentalDamage + SpiritualDamage;
            Debug.Log("Damage: " + AttackDamage);
        }//end CalculateBaseStats()

        public void LevelUp()
        {
            //
            //Method Name : void LevelUp()
            //Purpose     : This method increases the level by one and adjusts the Base Stats accordingly.
            //Re-use      : none
            //Input       : float damage
            //Output      : none
            //
            m_Level++;
            m_Strength = Strength + 5;
            m_Intelligence = Intelligence + 3;
            m_Faith = Faith + 1;
        }//end LevelUp()
    }//end class
}

