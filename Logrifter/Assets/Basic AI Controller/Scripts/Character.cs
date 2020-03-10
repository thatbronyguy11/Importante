using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ViridaxGameStudios.AI
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Animator))]
    [DisallowMultipleComponent()]
    public abstract class Character : MonoBehaviour
    {
        #region Variables
        [Header("Base Statistics")]
        [Range(1f,10f)]
        [Tooltip("This is used to calculate the attack damage based on your Strength, Intelligence and Faith.")]
        [SerializeField] public float m_StatsMultiplier = 1.75f;
        [Range(1f, 100f)]
        [Tooltip("This directly affects the HitPoints and physical damage.")]
        [SerializeField] public int m_Strength = 3;
        [Range(1f, 100f)]
        [Tooltip("This directly affects the HitPoints and mental damage.")]
        [SerializeField] public int m_Intelligence = 3;
        [Range(1f, 100f)]
        [Tooltip("This directly affects the HitPoints and spiritual damage.")]
        [SerializeField] public int m_Faith = 3;
        [Space(10)]

        [Header("Additional Statistics")]
        [Tooltip("The radius which the character can detect other objects")]
        public float m_DetectionRadius = 20f;
        [Tooltip("The maximum range that the character can be in order to attack a target.")]
        public float m_AttackRange = 7f;
        public float m_DamageAngle = 90.0f;
        public bool canAttack = true;
        [Tooltip("All the Tags that the character will consider as an enemy. NOTE: The default reaction is to attack.")]
        public List<string> enemyTags = new List<string>();
        [Tooltip("The points in the gameworld where you want the character to patrol. They can be anything, even empty gameObjects. Note: Ensure each patrol point is tagged as 'PatrolPoint'")]
        public List<GameObject> patrolPoints;
        public List<string> allyTags;
        [Tooltip("Whether or not the character should patrol each point in order of the list. False will allow the character to patrol randomly.")]
        public bool patrolInOrder = true;
        public bool isPatrolling = false;
        public bool enableHeadLook = false;
        public float headLookIntensity = 1f;
        CharacterStats stats;
        public int level = 0;
        public float attackDamage = 0;
        public float physicalDamage = 0;
        public float mentalDamage = 0;
        public float spiritualDamage = 0;
        public Animator Animator { get; set; }
        public float HitPoints { get; set; }
        public float MovementSpeed { get; set; }
        public float Damage { get; set; }
        public bool IsStunned { get; set; } = false;
        [HideInInspector] public int CURRENT_STATE = CharacterStates.STATE_IDLE;
        

        #endregion

        public class CharacterStates
        {
            //The different states that the character can be in. 
            //Note: A character can only be in one state at a time.
            public const int STATE_IDLE = 0;
            public const int STATE_DAME = 1;
            public const int STATE_ATTACK = 2;
            public const int STATE_FOLLOW = 3;
            public const int STATE_PATROL = 4;
            public const int STATE_GUARD = 5;
            public const int STATE_DEAD = 6;
        }


        // Start is called before the first frame update
        public virtual void Start()
        {
            stats = new CharacterStats(m_StatsMultiplier, m_Strength, m_Intelligence, m_Faith);
        }

        // Update is called once per frame
        public virtual void Update()
        {
            if(isPatrolling)
            {
                CURRENT_STATE = CharacterStates.STATE_PATROL;
            }
            else if(CURRENT_STATE == CharacterStates.STATE_PATROL)
            {
                CURRENT_STATE = CharacterStates.STATE_IDLE;
            }
            UpdateStats();
        }
        private void UpdateStats()
        {
            stats.DetectionRadius = m_DetectionRadius;
            stats.AttackRange = m_AttackRange;
            stats.DamageAngle = m_DamageAngle;
            stats.StatsMultiplier = m_StatsMultiplier;
            level = stats.Level;
            attackDamage = stats.AttackDamage;
            physicalDamage = stats.PhysicalDamage;
            mentalDamage = stats.MentalDamage;
            spiritualDamage = stats.SpiritualDamage;
            HitPoints = stats.HitPoints;
            Damage = stats.AttackDamage;
            MovementSpeed = stats.MovementSpeed;
        }
        public virtual void ReceiveDamage(float damage)
        {
            //
            //Method Name : void ReceiveDamage(float damage)
            //Purpose     : This method receives damage from various sources and applies it to the character.
            //Re-use      : none
            //Input       : float damage
            //Output      : none
            //
            IsStunned = true;
            if (HitPoints - damage <= 0)
            {
                HitPoints = 0; ;
                Animator.SetBool("isDead", true);
                CURRENT_STATE = CharacterStates.STATE_DEAD;
                //CharacterDead() method should be called after the death animation has finished playing using an Animation Event. 
                //Alternatively, you can implement your own logic here to suit your needs.
            }
            else
            {
                HitPoints -= damage;
                Animator.SetTrigger("isDamaged");
                CURRENT_STATE = CharacterStates.STATE_DAME;

            }
        }
        public virtual void ResumeFromDamage()
        {
            //
            //Method Name : void ResumeFromDamage()
            //Purpose     : This method allows the character to resume its normal functionality after the damage animation has played.
            //Re-use      : none
            //Input       : none
            //Output      : none
            //
            IsStunned = false;
            CURRENT_STATE = CharacterStates.STATE_IDLE;
        }
        public virtual void CharacterDead()
        {
            //
            //Method Name : void CharacterDead()
            //Purpose     : This method destroys the character GameObject as soon as the death animation has finished playing.
            //Re-use      : none
            //Input       : none
            //Output      : none
            //
            Destroy(gameObject);
        }



    }

}
